using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PiHire.BAL.ViewModels
{
    public class ReportRequestViewModel
    {
        public dashboardDateFilter DateFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? PuId { get; set; }
        public int? BuId { get; set; }
        public int? StageId { get; set; }
        public int? UserId { get; set; }
        public string StatusCode { get; set; }      
        public string SourcedFrom { get; set; }
    }


    public class BDMReportModel
    {
        public List<BDMOverviewModel> BDMOverviewModel { get; set; }
        public List<PerformanceOnFieldWise> PerformanceOnStatusWise { get; set; }

        public int JobsCount { get; set; }
        public int AccountsCount { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class RecruiterReportModel
    {
        public List<RecruiterOverviewModel> RecruiterOverviewModel { get; set; }
        public List<PerformanceOnFieldWise> PerformanceOnStatusWise { get; set; }
        public int JobsCount { get; set; }
        public int AccountsCount { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class BDMOpeingReportModel
    {
        public List<BDMOpeningOverviewModel> BDMOpeningOverviewModel { get; set; }
        public List<PerformanceOnFieldWise> PerformanceOnStatusWise { get; set; }
        public int JobsCount { get; set; }
        public int AccountsCount { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class RecruiterOpeningReportModel
    {
        public List<RecruiterOpeningOverviewModel> RecruiterOpeningOverviewModel { get; set; }
        public List<PerformanceOnFieldWise> PerformanceOnStatusWise { get; set; }
        public int JobsCount { get; set; }
        public int AccountsCount { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class PerformanceOnFieldWise
    {
        public DateTime CreatedDate { get; set; }
        public byte TabDispay { get; set; }
        public string Title { get; set; }
        public string JobStatus { get; set; }
        public int Count { get; set; }
        public int TotalCount { get; set; }
        public decimal Percentage { get; set; }
        public string RecruiterProfilePhoto { get; set; }
        public string RecruiterName { get; set; }
        public int? RecId { get; set; }
        public int? BdmId { get; set; }
        public string BdmName { get; set; }
        public string BdmProfilePhoto { get; set; }
    }

    public class PriorityChangeViewModel
    {
        [Required]
        public int JoId { get; set; }
        public int? Recruiter { get; set; }
        [Required]
        public int Priority { get; set; }
    }



}
