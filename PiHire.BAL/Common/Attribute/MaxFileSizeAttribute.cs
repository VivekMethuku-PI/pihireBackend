using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PiHire.BAL.Common.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.Common.Attribute
{
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly byte _fileType;
        public IConfigurationRoot config;
        public MaxFileSizeAttribute(byte fileType)
        {
            _fileType = fileType;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var file = value as IFormFile;

            config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();

            var SettingProperties = config.GetSection("AppSettings");
            var appSettingsProperties = SettingProperties.Get<AppSettingsProperties>();

            if (file != null)
            {
                if (_fileType == (byte)FileType.Video)
                {
                    if (file.Length > appSettingsProperties.AllowedVideoSize)
                    {
                        return new ValidationResult(GetErrorMessage(appSettingsProperties.AllowedVideoSize));
                    }
                }
                else if (_fileType == (byte)FileType.Audio)
                {
                    if (file.Length > appSettingsProperties.AllowedAudioSize)
                    {
                        return new ValidationResult(GetErrorMessage(appSettingsProperties.AllowedAudioSize));
                    }
                }
                else
                {
                    if (file.Length > appSettingsProperties.AllowedFileSize)
                    {
                        return new ValidationResult(GetErrorMessage(appSettingsProperties.AllowedFileSize));
                    }
                }
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage(int fileSize)
        {
            return $"Maximum allowed file size is { fileSize } bytes";
        }

    }

}
