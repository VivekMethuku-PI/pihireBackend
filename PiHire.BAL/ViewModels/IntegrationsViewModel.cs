using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class IntegrationsViewModel
    {
        public int Id { get; set; }
        public byte Category { get; set; }
        public byte? SubCategory { get; set; }
        public string Title { get; set; }
        public string Logo { get; set; }
        public string InteDesc { get; set; }
        public byte? QtyOrPeriodFlag { get; set; }
        public short? ValiPeriod { get; set; }
        public int? Quantity { get; set; }
        public int? Price { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string Account { get; set; }       
    }
    public class IntegrationStatusRespViewModel
    {
        public bool IsAuth { get; set; }
        public string AuthUrl { get; set; }
        public string message { get; set; }
    }
    public class IntegrationReqStatusViewModel
    {
        public bool IsCompleted { get; set; }
        public bool IsSuccess { get; set; }
    }
}
