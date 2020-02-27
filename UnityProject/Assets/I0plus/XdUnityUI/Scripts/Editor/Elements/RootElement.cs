using System.Collections.Generic;
using UnityEngine;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// RootElement class.
    /// based on Baum2.Editor.RootElement class.
    /// </summary>
    public class RootElement : GroupElement
    {
        private Vector2 sizeDelta = default;

        public RootElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
        }

        protected override GameObject CreateSelf(Renderer renderer)
        {
            var go = CreateUIGameObject(renderer);

            var rect = go.GetComponent<RectTransform>();
            SetAnchor(go, renderer);
            SetLayer(go, layer);
            SetMaskImage(renderer, go);
            return go;
        }

        public override Area CalcArea()
        {
            return new Area(-sizeDelta / 2.0f, sizeDelta / 2.0f);
        }
    }
}