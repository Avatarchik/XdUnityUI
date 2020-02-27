using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using OnionRing;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// PreprocessTexture class.
    /// based on Baum2.Editor.PreprocessTexture class.
    /// </summary>
    public sealed class PreprocessTexture : AssetPostprocessor
    {
        public override int GetPostprocessOrder() { return 990; }

        public static Dictionary<string, SlicedTexture> SlicedTextures;

        public void OnPreprocessTexture()
        {
            var importDirectoryPath = EditorUtil.GetImportDirectoryPath();
            if (assetPath.Contains(importDirectoryPath))
            {
                var importer = assetImporter as TextureImporter;
                if (importer == null) return;

                importer.textureType = TextureImporterType.Sprite;
                importer.isReadable = true;
            }
            else if (assetPath.Contains(EditorUtil.ToUnityPath(EditorUtil.GetOutputSpritesPath())))
            {
                var fileName = Path.GetFileName(assetPath);
                if (SlicedTextures == null || !SlicedTextures.ContainsKey(fileName)) return;
                var importer = assetImporter as TextureImporter;
                if (importer == null) return;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePackingTag = string.Format("{0}_{1}", "Baum2", Path.GetFileName(Path.GetDirectoryName(assetPath)));
                importer.spritePixelsPerUnit = 100.0f;
                importer.spritePivot = new Vector2(0.5f, 0.5f);
                importer.mipmapEnabled = false;
                importer.isReadable = false;
                importer.spriteBorder = SlicedTextures[fileName].Boarder.ToVector4();
                importer.filterMode = FilterMode.Bilinear;
#if UNITY_5_5_OR_NEWER
				importer.textureCompression = TextureImporterCompression.Uncompressed;
#else
                importer.textureFormat = TextureImporterFormat.ARGB32;
#endif
                SlicedTextures.Remove(fileName);
                if (SlicedTextures.Count == 0) SlicedTextures = null;
            }
        }
    }
}