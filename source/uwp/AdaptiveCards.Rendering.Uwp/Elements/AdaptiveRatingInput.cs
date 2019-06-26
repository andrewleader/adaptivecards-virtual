using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Windows.UI.Xaml.Controls;

namespace AdaptiveCards.Rendering.Uwp.Elements
{
    internal class AdaptiveRatingInput : BaseUserControl
    {
        private RatingControl _ratingControl = new RatingControl();

        public AdaptiveRatingInput()
        {
            Content = _ratingControl;

            _ratingControl.ValueChanged += _ratingControl_ValueChanged;
        }

        private void _ratingControl_ValueChanged(RatingControl sender, object args)
        {
            Renderer.UpdateInputValueProperty(ElementId, _ratingControl.Value);
        }

        public override void ApplyPropertyChange(string propertyName, JToken value)
        {
            base.ApplyPropertyChange(propertyName, value);
        }
    }
}
