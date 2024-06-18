using Microsoft.Extensions.Configuration;
using System.IO;

namespace PiHire.BAL.Common.Extensions
{
    public class AppSettings
    {
        public readonly AppSettingsProperties AppSettingsProperties;
        public readonly SmtpEmailConfig smtpEmailConfig;
        public IConfigurationRoot config;
        public AppSettings()
        {
            config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

            var SettingProperties = config.GetSection("AppSettings");
            AppSettingsProperties = SettingProperties.Get<AppSettingsProperties>();

            var SmtpEmailConfig = config.GetSection("SmtpEmailConfig");
            smtpEmailConfig = SmtpEmailConfig.Get<SmtpEmailConfig>();
        }
        public AppSettings(AppSettingsProperties appSettingsProperties, SmtpEmailConfig smtpEmailConfig)
        {
            this.AppSettingsProperties = appSettingsProperties;
            this.smtpEmailConfig = smtpEmailConfig;
        }
    }

    public class AppSettingsProperties
    {
        public string JwtSecret { get; set; }
        public string JwtIssuer { get; set; }
        public int JwtValidityMinutes { get; set; }
        public string GatewayUrl { get; set; }
        public string CRMUrl { get; set; }
        public string HireApiUrl { get; set; }       
        public string HappinessApiUrl { get; set; }     
        public string HireAppUrl { get; set; }
        public string CandidateAppUrl { get; set; }
        public int HawksAccountId { get; set; }
        public string HappinessApiUsername { get; set; }
        public string HappinessApiPassword { get; set; }
        public int HappinessApplicationId { get; set; }      
        public int AllowedVideoSize { get; set; }
        public int AllowedAudioSize { get; set; }
        public int AllowedFileSize { get; set; }
        public string AssessmentTemplateId { get; set; }
        public int AppId { get; set; }
        public string CompanyName { get; set; }
        public int SurveyExpiryDays { get; set; }
        public int WhenToSend { get; set; }
        
        public string MicrosoftTeamsTenantId { get; set; }
        public string MicrosoftTeamsClientId { get; set; }
        public string MicrosoftTeamClientSecret { get; set; }
     
        public int OnlineStatusValidMins { get; set; }
        public string CurrConvURL { get; set; }
        public string CurrConvApiKey { get; set; }
        public string GatewayMigratedToHireOn { get; set; }


        public string OdooBaseURL { get; set; }
        public string OdooLoginURL { get; set; }       
        public string OdooUsername { get; set; }
        public string OdooPassword { get; set; }
        public string OdooDb { get; set; }


        public string OdooConvtEmp { get; set; }
        public string CreateUpdateRefData { get; set; }
    }

    public class SmtpEmailConfig
    {
        public string SmtpAddress { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpLoginName { get; set; }
        public string SmtpLoginPassword { get; set; }
        public bool SmtpEnableSsl { get; set; }
        public string SmtpFromEmail { get; set; }
        public string SmtpFromName { get; set; }
    }
}
