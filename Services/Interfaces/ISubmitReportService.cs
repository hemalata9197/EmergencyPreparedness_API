using EmergencyManagement.Models.DTOs;
using EmergencyManagement.Models.DTOs.Common;
using EmergencyManagement.Models.DTOs.Task;

namespace EmergencyManagement.Services.Interfaces
{
    public interface ISubmitReportService
    {
        
        Task<UserInfoDto?> GetUserInfoAsync(string username);
        //Task<int> SubmitFireDrillAsync(FireDrillDto dto);
        Task<bool> UpdateFireDrillAsync(int fireDrillId, FireDrillDto dto);
        Task<bool> DeleteFireDrillAsync(int id, int empId);
        Task<int> AddRecommendationAsync(RecommendationDto dto);
        Task<int> AddTasksAsync(DrillTasksDto dto);
        Task AddtaskAssgntoUserAsync(taskAssgntoUserDto dto);
        Task<object?> GetFireDrillByIdAsync(int id);
        Task<FireDrillPagedResult> GetFireDrillsAsync(FireDrillFilterDto filter);
        Task<int> SaveFireDrillAsync(FireDrillDto dto);
        Task<getFireDrillDto> GetFireDrillsformailAsync(int fireDrillId);
        Task SaveFireDrillDocumentAsync(FireDrillDocumentDto dto);
        /* Task<List<getFireDrillDto>> GetFireDrillsAsync(); *///Dictionary<string, string?> filters
                                                               //Task<List<RecommendationDto>> GetBySubmissionIdAsync(int submissionId);
    }
}
