using Microsoft.Extensions.Logging;
using UnityEngine;
using ZLogger.Unity;

namespace GameLib.Log
{
    [CreateAssetMenu(menuName = "Logging/Providers/Unity Console")]
    public class UnityConsoleLoggerConfig 
        : LoggerProviderConfigBase<ZLoggerUnityDebugLoggerProvider>
    {
        protected override void AddProvider(ILoggingBuilder builder, LoggerConfiguration root)
        {
            builder.AddZLoggerUnityDebug(options =>
            {
                options.UsePlainTextFormatter();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                options.PrettyStacktrace = true;
#else
                options.PrettyStacktrace = false;
#endif
            });
        }
    }
}