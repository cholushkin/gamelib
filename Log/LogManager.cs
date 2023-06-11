using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Alg;
using UnityEngine;

namespace GameLib.Log
{
    public class LogManager : Singleton<LogManager>
    {
        [Serializable]
        public class Filter
        {
            public string AllowSubsystemWildcard;
            public bool Enabled;
            public bool Solo;
        }
        public LogChecker.Level GlobalLevel;

        [SerializeField]
        [Tooltip("If any of these wildcards pass the message, the message will be printed")]
        private List<Filter> FilterChain;
        private List<Filter> _solo;


        void OnValidate()
        {
            RefreshSoloCache();
        }

        // Note: call this method after you change solo states at runtime
        public void RefreshSoloCache()
        {
            _solo = FilterChain.Where(x => x.Enabled && x.Solo).ToList();
        }

        public bool IsPassed(string subsystem)
        {
            if (!subsystem.EndsWith("."))
                subsystem += ".";
            var filters = _solo.Count > 0 ? _solo : FilterChain;
            foreach (var filter in filters)
            {
                if (!filter.Enabled)
                    continue;

                var regExpression = _wildCardToRegular(filter.AllowSubsystemWildcard);
                var pass = Regex.IsMatch(subsystem, regExpression);
                if (pass)
                    return true;
            }
            return false;
        }

        private static string _wildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\*", ".*") + "$";
        }
    }
}

