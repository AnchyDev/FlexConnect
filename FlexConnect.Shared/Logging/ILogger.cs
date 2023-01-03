namespace FlexConnect.Shared.Logging
{
    public interface ILogger
    {
        public Task LogAsync(LogLevel logLevel, string message);
    }
}
