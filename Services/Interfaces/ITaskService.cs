using EmergencyManagement.Models.DTOs;
using EmergencyManagement.Models.DTOs.Task;
using EmergencyManagement.Models.Entities.Task;
using TaskStatus = EmergencyManagement.Models.Entities.Task.TaskStatus;

namespace EmergencyManagement.Services.Interfaces
{
    public interface ITaskService
    {
        Task<TaskPagedResult> GetAllTaskAsync(TaskFilterDto filter);
        Task<bool> UpdateTaskSatusAsync(UpdateTaskStatusDto dto);
        Task<List<TaskStatus>> GetFilteredTaskStatusAsync(string taskstatusFor);
        Task<List<ApprovalStatus>> GetFilteredApprovalStatusAsync();
        Task<bool> UpdateTaskApprovalAsync(TaskApprovalDto dto);
        Task<List<getTaskDto>> GetAllTaskForMailAsync(int fireDrillId);
        Task<getTaskDto> GetAllTaskByTaskIdForMailAsync(int TaskId);
        Task<getTaskDto> GetAllTaskDeatilsAfterStatusUpdationByTaskIdForMailAsync(int TaskId);

        Task<List<TaskHistoryDto>> getTaskHistoryAsync(int taskId);


    }
}
