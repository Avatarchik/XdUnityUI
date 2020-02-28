using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace XdUnityUI.Editor
{
    /// <summary>
    /// SliderElement class.
    /// based on Baum2.Editor.SliderElement class.
    /// </summary>
    public sealed class SliderElement : GroupElement
    {
        public SliderElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
        }

        public override GameObject Render(Renderer renderer, GameObject parentObject)
        {
            var go = CreateSelf(renderer);
            var rect = go.GetComponent<RectTransform>();
            if (parentObject)
            {
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);
            }

            RectTransform fillRect = null;
            RectTransform handleRect = null;
            RenderChildren(renderer, go, (g, element) =>
            {
                var name = element.name.ToLower();

                if (fillRect == null && name == "fill" || name.EndsWith("_fill"))
                {
                    fillRect = g.GetComponent<RectTransform>();
                    g.GetComponent<Image>().raycastTarget = false;
                }

                if (handleRect == null && name == "handle" || name.EndsWith("_handle"))
                {
                    handleRect = g.GetComponent<RectTransform>();
                }
            });

            var slider = go.AddComponent<Slider>();
            slider.transition = Selectable.Transition.None;
            slider.interactable = false;
            if (handleRect != null)
            {
                slider.handleRect = handleRect;
                slider.interactable = true;
            }

            if (fillRect != null)
            {
                slider.fillRect = fillRect;
                // slider.fillRectに登録することでRectパラメータが変わる
                // スムーズにいくためのレスポンシブパラメータ､太さの指定をTop-Bottom固定でやる
                /*
                 * slider
                 *   handle
                 *   gauge_image @fix=left-top-bottom
                 *   fill @fix=left-top-bottom
                 */
            }

            // SetStretch(go, renderer);
            SetAnchor(go, renderer);
            return go;
        }
    }
}