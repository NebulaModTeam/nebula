namespace NebulaModel.Logger;

public interface ILogger
{
    void LogDebug(object data);

    void LogInfo(object data);

    void LogWarning(object data);

    void LogError(object data);
}
