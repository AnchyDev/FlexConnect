using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace FlexConnect.Shared.Logging
{
    public interface ILogger
    {
        public Task LogAsync(LogLevel logLevel, string message);
    }
}
