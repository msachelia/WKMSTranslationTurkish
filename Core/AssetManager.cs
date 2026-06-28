using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using BepInEx.Logging;

namespace WKMSTranslation.Core
{
    public static class AssetManager
    {
        private static readonly Dictionary<string, Sprite> _sprites = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, Texture2D> _textures = new(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, AudioClip> _audioClips = new(StringComparer.OrdinalIgnoreCase);
        
        private static ManualLogSource _log;

        public static void Initialize(ManualLogSource log, string pluginPath)
        {
            _log = log;
            LoadFromAssetBundle(Path.Combine(pluginPath, "customassets"));
            LoadLooseTextures(Path.Combine(pluginPath, "Textures"));
            _log.LogInfo($"[AssetManager] Loaded {_sprites.Count} Sprites, {_textures.Count} Textures, {_audioClips.Count} AudioClips.");
        }

        private static void LoadFromAssetBundle(string bundlePath)
        {
            if (!File.Exists(bundlePath)) return;
            try
            {
                var bundle = AssetBundle.LoadFromFile(bundlePath);
                if (bundle == null) return;

                foreach (var sprite in bundle.LoadAllAssets<Sprite>()) _sprites[sprite.name] = sprite;
                foreach (var tex in bundle.LoadAllAssets<Texture2D>()) _textures[tex.name] = tex;
                foreach (var clip in bundle.LoadAllAssets<AudioClip>()) _audioClips[clip.name] = clip;

                bundle.Unload(false); 
            }
            catch (Exception ex) { _log.LogError($"[AssetManager] Error loading AssetBundle: {ex.Message}"); }
        }

        private static void LoadLooseTextures(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                return;
            }

            foreach (var file in Directory.GetFiles(folderPath, "*.png"))
            {
                try
                {
                    string name = Path.GetFileNameWithoutExtension(file);
                    byte[] data = File.ReadAllBytes(file);
                    Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                    ImageConversion.LoadImage(tex, data);
                    tex.name = name;

                    Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
                    sprite.name = name;

                    _textures[name] = tex;
                    _sprites[name] = sprite;
                }
                catch (Exception ex) { _log.LogError($"[AssetManager] Failed to load loose texture {file}: {ex.Message}"); }
            }
        }

        public static Sprite GetSprite(string name) => string.IsNullOrEmpty(name) ? null : (_sprites.TryGetValue(name, out var s) ? s : null);
        public static Texture2D GetTexture(string name) => string.IsNullOrEmpty(name) ? null : (_textures.TryGetValue(name, out var t) ? t : null);
        public static AudioClip GetAudio(string name) => string.IsNullOrEmpty(name) ? null : (_audioClips.TryGetValue(name, out var a) ? a : null);

        public static void ReplaceAllAssetsInScene()
        {
            if (_sprites.Count == 0 && _textures.Count == 0 && _audioClips.Count == 0) return;

            foreach (var img in Resources.FindObjectsOfTypeAll<Image>())
            {
                if (img.sprite == null) continue;
                var newSprite = GetSprite(img.sprite.name);
                if (newSprite != null && img.sprite != newSprite) img.sprite = newSprite;
            }

            foreach (var rImg in Resources.FindObjectsOfTypeAll<RawImage>())
            {
                if (rImg.texture == null) continue;
                var newTex = GetTexture(rImg.texture.name);
                if (newTex != null && rImg.texture != newTex) rImg.texture = newTex;
            }

            foreach (var sr in Resources.FindObjectsOfTypeAll<SpriteRenderer>())
            {
                if (sr.sprite == null) continue;
                var newSprite = GetSprite(sr.sprite.name);
                if (newSprite != null && sr.sprite != newSprite) sr.sprite = newSprite;
            }

            foreach (var src in Resources.FindObjectsOfTypeAll<AudioSource>())
            {
                if (src.clip == null) continue;
                var newClip = GetAudio(src.clip.name);
                if (newClip != null && src.clip != newClip) src.clip = newClip;
            }
        }
    }
}