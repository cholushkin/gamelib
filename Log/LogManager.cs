using System;
using System.Text.RegularExpressions;
using Alg;

namespace GameLib.Log
{
    [ScriptExecutionOrder(-2000)]
    public class LogManager : Singleton<LogManager>
    {
        [Serializable]
        public class FilterConfigItem
        {
            public LogFilters Filter;
            public bool IsEnabled;
        }
        public LogChecker.Level GlobalLevel;
        public FilterConfigItem[] FilterConfig;

        public bool IsPassed(string subsystem)
        {
            foreach (var cfg in FilterConfig)
            {
                if(!cfg.IsEnabled)
                    continue;
                var filter = cfg.Filter;
                // pass
                if (filter.PassFilters.Length > 0)
                {
                    foreach (var passFilter in filter.PassFilters)
                    {
                        var regExpression = _wildCardToRegular(passFilter);
                        var isPassed = Regex.IsMatch(subsystem, regExpression);
                        if (!isPassed)
                            return false;
                    }
                }

                // reject
                foreach (var rejectFilter in filter.RejectFilters)
                {
                    var regExpression = _wildCardToRegular(rejectFilter);
                    var isRejected = Regex.IsMatch(subsystem, regExpression);
                    if (isRejected)
                        return false;
                }
            }
            return true;
        }

        private static string _wildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\*", ".*") + "$";
        }
    }
}

