using Jurassic;
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
        private Border _container = new Border();
        private CoreDispatcher _dispatcher;

        public FrameworkElement Render(string cardJson, string dataJson)
        {
            _dispatcher = Window.Current.Dispatcher;
            _initialRenderTask = Task.Run(delegate
            {
                RenderHelper(cardJson, dataJson);
            });

            return _container;
        }

        private void RenderHelper(string cardJson, string dataJson)
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

                _scriptEngine.SetGlobalValue("cardJson", cardJson);
                _scriptEngine.SetGlobalValue("dataJson", dataJson);
                _scriptEngine.Execute("var renderer = new Shared.SharedRenderer(); renderer.initialize(cardJson, dataJson);");
            }
            catch (Exception ex)
            {
                var dontWait = _dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate
                {
                    try
                    {
                        _container.Child = new TextBlock()
                        {
                            Text = ex.ToString(),
                            TextWrapping = TextWrapping.Wrap
                        };
                    }
                    catch { }
                });
            }
        }

        private void OnChanges(string changesAsJson)
        {
            try
            {
                
            }
            catch
            {

            }
        }

        public void UpdateDataPayload(string text)
        {

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
