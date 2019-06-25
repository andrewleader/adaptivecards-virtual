using System;
using System.Collections.Generic;
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
    internal sealed partial class AdaptiveChoiceSetInput : BaseUserControl
    {
        public class Choice
        {
            public string Title { get; set; }
            public string Value { get; set; }
        }

        public AdaptiveChoiceSetInput()
        {
            this.InitializeComponent();
        }

        public override void ApplyPropertyChange(string propertyName, JToken value)
        {
            base.ApplyPropertyChange(propertyName, value);

            switch (propertyName)
            {
                case "choices":
                    HandleChoicesSet(value.Value<JArray>("values"));
                    break;
            }
        }

        private void HandleChoicesSet(JArray choices)
        {
            List<Choice> options = new List<Choice>();
            foreach (var item in choices.OfType<JObject>().Select(c => c.Value<JObject>("props")))
            {
                options.Add(new Choice()
                {
                    Title = item.Value<string>("title"),
                    Value = item.Value<string>("value")
                });
            }
            ComboBox.ItemsSource = options;

            UpdateInputValue();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateInputValue();
        }

        private void UpdateInputValue()
        {
            var selectedChoice = ComboBox.SelectedItem as Choice;
            if (selectedChoice != null)
            {
                Renderer.UpdateInputValue(ElementId, selectedChoice.Value);
            }
            else
            {
                Renderer.UpdateInputValue(ElementId, "");
            }
        }
    }
}
