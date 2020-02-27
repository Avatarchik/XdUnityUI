using System;
using System.Collections.Generic;
using System.Linq;
#if ENHANCEDSCROLLER_PRESENT
using EnhancedUI.EnhancedScroller;
#endif
using UnityEngine;
using UnityEngine.UI;


namespace Baum2.Editor
{
    // deprecated.
#if ENHANCEDSCROLLER_PRESENT
    public sealed class EnhancedScrollerElement : GroupElement
    {
        private string scroll;

        private Vector2 canvasPosition;
        private Vector2 sizeDelta;

        public EnhancedScrollerElement(Dictionary<string, object> json, Element parent) : base(json, parent, true)
        {
            canvasPosition = json.GetVector2("x", "y");
            sizeDelta = json.GetVector2("w", "h");
            if (json.ContainsKey("scroll")) scroll = json.Get("scroll");
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

            //　Contentの作成
            var content = new GameObject("Content");
            var contentRect = content.AddComponent<RectTransform>();
            content.transform.SetParent(go.transform);

            // 縦スクロールの場合､Contentは　縦は親と一緒､横ストレッチ可にする
            // 横ストレッチなので､横幅は0にして､親サイズにピッタリあわせる
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.offsetMin = new Vector2(0, 0);
            contentRect.offsetMax = new Vector2(0, 0);
            contentRect.pivot = new Vector2(0, 1);

            SetupScroll(go, content);
            SetMaskImage(renderer, go, content);

            var items = CreateItems(renderer, go); // ScrollerのitemはContentの中にいれない
            SetupList(go, items, content);

            //SetStretch(go, renderer);
            SetPivot(go, renderer);
            return go;
        }

        private void SetupScroll(GameObject go, GameObject content)
        {
            var scroller = go.AddComponent<EnhancedScroller>();
            // 空白部のタッチイベントを取得するようのImage　Imageが一番適しているかは要検討
            var imageComponent = go.AddComponent<Image>();
            imageComponent.color = new Color(0, 0, 0, 0);
            var scrollRect = go.GetComponent<ScrollRect>();
            scrollRect.content = content.GetComponent<RectTransform>();
            if (scroll == "vertical")
            {
                scroller.scrollDirection = EnhancedScroller.ScrollDirectionEnum.Vertical;
            }
            else if (scroll == "horizontal")
            {
                scroller.scrollDirection = EnhancedScroller.ScrollDirectionEnum.Horizontal;
            }

            if (layout != null)
            {
                var spacing = layout.GetFloat("spacing");
                var padding = layout.GetDic("padding");
                var paddingLeft = padding.GetInt("left");
                var paddingRight = padding.GetInt("right");
                var paddingTop = padding.GetInt("top");
                var paddingBottom = padding.GetInt("bottom");
                scroller.spacing = spacing;
                scroller.padding = new RectOffset(paddingLeft, paddingRight, paddingTop, paddingBottom);
            }
        }

        private void SetMaskImage(Renderer renderer, GameObject go, GameObject content)
        {
            var dummyMaskImage = CreateDummyMaskImage(renderer);
            if (dummyMaskImage)
            {
                var maskImage = go.AddComponent<Image>();
                dummyMaskImage.transform.SetParent(go.transform);
                go.GetComponent<RectTransform>().CopyTo(content.GetComponent<RectTransform>());
                content.GetComponent<RectTransform>().localPosition = Vector3.zero;
                dummyMaskImage.GetComponent<Image>().CopyTo(maskImage);
                GameObject.DestroyImmediate(dummyMaskImage);

                maskImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                go.AddComponent<Mask>();
            }
            else
            {
                go.AddComponent<RectMask2D>();
            }
        }

        private GameObject CreateDummyMaskImage(Renderer renderer)
        {
            var maskElement = elements.Find(x => (x is ImageElement && x.name == "Area"));
            if (maskElement == null)
            {
                return null;
            }

            elements.Remove(maskElement);

            var maskImage = maskElement.Render(renderer, null);
            maskImage.SetActive(false);
            return maskImage;
        }

        private List<GameObject> CreateItems(Renderer renderer, GameObject go)
        {
            var items = new List<GameObject>();
            foreach (var element in elements)
            {
                var item = element as GroupElement;
                if (item == null)
                    throw new Exception(string.Format("{0}'s element {1} is not group", name, element.name));

                var itemObject = item.Render(renderer, go);

                items.Add(itemObject);
            }

            return items;
        }

        private void SetupList(GameObject go, List<GameObject> itemSources, GameObject content)
        {
            // var list = go.AddComponent<List>();
            // list.ItemSources = itemSources;
            // list.LayoutGroup = content.GetComponent<ListLayoutGroup>();
        }

        public override Area CalcArea()
        {
            return Area.FromPositionAndSize(canvasPosition, sizeDelta);
        }
    }
#endif
}