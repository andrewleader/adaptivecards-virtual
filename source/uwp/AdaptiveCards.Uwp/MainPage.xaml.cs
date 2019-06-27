using AdaptiveCards.Rendering.Uwp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AdaptiveCards.Uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private AdaptiveCardRenderer _renderer;

        public MainPage()
        {
            this.InitializeComponent();

            Initialize();
        }

        public class Sample
        {
            public string Title { get; set; }

            public string Card { get; set; } = "{\n}";

            public string Data { get; set; } = "{\n}";

            public string CardScript { get; set; } = "";
        }

        private async void Initialize()
        {
            try
            {
                IsEnabled = false;

                var samples = await Package.Current.InstalledLocation.GetFolderAsync("Samples");
                var samplesCollection = new List<Sample>();

                foreach (var sample in await samples.GetFilesAsync())
                {
                    try
                    {
                        if (sample.FileType == ".json")
                        {
                            var sampleObj = new Sample()
                            {
                                Title = sample.Name
                            };

                            var json = await FileIO.ReadTextAsync(sample);
                            JObject parsed = JObject.Parse(json);

                            if (parsed.ContainsKey("$sampleData"))
                            {
                                sampleObj.Data = parsed.Value<JToken>("$sampleData").ToString();
                                parsed.Remove("$sampleData");
                            }

                            sampleObj.Card = parsed.ToString();

                            try
                            {
                                var cardScriptFile = await samples.GetFileAsync(sample.Name.Substring(0, sample.Name.Length - sample.FileType.Length) + ".js");
                                sampleObj.CardScript = await FileIO.ReadTextAsync(cardScriptFile);
                            }
                            catch { }

                            samplesCollection.Add(sampleObj);
                        }
                    }
                    catch { }
                }

                ListViewSamples.ItemsSource = samplesCollection;

                await ShowDebugWindowAsync();

                IsEnabled = true;
            }
            catch { }
        }

        private CoreApplicationView _debugView;
        private int _debugViewId;
        private async Task ShowDebugWindowAsync(bool bringToForeground = false)
        {
            _debugView = CoreApplication.CreateNewView();
            await _debugView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, delegate
            {
                Frame frame = new Frame();
                frame.Navigate(typeof(DebuggingPage), null);
                Window.Current.Content = frame;
                Window.Current.Activate();

                _debugViewId = ApplicationView.GetForCurrentView().Id;
            });

            await ApplicationViewSwitcher.TryShowAsStandaloneAsync(_debugViewId);
        }

        private void TextBoxCardPayload_TextChanged(object sender, TextChangedEventArgs e)
        {
            RequestRender();
        }

        private void TextBoxDataPayload_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_renderer != null)
            {
                try
                {
                    _renderer.UpdateData(TextBoxDataPayload.Text);
                }
                catch { }
            }
        }

        private bool _renderBlocked;
        private bool _needsRender;
        private async void RequestRender()
        {
            Render();
            //if (_renderBlocked)
            //{
            //    _needsRender = true;
            //    return;
            //}

            //_renderBlocked = true;
            //Render();
            //await Task.Delay(500);
            //_renderBlocked = false;
            //if (_needsRender)
            //{
            //    _needsRender = false;
            //    RequestRender();
            //}
        }

        private bool _shouldResetData;
        private void Render()
        {
            try
            {
                var cardPayload = TextBoxCardPayload.Text;

                DebuggingPage.RunInWindowThread((page) =>
                {
                    page.ResetForNewTemplate(cardPayload);
                });

                if (_renderer == null || _shouldResetData)
                {
                    _renderer = new AdaptiveCardRenderer();
                    _renderer.OnCardChanges += _renderer_OnCardChanges;
                    _renderer.OnTransformedTemplateChanged += _renderer_OnTransformedTemplateChanged;
                    _renderer.OnDataChanged += _renderer_OnDataChanged;
                    _renderer.OnVirtualCardChanged += _renderer_OnVirtualCardChanged;
                    CardContainer.Child = _renderer.Render(cardPayload, TextBoxDataPayload.Text, CardScript.Text);
                }
                else
                {
                    _renderer.UpdateTemplate(cardPayload);
                }

                _shouldResetData = false;
            }
            catch (Exception ex)
            {
                CardContainer.Child = new TextBlock()
                {
                    Text = ex.ToString(),
                    TextWrapping = TextWrapping.Wrap
                };
            }
        }

        private void _renderer_OnVirtualCardChanged(object sender, string newVirtualCard)
        {
            DebuggingPage.RunInWindowThread((page) =>
            {
                page.VirtualCardChanged(newVirtualCard);
            });
        }

        private void _renderer_OnDataChanged(object sender, string data)
        {
            DebuggingPage.RunInWindowThread((page) =>
            {
                page.Data = data;
            });
        }

        private void _renderer_OnTransformedTemplateChanged(object sender, string transformedTemplate)
        {
            DebuggingPage.RunInWindowThread((page) =>
            {
                page.TransformedTemplate = transformedTemplate;
            });
        }

        private void _renderer_OnCardChanges(object sender, string changes)
        {
            DebuggingPage.RunInWindowThread((page) =>
            {
                page.LatestChange = changes;
            });
        }

        private void ListViewSamples_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedSample = ListViewSamples.SelectedItem as Sample;
            if (selectedSample != null)
            {
                _shouldResetData = true;
                CardScript.Text = selectedSample.CardScript;
                if (CardScript.Text.Length > 0)
                {
                    CardScriptRow.Height = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    CardScriptRow.Height = new GridLength(0);
                }
                TextBoxDataPayload.Text = selectedSample.Data;
                TextBoxCardPayload.Text = selectedSample.Card;
            }
        }
    }
}
