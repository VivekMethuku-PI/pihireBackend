using PiHire.BAL.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.BAL.IRepositories
{
    public interface IBaseRepository
    {
        UserAuthorizationViewModel Usr { get; set; }
    }
}
