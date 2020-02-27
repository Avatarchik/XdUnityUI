using System.Collections.Generic;
using UnityEngine;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// RectElement class.
    /// </summary>
    public sealed class RectElement : Element
    {
        private Vector2? canvasPosition;
        private Vector2? sizeDelta;

        public RectElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
            canvasPosition = json.GetVector2("x", "y");
            sizeDelta = json.GetVector2("w", "h");
        }

        public override GameObject Render(Renderer renderer, GameObject parentObject)
        {
            var go = CreateUIGameObject(renderer);
            var rect = go.GetComponent<RectTransform>();
            if (parentObject)
            {
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);
            }
            SetAnchor(go, renderer);
            return go;
        }

        public override Area CalcArea()
        {
            return Area.FromPositionAndSize(canvasPosition.Value, sizeDelta.Value);
        }
    }
}