using System.Collections.Generic;

namespace PiHire.BAL.Common.Types
{
    public static class AppConstants
    {
        internal const string AES128Key = "P@ramInf0@549432";

        public enum RecordStatus
        {
            Inactive = 0,
            Active = 1,
            Edit = 2,
            InProgress = 3,
            Unsubscribe = 4,
            Delete = 5
        }

        public enum AuthenticateStatus
        {
            Success = 1,
            Failed = 2,
            AlreadyLogin = 3
        }

        public enum NotificationStatus
        {
            unRead = 0,
            read = 1
        }

        public enum WorkflowActionTypes
        {
            ChangeStatus = 1,
            EmailNotification = 2,
            SMSNotification = 3,
            SystemAlert = 4,
            SendAssessment = 5,
            RequestDocuments = 6
        }

        public enum SourceType
        {
            Website = 1,
            Naukri = 2,
            MonsterIndia = 3,
            Google = 4,
            CandidatePortal = 5,
            Facebook = 6,
            LinkedIn = 7,
            Indeed = 8,
            MonsterGulf = 9,
            References = 10,
            NaukriGulf = 11,
            Shine = 12,
            PersonalContact = 13
        }

        public enum TabNamesPerColumnWisePerformanceeReport
        {
            Account = 1,
            Opening = 2,
            BDM = 3,
            Recruiter = 4,
            Candidate = 5,
            CurrentStatus = 6,
            UpdateStatus = 7,
            Date = 8
        }

        public enum ResiType
        {
            Rented = 1,
            Owned = 2,
            Others = 3
        }

        public enum SocialProfileType
        {
            Website = 1,
            LinkedIn = 2,
            GitHub = 3
        }

        public enum WorkflowActionMode
        {
            Other = 10,
            Candidate = 1,
            Opening = 2
        }

        public enum SendMode
        {
            All = 10,
            Owner = 1
        }

        public enum DisplayFlag
        {
            Mandatory = 1,
            Optional = 2,
            Hide = 3
        }

        public enum JobFilterType
        {
            New = 2,
            Overdue = 3,
            OnHold = 4,
            Closed = 5,
            MoreCvsRequired = 6,
            ReOpen = 7,
            Submit = 8,
            MyJobs = 9,
            AllJobs = 10
        }

        public enum JobStatusCodes
        {
            OPN = 1,
            WIP = 2,
            REW = 3,
            SUB = 4,
            MCV = 5,
            HLD = 6,
            CLS = 7,
            RJT = 8,
            RPN = 9,
            SUC = 10,
            ACT = 12,
            NEW = 13
        }

        public enum CandidateStatusCodes
        {
            SCD = 1,
            CCV = 2,
            BCV = 3,
            FCV = 4,
            PIR = 5,
            CSB = 6,
            IRT = 7,
            CL1 = 8,
            CL2 = 9,
            CL3 = 10,
            CFF = 11,
            CIB = 12,
            CRT = 13,
            CRD = 14,
            PFF = 15,
            OFR = 16,
            OFA = 17,
            JDE = 18,
            SUC = 19,
            JBO = 20,
            BLT = 21,
            VPD = 22,
            RRT = 23,
            CHD = 24,
            CBT = 25,
            AFC = 26,
            FFC = 27,
            ADC = 28,
            TI1 = 29,
            FFI = 30,
            SRT = 31,
            HRD = 32,
            RTD = 33,
            OHD = 34,
            RRD = 35,
            DOR = 36,
            CJS = 37,
            COA = 38,
            PNS = 39,
            DPO = 40,
            SAE = 41,
            RCD = 42,
            SVD = 43,
            YSP = 44,
            CFD = 45,
            CSN = 46,
            BGA = 47,
            VPR = 48,
            CDB = 49,
            WFO = 50,
            HRR = 51,
            ORD = 52,
            EST = 53,
            OPS = 54,
            JND = 55,
            EXT = 56,
            NSB = 57,
            IHD = 58,
            ROF = 59,
            RFCS = 60,
            IOL = 61,
            OBD = 62,
            BSD = 63,
            BRD = 64,
            CTE = 65,  // Converted to Employee
            CLSL = 66, // Cl Shortlisted
            RCR = 67   // Re-Consider            
        }

        public enum TaskCode
        {
            JOP = 1, //	Job Opening
            CAD = 2, //	Candidate
            EAC = 3, //	Email & Calendar
            CDE = 4, //	Company Details
            UPF = 5, //	User Profile
            USR = 6, //	Users
            ETP = 7, //	Email Templates
            STP = 8, //	SMS Templates
            SMG = 9, //	Skillset Menagement
            JTP = 10, //	Job Templates
            APL = 11, //	Define Pipeline
            WFR = 12, //	Workflow Rules
            JBD = 13, //	Job Board
            CMN = 14, //	Comunication
            MKT = 15, //	Marketing
            RPT = 16, //	Reports
            DSB = 17, //	Dashboard
            ANL = 18, //	Analytics
            FBC = 19, //	FeedBack
            PPL = 20, //	Pipeline
            TEM = 21, //	Team
            SCD = 22, //	Share Candidate
            ASM = 23, //	Add Assessment
            EVL = 24, //	Evaluation
            USU = 25, //	Job Update Status
            TJB = 26, //	Tag Job
            RDC = 27, //	Request Documents
            SLP = 28, //	Salary Proposal
            FUP = 29, //	File uploads
            FAP = 30, //	File Approvals
            SNT = 31, //	Send Note
            RTN = 32, //	Reply to Notes
            DLN = 33, //	Delete a Note
            CAA = 34, //	Add Assessment
            CUS = 35, //	Candidate Update Status
            CAJ = 36, //	Apply Job
            NSU = 37, //	Not Suitable
            RCR = 38, //	Re Consider
            SVR = 39, //	Salary Verification
            SRJ = 40, //	Salary Rejection
            SAC = 41, //	Salary Acceptance
            FCV = 42, //	Final CV
            RAR = 43, //	Receive Assement Response
            CSB = 44, //	Client Submission
            SCI = 45, //	Schedule Client Interview
            CFF = 46, //	Schedule Client F2F Interview
            CIB = 47, //	Client Interview Backout
            RDU = 48, //	Result Due Update
            CRU = 49, //	Client Rejection Update
            CSU = 50, //	Client Selection Update 
            SBG = 51, //	Send BGV
            SUBG = 52, //	Submit BGV
            ABG = 53, //	Accept BGV
            BGC = 54, //	BGV Clarification
            PFF = 55, //	PI F2F Interview
            RIO = 56,  //	Release Intenet Offer
            CBU = 57,  //	Candidate Backout Update
            HDU = 58,  //	Hold Update
            AGJ = 59,  //	Assign Job
            CCD = 60,  //	Create Candidate
            CJB = 61,  //	Create Job
            CLJB = 62, //	Close Job
            REJB = 63, //	Reopen Job
            CSLD = 64, //	Client Shortlisted
            HJB = 65,  //	Hold Job
            BLC = 66,  //	BlackList Candidate
            COF = 67,  //   Candidate Offers
            MCV = 68,  //   More cv 
            ROF = 69,  //   Re Offer 
            CST = 70,  //   Tentative Date for Interview
            RSI = 71   //   ReSchedule Interview
        }

        public enum WorkFlowTaskCode
        {
            CAD = 1, //	    Create Candidate
            USU = 2, //	Job Update Status
            CUS = 3, //	Candidate Update Status
            CAJ = 4, //	Apply Job
            NSU = 5, //	Not Suitable
            RCR = 6, //	Re Consider
            SVR = 7, //	Salary Verification
            SRJ = 8, //	Salary Rejection
            SAC = 9, //	Salary Acceptance
            FCV = 10, //	Final CV
            RAR = 11, //	Receive Assement Response
            CSB = 12, //	Client Submission
            SCI = 13, //	Schedule Client Interview
            CFF = 14, //	Schedule Client F2F Interview
            CIB = 15, //	Client Interview Backout
            RDU = 16, //	Result Due Update
            CRU = 17, //	Client Rejection Update
            CSU = 18, //	Client Selection Update 
            SBG = 19, //	Send BGV
            SUBG = 20, //	Submit BGV
            ABG = 21, //	Accept BGV
            BGC = 22, //	BGV Clarification
            PFF = 23, //	PI F2F Interview
            RIO = 24, //	Release Intenet Offer
            CBU = 25, //	Candidate Backout 
            HDU = 26, //	Hold Update
            AGJ = 27, //	Assign Job
            CCD = 28, //	Create Candidate
            CJB = 29, //	Create Job
            CLJB = 30, //	Close Job
            REJB = 31, //	Reopen Job
            CSLD = 32, //	Client Shortlisted
            HJB = 33, //	Hold Job
            BLC = 34,  //	BlackList Candidate
            TJB = 35,  //	Tag Job
            SLP = 36,  //	Salary Proposal
            RDC = 37,  //	Request Documents
            MVC = 38,  //    More cv 
            ROF = 39,  //    Re Offer 
            SNT = 40,  //    Send Note         
            CST = 41,  //     Tentative Date for Interview
            RSI = 42
        }

        public enum CandOverallStatus
        {
            Available = 1,
            Joined = 2,
            Blacklisted = 3
        }

        public enum FileGroup
        {
            Profile = 1,
            Education = 2,
            Employment = 3,
            Other = 5
        }

        public enum Gender
        {
            Male = 1,
            Female = 2,
            Other = 3
        }

        public enum ActivityOn
        {
            Candidate = 1,
            Opening = 2,
            TeamMemerId = 3
        }

        public enum AuditActivityType
        {
            Other = 1,
            RecordUpdates = 2,
            StatusUpdates = 3,
            Authentication = 4,// Authentication/Authorization
            Critical = 5 //Delete/Remove updates
        }

        public enum LogActivityType
        {
            Other = 1,
            RecordUpdates = 2,
            StatusUpdates = 3,
            AssessementUpdates = 4,
            Critical = 5,
            ScheduleInterviewUpdates = 6,
            JobEditUpdates = 7
        }

        public enum AuditMessagesIds
        {
            Created = 1,
            Edited = 2,
            Get = 3,
            Delete = 4,
            List = 5
        }

        public enum ActivityMessagesIds
        {
            Created = 1,
            Edited = 2,
            Get = 3,
            Delete = 4,
            List = 5
        }

        public enum IntegrationCategory
        {
            JobBoards = 1,
            Communications = 2,
            Marketing = 3,
            Calenders = 4
        }

        public enum IntegrationCalendersCategory
        {
            Office365 = 1,
            GoogleMeet = 2
        }


        public enum QtyOrPeriodFlag
        {
            Qty = 1,
            Period = 2
        }

        public enum ResponseStatus
        {
            Nottakenyet = 0,
            Interrupted = 2,
            AssessmentStared = 1,
            AssessmentTaken = 3
        }

        public enum DocStatus
        {
            Notreviewd = 0,
            Accepted = 1,
            Rejected = 5,
            Requested = 7
        }

        public enum MessageType
        {
            Email = 1, // email templates
            SMS = 2,
            Notifications = 3, // message templates
            JobTemplates = 10 // job templates
        }

        public enum SentBy
        {
            System = 1,
            Team = 2
        }

        public enum UserType
        {
            SuperAdmin = 1,
            Admin = 2,
            BDM = 3,
            Recruiter = 4,
            Candidate = 5,
            //HR = 6,
            //Account = 7,
            ApiUser = 8
        }

        public enum ProfileType
        {
            Candidate = 1,
            Openings = 2,
            Interview = 3,
            StatusUpdates = 4,
            Clients = 5,
            Internal = 6,
            Others = 10
        }

        public enum ClreviewStatus
        {
            NotReviewd = 0,
            Reviewed = 1,
            Selected = 3,
            Rejected = 5
        }

        public enum InterviewStatus
        {
            Notscheduled = 0,
            Active = 1,
            Completed = 3,
            Rescheduled = 2,
            Rejected = 5,
            Canceled = 6
        }

        public enum ModeOfInterview
        {
            Telephonic = 1,
            Onsite = 2,
            F2F = 3,
            Google = 4,
            Microsoft = 5,
            PiF2F = 6
        }

        public enum ScheduledBy
        {
            PiTeam = 1,
            Client = 2
        }

        public enum ReadStatus
        {
            Unread = 0,
            Read = 1
        }

        public enum ContactDistributionResponseStatus
        {
            NotStarted = 0,
            Started = 1,
            Interepted = 2,
            Completed = 3
        }

        public enum FileType
        {
            Video = 1,
            Audio = 2,
            File = 3
        }

        public enum EmailFields
        {
            JobName = 2,
            Email = 3,
            MobileNumber = 4,
            NoticePeriod = 5,
            Location = 6,
            Experience = 7,
            Budget = 8
        }

        public enum ConfigureView
        {
            Resume = 1,
            ContactNumber = 2,
            VideoProfile = 3,
            Evaluation = 4,
            Assessments = 5,
            ResumeAttachment = 6
        }

        public enum BGCompStatus
        {
            PersonalDetails = 1,
            Familydetails = 2,
            Address = 3,
            Employmentdetails = 4,
            Educationdetails = 5,
            Completed = 10
        }

        public enum LeaveStatus
        {
            Requested = 1,
            Accepted = 2,
            Rejected = 3,
            Canceled = 5,
        }

    }
    public enum CustomSchedulerEventTypes
    {
        /// <summary>
        /// New Job Published
        /// </summary>
        NJ,
        /// <summary>
        /// Birthday
        /// </summary>
        BD,
        /// <summary>
        /// Special Day
        /// </summary>
        SD,
        /// <summary>
        /// Event is happened
        /// </summary>
        EH
    }
    public enum CustomSchedulerSendTo
    {
        AllUsers = 1,
        piHireUsers = 2,
        piHireCandidates = 3
    }
    public enum CustomSchedulerGender
    {
        /// <summary>
        /// All Users
        /// </summary>
        A,
        /// <summary>
        /// Male
        /// </summary>
        M,
        /// <summary>
        /// Female
        /// </summary>
        F
    }
    public enum CustomSchedulerDtlsExecutionStatus
    {
        NotStarted = 0,
        SentSuccessfully = 1,
        Failed = 3
    }
    public enum CustomSchedulerFrequency
    {
        Daily = 1,
        DateAndTime = 2
    }

    public enum RecruiterJobAssignmentLogType
    {
        AutoPendingCvForwardFrom = 1,
        AutoPendingCvForwardTo = 2,

        ManualCvIncrement = 3,
        ManualCvDecrement = 4,
    }

    public enum JobAssignBy
    {
        System = 0,
        Manual = 1,
    }
    public enum ReferenceGroupType
    {
        OpeningType = 59,
        JobDomain = 197,
        JobTeamRole = 198,
        JobWorkPattern = 199,
        //JobCandidateValidPassport ,
        //JobCandidateDOB,
        JobCandidateGender = 55,
        JobCandidateMaritalStatus = 23,
        JobCandidateLanguagePreference = 200,
        JobCandidateVisaPreference = 201,// = 43
        JobCandidateRegion = 202,
        //JobCandidateNationality,
        //JobCandidateResidingCountry,
        //JobCandidateResidingCity,
        JobCandidateDrivingLicense = 203,
        //JobCandidateEmployeeStatus,
        //JobCandidateResume,
        //JobCandidateVideoProfile,
        //JobCandidatePaySlip,
        //JobCandidateServingNoticePeriod

        CandidateEducationQualification_Graduation = 92,
        CandidateEducationQualification_Course = 189,
        CandidateEducationCertification = 190,//204
        JobQualification = 92,//205
        JobCertification = 190,//206
        JobTenure = 207
    }
    public enum CandidateEmployeeStatus
    {
        ServingNoticePeriod = 1,
        FreeLancerOrImmediateJoiner = 2,
        CurrenltyWorking = 3
    }
    public enum JobOpeningPreferenceTypes
    {
        Desirable = 1,
        Essential = 2
    }
    public enum EducationQualificationGroup
    {
        Graduation = 1,
        GraduationSpecialization = 2
    }
    public enum JobDesirableSkillGroupTypes
    {
        Specializations = 1,
        Implementations = 2,
        Designs = 3,
        Developments = 4,
        Supports = 5,
        Qualities = 6,
        Documentations = 7
    }
    public enum JobCandQualCertGroupTypes
    {
        JobCandiateEducationQualification = 1,
        JobCandidateEducationCertifications = 2,
        JobOpeningQualification = 3,
        JobOpeningCertification = 4
    }
}
