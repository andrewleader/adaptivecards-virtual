using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace AdaptiveCards.Rendering.Uwp.Elements
{
    internal class AdaptiveChoice : BaseUserControl, INotifyPropertyChanged
    {
        public string Title { get; set; }
        public string Value { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public AdaptiveChoice()
        {
            Content = new TextBlock();
            (Content as TextBlock).SetBinding(TextBlock.TextProperty, new Binding()
            {
                Source = this,
                Path = new Windows.UI.Xaml.PropertyPath(nameof(Title))
            });
        }

        public override void ApplyPropertyChange(string propertyName, JToken value)
        {
            base.ApplyPropertyChange(propertyName, value);

            switch (propertyName)
            {
                case "title":
                    Title = value.Value<string>();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
                    break;

                case "value":
                    Value = value.Value<string>();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
                    break;
            }
        }
    }
}
