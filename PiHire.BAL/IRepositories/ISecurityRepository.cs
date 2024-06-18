using Microsoft.Extensions.Logging;

namespace PiHire.BAL.IRepositories
{
    public interface ISecurityRepository
    {      
        /// <param name="logLevel">LogLevel</param>
        /// <param name="loggingEvent">LoggingEvents</param>
        /// <param name="msg">message</param>
        /// <param name="RequestID"></param>
        void LogMessage(LogLevel logLevel, int loggingEvent, string msg, string RequestID);
    }
}
