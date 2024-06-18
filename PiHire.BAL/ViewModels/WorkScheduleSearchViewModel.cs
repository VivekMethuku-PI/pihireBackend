using PiHire.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class WorkScheduleSearchViewModel
    {
        public int PuId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }


    public class WorkScheduleDtlsViewModel
    {
        public int? ShiftId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public int? LocationId { get; set; }
        public List<string> Weekend { get; set; }
        //public List<string> AlternativeWeekend { get; set; }
        public List<SubClassWeekend> WeekModelObj { get; set; }
        //public List<DateTime?> AlternativeStartDate { get; set; }
        public List<ShiftDataModel> ShiftData { get; set; }
    }




    public class SubClassWeekend
    {
        public int? WeekendModel { get; set; }
        public string DayName { get; set; }
    }

    public class ShiftDataModel
    {
        public string DayName { get; set; }
        public DateTime Date { get; set; }
        public bool IsOnLeave { get; set; }
        public string  LeaveName { get; set; }      
        public int FromHour { get; set; }
        public int ToHour { get; set; }
        public bool IsWeekEnd { get; set; }        
        public bool IsLeaveRequest { get; set; }
        public int UserId { get; set; }
    }

}
