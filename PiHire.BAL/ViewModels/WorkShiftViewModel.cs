using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class WorkShiftViewModel
    {
        public byte? Status { get; set; }
        public int Id { get; set; }
        public string ShiftName { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<WorkShiftDtlsViewModel> WorkShiftDtlsViewModel { get; set; }
    }

    public class WorkShiftDtlsViewModel
    {
        public int Id { get; set; }
        public string DayName { get; set; }
        public bool? IsWeekend { get; set; }
        public int? From { get; set; }
        public string FromMeridiem { get; set; }
        public int? To { get; set; }
        public string ToMeridiem { get; set; }
        public int ShiftId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public byte? Status { get; set; }
        public bool? IsAlternateWeekend { get; set; }
        public bool? AlternativeStart { get; set; }
        public DateTime? AlternativeWeekStartDate { get; set; }
        public int? FromMinutes { get; set; }
        public int? ToMinutes { get; set; }
        public int? WeekendModel { get; set; }
    }

    public class CreateWorkShiftDtlsViewModel
    {
        [Required]
        [MaxLength(20)]
        public string ShiftName { get; set; }
        public List<CreateWorkShiftViewModel> createWorkShiftViewModels { get; set; }
    }

    public class CreateWorkShiftViewModel
    {
        [Required]
        [MaxLength(20)]
        public string DayName { get; set; }
        public bool? IsWeekend { get; set; }
        public int? From { get; set; }
        public string FromMeridiem { get; set; }
        public int? To { get; set; }
        public string ToMeridiem { get; set; }
        public bool? IsAlternateWeekend { get; set; }
        public bool? AlternativeStart { get; set; }
        public DateTime? AlternativeWeekStartDate { get; set; }
        public int? FromMinutes { get; set; }
        public int? ToMinutes { get; set; }
        public int? WeekendModel { get; set; }
    }


    public class UpdateWorkShiftDtlsViewModel
    {
        [Required]       
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string ShiftName { get; set; }
        public List<UpdateWorkShiftViewModel> UpdateWorkShiftViewModels { get; set; }
      
    }

    public class UpdateWorkShiftViewModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [MaxLength(20)]
        public string DayName { get; set; }
        public bool? IsWeekend { get; set; }
        public int? From { get; set; }
        public string FromMeridiem { get; set; }
        public int? To { get; set; }
        public string ToMeridiem { get; set; }
        public bool? IsAlternateWeekend { get; set; }
        public bool? AlternativeStart { get; set; }
        public DateTime? AlternativeWeekStartDate { get; set; }
        public int? FromMinutes { get; set; }
        public int? ToMinutes { get; set; }
        public int? WeekendModel { get; set; }
    }
}
