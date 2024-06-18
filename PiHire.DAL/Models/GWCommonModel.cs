using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public class GWCommonModel
    {

    }

    public class GetPUViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string? logo { get; set; }
    }

    public class GetUserPUViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
    }

    public class GetBUViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public int PUID { get; set; }
    }
}
