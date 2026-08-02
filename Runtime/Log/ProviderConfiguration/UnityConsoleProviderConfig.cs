using Microsoft.Extensions.Logging;
using UnityEngine;
using ZLogger;
using ZLogger.Unity;

namespace GameLib.Log
{
    [CreateAssetMenu(menuName = "GameLib/Log/UnityConsoleLoggerConfig")]
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