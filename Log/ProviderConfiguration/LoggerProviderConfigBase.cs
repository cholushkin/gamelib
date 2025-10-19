using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using UnityEngine;

namespace GameLib.Log
{
    // Non-generic base for all logging provider configs
    // Stores category rules (Solo/Mute/MinLevel) and exposes configuration API
    public abstract class LoggerProviderConfigBase : ScriptableObject
    {
        [Serializable]
        public class CategoryRule
        {
            [Tooltip("Category prefix (e.g., 'Game.', 'AI.'). Prefix match.")]
            public string CategoryPrefix = string.Empty;

            [Tooltip("Minimum level required for this prefix.")]
            public LogLevel MinLevel = LogLevel.Information;

            [Tooltip("If true, only Solo categories are kept active (others muted).")]
            public bool Solo;

            [Tooltip("If true, this category is completely muted.")]
            public bool Mute;
        }

        [Header("Provider-Scoped Category Filters")]
        [Tooltip("Rules apply only to this provider. Solo/Mute/MinLevel logic handled by base.")]
        public List<CategoryRule> CategoryFilters = new();

        // For UI/diagnostics
        public abstract Type ProviderType { get; }

        // Configure this provider with the builder
        public abstract void Configure(ILoggingBuilder builder, LoggerConfiguration root);

        // Set a provider-wide minimum level (floor)
        public abstract void SetProviderWideFloor(ILoggingBuilder builder, LogLevel minLevel);
    }

    // Generic base implementing AddFilter logic for concrete TProvider
    public abstract class LoggerProviderConfigBase<TProvider> : LoggerProviderConfigBase
        where TProvider : ILoggerProvider
    {
        // Concrete provider type
        public override Type ProviderType => typeof(TProvider);

        // Subclasses must implement adding the provider itself
        protected abstract void AddProvider(ILoggingBuilder builder, LoggerConfiguration root);

        // Apply provider-wide floor using typed AddFilter
        protected virtual void ApplyProviderWideFloor(ILoggingBuilder builder, LogLevel minLevel)
        {
            builder.AddFilter<TProvider>(string.Empty, level => level >= minLevel);
        }

        // Apply category filters (Solo/Mute/MinLevel logic)
        protected virtual void ApplyProviderCategoryFilters(ILoggingBuilder builder, IReadOnlyList<CategoryRule> filters)
        {
            if (filters == null || filters.Count == 0) return;

            var rules = new List<CategoryRule>(filters);
            rules.RemoveAll(r => r == null);
            if (rules.Count == 0) return;

            // If any Solo exists, only keep those
            if (rules.Exists(r => r.Solo))
                rules.RemoveAll(r => !r.Solo);

            foreach (var r in rules)
            {
                var prefix = string.IsNullOrEmpty(r.CategoryPrefix) ? string.Empty : r.CategoryPrefix;
                if (r.Mute)
                    builder.AddFilter<TProvider>(prefix, _ => false);
                else
                    builder.AddFilter<TProvider>(prefix, level => level >= r.MinLevel);
            }
        }

        // Configure provider: add provider then apply category filters
        public override void Configure(ILoggingBuilder builder, LoggerConfiguration root)
        {
            AddProvider(builder, root);
            ApplyProviderCategoryFilters(builder, CategoryFilters);
        }

        // Set provider-wide floor
        public override void SetProviderWideFloor(ILoggingBuilder builder, LogLevel minLevel)
        {
            if (minLevel < LogLevel.Trace) return;
            ApplyProviderWideFloor(builder, minLevel);
        }
    }
}
