using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using PiHire.BAL.Common.Logging;
using PiHire.BAL.IRepositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.BAL.Repositories
{
    public class ChatMessagesRepository: BaseRepository, IChatMessagesRepository
    {
        readonly Logger logger;
        private readonly IWebHostEnvironment _environment;
        public ChatMessagesRepository(DAL.PiHIRE2Context dbContext,
            Common.Extensions.AppSettings appSettings, ILogger<CandidateRepository> logger, IWebHostEnvironment environment) : base(dbContext, appSettings)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            this.logger = new Logger(logger, GetType());
            _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        }


    }
}
