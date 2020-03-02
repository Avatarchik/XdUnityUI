using System;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OnionRing;
using UnityEditor.U2D;
using UnityEngine.U2D;
using Match = System.Text.RegularExpressions.Match;
using Object = UnityEngine.Object;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// based on Baum2/Editor/Scripts/BaumImporter file.
    /// </summary>
    public sealed class updateDisplayProgressBar : AssetPostprocessor
    {
        public override int GetPostprocessOrder()
        {
            return 1000;
        }

        private class FileInfoComparer : IEqualityComparer<FileInfo>
        {
            public bool Equals(FileInfo iLhs, FileInfo iRhs)
            {
                if (iLhs.Name == iRhs.Name)
                {
                    return true;
                }

                return false;
            }

            public int GetHashCode(FileInfo fi)
            {
                var s = fi.Name;
                return s.GetHashCode();
            }
        }

        private static int progressTotal = 1;
        private static int progressCount = 0;

        private static void UpdateDisplayProgressBar(string message = "")
        {
            if (progressTotal > 1)
            {
                EditorUtility.DisplayProgressBar("XdUnitUI Import", $"{progressCount}/{progressTotal} {message}",
                    ((float) progressCount / progressTotal));
            }
        }

        public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            var importedDirectoryPath = EditorUtil.ToUnityPath(EditorUtil.GetImportDirectoryPath());
            progressTotal = importedAssets.Length + deletedAssets.Length + movedAssets.Length;
            progressCount = 0;

            var changed = false;

            // スプライト出力フォルダの作成
            foreach (var importedAsset in importedAssets)
            {
                // 入力アセットがインポートフォルダ内あるか
                if (!importedAsset.Contains(importedDirectoryPath)) continue;
                // 拡張子をもっているかどうかでディレクトリインポートかどうかを判定する
                if (!string.IsNullOrEmpty(Path.GetExtension(importedAsset))) continue;
                // 拡張子が無いのでディレクトリ
                var exportPath = EditorUtil.GetBaumSpritesFullPath(importedAsset);
                var importPath = Path.GetFullPath(importedAsset);
                if (Directory.Exists(exportPath))
                {
                    // すでにあるフォルダ　インポートファイルと比較して、多い分を削除する
                    // ダブっている分は上書きするようにする
                    var exportInfo = new DirectoryInfo(exportPath);
                    var importInfo = new DirectoryInfo(importPath);

                    var list1 = exportInfo.GetFiles("*.png", SearchOption.AllDirectories);
                    var list2 = importInfo.GetFiles("*.png", SearchOption.AllDirectories);

                    // exportフォルダにある importにはないファイルをリストアップする
                    // 注意：
                    // 　-no-slice -9slice付きのファイルなどは、イメージ名が変更されexportフォルダに入るので
                    // 　差分としてでる
                    var list3 = list1.Except(list2, new FileInfoComparer());

                    foreach (var fileInfo in list3)
                    {
                        fileInfo.Delete();
                        changed = true;
                    }
                }
                else
                {
                    CreateSpritesDirectory(importedAsset);
                    changed = true;
                }
            }

            if (changed)
            {
                // ディレクトリが作成されたり、画像が削除されるためRefresh
                AssetDatabase.Refresh();
                changed = false;
            }

            // フォルダが作成され、そこに画像を作成する場合
            // Refresh後、DelayCallで画像生成することで、処理が安定した
            EditorApplication.delayCall += () =>
            {
                // 画像コンバート　スライス処理
                foreach (var importedAsset in importedAssets)
                {
                    if (!importedAsset.Contains(importedDirectoryPath)) continue;
                    if (!importedAsset.EndsWith(".png", System.StringComparison.Ordinal)) continue;
                    // スライス処理
                    var message = SliceSprite(importedAsset);
                    // 元画像を削除する
                    File.Delete(Path.GetFullPath(importedAsset));
                    // AssetDatabase.DeleteAsset(EditorUtil.ToUnityPath(asset));
                    changed = true;
                    progressCount += 1;
                    UpdateDisplayProgressBar(message);
                }

                if (changed)
                {
                    AssetDatabase.Refresh();
                    changed = false;
                }

                EditorApplication.delayCall += () =>
                {
                    // ディレクトリ削除
                    foreach (var asset in importedAssets)
                    {
                        if (!asset.Contains(importedDirectoryPath)) continue;
                        // 拡張子がなければ、ディレクトリと判定する
                        if (!string.IsNullOrEmpty(Path.GetExtension(asset))) continue;
                        var fullPath = Path.GetFullPath(asset);
                        // ディレクトリが空っぽかどうか調べる　コンバート用PNGファイルがはいっていた場合、
                        // 変換後削除されるため、すべて変換された場合、空になる
                        if (Directory.EnumerateFileSystemEntries(fullPath).Any()) continue;
                        // 空であれば削除
                        Debug.LogFormat("[XdUnityUI] Delete Directory: {0}", EditorUtil.ToUnityPath(asset));
                        AssetDatabase.DeleteAsset(EditorUtil.ToUnityPath(asset));
                    }

                    // Create Prefab
                    foreach (var asset in importedAssets)
                    {
                        UpdateDisplayProgressBar("layout");
                        progressCount += 1;
                        if (!asset.Contains(importedDirectoryPath)) continue;
                        if (!asset.EndsWith(".layout.json", System.StringComparison.Ordinal)) continue;

                        var name = Path.GetFileName(asset).Replace(".layout.json", "");
                        var spriteRootPath =
                            EditorUtil.ToUnityPath(Path.Combine(EditorUtil.GetOutputSpritesPath(), name));
                        var fontRootPath = EditorUtil.ToUnityPath(EditorUtil.GetFontsPath());
                        var creator = new PrefabCreator(spriteRootPath, fontRootPath, asset);
                        var go = creator.Create();
                        var savePath =
                            EditorUtil.ToUnityPath(Path.Combine(EditorUtil.GetOutputPrefabsPath(), name + ".prefab"));
                        GameObject savedAsset;
                        try
                        {
#if UNITY_2018_3_OR_NEWER
                            savedAsset = PrefabUtility.SaveAsPrefabAsset(go, savePath);
#else
                    Object originalPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(savePath);
                    if (originalPrefab == null) originalPrefab = PrefabUtility.CreateEmptyPrefab(savePath);
                    PrefabUtility.ReplacePrefab(go, originalPrefab, ReplacePrefabOptions.ReplaceNameBased);
#endif
                        }
                        catch
                        {
                            // 変換中例外が起きた場合もテンポラリGameObjectを削除する
                            Object.DestroyImmediate(go);
                            EditorUtility.ClearProgressBar();
                            throw;
                        }

                        // 作成に成功した
                        Object.DestroyImmediate(go);
                        Debug.Log($"[XdUnityUI] Create Prefab: {savePath}", savedAsset);
                        // layout.jsonを削除する
                        AssetDatabase.DeleteAsset(EditorUtil.ToUnityPath(asset));
                    }

                    EditorUtility.ClearProgressBar();
                };
            };
        }

        private static void CreateSpritesDirectory(string asset)
        {
            var directoryName = Path.GetFileName(Path.GetFileName(asset));
            var directoryPath = EditorUtil.GetOutputSpritesPath();
            var directoryFullPath = Path.Combine(directoryPath, directoryName);
            if (Directory.Exists(directoryFullPath))
            {
                // 画像出力用フォルダに画像がのこっていればすべて削除
                // Debug.LogFormat("[XdUnityUI] Delete Exist Sprites: {0}", EditorUtil.ToUnityPath(directoryFullPath));
                foreach (var filePath in Directory.GetFiles(directoryFullPath, "*.png", SearchOption.TopDirectoryOnly))
                    File.Delete(filePath);
            }
            else
            {
                // Debug.LogFormat("[XdUnityUI] Create Directory: {0}", EditorUtil.ToUnityPath(directoryPath) + "/" + directoryName);
                AssetDatabase.CreateFolder(EditorUtil.ToUnityPath(directoryPath), Path.GetFileName(directoryFullPath));
            }
        }

        /// <summary>
        /// 読み込み可能なTextureを作成する
        /// Texture2DをC#ScriptでReadableに変更するには？ - Qiita
        /// https://qiita.com/Katumadeyaruhiko/items/c2b9b4ccdfe51df4ad4a
        /// </summary>
        /// <param name="texture2d"></param>
        /// <returns></returns>
        private static Texture2D CreateReadabeTexture2D(Texture2D texture2d)
        {
            // オプションをRenderTextureReadWrite.sRGBに変更した
            RenderTexture renderTexture = RenderTexture.GetTemporary(
                texture2d.width,
                texture2d.height,
                0,
                RenderTextureFormat.ARGB32,
                RenderTextureReadWrite.sRGB);

            Graphics.Blit(texture2d, renderTexture);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            Texture2D readableTextur2D = new Texture2D(texture2d.width, texture2d.height);
            readableTextur2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            readableTextur2D.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);
            return readableTextur2D;
        }

        private static string SliceSprite(string asset)
        {
            var directoryName = Path.GetFileName(Path.GetDirectoryName(asset));
            var directoryPath = Path.Combine(EditorUtil.GetOutputSpritesPath(), directoryName);
            var fileName = Path.GetFileName(asset);
            var texture = CreateReadabeTexture2D(AssetDatabase.LoadAssetAtPath<Texture2D>(asset));
            if (PreprocessTexture.SlicedTextures == null)
                PreprocessTexture.SlicedTextures = new Dictionary<string, SlicedTexture>();

            // Textureデータの書き出し
            // 同じファイル名の場合書き込みしない
            string CheckWrite(string newPath, byte[] pngData)
            {
                if (File.Exists(newPath))
                {
                    var oldPngData = File.ReadAllBytes(newPath);
                    if (oldPngData.Length == pngData.Length && pngData.SequenceEqual(oldPngData))
                    {
                        return "same texture";
                    }
                }

                File.WriteAllBytes(newPath, pngData);
                return "new texture";
            }

            var noSlice = fileName.EndsWith("-noslice.png", StringComparison.Ordinal);
            if (noSlice)
            {
                var slicedTexture = new SlicedTexture(texture, new Boarder(0, 0, 0, 0));
                fileName = fileName.Replace("-noslice.png", ".png");
                var newPath = Path.Combine(directoryPath, fileName);
                PreprocessTexture.SlicedTextures[fileName] = slicedTexture;
                var pngData = texture.EncodeToPNG();
                Object.DestroyImmediate(slicedTexture.Texture);

                return CheckWrite(newPath, pngData);
            }

            const string pattern = "-9slice,([0-9]+)px,([0-9]+)px,([0-9]+)px,([0-9]+)px\\.png";
            var matches = Regex.Match(fileName, pattern);
            if (matches.Length > 0)
            {
                // 上・右・下・左の端から内側へのオフセット量
                var top = int.Parse(matches.Groups[1].Value);
                var right = int.Parse(matches.Groups[2].Value);
                var bottom = int.Parse(matches.Groups[3].Value);
                var left = int.Parse(matches.Groups[4].Value);

                var slicedTexture = new SlicedTexture(texture, new Boarder(left, bottom, right, top));
                fileName = Regex.Replace(fileName, pattern, ".png");
                var newPath = Path.Combine(directoryPath, fileName);

                PreprocessTexture.SlicedTextures[fileName] = slicedTexture;
                var pngData = texture.EncodeToPNG();
                Object.DestroyImmediate(slicedTexture.Texture);

                return CheckWrite(newPath, pngData);
            }

            {
                var slicedTexture = TextureSlicer.Slice(texture);
                var newPath = Path.Combine(directoryPath, fileName);

                PreprocessTexture.SlicedTextures[fileName] = slicedTexture;
                var pngData = slicedTexture.Texture.EncodeToPNG();
                Object.DestroyImmediate(slicedTexture.Texture);

                return CheckWrite(newPath, pngData);
            }
            // Debug.LogFormat("[XdUnityUI] Slice: {0} -> {1}", EditorUtil.ToUnityPath(asset), EditorUtil.ToUnityPath(newPath));
        }

        /**
        * SliceSpriteではつかなくなったが､CreateAtlasでは使用する
        */
        private static string ImportSpritePathToOutputPath(string asset)
        {
            var directoryName = Path.GetFileName(Path.GetDirectoryName(asset));
            var directoryPath = Path.Combine(EditorUtil.GetOutputSpritesPath(), directoryName);
            var fileName = Path.GetFileName(asset);
            return Path.Combine(directoryPath, fileName);
        }

        private static void CreateAtlas(string name, List<string> importPaths)
        {
            var filename = Path.Combine(EditorUtil.GetBaumAtlasPath(), name + ".spriteatlas");

            var atlas = new SpriteAtlas();
            var settings = new SpriteAtlasPackingSettings
            {
                padding = 8,
                enableTightPacking = false
            };
            atlas.SetPackingSettings(settings);
            var textureSettings = new SpriteAtlasTextureSettings
            {
                filterMode = FilterMode.Point,
                generateMipMaps = false,
                sRGB = true
            };
            atlas.SetTextureSettings(textureSettings);

            var textureImporterPlatformSettings = new TextureImporterPlatformSettings {maxTextureSize = 8192};
            atlas.SetPlatformSettings(textureImporterPlatformSettings);

            // iOS用テクスチャ設定
            // ASTCに固定してしまいっている　これらの設定を記述できるようにしたい
            textureImporterPlatformSettings.overridden = true;
            textureImporterPlatformSettings.name = "iPhone";
#if UNITY_2019_1_OR_NEWER
            textureImporterPlatformSettings.format = TextureImporterFormat.ASTC_4x4;
#endif
            atlas.SetPlatformSettings(textureImporterPlatformSettings);

            // アセットの生成
            AssetDatabase.CreateAsset(atlas, EditorUtil.ToUnityPath(filename));

            // ディレクトリを登録する場合
            // var iconsDirectory = AssetDatabase.LoadAssetAtPath<Object>("Assets/ExternalAssets/Baum2/CreatedSprites/UIESMessenger");
            // atlas.Add(new Object[]{iconsDirectory});
        }
    }
}