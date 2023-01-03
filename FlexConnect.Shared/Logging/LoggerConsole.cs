namespace FlexConnect.Shared.Logging
{
    public class LoggerConsole : ILogger
    {
        public async Task LogAsync(LogLevel logLevel, string message)
        {
            await Task.Run(() =>
            {
                Console.WriteLine($"[{logLevel}] {message}");
            });
        }
    }
}
