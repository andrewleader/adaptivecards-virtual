using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Newtonsoft.Json.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace AdaptiveCards.Rendering.Uwp.Elements
{
    internal sealed partial class AdaptiveTextInput : BaseUserControl
    {
        public AdaptiveTextInput()
        {
            this.InitializeComponent();
        }

        public override void ApplyPropertyChange(string propertyName, JToken value)
        {
            base.ApplyPropertyChange(propertyName, value);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            Renderer.UpdateInputValueProperty(ElementId, TextBox.Text);
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            dynamic values = new ExpandoObject();
            values.focused = true;
            Renderer.UpdateInput(ElementId, values);
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            dynamic values = new ExpandoObject();
            values.focused = false;
            Renderer.UpdateInput(ElementId, values);
        }
    }
}
