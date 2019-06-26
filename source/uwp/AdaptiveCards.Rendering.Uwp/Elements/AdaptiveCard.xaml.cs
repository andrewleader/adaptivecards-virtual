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
    internal sealed partial class AdaptiveCard : BaseUserControl
    {
        public AdaptiveCard()
        {
            this.InitializeComponent();
        }

        public override void ApplyPropertyChange(string propertyName, JToken value)
        {
            if (BodyContainer.Renderer == null)
            {
                BodyContainer.Renderer = Renderer;
            }

            base.ApplyPropertyChange(propertyName, value);

            switch (propertyName)
            {
                case "body":
                    BodyContainer.ApplyPropertyChange("items", value);
                    break;
            }
        }

        public override IEnumerable<BaseUserControl> GetChildren()
        {
            return BodyContainer.GetChildren();
        }

        public override void ApplyArrayChanges(string propertyName, JArray changes)
        {
            switch (propertyName)
            {
                case "body":
                    BodyContainer.ApplyArrayChanges("items", changes);
                    break;
            }
        }
    }
}
