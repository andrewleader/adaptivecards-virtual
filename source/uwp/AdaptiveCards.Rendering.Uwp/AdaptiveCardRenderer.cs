using AdaptiveCards.Rendering.Uwp.Elements;
using Jurassic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AdaptiveCards.Rendering.Uwp
{
    public sealed class AdaptiveCardRenderer
    {
        private ScriptEngine _scriptEngine;
        private Task _initialRenderTask;
        private AdaptiveCard _card = new AdaptiveCard();
        private CoreDispatcher _dispatcher;

        public event EventHandler<string> OnCardChanges;
        public event EventHandler<string> OnTransformedTemplateChanged;
        public event EventHandler<string> OnDataChanged;
        public event EventHandler<string> OnVirtualCardChanged;

        internal void ExecuteAction(string id)
        {
            _scriptEngine.SetGlobalValue("id", id);
            _scriptEngine.Execute("renderer.executeAction(id);");
        }

        public AdaptiveCardRenderer()
        {
            _card.Renderer = this;
            _card.SizeChanged += _card_SizeChanged;
        }

        private void _card_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width != e.PreviousSize.Width)
            {
                UpdateCardWidth(e.NewSize.Width);
            }
        }

        private void UpdateCardWidth(double width)
        {
            // This logic should be improved to batch changes and whatnot
            Task.Run(delegate
            {
                try
                {
                    _scriptEngine.SetGlobalValue("cardWidth", width);
                    _scriptEngine.Execute("renderer.updateCardWidth(cardWidth);");
                }
                catch { }
            });
        }

        public FrameworkElement Render(string cardJson, string dataJson, string cardScript)
        {
            _dispatcher = Window.Current.Dispatcher;
            var cardWidth = _card.ActualWidth;
            _initialRenderTask = Task.Run(delegate
            {
                RenderHelper(cardJson, dataJson, cardScript, cardWidth);
            });

            return _card;
        }

        private void RenderHelper(string cardJson, string dataJson, string cardScript, double cardWidth)
        {
            try
            {
                _scriptEngine = new ScriptEngine();
                _scriptEngine.Execute(ScriptLoader.GetScript());

                _scriptEngine.SetGlobalFunction("get", new Func<string, bool>((url) =>
                {
                    HttpGet(url);
                    return true;
                }));

                _scriptEngine.SetGlobalFunction("exec", new Func<string, object>((script) =>
                {
                    try
                    {
                        _scriptEngine.Execute(script);
                    }
                    catch { }
                    return null;
                }));

                _scriptEngine.SetGlobalFunction("getBlocking", new Func<string, string>((url) =>
                {
                    var client = new HttpClient();
                    string answer;
                    try
                    {
                        var task = client.GetStringAsync(url);
                        task.Wait();
                        answer = task.Result;
                    }
                    catch (Exception ex)
                    {
                        answer = ex.ToString();
                    }
                    return answer;
                }));

                _scriptEngine.Execute("var setData = function(data) { renderer.updateData(JSON.stringify(data)); }");
                _scriptEngine.Execute("var getData = function() { return renderer.getData(); }");

                _scriptEngine.SetGlobalFunction("onChanges", new Func<string, bool>((changesAsJson) =>
                {
                    var dontWait = _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate
                    {
                        try
                        {
                            OnChanges(changesAsJson);
                        }
                        catch { }
                    });
                    return true;
                }));

                _scriptEngine.SetGlobalFunction("onTransformedTemplateChanged", new Func<string, bool>((newTemplate) =>
                {
                    try
                    {
                        OnTransformedTemplateChanged?.Invoke(this, newTemplate);
                    }
                    catch { }
                    return true;
                }));

                _scriptEngine.SetGlobalFunction("onDataChanged", new Func<string, bool>((newData) =>
                {
                    try
                    {
                        OnDataChanged?.Invoke(this, newData);
                    }
                    catch { }
                    return true;
                }));

                _scriptEngine.SetGlobalFunction("onVirtualCardChanged", new Func<string, bool>((newVirtualCard) =>
                {
                    try
                    {
                        OnVirtualCardChanged?.Invoke(this, newVirtualCard);
                    }
                    catch { }
                    return true;
                }));

                _scriptEngine.SetGlobalValue("cardJson", cardJson);
                _scriptEngine.SetGlobalValue("dataJson", dataJson);
                _scriptEngine.SetGlobalValue("cardScript", cardScript);
                _scriptEngine.SetGlobalValue("cardWidth", cardWidth);
                _scriptEngine.Execute("var renderer = new Shared.SharedRenderer(); renderer.initialize(cardJson, dataJson, cardScript, cardWidth);");
            }
            catch (Exception ex)
            {
                var dontWait = _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate
                {
                    try
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                    }
                    catch { }
                });
            }
        }

        private async void HttpGet(string url)
        {
            var client = new HttpClient();
            string answer;
            try
            {
                answer = await client.GetStringAsync(url);
            }
            catch (Exception ex)
            {
                answer = ex.ToString();
            }
            _scriptEngine.SetGlobalValue("url", url);
            _scriptEngine.SetGlobalValue("response", answer);
            _scriptEngine.Execute("renderer.gotHttpResponse(url, response);");
        }

        internal void UpdateInputValueProperty(string inputId, object value)
        {
            dynamic values = new ExpandoObject();
            values.value = value;
            UpdateInput(inputId, values);
        }

        internal async void UpdateInput(string inputId, object values)
        {
            await Task.Run(delegate
            {
                string valueJson = JsonConvert.SerializeObject(values);

                try
                {
                    _scriptEngine.SetGlobalValue("inputId", inputId);
                    _scriptEngine.SetGlobalValue("inputValue", valueJson);
                    _scriptEngine.Execute("renderer.updateInputValue(inputId, inputValue);");
                }
                catch { }
            });
        }

        private void OnChanges(string changesAsJson)
        {
            try
            {
                OnCardChanges?.Invoke(this, changesAsJson);
                JArray changes = JArray.Parse(changesAsJson);
                OnChanges(changes);
            }
            catch
            {

            }
        }

        private void OnChanges(JArray changes)
        {
            foreach (var change in changes.OfType<JObject>())
            {
                switch (change.Value<string>("type"))
                {
                    case "ObjectChanges":
                        OnObjectChanges(change);
                        break;

                    case "ArrayChanges":
                        OnArrayChanges(change);
                        break;
                }
            }
        }

        private void OnObjectChanges(JObject objectChanges)
        {
            string id = objectChanges.Value<string>("id");
            var renderedItem = _card.GetById(id);
            if (renderedItem == null)
            {
                renderedItem = _card;
            }
            if (renderedItem != null)
            {
                JObject changedProperties = objectChanges.Value<JObject>("changes");
                foreach (var prop in changedProperties)
                {
                    renderedItem.ApplyPropertyChange(prop.Key, prop.Value);
                }
            }
        }

        private void OnArrayChanges(JObject arrayChanges)
        {
            string id = arrayChanges.Value<string>("id");
            var renderedItem = _card.GetById(id);
            if (renderedItem == null)
            {
                renderedItem = _card;
            }
            if (renderedItem != null)
            {
                renderedItem.ApplyArrayChanges(arrayChanges.Value<string>("property"), arrayChanges.Value<JArray>("changes"));
            }
        }

        public async void UpdateData(string text)
        {
            await Task.Run(delegate
            {
                try
                {
                    _scriptEngine.SetGlobalValue("data", text);
                    _scriptEngine.Execute("renderer.updateData(data);");
                }
                catch { }
            });
        }

        public async void UpdateTemplate(string template)
        {
            await Task.Run(delegate
            {
                try
                {
                    _scriptEngine.SetGlobalValue("template", template);
                    _scriptEngine.Execute("renderer.updateTemplate(template);");
                }
                catch { }
            });
        }
    }

    internal static class ScriptLoader
    {
        private static string _sharedJs;
        public static string GetScript()
        {
            if (_sharedJs != null)
            {
                return _sharedJs;
            }

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().First(i => i.EndsWith("shared.min.js"));

            using (Stream s = assembly.GetManifestResourceStream(resourceName))
            {
                using (StreamReader reader = new StreamReader(s))
                {
                    _sharedJs = reader.ReadToEnd();
                }
            }

            return _sharedJs;
        }
    }
}
