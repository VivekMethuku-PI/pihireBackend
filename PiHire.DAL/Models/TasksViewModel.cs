using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.DAL.Models
{
    public class TasksViewModel
    {
        public short Id { get; set; }
        public short ModuleId { get; set; }
        public string Task { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public string Activities { get; set; }
        public bool ActivityFlag { get; set; }
    }
}
