using BepInEx.Logging;
using NebulaModel.Logger;

namespace NebulaPatcher
{
    public class BepInExLogger : ILogger
    {
        ManualLogSource logger;

        public BepInExLogger(ManualLogSource logger)
        {
            this.logger = logger;
        }

        public void LogDebug(object data)
        {
            logger.LogDebug(data);
        }

        public void LogError(object data)
        {
            logger.LogError(data);
        }

        public void LogInfo(object data)
        {
            logger.LogInfo(data);
        }

        public void LogWarning(object data)
        {
            logger.LogWarning(data);
        }
    }
}
