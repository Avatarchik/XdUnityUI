using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif
using Baum2;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// TextMeshProElement class.
    /// </summary>
#if TMP_PRESENT
    public sealed class TextMeshProElement : Element
    {
        private string message;
        private string fontName;
        private string fontStyle;
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

        public TextMeshProElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
            message = json.Get("text");
            fontName = json.Get("font");
            fontStyle = json.Get("style");
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

            var rect = go.GetComponent<RectTransform>();
            if (parentObject)
            {
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);
            }
            //rect.anchoredPosition = renderer.CalcPosition(canvasPosition, sizeDelta);
            //rect.sizeDelta = sizeDelta;

            var raw = go.AddComponent<RawData>();
            raw.Info["font_size"] = fontSize;
            raw.Info["align"] = align;

            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = message;
            text.font = renderer.GetTMPFontAsset(fontName, fontStyle);
            text.fontSize = fontSize.Value;
            text.color = fontColor;
            // BAUM2からTextMeshProへの変換を行うと少し横にひろがってしまうことへの対応
            // text.textInfo.textComponent.characterSpacing = -1.7f; // 文字幅を狭める
            text.textInfo.textComponent.enableWordWrapping = false; // 自動的に改行されるのを抑える

            bool middle = true;
            if (type == "point")
            {
                text.horizontalMapping = TextureMappingOptions.Line;
                text.verticalMapping = TextureMappingOptions.Line;
                middle = true;
            }
            else if (type == "paragraph")
            {
                text.horizontalMapping = TextureMappingOptions.Paragraph;
                text.verticalMapping = TextureMappingOptions.Line;
                if (align.Contains("upper"))
                {
                    middle = false;
                }
                else
                {
                    middle = !message.Contains("\n");
                }
            }
            else
            {
                Debug.LogError("unknown type " + type);
            }

            // var fixedPos = rect.anchoredPosition;
            if (align.Contains("left"))
            {
                text.alignment = middle ? TextAlignmentOptions.MidlineLeft : TextAlignmentOptions.TopLeft;
                // rect.pivot = new Vector2(0.0f, 0.5f);
                // fixedPos.x -= sizeDelta.x / 2.0f;
            }
            else if (align.Contains("center"))
            {
                // text.alignment =　middle ? TextAnchor.MiddleCenter : TextAnchor.UpperCenter;
                text.alignment = middle ? TextAlignmentOptions.Midline : TextAlignmentOptions.Top;
                // rect.pivot = new Vector2(0.5f, 0.5f);
            }
            else if (align.Contains("right"))
            {
                // text.alignment =　middle ? TextAnchor.MiddleRight : TextAnchor.UpperRight;
                text.alignment = middle ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.TopRight;
                // rect.pivot = new Vector2(1.0f, 0.5f);
                // fixedPos.x += sizeDelta.x / 2.0f;
            }

            // rect.anchoredPosition = fixedPos;

            // var d = rect.sizeDelta;
            // d.y = virtualHeight;
            // rect.sizeDelta = d;

            if (enableStroke)
            {
                var outline = go.AddComponent<Outline>();
                outline.effectColor = strokeColor;
                outline.effectDistance = new Vector2(strokeSize.Value / 2.0f, -strokeSize.Value / 2.0f);
                outline.useGraphicAlpha = false;
            }

            // SetStretch(go, renderer);
            SetAnchor(go, renderer);
            return go;
        }

        public override Area CalcArea()
        {
            return Area.FromPositionAndSize(canvasPosition.Value, sizeDelta.Value);
        }
    }
#endif
}