using EmergencyManagement.Models.DTOs.Common;

namespace EmergencyManagement.Services.Interfaces
{
    public interface IEmailService
    {
         Task SendMailFireDrillAfterSubmissionAsync(int fireDrillId, int UnitId);
        Task SendMailFireDrillDeletedAsync(int fireDrillId, int EmpId);
        Task SendMailFireDrillAfterReleasedAsync(int fireDrillId, int TaskId, int UnitId);
        Task SendMailOnFireDrillTaskStatusUpdationAsync(int fireDrillId, int taskId);
        Task SendMailOnFireDrillTaskApprovalAsync(int fireDrillId, int taskId);
        Task<bool> SendMailForgotPasswordAsync(ForgotPasswordDto request);
    }
}
