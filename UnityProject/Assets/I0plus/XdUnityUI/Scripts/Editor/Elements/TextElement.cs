using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Baum2;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// TextElement class.
    /// based on Baum2.Editor.TextElement class.
    /// </summary>
    public sealed class TextElement : Element
    {
        private string message;
        private string fontName;
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
        private string style;

        public TextElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
            message = json.Get("text");
            fontName = json.Get("font");
            fontSize = json.GetFloat("size");
            align = json.Get("align");
            type = json.Get("textType");
            if (json.ContainsKey("style"))
            {
                style = json.Get("style");
                style.ToLower();
            }

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

            var rect = go.GetComponent<RectTransform>();
            if (parentObject)
            {
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);
            }
            // rect.anchoredPosition = renderer.CalcPosition(canvasPosition, sizeDelta);
            // rect.sizeDelta = sizeDelta;

            var raw = go.AddComponent<RawData>();
            raw.Info["font_size"] = fontSize;
            raw.Info["align"] = align;

            var text = go.AddComponent<Text>();
            text.text = message;

            // 検索するフォント名を決定する
            var fontFilename = fontName;
            if (style != null)
            {
                fontFilename += "-" + style;
            }

            text.font = renderer.GetFont(fontFilename);
            text.fontSize = Mathf.RoundToInt(fontSize.Value);
            text.color = fontColor;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            if (style != null)
            {
                if (style.Contains("normal") || style.Contains("medium"))
                {
                    text.fontStyle = FontStyle.Normal;
                }

                if (style.Contains("bold"))
                {
                    text.fontStyle = FontStyle.Bold;
                }
            }

            if (type == "point")
            {
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.verticalOverflow = VerticalWrapMode.Overflow;
            }
            else if (type == "paragraph")
            {
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
                text.verticalOverflow = VerticalWrapMode.Overflow;
            }
            else
            {
                Debug.LogError("unknown type " + type);
            }

            var vertical = "";
            var horizontal = "";
            var alignLowerString = align.ToLower();
            if (alignLowerString.Contains("left"))
            {
                horizontal = "left";
            }
            else if (alignLowerString.Contains("center"))
            {
                horizontal = "center";
            }
            else if (alignLowerString.Contains("right"))
            {
                horizontal = "right";
            }

            if (alignLowerString.Contains("upper"))
            {
                vertical = "upper";
            }
            else if (alignLowerString.Contains("middle"))
            {
                vertical = "middle";
            }
            else if (alignLowerString.Contains("lower"))
            {
                vertical = "lower";
            }

            switch ((vertical + "-" + horizontal).ToLower())
            {
                case "upper-left":
                    text.alignment = TextAnchor.UpperLeft;
                    break;
                case "upper-center":
                    text.alignment = TextAnchor.UpperCenter;
                    break;
                case "upper-right":
                    text.alignment = TextAnchor.UpperRight;
                    break;
                case "middle-left":
                    text.alignment = TextAnchor.MiddleLeft;
                    break;
                case "middle-center":
                    text.alignment = TextAnchor.MiddleCenter;
                    break;
                case "middle-right":
                    text.alignment = TextAnchor.MiddleRight;
                    break;
                case "lower-left":
                    text.alignment = TextAnchor.LowerLeft;
                    break;
                case "lower-center":
                    text.alignment = TextAnchor.LowerCenter;
                    break;
                case "lower-right":
                    text.alignment = TextAnchor.LowerRight;
                    break;
            }

            if (enableStroke)
            {
                var outline = go.AddComponent<Outline>();
                outline.effectColor = strokeColor;
                outline.effectDistance = new Vector2(strokeSize.Value / 2.0f, -strokeSize.Value / 2.0f);
                outline.useGraphicAlpha = false;
            }

            //SetStretch(go, renderer);
            SetAnchor(go, renderer);
            return go;
        }

        public override Area CalcArea()
        {
            return Area.FromPositionAndSize(canvasPosition.Value, sizeDelta.Value);
        }
    }
}