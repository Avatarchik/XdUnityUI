using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// ImageElement class.
    /// based on Baum2.Editor.ImageElement class.
    /// </summary>
    public class ImageElement : Element
    {
        private string spriteName;

        //private Vector2? canvasPosition;
        //private Vector2? sizeDelta;
        private float? opacity;
        public Dictionary<string, object> component;
        public Dictionary<string, object> imageJson;

        public ImageElement(Dictionary<string, object> json, Element parent) : base(json, parent)
        {
            if (json.ContainsKey("image"))
            {
                spriteName = json.Get("image");
            }

            //canvasPosition = json.GetVector2("x", "y");
            //sizeDelta = json.GetVector2("w", "h");
            opacity = json.GetFloat("opacity");
            component = json.GetDic("component");
            imageJson = json.GetDic("image");
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

            var image = go.AddComponent<Image>();
            if (spriteName != null)
                image.sprite = renderer.GetSprite(spriteName);
            
            image.color = new Color(1.0f, 1.0f, 1.0f, opacity != null ? opacity.Value / 100.0f : 0);
            var raycastTarget = imageJson.GetBool("raycast_target");
            if (raycastTarget != null)
                image.raycastTarget = raycastTarget.Value;

            image.type = Image.Type.Sliced;
            var imageType = imageJson.Get("image_type");
            if (imageType != null)
            {
                switch (imageType.ToLower())
                {
                    case "sliced":
                        image.type = Image.Type.Sliced;
                        break;
                    case "filled":
                        image.type = Image.Type.Filled;
                        break;
                    case "tiled":
                        image.type = Image.Type.Tiled;
                        break;
                    case "simple":
                        image.type = Image.Type.Simple;
                        break;
                    default:
                        Debug.LogAssertion("[Baum2+] unknown image_type:" + imageType);
                        break;
                }
            }

            var preserveAspect = imageJson.GetBool("preserve_aspect");
            if (preserveAspect != null)
            {
                // アスペクト比を保つ場合はSimpleにする
                image.type = Image.Type.Simple;
                image.preserveAspect = preserveAspect.Value;
            }

            SetAnchor(go, renderer);

            return go;
        }

        public override Area CalcArea()
        {
            /*
            if (canvasPosition != null && sizeDelta != null)
                return Area.FromPositionAndSize(canvasPosition.Value, sizeDelta.Value);
                */
            return null;
        }
    }
}