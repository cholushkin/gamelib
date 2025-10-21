using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using UnityEngine;
using NaughtyAttributes;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace GameLib.Log
{
    /*
    Intent of each knob
    =================================
    • Global HardFloor — absolute clamp for all outputs (global AddFilter).
    • Global DefaultMin — soft default when nothing more specific applies (SetMinimumLevel).
    • Per-provider HardFloor — clamp for that provider (AddFilter<ThatProvider>).
    • Per-provider DefaultMin — effective only if STRICTER than Global DefaultMin (otherwise ignored).
    • Provider Solo — if any provider has Solo = true, only those providers are enabled.
    • Provider Mute — provider is skipped entirely.
    • Category Solo — within a provider, if any category rule has Solo = true, only those categories are active for that provider.
    • Category Mute — within a provider, mutes that specific prefix entirely.

    Precedence (strongest → weakest)
    1) Global HardFloor
    2) Provider HardFloor
    3) Provider DefaultMin (only if > Global DefaultMin)
    4) Global DefaultMin
    5) Category/prefix filters (Solo/Mute/MinLevel within each provider)

    Rule of thumb:
    AddFilter is final for that scope. SetMinimumLevel is a fallback.
    */

    [CreateAssetMenu(fileName = "LoggerConfiguration", menuName = "GameLib/Log/Logger Configuration")]
    public class LoggerConfiguration : ScriptableObject
    {
        [Serializable]
        public class ProviderConfiguration
        {
            [BoxGroup("Provider"), Required]
            public LoggerProviderConfigBase Provider;

            [BoxGroup("Provider")]
            [Label("Hard Floor")]
            [Tooltip("Provider-wide hard floor (cannot be loosened by other filters).")]
            public LogLevel HardFloor;

            [BoxGroup("Provider/Advanced")]
            [Label("Default Min")]
            [Tooltip("Soft default for this provider. Effective only if stricter than Global DefaultMin.")]
            public LogLevel DefaultMin;

            [BoxGroup("Provider/Flags")]
            public bool Solo;

            [BoxGroup("Provider/Flags")]
            public bool Mute;
        }

        [InfoBox(
            "•Global HardFloor is an ABSOLUTE clamp; nothing below it passes.\n" +
            "•Global DefaultMin is a soft default used when nothing more specific applies.\n" +
            "•Provider HardFloor clamps ALL categories for that provider.\n" +
            "•Provider DefaultMin takes effect ONLY if it is STRICTER than Global DefaultMin.\n" +
            "•Category filters (by prefix) can further TIGHTEN or MUTE categories inside the provider.\n" +
            "•Solo flags (provider or category) isolate only selected outputs.\n" +
            "\n" +
            "Tips:\n" +
            "•Keep DefaultMin ≥ HardFloor (global & per-provider).\n" +
            "•Use Provider DefaultMin sparingly; prefer category filters.\n" +
            "•In production, raise Global HardFloor (Warning/Error) and keep file provider stricter than console.\n" +
            "•Apply provider-wide floors BEFORE category rules (done in LogManager).\n"
        )]
        [Label("Global Hard Floor")]
        public LogLevel HardFloor;

        [Label("Global Default Min")]
        public LogLevel DefaultMin;

        [Tooltip("Enabled log providers and their local rules.")]
        public List<ProviderConfiguration> Providers = new();


        [Button]
        private void PrintLoggerConfiguration()
        {
            Debug.Log("=== Logger Configuration ===");

            // Global settings
            string globalInfo = $"Global Hard Floor: {HardFloor}, Global Default Min Level: {DefaultMin}";
            Debug.Log(globalInfo);

            if (Providers == null || Providers.Count == 0)
            {
                Debug.Log("No providers configured.");
                return;
            }

            foreach (var providerConfig in Providers)
            {
                if (providerConfig == null)
                {
                    Debug.LogWarning("Encountered a null Provider Configuration entry. Skipping...");
                    continue;
                }

                string providerName = providerConfig.Provider?.name ?? "<null>";
                var sb = new System.Text.StringBuilder();

                sb.AppendLine($"--- Provider: {providerName} ---");
                sb.AppendLine($"Hard Floor: {providerConfig.HardFloor}, Default Min: {providerConfig.DefaultMin}, Solo: {providerConfig.Solo}, Mute: {providerConfig.Mute}");

                if (providerConfig.Provider != null && providerConfig.Provider.CategoryFilters != null &&
                    providerConfig.Provider.CategoryFilters.Count > 0)
                {
                    sb.AppendLine("Category Filters:");
                    foreach (var rule in providerConfig.Provider.CategoryFilters)
                    {
                        if (rule == null) continue;
                        string prefix = string.IsNullOrEmpty(rule.CategoryPrefix) ? "<empty>" : rule.CategoryPrefix;
                        sb.AppendLine($"  Prefix: {prefix}, MinLevel: {rule.MinLevel}, Solo: {rule.Solo}, Mute: {rule.Mute}");
                    }
                }
                else
                {
                    sb.AppendLine("No category filters defined.");
                }

                Debug.Log(sb.ToString());
            }

            Debug.Log("=== End of Logger Configuration ===");
        }


#if UNITY_EDITOR
        void OnValidate()
        {
            // Global rule: DefaultMin must be >= HardFloor
            if (DefaultMin < HardFloor)
            {
                Debug.LogWarning(
                    $"[LoggerConfiguration] Global DefaultMin ({DefaultMin}) is lower than Global HardFloor ({HardFloor}). " +
                    $"Raising DefaultMin to {HardFloor}.");
                DefaultMin = HardFloor;
                EditorUtility.SetDirty(this);
            }

            // Provider rule: Provider.DefaultMin must be >= Provider.HardFloor
            foreach (var p in Providers)
            {
                if (p == null) continue;

                if (p.DefaultMin < p.HardFloor)
                {
                    Debug.LogWarning(
                        $"[LoggerConfiguration] Provider '{p.Provider?.name ?? "<null>"}': DefaultMin ({p.DefaultMin}) " +
                        $"is lower than HardFloor ({p.HardFloor}). Raising DefaultMin to {p.HardFloor}.");
                    p.DefaultMin = p.HardFloor;
                    EditorUtility.SetDirty(this);
                }
            }
        }
#endif
    }
}
