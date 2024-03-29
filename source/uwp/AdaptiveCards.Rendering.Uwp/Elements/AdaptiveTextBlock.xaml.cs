﻿using System;
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
    internal sealed partial class AdaptiveTextBlock : BaseUserControl
    {
        public AdaptiveTextBlock()
        {
            this.InitializeComponent();
        }

        public override void ApplyPropertyChange(string propertyName, JToken value)
        {
            base.ApplyPropertyChange(propertyName, value);

            switch (propertyName)
            {
                case "text":
                    TextBlock.Text = value.Value<string>();
                    break;
            }
        }
    }
}
