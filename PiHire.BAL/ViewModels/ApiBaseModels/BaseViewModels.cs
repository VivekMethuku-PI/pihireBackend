using PiHire.BAL.Repositories;
using System;

namespace PiHire.BAL.ViewModels.ApiBaseModels
{
    public class CollectionMinimumLengthAttribute : System.ComponentModel.DataAnnotations.ValidationAttribute
    {
        int minLength;
        public CollectionMinimumLengthAttribute(int minLength = 1)
        {
            this.minLength = minLength;
        }
        public override bool IsValid(object value)
        {
            var list = value as System.Collections.IList;
            if (list != null)
            {
                return list.Count >= minLength;
            }
            return false;
        }
    }

    public class RequestBaseViewModel
    {
    }
    public class ResponseBaseViewModel
    {
        public ResponseBaseViewModel()
        {
            Meta = new ResponseMetaViewModel()
            {
                DateTime = DateTime.UtcNow.ToString("dd-MMM-yyyy HH:mm:ss"),
                RequestID = Guid.NewGuid().ToString(),
            };
        }
        public ResponseMetaViewModel Meta { get; set; }
        public bool Status { get; set; }
    }
    public class ResponseMetaViewModel
    {
        public int HttpStatusCode { get; set; }
        public string HttpStatusMessage { get; set; }
        public string RequestID { get; set; }
        public string DateTime { get; set; }
        public ResponseErrorViewModel Error { get; set; }

        public void SetError(ApiResponseErrorCodes statusCode, string ErrorMessage, bool isOverride = false)
        {
            if (Error == null)
            {
                Error = new ResponseErrorViewModel();
            }
            Error.ErrorMessageCode = (int)statusCode;
            Error.ErrorMessage += (isOverride == false && BaseRepository.ErrorMessages.ContainsKey(statusCode)) ? BaseRepository.ErrorMessages[statusCode] : (string.IsNullOrEmpty(Error.ErrorMessage) ? "" : ", ") + ErrorMessage;
            if (BaseRepository.GetHttpCode.ContainsKey(statusCode))
            {
                var httpcd = BaseRepository.GetHttpCode[statusCode];
                HttpStatusCode = (int)httpcd;
                if (BaseRepository.HttpMessages.ContainsKey(httpcd))
                {
                    HttpStatusMessage = BaseRepository.HttpMessages[httpcd];
                }
            }
        }
        public void SetHttpStatus(ApipResponseHttpCodes httpCode)
        {
            HttpStatusCode = (int)httpCode;
            if (BaseRepository.HttpMessages.ContainsKey(httpCode))
            {
                HttpStatusMessage = BaseRepository.HttpMessages[httpCode];
            }
        }
    }
    public class ResponseErrorViewModel
    {
        public int ErrorMessageCode { get; set; }
        public string ErrorMessage { get; set; }
    }
}
