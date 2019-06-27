using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Controls;

namespace AdaptiveCards.Rendering.Uwp.Elements
{
    internal class AdaptiveAction : BaseUserControl
    {
        private Button _button = new Button();
        public AdaptiveAction()
        {
            Content = _button;
            _button.Click += _button_Click;
        }

        public override void ApplyPropertyChange(string propertyName, JToken value)
        {
            switch (propertyName)
            {
                case "title":
                    _button.Content = value.Value<string>();
                    break;
            }

            base.ApplyPropertyChange(propertyName, value);
        }

        private void _button_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Renderer.ExecuteAction(base.Id);
        }
    }
}
