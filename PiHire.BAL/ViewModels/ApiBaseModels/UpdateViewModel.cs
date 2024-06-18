using PiHire.BAL.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.BAL.ViewModels.ApiBaseModels
{
    public class UpdateResponseViewModel<T> : ResponseBaseViewModel
    {
        public T Result { get; set; }
        public void SetResult(T dat)
        {
            Result = dat;
            Status = true;
            Meta.SetHttpStatus(Repositories.ApipResponseHttpCodes.OK);
        }

        public void SetError(ApiResponseErrorCodes statusCode, string ErrorMessage, bool isOverride = false)
        {
            Status = false;
            Meta.SetError(statusCode, ErrorMessage, isOverride);
        }
    }
}
