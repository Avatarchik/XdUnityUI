using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// ElementFactory class.
    /// based on Baum2.Editor.ElementFactory class.
    /// </summary>
    public static class ElementFactory
    {
        public static readonly Dictionary<string, Func<Dictionary<string, object>, Element, Element>> Generator =
            new Dictionary<string, Func<Dictionary<string, object>, Element, Element>>()
            {
                {"Root", (d, p) => new RootElement(d, p)},
                {"Image", (d, p) => new ImageElement(d, p)},
                {"Mask", (d, p) => new MaskElement(d, p)},
                {"Group", (d, p) => new GroupElement(d, p)},
                {"Text", (d, p) => new TextElement(d, p)},
                {"Button", (d, p) => new ButtonElement(d, p)},
                {"Slider", (d, p) => new SliderElement(d, p)},
                {"Scrollbar", (d, p) => new ScrollbarElement(d, p)},
                {"Toggle", (d, p) => new ToggleElement(d, p)},
#if ENHANCEDSCROLLER_PRESENT
                {"List", (d, p) => new EnhancedScrollerElement(d, p)},
#endif
                {"Input", (d, p) => new InputElement(d, p)},
#if TMP_PRESENT
                {"TextMeshPro", (d, p) => new TextMeshProElement(d, p)},
#endif
                {"Viewport", (d, p) => new ViewportElement(d, p)},
                {"Rect", (d, p) => new RectElement(d, p)}
            };

        public static Element Generate(Dictionary<string, object> json, Element parent)
        {
            var type = json.Get("type");
            Assert.IsTrue(Generator.ContainsKey(type), "[XdUnityUI] Unknown type: " + type);
            return Generator[type](json, parent);
        }
    }
}