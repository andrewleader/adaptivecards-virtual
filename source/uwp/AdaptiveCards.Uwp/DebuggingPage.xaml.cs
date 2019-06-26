using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AdaptiveCards.Uwp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DebuggingPage : Page, INotifyPropertyChanged
    {
        public static DebuggingPage Current { get; private set; }
        private CoreDispatcher _dispatcher;

        public static async void RunInWindowThread(Action<DebuggingPage> action)
        {
            await RunInWindowThreadAsync(action);
        }

        public static async Task RunInWindowThreadAsync(Action<DebuggingPage> action)
        {
            var curr = Current;
            if (curr != null)
            {
                await curr._dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
                {
                    try
                    {
                        action(curr);
                    }
                    catch { }
                });
            }
        }

        public DebuggingPage()
        {
            this.InitializeComponent();

            _dispatcher = CoreApplication.GetCurrentView().Dispatcher;
            Current = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private string _cardTemplate = "";
        public string CardTemplate
        {
            get => _cardTemplate;
            set => SetProperty(ref _cardTemplate, value);
        }

        private string _data = "";
        public string Data
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }

        private string _transformedTemplate = "";
        public string TransformedTemplate
        {
            get => _transformedTemplate;
            set => SetProperty(ref _transformedTemplate, value);
        }

        private string _previousVirtualCard = "";
        public string PreviousVirtualCard
        {
            get => _previousVirtualCard;
            set => SetProperty(ref _previousVirtualCard, value);
        }

        private string _currentVirtualCard = "";
        public string CurrentVirtualCard
        {
            get => _currentVirtualCard;
            set => SetProperty(ref _currentVirtualCard, value);
        }

        private string _latestChange = "";
        public string LatestChange
        {
            get => _latestChange;
            set => SetProperty(ref _latestChange, value);
        }

        private void SetProperty<T>(ref T storage, T value, [CallerMemberName]string propertyName = "")
        {
            if (value is string)
            {
                // Indent the JSON
                try
                {
                    object indented = JToken.Parse(value as string).ToString();
                    value = (T)indented;
                }
                catch { }
            }

            if (object.Equals(storage, value))
            {
                return;
            }

            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ResetForNewTemplate(string template)
        {
            CardTemplate = template;
            LatestChange = "";
            PreviousVirtualCard = "";
            TransformedTemplate = "";
            Data = "";
        }

        public void VirtualCardChanged(string newVirtualCard)
        {
            PreviousVirtualCard = CurrentVirtualCard;
            CurrentVirtualCard = newVirtualCard;
        }
    }
}
