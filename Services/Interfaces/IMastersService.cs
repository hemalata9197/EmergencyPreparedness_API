using EmergencyManagement.Models.DTOs;
using EmergencyManagement.Models.DTOs.Common;
using EmergencyManagement.Models.DTOs.Master;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EmergencyManagement.Services.Interfaces
{
    public interface IMastersService
    {
        Task<List<DropdownOptionDto>> GetDropdownOptionsAsync(string source, int currentUnitId);
        //Task<List<getFacilityMasterDto>> GetFacilityAreasAsync(int UnitId);
        //Task<List<getFacilityMasterDto>> GetFacilitySectionsAsync(int UnitId);
        //Task<List<getScenarioMasterDto>> GetScenariosAsync(int UnitId);
        //Task<List<getRolesMasterDto>> GetRolesAsync(int UnitId);
        //Task<List<getDesigMasterDto>> GetDesignationsAsync(int UnitId);
        //Task<List<getEmployeeMasterDto>> GetEmployeesAsync(int UnitId);
        Task<PagedResult<getUnitsMasterDto>> GetUnitsAsync(MasterFilterDto filter);
        Task<PagedResult<getFacilityMasterDto>> GetFacilityAreasAsync(MasterFilterDto filter);
        Task<PagedResult<getFacilityMasterDto>> GetFacilitySectionsAsync(MasterFilterDto filter);
        Task<PagedResult<getScenarioMasterDto>> GetScenariosAsync(MasterFilterDto filter);
        Task<PagedResult<getRolesMasterDto>> GetRolesAsync(MasterFilterDto filter);
        Task<PagedResult<getDesigMasterDto>> GetDesignationsAsync(MasterFilterDto filter);
        Task<PagedResult<getEmployeeMasterDto>> GetEmployeesAsync(MasterFilterDto filter);
        Task<OperationResultDto> CreateAsync(string source, JsonElement data);
        Task<OperationResultDto> UpdateAsync(string source, int id, JsonElement data);
        Task<bool> UpdateStatusAsync(StatusUpdateDto dto);
 

        //Task<List<GetMasterDto>> GetAllMasterFieldsAsync(string source, int unitId);
        Task<string> CheckHODEHSAsync(int configElementId, int unitId);
        Task<List<DropdownOptionDto>> GetMenusAsync();
        Task AssignMenusToRoleAsync(RoleMenuAssignmentDto dto);
        Task<Dictionary<string, PaginatedResult<PendingTaskDto>>> GetFiredrillDashboardAsync(PendingTaskFilterDto dto);
    }
}
