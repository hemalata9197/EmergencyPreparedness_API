using EmergencyManagement.Models.DTOs.Common;
using Microsoft.AspNetCore.Identity.Data;

namespace EmergencyManagement.Services.Interfaces
{
    public interface IAuthService
    {
       Task<AuthResponseDto> AuthenticateAsync(LoginDto request);
        Task<UserInfoDto?> GetUserInfoAsync(string username);
        Task<object?> GetUnitsByEmployeeIdAsync(int EmpId);
    }
}
