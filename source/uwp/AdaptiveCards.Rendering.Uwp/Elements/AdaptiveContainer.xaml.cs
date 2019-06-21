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
                switch (item.Value<JObject>("props").Value<string>("type"))
                {
                    case "TextBlock":
                        var tb = new AdaptiveTextBlock();
                        tb.Initialize(item, Renderer);
                        Items.Add(tb);
                        break;

                    case "Input.Text":
                        var textInput = new AdaptiveTextInput();
                        textInput.Initialize(item, Renderer);
                        Items.Add(textInput);
                        break;
                }
            }
        }

        public override IEnumerable<BaseUserControl> GetChildren()
        {
            return Items;
        }
    }
}
