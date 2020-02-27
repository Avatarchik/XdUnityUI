using System.Collections.Generic;
using Baum2;
using UnityEngine;
using UnityEngine.UI;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// RootElement class.
    /// based on Baum2.Editor.ScrollbarElement class.
    /// </summary>
    public sealed class ScrollbarElement : GroupElement
    {
        private Dictionary<string, object> _scrollbar;

        public ScrollbarElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
            _scrollbar = json.GetDic("scrollbar");
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

            SetAnchor(go, renderer);

            var children = RenderChildren(renderer, go);
            SetupChildImageComponent(go, children);

            // DotsScrollberかどうかの判定に、Toggleがあるかどうかを確認する
            var toggleChild = children.Find(child => child.Item2 is ToggleElement);
            Scrollbar scrollbar;
            if (toggleChild == null)
            {
                scrollbar = go.AddComponent<Scrollbar>();
            }
            else
            {
                // DotScrollbarとなる
                var dotScrollbar = go.AddComponent<DotScrollbar>();
                dotScrollbar.isAutoLayoutEnableOnEditMode = false;
                dotScrollbar.DotContainer = rect;
                dotScrollbar.DotPrefab = toggleChild.Item1.GetComponent<Toggle>();
                // Toggleボタンの並びレイアウト
                SetupLayoutGroup(go, LayoutParam);
                dotScrollbar.size = 1; // sizeを1にすることで、Toggleが複数Cloneされることをふせぐ
                scrollbar = dotScrollbar;
            }

            var direction = _scrollbar.Get("direction");
            if (direction != null)
            {
                switch (direction)
                {
                    case "left-to-right":
                    case "ltr":
                    case "x":
                        scrollbar.direction = Scrollbar.Direction.LeftToRight;
                        break;
                    case "right-to-left":
                    case "rtl":
                        scrollbar.direction = Scrollbar.Direction.RightToLeft;
                        break;
                    case "bottom-to-top":
                    case "btt":
                    case "y":
                        scrollbar.direction = Scrollbar.Direction.BottomToTop;
                        break;
                    case "top-to-bottom":
                    case "ttb":
                        scrollbar.direction = Scrollbar.Direction.TopToBottom;
                        break;
                }
            }

            var handleClassName = _scrollbar.Get("handle_class");
            if (handleClassName != null)
            {
                var found = children.Find(child => child.Item2.HasClassName(handleClassName));
                if (found != null)
                {
                    scrollbar.handleRect = found.Item1.GetComponent<RectTransform>();
                }
            }

            SetupContentSizeFitter(go, ContentSizeFitterParam);
            return go;
        }

    }
}