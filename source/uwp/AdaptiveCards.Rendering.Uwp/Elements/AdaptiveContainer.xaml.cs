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
    internal sealed partial class AdaptiveContainer : BaseUserControl
    {
        public AdaptiveContainer()
        {
            this.InitializeComponent();
        }

        public ObservableCollection<BaseUserControl> Items { get; private set; } = new ObservableCollection<BaseUserControl>();

        public override void ApplyPropertyChange(string propertyName, JToken value)
        {
            base.ApplyPropertyChange(propertyName, value);

            switch (propertyName)
            {
                case "items":
                    HandleItemsSet(value.Value<JArray>("values"));
                    break;
            }
        }

        private void HandleItemsSet(JArray items)
        {
            Items.Clear();

            foreach (var item in items.OfType<JObject>())
            {
                var created = CreateItem(item);
                Items.Add(created);
            }
        }

        private BaseUserControl CreateItem(JObject item)
        {
            switch (item.Value<JObject>("props").Value<string>("type"))
            {
                case "TextBlock":
                    var tb = new AdaptiveTextBlock();
                    tb.Initialize(item, Renderer);
                    return tb;

                case "Input.Text":
                    var textInput = new AdaptiveTextInput();
                    textInput.Initialize(item, Renderer);
                    return textInput;

                case "Input.ChoiceSet":
                    var choiceSetInput = new AdaptiveChoiceSetInput();
                    choiceSetInput.Initialize(item, Renderer);
                    return choiceSetInput;

                case "Progress":
                    var progress = new AdaptiveProgress();
                    progress.Initialize(item, Renderer);
                    return progress;

                case "Input.Rating":
                    var rating = new AdaptiveRatingInput();
                    rating.Initialize(item, Renderer);
                    return rating;

                case "Container":
                    var container = new AdaptiveContainer();
                    container.Initialize(item, Renderer);
                    return container;

                default:
                    return new AdaptiveUnknown();
            }
        }

        public override void ApplyArrayChanges(string propertyName, JArray changes)
        {
            switch (propertyName)
            {
                case "items":
                    foreach (var change in changes.OfType<JObject>())
                    {
                        if (change.Value<string>("type") == "Add")
                        {
                            var created = CreateItem(change.Value<JObject>("item"));
                            Items.Insert(change.Value<int>("index"), created);
                        }
                        else
                        {
                            Items.RemoveAt(change.Value<int>("index"));
                        }
                    }
                    break;
            }
        }

        public override IEnumerable<BaseUserControl> GetChildren()
        {
            return Items;
        }
    }
}
