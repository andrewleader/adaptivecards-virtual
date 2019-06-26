using AdaptiveCards.Rendering.Uwp.Elements;
using Jurassic;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public FrameworkElement Render(string cardJson, string dataJson)
        {
            _dispatcher = Window.Current.Dispatcher;
            var cardWidth = _card.ActualWidth;
            _initialRenderTask = Task.Run(delegate
            {
                RenderHelper(cardJson, dataJson, cardWidth);
            });

            return _card;
        }

        private void RenderHelper(string cardJson, string dataJson, double cardWidth)
        {
            try
            {
                _scriptEngine = new ScriptEngine();
                _scriptEngine.Execute(ScriptLoader.GetScript());

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

                _scriptEngine.SetGlobalValue("cardJson", cardJson);
                _scriptEngine.SetGlobalValue("dataJson", dataJson);
                _scriptEngine.SetGlobalValue("cardWidth", cardWidth);
                _scriptEngine.Execute("var renderer = new Shared.SharedRenderer(); renderer.initialize(cardJson, dataJson, cardWidth);");
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

        internal async void UpdateInputValue(string inputId, string value)
        {
            await Task.Run(delegate
            {
                try
                {
                    _scriptEngine.SetGlobalValue("inputId", inputId);
                    _scriptEngine.SetGlobalValue("inputValue", value);
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
