using Microsoft.Extensions.Logging;
using System;
using System.Reflection;


namespace PiHire.BAL.Common.Logging
{
    public class Logger
    {
        ILogger logger;
        string ClsNm = string.Empty, MethodName = string.Empty;
        public Logger(ILogger logger, Type ClassType)
        {
            this.logger = logger;
            this.ClsNm = ClassType.FullName;
        }
        public void SetMethodName(MethodBase methodBase)
        {
            MethodName = methodBase.DeclaringType.Name;
        }
        /// <summary>
        /// Log the info
        /// </summary>
        /// <param name="level">Log level(Trace,Debug,Information,Warning,Error,Critical)</param>
        /// <param name="eventId"></param>
        /// <param name="msg">Message</param>
        public void Log(LogLevel level, int eventId, string msg)
        {
            Log(level, eventId, msg, null, null);
        }

        public void Log(LogLevel level, int eventId, string msg, string RefId)
        {
            Log(level, eventId, msg, RefId, null);
        }

        /// <summary>
        /// Log the info
        /// </summary>
        /// <param name="level">Log level(Trace,Debug,Information,Warning,Error,Critical)</param>
        /// <param name="eventId"></param>
        /// <param name="msg">Message</param>
        /// <param name="exception">Exception</param>
        public void Log(LogLevel level, int eventId, string msg, Exception exception)
        {
            Log(level, eventId, msg, null, exception);
        }

        /// <summary>
        /// Log the info
        /// </summary>
        /// <param name="level">Log level(Trace,Debug,Information,Warning,Error,Critical)</param>
        /// <param name="eventId"></param>
        /// <param name="msg">Message</param>
        /// <param name="RefId"></param>
        /// <param name="exception">Exception</param>
        public void Log(LogLevel level, int eventId, string msg, string RefId, Exception exception)
        {
            try
            {
                var _msg = ClsNm + "/" + MethodName + ":" + msg + (string.IsNullOrEmpty(RefId) ? "" : ", ReferenceId:" + RefId);

                if (exception == null)
                    logger.Log(level, eventId, _msg);
                else
                    logger.Log(level, eventId, exception, _msg);
            }
            catch (Exception e)
            {
                var filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "piHire Service");
                if (!System.IO.Directory.Exists(filePath))
                {
                    System.IO.Directory.CreateDirectory(filePath);
                }
                filePath = System.IO.Path.Combine(filePath, DateTime.Now.ToString("log exception") + ".txt");
                System.IO.File.AppendAllLines(filePath, new string[] { DateTime.Now + ":" + (e == null ? "" : ", exception:" + (e.Message + ".InnerException:" + e?.InnerException?.Message)) });
            }
        }
    }
}
