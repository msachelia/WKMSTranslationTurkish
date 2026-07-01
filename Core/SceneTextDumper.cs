using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Febucci.UI.Core;

namespace WKMSTranslation.Core
{
    public static class SceneTextDumper
    {
        public static void DumpSceneTextInfo(string folder, string fileName = "SceneTextDump.json")
        {
            if (string.IsNullOrEmpty(folder)) return;
            Directory.CreateDirectory(folder);

            var dump = new SceneTextDump
            {
                GeneratedAt = DateTime.Now,
                UnityVersion = Application.unityVersion,
                Entries = CollectEntries()
            };

            string path = Path.Combine(folder, fileName);
            File.WriteAllText(path, JsonConvert.SerializeObject(dump, Formatting.Indented));
            Debug.Log($"[WKMSTranslation] Dumped text info to: {path}");
        }

        private static List<SceneTextDumpEntry> CollectEntries()
        {
            var result = new List<SceneTextDumpEntry>();

            foreach (var txt in Resources.FindObjectsOfTypeAll<TMP_Text>())
            {
                if (!txt.gameObject.activeInHierarchy) continue;
                result.Add(CreateEntry(txt));
            }

            foreach (var txt in Resources.FindObjectsOfTypeAll<Text>())
            {
                if (!txt.gameObject.activeInHierarchy) continue;
                result.Add(CreateEntry(txt));
            }

            foreach (var input in Resources.FindObjectsOfTypeAll<InputField>())
            {
                if (!input.gameObject.activeInHierarchy) continue;
                result.Add(CreateEntry(input));
            }

            foreach (var tmpInput in Resources.FindObjectsOfTypeAll<TMP_InputField>())
            {
                if (!tmpInput.gameObject.activeInHierarchy) continue;
                result.Add(CreateEntry(tmpInput));
            }

            return result;
        }

        private static SceneTextDumpEntry CreateEntry(TMP_Text txt)
        {
            var fontAsset = txt.font;

            var entry = new SceneTextDumpEntry
            {
                ComponentType = txt.GetType().Name,
                GameObjectPath = GetGameObjectPath(txt.transform),
                Text = txt.text,
                TextPreview = CreatePreview(txt.text),
                FontName = fontAsset?.name,
                FontSize = txt.fontSize,
                AutoSize = txt.enableAutoSizing,
                FontAssetName = fontAsset?.name,
                AtlasWidth = fontAsset?.atlasWidth ?? 0,
                AtlasHeight = fontAsset?.atlasHeight ?? 0,
                AtlasFilterMode = fontAsset?.atlasTexture != null ? fontAsset.atlasTexture.filterMode.ToString() : null,

                RenderMode = fontAsset != null ? fontAsset.atlasRenderMode.ToString() : null,
                AtlasPadding = fontAsset != null ? fontAsset.atlasPadding : 0,

                // Получение оригинального Point Size генерации шрифта
                PointSize = fontAsset != null ? fontAsset.faceInfo.pointSize : 0,

                ShaderName = txt.fontSharedMaterial?.shader?.name,
                AdditionalInfo = CreateTextMaterialInfo(txt.fontSharedMaterial!)
            };

            var tAnim = txt.GetComponent<TAnimCore>();
            if (tAnim != null) entry.AdditionalInfo["Febucci_TextAnimator"] = tAnim.GetType().Name;
            
            var tWriter = txt.GetComponent<TypewriterCore>();
            if (tWriter != null) entry.AdditionalInfo["Febucci_Typewriter"] = tWriter.GetType().Name;

            return entry;
        }

        private static SceneTextDumpEntry CreateEntry(Text txt)
        {
            return new SceneTextDumpEntry
            {
                ComponentType = txt.GetType().Name,
                GameObjectPath = GetGameObjectPath(txt.transform),
                Text = txt.text,
                TextPreview = CreatePreview(txt.text),
                FontName = txt.font?.name,
                FontSize = txt.fontSize,
                BestFit = txt.resizeTextForBestFit,
                AdditionalInfo = CreateLegacyTextInfo(txt)
            };
        }

        private static SceneTextDumpEntry CreateEntry(InputField input)
        {
            return new SceneTextDumpEntry
            {
                ComponentType = input.GetType().Name,
                GameObjectPath = GetGameObjectPath(input.transform),
                Text = input.text,
                TextPreview = CreatePreview(input.text),
                FontName = input.textComponent?.font?.name,
                FontSize = input.textComponent?.fontSize ?? 0,
                AdditionalInfo = new Dictionary<string, string>
                {
                    ["Placeholder"] = input.placeholder?.gameObject.name ?? "",
                    ["CharacterLimit"] = input.characterLimit.ToString()
                }
            };
        }

        private static SceneTextDumpEntry CreateEntry(TMP_InputField input)
        {
            return new SceneTextDumpEntry
            {
                ComponentType = input.GetType().Name,
                GameObjectPath = GetGameObjectPath(input.transform),
                Text = input.text,
                TextPreview = CreatePreview(input.text),
                FontName = input.textComponent?.font?.name,
                FontSize = input.textComponent?.fontSize ?? 0,
                AdditionalInfo = new Dictionary<string, string>
                {
                    ["Placeholder"] = input.placeholder?.gameObject.name ?? "",
                    ["CharacterLimit"] = input.characterLimit.ToString()
                }
            };
        }

        private static Dictionary<string, string> CreateTextMaterialInfo(Material material)
        {
            var info = new Dictionary<string, string>();
            if (material == null) return info;

            info["MaterialName"] = material.name;
            info["ShaderName"] = material.shader?.name ?? string.Empty;
            if (material.HasProperty("_FaceDilate")) info["FaceDilate"] = material.GetFloat("_FaceDilate").ToString();
            if (material.HasProperty("_OutlineWidth")) info["OutlineWidth"] = material.GetFloat("_OutlineWidth").ToString();

            return info;
        }

        private static Dictionary<string, string> CreateLegacyTextInfo(Text txt)
        {
            var info = new Dictionary<string, string>();
            if (txt.font?.material?.mainTexture != null)
                info["TextureFilterMode"] = txt.font.material.mainTexture.filterMode.ToString();
            return info;
        }

        private static string CreatePreview(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            string preview = text.Replace("\n", "\\n");
            return preview.Length > 50 ? preview.Substring(0, 50) + "..." : preview;
        }

        private static string GetGameObjectPath(Transform transform)
        {
            if (transform == null) return string.Empty;
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        private class SceneTextDump
        {
            public DateTime GeneratedAt { get; set; }
            public string? UnityVersion { get; set; }
            public List<SceneTextDumpEntry>? Entries { get; set; }
        }

        private class SceneTextDumpEntry
        {
            public string? ComponentType { get; set; }
            public string? GameObjectPath { get; set; }
            public string? Text { get; set; }
            public string? TextPreview { get; set; }
            public string? FontName { get; set; }
            public float FontSize { get; set; }
            public bool? AutoSize { get; set; }
            public bool? BestFit { get; set; }
            public string? FontAssetName { get; set; }
            public int AtlasWidth { get; set; }
            public int AtlasHeight { get; set; }
            public string? AtlasFilterMode { get; set; }
            public string? RenderMode { get; set; }
            public int AtlasPadding { get; set; }
            public int PointSize { get; set; } 
            public string? ShaderName { get; set; }
            public Dictionary<string, string>? AdditionalInfo { get; set; }
        }
    }
}