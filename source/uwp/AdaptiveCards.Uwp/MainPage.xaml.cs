using AdaptiveCards.Rendering.Uwp;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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

                        samplesCollection.Add(sampleObj);
                    }
                    catch { }
                }

                ListViewSamples.ItemsSource = samplesCollection;

                IsEnabled = true;
            }
            catch { }
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
                    _renderer.UpdateDataPayload(TextBoxDataPayload.Text);
                }
                catch { }
            }
        }

        private bool _renderBlocked;
        private bool _needsRender;
        private async void RequestRender()
        {
            if (_renderBlocked)
            {
                _needsRender = true;
                return;
            }

            _renderBlocked = true;
            Render();
            await Task.Delay(500);
            _renderBlocked = false;
            if (_needsRender)
            {
                _needsRender = false;
                RequestRender();
            }
        }

        private void Render()
        {
            try
            {
                _renderer = new AdaptiveCardRenderer();
                CardContainer.Child = _renderer.Render(TextBoxCardPayload.Text, TextBoxDataPayload.Text);
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

        private void ListViewSamples_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedSample = ListViewSamples.SelectedItem as Sample;
            if (selectedSample != null)
            {
                TextBoxDataPayload.Text = selectedSample.Data;
                TextBoxCardPayload.Text = selectedSample.Card;
            }
        }
    }
}
