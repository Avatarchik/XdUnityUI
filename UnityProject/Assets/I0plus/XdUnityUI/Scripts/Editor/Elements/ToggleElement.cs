using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// TextMeshProElement class.
    /// </summary>
    public sealed class ToggleElement : GroupElement
    {
        private Dictionary<string, object> _toggleJson;

        public ToggleElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
            _toggleJson = json.GetDic("toggle");
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

            var children = RenderChildren(renderer, go);

            var toggle = go.AddComponent<Toggle>();

            var targetImage = FindImageByClassName(children, _toggleJson.Get("target_graphic_class"));
            if (targetImage != null)
            {
                toggle.targetGraphic = targetImage;
            }

            var graphicImage = FindImageByClassName(children, _toggleJson.Get("graphic_class"));
            if (graphicImage != null)
            {
                toggle.graphic = graphicImage;
            }

            var spriteStateJson = _toggleJson.GetDic("sprite_state");
            if (spriteStateJson != null)
            {
                var spriteState = new SpriteState();
                var image = FindImageByClassName(children, spriteStateJson.Get("highlighted_sprite_class"));
                if (image != null)
                {
                    spriteState.highlightedSprite = image.sprite;
                    Object.DestroyImmediate(image.gameObject);
                }

                image = FindImageByClassName(children, spriteStateJson.Get("pressed_sprite_class"));
                if (image != null)
                {
                    spriteState.pressedSprite = image.sprite;
                    Object.DestroyImmediate(image.gameObject);
                }

                image = FindImageByClassName(children, spriteStateJson.Get("selected_sprite_class"));
                if (image != null)
                {
                    spriteState.selectedSprite = image.sprite;
                    Object.DestroyImmediate(image.gameObject);
                }

                image = FindImageByClassName(children, spriteStateJson.Get("disabled_sprite_class"));
                if (image != null)
                {
                    spriteState.disabledSprite = image.sprite;
                    Object.DestroyImmediate(image.gameObject);
                }

                toggle.spriteState = spriteState;
            }

            // トグルグループ名
            var group = _toggleJson.Get("group");
            if (group != null)
            {
                var toggleGroup = renderer.GetToggleGroup(group);
                //Debug.Log("toggleGroup:" + toggleGroup);
                toggle.group = toggleGroup;
            }

            SetupLayoutElement(go, LayoutElementParam);
            SetAnchor(go, renderer);

            return go;
        }
    }
}