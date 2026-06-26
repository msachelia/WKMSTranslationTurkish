using HarmonyLib;
using TMPro;
using WKMSTranslation.Core;
using Febucci.UI;
using Febucci.UI.Core;
using System;

namespace WKMSTranslation.Hooks
{
    public static class ThirdPartyHooks
    {
        [ThreadStatic] public static int ThirdPartyDepth;

        [HarmonyPatch(typeof(UT_TextScrawl), "ShowText")]
        public static class ScrawlHook
        {
            [HarmonyPrefix]
            public static void Prefix(ref string s)
            {
                if (!string.IsNullOrWhiteSpace(s)) s = TranslationEngine.GetTranslation(s);
                ThirdPartyDepth++;
            }
            [HarmonyFinalizer] public static void Finalizer() => ThirdPartyDepth--;
        }

        [HarmonyPatch(typeof(TypewriterCore), "ShowText", new Type[] { typeof(string) })]
        public static class TypewriterCore_ShowText_Hook
        {
            [HarmonyPrefix]
            public static void Prefix(ref string text)
            {
                if (!string.IsNullOrWhiteSpace(text)) text = TranslationEngine.GetTranslation(text);
                ThirdPartyDepth++;
            }
            [HarmonyFinalizer] public static void Finalizer() => ThirdPartyDepth--;
        }

        [HarmonyPatch(typeof(TAnimCore), "ConvertText")]
        public static class TAnimCore_ConvertText_Hook
        {
            [HarmonyPrefix]
            public static void Prefix(TAnimCore __instance, ref string textToParse)
            {
                if (!string.IsNullOrWhiteSpace(textToParse)) 
                    textToParse = TranslationEngine.GetTranslation(textToParse);
                
                if (__instance is TextAnimator_TMP animTmp && animTmp.TMProComponent != null)
                {
                    if (TranslationEngine.IsEnabled)
                        FontManager.TryReplace(animTmp.TMProComponent);
                }
                
                ThirdPartyDepth++;
            }
            [HarmonyFinalizer] public static void Finalizer() => ThirdPartyDepth--;
        }
    }
}