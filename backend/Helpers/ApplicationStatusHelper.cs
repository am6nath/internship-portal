using InternshipPortal.API.Entities;
using InternshipPortal.API.Enums;

namespace InternshipPortal.API.Helpers
{
    public static class ApplicationStatusHelper
    {
        public static string GetDisplayStatus(
            Application application,
            int preTestAttemptsUsed,
            int postTestAttemptsUsed)
        {
            if (application.IsCompleted || application.Status == ApplicationStatus.Completed)
            {
                return "Completed";
            }

            return application.Status switch
            {
                ApplicationStatus.Pending => "Pending Review",
                ApplicationStatus.Shortlisted => "Shortlisted",
                ApplicationStatus.Rejected => "Rejected",
                ApplicationStatus.Accepted when application.IsPreTestPassed => "Ongoing",
                ApplicationStatus.Accepted when preTestAttemptsUsed >= 1 && !application.IsPreTestPassed =>
                    "Pre-Test Failed",
                ApplicationStatus.Accepted => "Accepted — Pre-Test Required",
                ApplicationStatus.InProgress when application.IsTestPassed => "Post-Test Passed",
                ApplicationStatus.InProgress when postTestAttemptsUsed >= 1 && !application.IsTestPassed =>
                    "Post-Test Failed — Retake Available",
                ApplicationStatus.InProgress => "Ongoing — Post-Test Required",
                _ => application.Status.ToString()
            };
        }

        public static string GetDisplayPhase(Application application, int preTestAttemptsUsed)
        {
            if (application.IsCompleted || application.Status == ApplicationStatus.Completed)
            {
                return "Completed";
            }

            if (application.Status == ApplicationStatus.Rejected)
            {
                return "Rejected";
            }

            if (application.Status is ApplicationStatus.Pending or ApplicationStatus.Shortlisted)
            {
                return "Applied";
            }

            if (application.Status == ApplicationStatus.Accepted && !application.IsPreTestPassed)
            {
                return preTestAttemptsUsed >= 1 ? "PreTestFailed" : "PreTest";
            }

            if (application.IsPreTestPassed && !application.IsTestPassed)
            {
                return application.Status == ApplicationStatus.InProgress ? "Ongoing" : "PreTest";
            }

            if (application.IsTestPassed && !application.IsCompleted)
            {
                return "PostTest";
            }

            return "Applied";
        }

        public static string GetPreTestStatus(Application application, int preTestAttemptsUsed)
        {
            if (application.Status is ApplicationStatus.Pending
                or ApplicationStatus.Shortlisted
                or ApplicationStatus.Rejected)
            {
                return "NotApplicable";
            }

            if (application.IsPreTestPassed)
            {
                return "Passed";
            }

            if (preTestAttemptsUsed >= 1)
            {
                return "Failed";
            }

            if (application.Status == ApplicationStatus.Accepted)
            {
                return "Pending";
            }

            return "NotApplicable";
        }

        public static string GetPostTestStatus(
            Application application,
            int postTestAttemptsUsed)
        {
            if (!application.IsPreTestPassed)
            {
                return "Locked";
            }

            if (application.IsTestPassed)
            {
                return "Passed";
            }

            if (application.Status == ApplicationStatus.InProgress && postTestAttemptsUsed >= 1)
            {
                return "Failed";
            }

            if (application.Status == ApplicationStatus.InProgress)
            {
                return "Pending";
            }

            if (application.IsCompleted)
            {
                return application.IsTestPassed ? "Passed" : "NotApplicable";
            }

            return "Locked";
        }

        public static decimal? GetOverallAssessmentScore(Application application)
        {
            if (!application.IsPreTestPassed && !application.IsTestPassed)
            {
                return null;
            }

            if (application.IsPreTestPassed && application.IsTestPassed)
            {
                return Math.Round(
                    ((application.PreTestScore ?? 0) + (application.TestScore ?? 0)) / 2,
                    2);
            }

            return application.IsPreTestPassed
                ? application.PreTestScore
                : application.TestScore;
        }
    }
}
