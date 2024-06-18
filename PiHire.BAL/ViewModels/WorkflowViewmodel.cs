using PiHire.BAL.Repositories;
using PiHire.DAL.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static PiHire.BAL.Common.Types.AppConstants;

namespace PiHire.BAL.ViewModels
{
    public class WorkflowViewmodel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Status { get; set; }
    }

    public class CreateWorkflowRuleViewmodel
    {
        [Required]
        public short TaskId { get; set; }
        [Required]
        public string TaskCode { get; set; }
        [Required]
        [EnumDataType(typeof(WorkflowActionMode))]
        public byte ActionMode { get; set; }

        public List<CreateWorkflowRuleDetailsViewmodel> WorkflowRuleDetailsViewmodel { get; set; }
    }

    public class CreateWorkflowRuleDetailsViewmodel
    {
        [Required]
        [EnumDataType(typeof(WorkflowActionTypes))]
        public byte ActionType { get; set; }
        public int? CurrentStatusId { get; set; }
        public int? UpdateStatusId { get; set; }
        public int? AsmtOrTplId { get; set; }
        public byte? SendMode { get; set; }
        public byte? SendTo { get; set; }
        [MaxLength(100)]
        public string DocsReqstdIds { get; set; }
    }

    public class EditWorkflowRuleViewmodel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public short TaskId { get; set; }
        [Required]
        public string TaskCode { get; set; }
        public string TaskName { get; set; }
        [Required]
        [EnumDataType(typeof(WorkflowActionMode))]
        public byte ActionMode { get; set; }

        public List<EditWorkflowRuleDetailsViewmodel> WorkflowRuleDetailsViewmodel { get; set; }
    }

    public class EditWorkflowRuleDetailsViewmodel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [EnumDataType(typeof(WorkflowActionTypes))]
        public byte ActionType { get; set; }
        public int? CurrentStatusId { get; set; }
        public int? UpdateStatusId { get; set; }
        public int? AsmtOrTplId { get; set; }
        public byte? SendMode { get; set; }
        public byte? SendTo { get; set; }
        [MaxLength(100)]
        public string DocsReqstdIds { get; set; }
    }

    public class WorkflowRuleViewmodel
    {
        public int Id { get; set; }
        public short TaskId { get; set; }
        public string TaskName { get; set; }
        public string TaskCode { get; set; }
        public byte ActionMode { get; set; }
        public string ActionModeName { get; set; }
        public byte Status { get; set; }
        public DateTime CreatedDate { get; set; }

        public List<WorkflowRuleDetailsViewmodel> WorkflowRuleDetailsViewmodel { get; set; }
    }

    public class WorkflowRuleDetailsViewmodel
    {
        public short Id { get; set; }
        public int WorkflowId { get; set; }
        public byte ActionType { get; set; }
        public string ActionTypeName { get; set; }
        public int? CurrentStatusId { get; set; }
        public byte CurrentStatusIdIsEnable { get; set; }
        public string CurrentStatusName { get; set; }
        public int? UpdateStatusId { get; set; }
        public byte UpdateStatusIdEnable { get; set; }
        public string UpdateStatusName { get; set; }
        public int? AsmtOrTplId { get; set; }
        public string AsmtOrTplName { get; set; }
        public byte? SendMode { get; set; }
        public string SendModeName { get; set; }
        public byte? SendTo { get; set; }
        public string SendToName { get; set; }
        public byte Status { get; set; }
        public string DocsReqstdIds { get; set; }
        public string DocsReqstdNames { get; set; }
    }

    public class WorkFlowRuleSearchViewModel
    {
        public string TaskCode { get; set; }
        public int ActionMode { get; set; }
        public int? CanProfId { get; set; }
        public int? JobId { get; set; }
        public int? CurrentStatusId { get; set; }
        public int? UpdateStatusId { get; set; }
        public int UserId { get; set; }
        public string UserPassword { get; set; }
        public string UserName { get; set; }
        public int AssignTo { get; set; } // column reffering to assign job 
        public int NoOfCvs { get; set; }
        public byte[] IntentOfferContent { get; set; }
        public DateTime? DOJ { get; set; }
        public string IntentOfferRemarks { get; set; }
        public List<CVAssigntoTeamMembers> AssignTo_CV { get; set; } // column reffering to more cvs for job 
        public string RequestDocuments { get; set; }
        public int? LocationId { get; set; } // column reffering to company location
        public List<UsersViewModel> UsersViewModel { get; set; }
        public List<string> UserIds { get; set; }
        public string[] SalaryProposalOfferBenefits { get; set; }

    }

    public class WorkFlowResponse
    {
        public bool Status { get; set; }
        public List<string> Message { get; set; }
        public bool isNotification { get; set; }
        public int? JoId { get; set; }

        public List<WFNotifications> WFNotifications { get; set; }
    }

    public class WFNotifications
    {
        public string Title { get; set; }
        public string NoteDesc { get; set; }
        public int[] UserIds { get; set; }
    }


    public class WorkFlowIdsViewModel
    {
        public int Id { get; set; }
    }

    public class ChangeStatusViewModel
    {
        public string TaskCode { get; set; }
        public int ActionMode { get; set; }
        public int? JobId { get; set; }
        public int? CanProfId { get; set; }
        public int? CurrentStatusId { get; set; }
        public int? UpdatedStatusId { get; set; }
        public int UserId { get; set; }
        [MaxLength(200)]
        public string Remarks { get; set; }
    }

    public class RequestDocumentViewModel
    {
        public int ActionMode { get; set; }
        public int? JobId { get; set; }
        public int? CanProfId { get; set; }
        public int[] RequestDocuments { get; set; }
        public int UserId { get; set; }
    }

    public class SMSNotificationViewModel
    {

    }

    public class SystemAlertResponseViewModel
    {
        public int[] UserId { get; set; }
        public string NoteDesc { get; set; }
        public string Title { get; set; }
        public int? JobId { get; set; }
        public string Message { get; set; }
    }

    public class NotificationViewModel
    {
        public int? JobId { get; set; }
        public int? CanProfId { get; set; }
        public string UserPassword { get; set; }
        public string UserName { get; set; }
        public int ActionMode { get; set; }
        public int UserId { get; set; }
        public int? TemplateId { get; set; }
        public byte? SendType { get; set; }
        public byte? SendTo { get; set; }
        public string Signature { get; set; }
        public int AssignTo { get; set; }
        public int NoOfCvs { get; set; }
        public int? LocationId { get; set; }
        public byte[] IntentOfferContent { get; set; }
        public string IntentOfferRemarks { get; set; }
        public string TaskCode { get; set; }
        public DateTime? DOJ { get; set; }
        public List<UsersViewModel> UsersViewModel { get; set; }
        public List<CVAssigntoTeamMembers> CVAssigntoTeamMembers { get; set; }
        public string RequestDocuments { get; set; }
        public List<string> UserIds { get; set; }
        public string[] SalaryProposalOfferBenefits { get; set; }
    }


    public class AssessmentResponseViewmodel
    {
        public string ContactId { get; set; }
        public string DistributionId { get; set; }
        public DateTime? ResponseDate { get; set; }
        public byte? ResponseStatus { get; set; }
        public string ResponseURL { get; set; }
    }

    public class GetAssessmentRequestViewmodel
    {
        public string WhenToSend { get; set; }
        public int SurveyExpiryDays { get; set; }
    }


    public class SendAssessmentViewModel
    {
        public string SurveyID { get; set; }
        public string SurveyInstanceID { get; set; }
        public string WhenToSend { get; set; }
        public string GroupID { get; set; }
        public List<AssessmentContacts> Contacts { get; set; }
        public int SurveyExpiryDays { get; set; }
        public string EmailTemplateID { get; set; }
        public string FromName { get; set; }
        public string FromEmail { get; set; }
        public string ReplyToEmail { get; set; }
        public string Subject { get; set; }
    }

    public class AssessmentContacts
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public AssessmentAdditionalData AdditionalData { get; set; }
    }

    public class AssessmentAdditionalData
    {
        public string JOB_TITLE { get; set; }
    }

}
