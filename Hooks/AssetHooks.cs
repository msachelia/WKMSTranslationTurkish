using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using WKMSTranslation.Core;

namespace WKMSTranslation.Hooks
{
    [HarmonyPatch]
    public static class AssetHooks
    {
        // 1. ИЗОБРАЖЕНИЯ (UI Image)
        [HarmonyPatch(typeof(Image), "sprite", MethodType.Setter)]
        [HarmonyPrefix]
        public static void Image_sprite_Setter(ref Sprite value)
        {
            if (value == null) return;
            var newSprite = AssetManager.GetSprite(value.name);
            if (newSprite != null) value = newSprite;
        }

        // 2. ИЗОБРАЖЕНИЯ (RawImage - использует Texture)
        [HarmonyPatch(typeof(RawImage), "texture", MethodType.Setter)]
        [HarmonyPrefix]
        public static void RawImage_texture_Setter(ref Texture value)
        {
            if (value == null) return;
            var newTex = AssetManager.GetTexture(value.name);
            if (newTex != null) value = newTex;
        }

        // 3. 2D СПРАЙТЫ В МИРЕ (SpriteRenderer)
        [HarmonyPatch(typeof(SpriteRenderer), "sprite", MethodType.Setter)]
        [HarmonyPrefix]
        public static void SpriteRenderer_sprite_Setter(ref Sprite value)
        {
            if (value == null) return;
            var newSprite = AssetManager.GetSprite(value.name);
            if (newSprite != null) value = newSprite;
        }

        // 4. АУДИО (AudioSource)
        [HarmonyPatch(typeof(AudioSource), "clip", MethodType.Setter)]
        [HarmonyPrefix]
        public static void AudioSource_clip_Setter(ref AudioClip value)
        {
            if (value == null) return;
            var newClip = AssetManager.GetAudio(value.name);
            if (newClip != null) value = newClip;
        }
    }
}