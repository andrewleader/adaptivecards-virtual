using AdaptiveCards.Rendering.Uwp;
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

        private async void Initialize()
        {
            try
            {
                IsEnabled = false;

                var samples = await Package.Current.InstalledLocation.GetFolderAsync("Samples");
                var basicCard = await samples.GetFileAsync("Basic.json");
                var basicCardData = await samples.GetFileAsync("Basic.data.json");

                TextBoxDataPayload.Text = await FileIO.ReadTextAsync(basicCardData);
                TextBoxCardPayload.Text = await FileIO.ReadTextAsync(basicCard);

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
    }
}
