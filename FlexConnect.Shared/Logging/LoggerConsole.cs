using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexConnect.Shared.Logging
{
    public class LoggerConsole : ILogger
    {
        public async Task LogAsync(LogLevel logLevel, string message)
        {
            Console.WriteLine($"[{logLevel}] {message}");
        }
    }
}
