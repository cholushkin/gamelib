using Microsoft.Extensions.Logging;
using ZLogger;
using ZLogger.Providers;

namespace GameLib.Log
{
    [UnityEngine.CreateAssetMenu(menuName = "GameLib/Log/FileLoggerConfig")]
    public class FileLoggerConfig 
        : LoggerProviderConfigBase<ZLoggerFileLoggerProvider>
    {
        public string FilePath = "Logs/game.log";

        protected override void AddProvider(ILoggingBuilder builder, LoggerConfiguration root)
        {
            builder.AddZLoggerFile(FilePath, options =>
            {
                options.UsePlainTextFormatter();
            });
        }
    }
}