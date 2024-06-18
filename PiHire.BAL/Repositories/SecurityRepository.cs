using Microsoft.Extensions.Logging;
using PiHire.BAL.Common.Extensions;
using PiHire.BAL.Common.Logging;
using PiHire.DAL;
using System;
using System.Reflection;

namespace PiHire.BAL.Repositories
{
    public class SecurityRepository : BaseRepository, IRepositories.ISecurityRepository
    {
        private Logger logger;
        public SecurityRepository(PiHIRE2Context dbContext, AppSettings appSettings, ILogger<SecurityRepository> logger) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logLevel">LogLevel</param>
        /// <param name="loggingEvent">LoggingEvents</param>
        /// <param name="msg">message</param>
        /// <param name="RequestID"></param>
        public void LogMessage(LogLevel logLevel, int loggingEvent, string msg, string RequestID)
        {
            logger.SetMethodName(MethodBase.GetCurrentMethod());
            logger.Log(logLevel, loggingEvent, msg, RequestID);
        }

    }
}
