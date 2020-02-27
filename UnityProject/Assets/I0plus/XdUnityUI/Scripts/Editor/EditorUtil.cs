using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// EditorUtil class.
    /// based on Baum2.Editor.EditorUtil class.
    /// </summary>
    public static class EditorUtil
    {
        public static string ToUnityPath(string path)
        {
            path = path.Substring(path.IndexOf("Assets", System.StringComparison.Ordinal));
            if (path.EndsWith("/", System.StringComparison.Ordinal) ||
                path.EndsWith("\\", System.StringComparison.Ordinal)) path = path.Substring(0, path.Length - 1);
            return path.Replace("\\", "/");
        }

        public static string GetImportDirectoryPath()
        {
            var path= GetPath("_XdUnityUIImport1", false);
            if (path != null) return path;
            return GetPath("_XdUnityUIImport");
        }

        public static string GetOutputSpritesPath()
        {
            var path= GetPath("_XdUnityUISprites1", false);
            if (path != null) return path;
            return GetPath("_XdUnityUISprites");
        }

        public static string GetOutputPrefabsPath()
        {
            var path= GetPath("_XdUnityUIPrefabs1", false);
            if (path != null) return path;
            return GetPath("_XdUnityUIPrefabs");
        }

        public static string GetFontsPath()
        {
            var path= GetPath("_XdUnityUIFonts1", false);
            if (path != null) return path;
            return GetPath("_XdUnityUIFonts");
        }

        public static string GetBaumAtlasPath()
        {
            var path= GetPath("_XdUnityUIAtlas1", false);
            if (path != null) return path;
            return GetPath("_XdUnityUIAtlas");
        }

        public static string GetPath(string fileName, bool throwException = true)
        {
            var files = Directory.GetFiles(Application.dataPath, fileName, SearchOption.AllDirectories);
            if (files.Length > 1)
            {
                files = files.Where(x => !x.Contains("Baum2/Sample")).ToArray();
            }

            if (files.Length > 1)
            {
                Debug.LogErrorFormat("{0}ファイルがプロジェクト内に複数個存在します。", fileName);
            }

            if (files.Length == 0)
            {
                if (throwException)
                    throw new System.ApplicationException(string.Format("{0}ファイルがプロジェクト内に存在しません。", fileName));
                return null;
            }

            string path = files[0];
            return path.Substring(0, path.Length - fileName.Length);
        }

        /// <summary>
        /// サブディレクトリを含めたスプライトの出力パスを取得する
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public static string GetBaumSpritesFullPath(string asset)
        {
            // サブディレクトリ名を取得する
            var directoryName = Path.GetFileName(Path.GetFileName(asset));
            var directoryPath = GetOutputSpritesPath();
            var directoryFullPath = Path.Combine(directoryPath, directoryName);
            return directoryFullPath;
        }

        public static Color HexToColor(string hex)
        {
            if (hex[0] == '#')
            {
                hex = hex.Substring(1);
            }

            var r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            var g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            var b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            return new Color32(r, g, b, 255);
        }

        public static RectTransform CopyTo(this RectTransform self, RectTransform to)
        {
            to.sizeDelta = self.sizeDelta;
            to.position = self.position;
            return self;
        }

        public static Image CopyTo(this Image self, Image to)
        {
            to.sprite = self.sprite;
            to.type = self.type;
            to.color = self.color;
            return self;
        }
    }
}