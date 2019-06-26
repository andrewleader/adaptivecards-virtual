using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace AdaptiveCards.Rendering.Uwp.Elements
{
    internal class AdaptiveUnknown : BaseUserControl
    {
        private TextBlock _tb = new TextBlock()
        {
            Text = "Unknown element"
        };

        public AdaptiveUnknown()
        {
            Content = _tb;
        }
    }
}
