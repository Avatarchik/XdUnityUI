using System.Collections.Generic;
using Baum2;
using UnityEngine;
using UnityEngine.UI;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// InputElement class.
    /// based on Baum2.Editor.InputElement class.
    /// </summary>
    public sealed class InputElement : Element
    {
        private string message;
        private string font;
        private float? fontSize;
        private string align;
        private float? virtualHeight;
        private Color fontColor;
        private Vector2? canvasPosition;
        private Vector2? sizeDelta;
        private bool enableStroke;
        private int? strokeSize;
        private Color strokeColor;
        private string type;

        public InputElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
            message = json.Get("text");
            font = json.Get("font");
            fontSize = json.GetFloat("size");
            align = json.Get("align");
            type = json.Get("textType");
            if (json.ContainsKey("strokeSize"))
            {
                enableStroke = true;
                strokeSize = json.GetInt("strokeSize");
                strokeColor = EditorUtil.HexToColor(json.Get("strokeColor"));
            }

            fontColor = EditorUtil.HexToColor(json.Get("color"));
            sizeDelta = json.GetVector2("w", "h");
            canvasPosition = json.GetVector2("x", "y");
            virtualHeight = json.GetFloat("vh");
        }

        public override GameObject Render(Renderer renderer, GameObject parentObject)
        {
            var go = CreateUIGameObject(renderer);
            var textObject = new GameObject("Text");
            var textObjectRect = textObject.AddComponent<RectTransform>();
            textObjectRect.SetParent(go.transform);

            var rect = go.GetComponent<RectTransform>();
            if (parentObject)
            {
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);
            }
            //rect.anchoredPosition = renderer.CalcPosition(canvasPosition, sizeDelta);
            //rect.sizeDelta = sizeDelta;

            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            textRect.offsetMin = new Vector2(0, 0);
            textRect.offsetMax = new Vector2(0, 0);

            var input = go.AddComponent<InputField>();
            input.transition = Selectable.Transition.None;
            input.text = message;

            var raw = textObject.AddComponent<RawData>();
            raw.Info["font_size"] = fontSize;
            raw.Info["align"] = align;

            var text = textObject.AddComponent<Text>();
            // text.text = message;
            text.supportRichText = false; // Using Rich Text with input in unsupported
            text.font = renderer.GetFont(font);
            if (fontSize != null) text.fontSize = Mathf.RoundToInt(fontSize.Value);
            text.color = fontColor;

            input.textComponent = text;

            bool middle = true;
            if (type == "point")
            {
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.verticalOverflow = VerticalWrapMode.Overflow;
                middle = true;
            }
            else if (type == "paragraph")
            {
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Overflow;
                middle = !message.Contains("\n");
            }
            else
            {
                Debug.LogError("unknown type " + type);
            }

            // var fixedPos = rect.anchoredPosition;
            switch (align)
            {
                case "left":
                    text.alignment = middle ? TextAnchor.MiddleLeft : TextAnchor.UpperLeft;
                    // rect.pivot = new Vector2(0.0f, 0.5f);
                    // fixedPos.x -= sizeDelta.x / 2.0f;
                    break;

                case "center":
                    text.alignment = middle ? TextAnchor.MiddleCenter : TextAnchor.UpperCenter;
                    // rect.pivot = new Vector2(0.5f, 0.5f);
                    break;

                case "right":
                    text.alignment = middle ? TextAnchor.MiddleRight : TextAnchor.UpperRight;
                    // rect.pivot = new Vector2(1.0f, 0.5f);
                    // fixedPos.x += sizeDelta.x / 2.0f;
                    break;
            }

            // rect.anchoredPosition = fixedPos;

            // var d = rect.sizeDelta;
            // d.y = virtualHeight;
            // rect.sizeDelta = d;

            if (enableStroke)
            {
                var outline = go.AddComponent<Outline>();
                outline.effectColor = strokeColor;
                if (strokeSize != null)
                    outline.effectDistance = new Vector2(strokeSize.Value / 2.0f, -strokeSize.Value / 2.0f);
                outline.useGraphicAlpha = false;
            }

            //SetStretch(go, renderer);
            SetAnchor(go, renderer);
            return go;
        }

        public override Area CalcArea()
        {
            if (canvasPosition != null && sizeDelta != null)
                return Area.FromPositionAndSize(canvasPosition.Value, sizeDelta.Value);
            return null;
        }
    }
}