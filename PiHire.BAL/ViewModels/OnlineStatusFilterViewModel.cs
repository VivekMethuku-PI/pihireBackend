using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class OnlineStatusFilterViewModel
    {
        public int[] UserIds { get; set; }
        public string[] EmailIds { get; set; }
        public int[] UserTypes { get; set; }
    }
}
