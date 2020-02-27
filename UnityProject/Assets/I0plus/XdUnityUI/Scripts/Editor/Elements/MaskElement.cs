using System.Collections.Generic;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// MaskElement class.
    /// based on Baum2.Editor.MaskElement class.
    /// </summary>
    public sealed class MaskElement : ImageElement
    {
        public MaskElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
        }
    }
}