using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;
using UnityEngine.UI;
#if TMP_PRESENT
using TMPro;
#endif

namespace XdUnityUI.Editor
{
    public class Renderer
    {
        private readonly string spriteRootPath;
        private readonly string fontRootPath;
        private readonly Vector2 imageSize;
        public Vector2 CanvasSize { get; private set; }

        private readonly Vector2 basePosition;
        //

        public Dictionary<string, GameObject> ToggleGroupMap { get; } = new Dictionary<string, GameObject>();

        public ToggleGroup GetToggleGroup(string name)
        {
            ToggleGroup toggleGroup;
            if (!ToggleGroupMap.ContainsKey(name))
            {
                // まだそのグループが存在しない場合は､GameObjectを作成
                var go = new GameObject(name);
                // AddComponent･登録する
                toggleGroup = go.AddComponent<ToggleGroup>();
                // Allow Switch Off を True にする
                // 190711 false(デフォルト)だと DoozyUIがHideするときに､トグルONボタンを初期位置に戻してしまうため
                toggleGroup.allowSwitchOff = true;
                ToggleGroupMap[name] = go;
            }
            else
            {
                // 存在する場合は利用する
                toggleGroup = ToggleGroupMap[name].GetComponent<ToggleGroup>();
            }

            return toggleGroup;
        }


        public Renderer(string spriteRootPath, string fontRootPath, Vector2 imageSize, Vector2 canvasSize,
            Vector2 basePosition)
        {
            this.spriteRootPath = spriteRootPath;
            this.fontRootPath = fontRootPath;
            this.imageSize = imageSize;
            CanvasSize = canvasSize;
            this.basePosition = basePosition;
        }

        public Sprite GetSprite(string spriteName)
        {
            var fullPath = Path.Combine(spriteRootPath, spriteName) + ".png";
            // 相対パスの記述に対応した
            var fileInfo = new System.IO.FileInfo(fullPath);
            fullPath = EditorUtil.ToUnityPath(fileInfo.FullName);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
            Assert.IsNotNull(sprite,
                $"[XdUnityUI] sprite \"{spriteName}\" is not found fullPath:{fullPath}");
            return sprite;
        }

        public Font GetFont(string fontName)
        {
            var font = AssetDatabase.LoadAssetAtPath<Font>(Path.Combine(fontRootPath, fontName) + ".ttf");
            if (font == null) font = AssetDatabase.LoadAssetAtPath<Font>(Path.Combine(fontRootPath, fontName) + ".otf");
            if (font == null) font = Resources.GetBuiltinResource<Font>(fontName + ".ttf");
            Assert.IsNotNull(font, $"[XdUnityUI] font \"{fontName}\" is not found");
            return font;
        }

#if TMP_PRESENT
        public TMP_FontAsset GetTMPFontAsset(string fontName, string style)
        {
            var fontFileName = Path.Combine(fontRootPath, fontName) + "-" + style + " SDF.asset";
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontFileName);
            Assert.IsNotNull(font, string.Format("[XdUnityUI] TMP_FontAsset \"{0}\" is not found", fontFileName));
            return font;
        }
#endif

        public Vector2 CalcPosition(Vector2 position, Vector2 size)
        {
            return CalcPosition(position + size / 2.0f);
        }

        private Vector2 CalcPosition(Vector2 position)
        {
            var tmp = position - basePosition;
            tmp.y *= -1.0f;
            return tmp;
        }

        public Vector2[] GetFourCorners()
        {
            var corners = new Vector2[4];
            corners[0] = CalcPosition(Vector2.zero) + (imageSize - CanvasSize) / 2.0f;
            corners[2] = CalcPosition(imageSize) - (imageSize - CanvasSize) / 2.0f;
            return corners;
        }
    }
}
