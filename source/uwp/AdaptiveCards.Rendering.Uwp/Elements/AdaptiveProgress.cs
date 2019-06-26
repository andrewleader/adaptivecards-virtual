using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Controls;

namespace AdaptiveCards.Rendering.Uwp.Elements
{
    internal class AdaptiveProgress : BaseUserControl
    {
        private ProgressBar _progressBar;

        public AdaptiveProgress()
        {
            _progressBar = new ProgressBar();
            Content = _progressBar;
        }

        public override void ApplyPropertyChange(string propertyName, JToken value)
        {
            switch (propertyName)
            {
                case "value":
                    _progressBar.Value = value.Value<double>();
                    break;
            }
        }
    }
}
