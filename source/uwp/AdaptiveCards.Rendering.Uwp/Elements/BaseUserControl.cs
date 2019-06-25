using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace AdaptiveCards.Rendering.Uwp.Elements
{
    abstract class BaseUserControl : UserControl
    {
        public string Id { get; set; }
        public string ElementId { get; set; }
        public AdaptiveCardRenderer Renderer { get; set; }

        public virtual IEnumerable<BaseUserControl> GetChildren()
        {
            return new BaseUserControl[0];
        }

        public IEnumerable<BaseUserControl> GetDescendants()
        {
            foreach (var child in GetChildren())
            {
                yield return child;

                foreach (var descendant in child.GetDescendants())
                {
                    yield return descendant;
                }
            }
        }

        public IEnumerable<BaseUserControl> GetDescendantsAndSelf()
        {
            yield return this;

            foreach (var descendant in GetDescendants())
            {
                yield return descendant;
            }
        }

        public BaseUserControl GetById(string id)
        {
            return GetDescendantsAndSelf().FirstOrDefault(i => i.Id == id);
        }

        public virtual void ApplyPropertyChange(string propertyName, JToken value)
        {
            switch (propertyName)
            {
                case "id":
                    ElementId = value.Value<string>();
                    break;
            }
        }

        public void Initialize(JObject item, AdaptiveCardRenderer renderer)
        {
            Renderer = renderer;
            Id = item.Value<string>("id");
            foreach (var prop in item.Value<JObject>("props"))
            {
                ApplyPropertyChange(prop.Key, prop.Value);
            }
        }
    }
}
