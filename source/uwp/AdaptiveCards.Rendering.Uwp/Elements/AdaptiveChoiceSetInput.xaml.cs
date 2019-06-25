using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ObservableCollection<AdaptiveChoice> Choices { get; private set; } = new ObservableCollection<AdaptiveChoice>();

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

        public override void ApplyArrayChanges(string propertyName, JArray changes)
        {
            switch (propertyName)
            {
                case "choices":
                    foreach (var change in changes.OfType<JObject>())
                    {
                        if (change.Value<string>("type") == "Add")
                        {
                            var choice = new AdaptiveChoice();
                            choice.Initialize(change.Value<JObject>("item"), Renderer);
                            Choices.Insert(change.Value<int>("index"), choice);
                        }
                        else
                        {
                            Choices.RemoveAt(change.Value<int>("index"));
                        }
                    }
                    break;
            }
        }

        private void HandleChoicesSet(JArray choices)
        {
            Choices.Clear();

            foreach (var item in choices.OfType<JObject>())
            {
                var choice = new AdaptiveChoice();
                choice.Initialize(item, Renderer);
                Choices.Add(choice);
            }

            if (ComboBox.SelectedIndex != -1)
            {
                UpdateInputValue();
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateInputValue();
        }

        private void UpdateInputValue()
        {
            var selectedChoice = ComboBox.SelectedItem as AdaptiveChoice;
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
