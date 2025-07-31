using EmergencyManagement.Data;
using EmergencyManagement.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using EmergencyManagement.Models.Entities;
using EmergencyManagement.Models.Entities.Admin;
using EmergencyManagement.Services.Interfaces;
using EmergencyManagement.Models.DTOs.Master;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
//using static EmergencyManagement.Utilities.DatabaseUtility;
using EmergencyManagement.Utilities;
using EmergencyManagement.Models.DTOs.Common;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace EmergencyManagement.Services.Implementations
{
    public class MastersService : IMastersService
    {
        private readonly EHSDbContext _context;

        public MastersService(EHSDbContext context)
        {
            _context = context;

        }


        public async Task<List<DropdownOptionDto>> GetDropdownOptionsAsync(string source, int unitId)
        {
            switch (source)
            {
                case "UnitsMaster":
                    return await _context.UnitsMaster
                        .Where(f => f.IsActive)
                         .OrderBy(f => f.UnitId)
                        .Select(f => new DropdownOptionDto { Id = f.UnitId, Label = f.UnitName })
                        .ToListAsync();
                case "FacilityMaster-Area":
                    return await _context.FacilityMasters
                        .Where(f => f.ParentId == null && f.IsActive && (unitId == 0 || f.UnitId == unitId))
                         .OrderBy(f => f.Id)
                        .Select(f => new DropdownOptionDto { Id = f.Id, Label = f.Name })
                        .ToListAsync();

                case "FacilityMaster-Section":
                    return await _context.FacilityMasters
                        .Where(f => f.ParentId != null && f.IsActive && (unitId == 0 || f.UnitId == unitId))
                         .OrderBy(f => f.Id)
                        .Select(f => new DropdownOptionDto { Id = f.Id, Label = f.Name, ParentId = f.ParentId })
                        .ToListAsync();

                case "ScenarioMaster":
                    return await _context.ScenarioMasters
                         .Where(s => s.IsActive && (unitId == 0 || s.UnitId == unitId))
                          .OrderBy(s => s.Id)
                        .Select(s => new DropdownOptionDto { Id = s.Id, Label = s.ScenarioName })
                        .ToListAsync();

                case "Employees":
                    return await _context.Employees
                      .Where(e => e.IsActive && (unitId == 0 || e.UnitId == unitId))
                       .OrderBy(e => e.Id)
                        .Select(e => new DropdownOptionDto { Id = e.Id, Label = e.FullName })
                        .ToListAsync();
                case "Roles":
                    return await _context.Roles
                          .Where(r => r.IsActive && (unitId == 0 || r.UnitId == unitId))
                           .OrderBy(r => r.Id)
                        .Select(r => new DropdownOptionDto { Id = r.Id, Label = r.Name })
                        .ToListAsync();
                case "Designation":
                    return await _context.Designations
                           .Where(d => d.IsActive && (unitId == 0 || d.UnitId == unitId))
                            .OrderBy(d => d.Id)
                        .Select(d => new DropdownOptionDto { Id = d.Id, Label = d.Name })
                        .ToListAsync();
                case "Users":
                    return await _context.Employees
                      .Where(e => e.IsActive && e.UserId != 0 && (unitId == 0 || e.UnitId == unitId))
                       .OrderBy(e => e.Id)
                        .Select(e => new DropdownOptionDto { Id = e.Id, Label = e.FullName })
                        .ToListAsync();
                case "SeverityMaster":
                    return await _context.SeverityMaster
                      .Where(s => s.IsActive)
                       .OrderBy(s => s.Id)
                        .Select(s => new DropdownOptionDto { Id = s.Id, Label = s.Level })
                        .ToListAsync();

                default:
                    return new List<DropdownOptionDto>();
            }
        }
        
        public async Task<PagedResult<getUnitsMasterDto>> GetUnitsAsync(MasterFilterDto filter)
        {
            var query = from u in _context.UnitsMaster
                        join emp in _context.Employees on u.UnitHead equals emp.Id into empJoin
                        from emp in empJoin.DefaultIfEmpty()
                        where u.IsActive == true
                        orderby u.UnitId
                        select new getUnitsMasterDto
                        {
                            Id = u.UnitId,
                            Label = u.UnitName,
                            UnitHeadId = emp.Id,
                            UnitHeadName = emp.FullName,
                            isActive = u.IsActive,
                            isTaskAssigned = (from e in _context.Employees
                                              where e.UnitId == u.UnitId && e.IsActive
                                              select 1).Any()
                        };

            var total = await query.CountAsync();
            var data = await query.Skip(filter.Page * filter.Size).Take(filter.Size).ToListAsync();

            return new PagedResult<getUnitsMasterDto> { Data = data, TotalCount = total };
        }

        public async Task<PagedResult<getFacilityMasterDto>> GetFacilityAreasAsync(MasterFilterDto filter)
        {
            var query = from f in _context.FacilityMasters
                        join emp in _context.Employees on f.FacilityHead equals emp.Id into empJoin
                        from emp in empJoin.DefaultIfEmpty()
                        where f.ParentId == null && (filter.UnitId == 0 || f.UnitId == filter.UnitId)
                        orderby f.Id
                        select new getFacilityMasterDto
                        {
                            Id = f.Id,
                            Label = f.Name,
                            FacilityHeadId = emp.Id,
                            FacilityHeadName = emp.FullName,
                            isActive = f.IsActive,
                            isTaskAssigned = (from e in _context.Employees
                                              where e.DeptId == f.Id && e.IsActive
                                              select 1).Any()
                        };

            var total = await query.CountAsync();
            var data = await query.Skip(filter.Page * filter.Size).Take(filter.Size).ToListAsync();

            return new PagedResult<getFacilityMasterDto> { Data = data, TotalCount = total };
        }

        public async Task<PagedResult<getFacilityMasterDto>> GetFacilitySectionsAsync(MasterFilterDto filter)
        {
            var query = from f in _context.FacilityMasters
                        join parent in _context.FacilityMasters on f.ParentId equals parent.Id
                        join emp in _context.Employees on f.FacilityHead equals emp.Id into empJoin
                        from emp in empJoin.DefaultIfEmpty()
                        where f.ParentId != null && (filter.UnitId == 0 || f.UnitId == filter.UnitId)
                        orderby f.Id
                        select new getFacilityMasterDto
                        {
                            Id = f.Id,
                            Label = f.Name,
                            ParentId = f.ParentId ?? 0,
                            ParentName = parent.Name,
                            FacilityHeadId = emp.Id,
                            FacilityHeadName = emp.FullName,
                            isActive = f.IsActive,
                            isTaskAssigned = (from e in _context.Employees
                                              where e.DeptId == f.Id && e.IsActive
                                              select 1).Any()
                        };

            var total = await query.CountAsync();
            var data = await query.Skip(filter.Page * filter.Size).Take(filter.Size).ToListAsync();

            return new PagedResult<getFacilityMasterDto> { Data = data, TotalCount = total };
        }

        public async Task<PagedResult<getScenarioMasterDto>> GetScenariosAsync(MasterFilterDto filter)
        {
            var query = _context.ScenarioMasters
                .Where(s => filter.UnitId == 0 || s.UnitId == filter.UnitId)
                 .OrderBy(s => s.Id)
                .Select(s => new getScenarioMasterDto
                {
                    Id = s.Id,
                    Label = s.ScenarioName,
                    Description = s.Description,
                    isActive = s.IsActive,

                });

            var total = await query.CountAsync();
            var data = await query.Skip(filter.Page * filter.Size).Take(filter.Size).ToListAsync();

            return new PagedResult<getScenarioMasterDto> { Data = data, TotalCount = total };
        }

        public async Task<PagedResult<getRolesMasterDto>> GetRolesAsync(MasterFilterDto filter)
        {
            var query = _context.Roles
                .Where(r => filter.UnitId == 0 || r.UnitId == filter.UnitId)
                .OrderBy(r => r.Id)
                .Select(r => new getRolesMasterDto
                {
                    Id = r.Id,
                    Label = r.Name,
                    isActive = r.IsActive,
                    isTaskAssigned = (from e in _context.Users
                                      where e.RoleId == r.Id && e.IsActive
                                      select 1).Any()
                });

            var total = await query.CountAsync();
            var data = await query.Skip(filter.Page * filter.Size).Take(filter.Size).ToListAsync();

            return new PagedResult<getRolesMasterDto> { Data = data, TotalCount = total };
        }

        public async Task<PagedResult<getDesigMasterDto>> GetDesignationsAsync(MasterFilterDto filter)
        {
            var query = _context.Designations
                .Where(d => filter.UnitId == 0 || d.UnitId == filter.UnitId)
                 .OrderBy(d => d.Id)
                .Select(d => new getDesigMasterDto
                {
                    Id = d.Id,
                    Label = d.Name,
                    isActive = d.IsActive,
                    isTaskAssigned = (from e in _context.Employees
                                      where e.Designation == d.Id && e.IsActive
                                      select 1).Any()
                });

            var total = await query.CountAsync();
            var data = await query.Skip(filter.Page * filter.Size).Take(filter.Size).ToListAsync();

            return new PagedResult<getDesigMasterDto> { Data = data, TotalCount = total };
        }

        public async Task<PagedResult<getEmployeeMasterDto>> GetEmployeesAsync(MasterFilterDto filter)
        {
            var query = (from e in _context.Employees
                         join f in _context.FacilityMasters on e.DeptId equals f.Id into fJoin
                         from f in fJoin.DefaultIfEmpty()
                         join d in _context.Designations on e.Designation equals d.Id into dJoin
                         from d in dJoin.DefaultIfEmpty()
                         join u in _context.Users on e.UserId equals u.Id into uJoin
                         from u in uJoin.DefaultIfEmpty()
                         join r in _context.Roles on u.RoleId equals r.Id into rJoin
                         from r in rJoin.DefaultIfEmpty()
                         where filter.UnitId == 0 || e.UnitId == filter.UnitId
                         orderby e.Id
                         select new getEmployeeMasterDto
                         {
                             Id = e.Id,
                             EmployeeCode = e.EmployeeCode,
                             Label = e.FullName,
                             MobileNumber = e.MobileNumber,
                             Email = e.Email,
                             areaId = f != null ? f.Id : 0,
                             areaName = f != null ? f.Name : null,

                             desigId = d != null ? d.Id : 0,
                             designationName = d != null ? d.Name : null,

                             RoleId = r != null ? r.Id : 0,
                             Role = r != null ? r.Name : null,
                             isActive = e.IsActive,
                             isDeleted = e.IsDeleted,
                             isTaskAssigned = (from t in _context.taskAssgntoUsers
                                               where t.EmployeeId == e.Id && t.IsActive
                                               select 1).Any()
                         }).AsNoTracking();

            var total = await query.CountAsync();


            var data = await query.Skip(filter.Page * filter.Size).Take(filter.Size).ToListAsync();

            return new PagedResult<getEmployeeMasterDto> { Data = data, TotalCount = total };
        }

        public async Task<OperationResultDto> CreateAsync(string source, JsonElement data)
        {
            try
            {
                switch (source)
                {
                    case "UnitsMaster":
                        //var areaDto = JsonSerializer.Deserialize<AreaMasterDto>(data);
                        var UnitDto = JsonSerializer.Deserialize<UnitsMasterDeto>(data, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (UnitDto == null) return new OperationResultDto { Success = false, Message = "Invalid data" };
                        if (!Regex.IsMatch(UnitDto.Label, @"^[a-zA-Z0-9\s-_]+$"))
                        {
                            return new OperationResultDto
                            {
                                Success = false,
                                Message = "Invalid characters in Unit Name. Only letters, numbers, spaces, hyphen and underscore are allowed."
                            };
                        }

                        var Unit = new UnitsMaster
                        {
                            UnitName = UnitDto.Label,
                            UnitHead = 1,
                            CreatedBy = UnitDto.CreatedBy,
                            CreatedOn = DateTime.UtcNow,

                        };
                        _context.UnitsMaster.Add(Unit);
                        await _context.SaveChangesAsync();
                        var newrole = new Role
                        {
                            Name = "Admin",
                            CreatedBy = UnitDto.CreatedBy,
                            UnitId = Unit.UnitId,
                            CreatedOn = DateTime.UtcNow,
                        };
                        _context.Roles.Add(newrole);
                        await _context.SaveChangesAsync();

                        var newuser = new Users
                        {
                            RoleId = newrole.Id,
                            Username = Unit.UnitName + "Admin",
                            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Unit.UnitName + "Admin"),
                            IsActive = true
                        };

                        _context.Users.Add(newuser);
                        await _context.SaveChangesAsync();
                        var newemployee = new Employees
                        {
                            EmployeeCode = Unit.UnitName + "Admin",
                            UserId = newuser.Id,
                            FullName = Unit.UnitName + "Admin",
                            Email = "",
                            DeptId = 0,
                            Designation =0,
                            CreatedBy = UnitDto.CreatedBy,
                            UnitId = Unit.UnitId,
                            CreatedOn = DateTime.UtcNow,
                        };

                        _context.Employees.Add(newemployee);
                        await _context.SaveChangesAsync();

                        var menuIds = await _context.Menus
       .Where(m => m.Route != null && m.IsActive)
       .Select(m => m.Id)
       .ToListAsync();

                        var roleMenus = menuIds.Select(menuId => new RoleMenu
                        {
                            RoleId = newrole.Id,
                            MenuId = menuId,
                            SubmittedBy = UnitDto.CreatedBy,
                            SubmittedOn = DateTime.UtcNow,
                            IsActive = true,
                            UnitId = Unit.UnitId
                        }).ToList();

                        await _context.RoleMenus.AddRangeAsync(roleMenus);
                      
                        await _context.SaveChangesAsync();



                        break;
                    case "FacilityMaster-Area":
                        //var areaDto = JsonSerializer.Deserialize<AreaMasterDto>(data);
                        var areaDto = JsonSerializer.Deserialize<AreaMasterDto>(data, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (areaDto == null) return new OperationResultDto { Success = false, Message = "Invalid data" };
                        if (!Regex.IsMatch(areaDto.Label, @"^[a-zA-Z0-9\s-_]+$"))
                        {
                            return new OperationResultDto
                            {
                                Success = false,
                                Message = "Invalid characters in Area Name. Only letters, numbers, spaces, hyphen and underscore are allowed."
                            };
                        }
                        var area = new FacilityMaster
                        {
                            Name = areaDto.Label,
                            FacilityHead = areaDto.FacilityHeadId,
                            CreatedBy = areaDto.CreatedBy,
                            UnitId = areaDto.UnitId,
                            CreatedOn = DateTime.UtcNow,

                        };

                        _context.FacilityMasters.Add(area);
                        await _context.SaveChangesAsync();
                        break;
                    case "FacilityMaster-Section":
                        var sectionDto = JsonSerializer.Deserialize<SectionMasterDto>(data, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (sectionDto == null) return new OperationResultDto { Success = false, Message = "Invalid data" };
                        if (!Regex.IsMatch(sectionDto.Label, @"^[a-zA-Z0-9\s-_]+$"))
                        {
                            return new OperationResultDto
                            {
                                Success = false,
                                Message = "Invalid characters in Section Name. Only letters, numbers, spaces, hyphen and underscore are allowed."
                            };
                        }
                        var section = new FacilityMaster
                        {
                            Name = sectionDto.Label,
                            FacilityHead = sectionDto.FacilityHeadId,
                            ParentId = sectionDto.ParentId,
                            CreatedBy = sectionDto.CreatedBy,
                            UnitId = sectionDto.UnitId,
                            CreatedOn = DateTime.UtcNow,

                        };

                        _context.FacilityMasters.Add(section);
                        await _context.SaveChangesAsync();
                        break;
                    case "ScenarioMaster":
                        var scenarioDto = JsonSerializer.Deserialize<ScenarioMasterDto>(data, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (scenarioDto == null) return new OperationResultDto { Success = false, Message = "Invalid data" };
                        if (!Regex.IsMatch(scenarioDto.Label, @"^[a-zA-Z0-9\s-_]+$"))
                        {
                            return new OperationResultDto
                            {
                                Success = false,
                                Message = "Invalid characters in Scenario Name. Only letters, numbers, spaces, hyphen and underscore are allowed."
                            };
                        }
                        var scenario = new ScenarioMaster
                        {
                            ScenarioName = scenarioDto.Label,
                            Description = scenarioDto.Description,
                            CreatedBy = scenarioDto.CreatedBy,
                            UnitId = scenarioDto.UnitId,
                            CreatedOn = DateTime.UtcNow,
                        };
                        _context.ScenarioMasters.Add(scenario);
                        await _context.SaveChangesAsync();
                        break;
                    case "Roles":
                        var roleDto = JsonSerializer.Deserialize<RolesMasterDto>(data, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (roleDto == null) return new OperationResultDto { Success = false, Message = "Invalid data" };

                        if (!Regex.IsMatch(roleDto.Label, @"^[a-zA-Z0-9\s-_]+$"))
                        {
                            return new OperationResultDto
                            {
                                Success = false,
                                Message = "Invalid characters in Role Name. Only letters, numbers, spaces, hyphen and underscore are allowed."
                            };
                        }

                        bool exists = await _context.Roles
                        .AnyAsync(r => r.Name == roleDto.Label && r.UnitId == roleDto.UnitId);

                        if (exists)
                            return new OperationResultDto { Success = false, Message = $"RoleName '{roleDto.Label}' cannot be added twice in the same unit." };

                        var role = new Role
                        {
                            Name = roleDto.Label,
                            CreatedBy = roleDto.CreatedBy,
                            UnitId = roleDto.UnitId,
                            CreatedOn = DateTime.UtcNow,
                        };
                        _context.Roles.Add(role);
                        await _context.SaveChangesAsync();
                        break;

                    case "Designation":
                        var deignationDto = JsonSerializer.Deserialize<DesignationMasterDto>(data, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (deignationDto == null) return new OperationResultDto { Success = false, Message = "Invalid data" };
                        if (!Regex.IsMatch(deignationDto.Label, @"^[a-zA-Z0-9\s-_]+$"))
                        {
                            return new OperationResultDto
                            {
                                Success = false,
                                Message = "Invalid characters in Designation Name. Only letters, numbers, spaces, hyphen and underscore are allowed."
                            };
                        }
                        var designation = new Designation
                        {
                            Name = deignationDto.Label,
                            CreatedBy = deignationDto.CreatedBy,
                            UnitId = deignationDto.UnitId,
                            CreatedOn = DateTime.UtcNow,
                        };
                        _context.Designations.Add(designation);
                        await _context.SaveChangesAsync();
                        break;

                    case "Employees":

                        using (var transaction = await _context.Database.BeginTransactionAsync())
                        {
                            try
                            {
                                int userId = 0;

                                var employeeDto = JsonSerializer.Deserialize<EmployeeMasterDto>(data, new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });

                                if (employeeDto == null)
                                    return new OperationResultDto { Success = false, Message = "Invalid employee data" };
                                if (!Regex.IsMatch(employeeDto.Label, @"^[a-zA-Z0-9\s-_]+$"))
                                {
                                    return new OperationResultDto
                                    {
                                        Success = false,
                                        Message = "Invalid characters in Employee Name. Only letters, numbers, spaces, hyphen and underscore are allowed."
                                    };
                                }
                                if (!Regex.IsMatch(employeeDto.EmployeeCode, @"^[a-zA-Z0-9\s-_]+$"))
                                {
                                    return new OperationResultDto
                                    {
                                        Success = false,
                                        Message = "Invalid characters in Employee Code. Only letters, numbers, spaces, hyphen and underscore are allowed."
                                    };
                                }

                                bool isEmployeeCodeExist = await _context.Employees
                                    .AnyAsync(e => e.EmployeeCode == employeeDto.EmployeeCode && !e.IsDeleted);

                                if (isEmployeeCodeExist)
                                    return new OperationResultDto { Success = false, Message = "Employee code already exists" };

                                if (employeeDto.RoleId > 0)
                                {
                                    bool isLoginExist = await _context.Users
                                        .AnyAsync(u => u.Username == employeeDto.EmployeeCode && u.IsActive);

                                    if (isLoginExist)
                                        return new OperationResultDto { Success = false, Message = "Login name already exists" };
                                    

                                    var user = new Users
                                    {
                                        RoleId = employeeDto.RoleId,
                                        Username = employeeDto.EmployeeCode,
                                        PasswordHash = BCrypt.Net.BCrypt.HashPassword(employeeDto.EmployeeCode),
                                        IsActive = true
                                    };

                                    _context.Users.Add(user);
                                    await _context.SaveChangesAsync(); // Save to generate UserId

                                    userId = user.Id;

                                    var userRole = new UserRole
                                    {
                                        RoleId = employeeDto.RoleId,
                                        UserId = userId
                                    };

                                    _context.UserRoles.Add(userRole);
                                    await _context.SaveChangesAsync();
                                }

                                var employee = new Employees
                                {
                                    EmployeeCode = employeeDto.EmployeeCode,
                                    UserId = userId,
                                    FullName = employeeDto.Label,
                                    MobileNumber = employeeDto.MobileNumber,
                                    Email = employeeDto.Email,
                                    DeptId = employeeDto.areaId,
                                    Designation = employeeDto.DesigId,
                                    CreatedBy = employeeDto.CreatedBy,
                                    UnitId = employeeDto.UnitId,
                                    CreatedOn = DateTime.UtcNow,
                                };

                                _context.Employees.Add(employee);
                                await _context.SaveChangesAsync();

                                await transaction.CommitAsync();
                                return new OperationResultDto { Success = true, Message = "Employee saved successfully" };
                            }
                            catch (Exception ex)
                            {
                                await transaction.RollbackAsync();
                                return new OperationResultDto
                                {
                                    Success = false,
                                    Message = "Error occurred while saving employee: " + ex.Message
                                };
                            }
                        }

                        break;


                    // Add other cases like Employees, Section, etc.

                    default:
                        return new OperationResultDto { Success = false, Message = "Unknown master type" };
                }

                //await _context.SaveChangesAsync();
                return new OperationResultDto { Success = true, Message = "Saved successfully" };
            }
            catch (Exception ex)
            {
                return new OperationResultDto
                {
                    Success = false,
                    Message = $"Error occurred: {ex.Message}"
                };
            }
        }

        public async Task<OperationResultDto> UpdateAsync(string source, int id, JsonElement data)
        {
            try
            {

                switch (source)
                {
                    case "UnitsMaster":
                        var unitdto = JsonSerializer.Deserialize<UnitsMasterDeto>(data, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (unitdto == null) return new OperationResultDto { Success = false, Message = "Invalid data" };

                        var unit = await _context.UnitsMaster.FirstOrDefaultAsync(a => a.UnitId == id);
                        if (unit == null) return new OperationResultDto { Success = false, Message = "Area not found" };

                        unit.UnitName = unitdto.Label;
                        unit.UnitHead = unitdto.UnitHeadId;
                        unit.ModifiedBy = unitdto.ModifiedBy;
                        unit.ModifiedOn = DateTime.UtcNow;

                        break;
                    case "FacilityMaster-Area":
                        var areadto = JsonSerializer.Deserialize<AreaMasterDto>(data, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (areadto == null) return new OperationResultDto { Success = false, Message = "Invalid data" };

                        var area = await _context.FacilityMasters.FirstOrDefaultAsync(a => a.Id == id);
                        if (area == null) return new OperationResultDto { Success = false, Message = "Area not found" };

                        area.Name = areadto.Label;
                        area.FacilityHead = areadto.FacilityHeadId;
                        area.ModifiedBy = areadto.ModifiedBy;
                        area.ModifiedOn = DateTime.UtcNow;

                        break;
                    case "FacilityMaster-Section":
                        var sectiondto = JsonSerializer.Deserialize<SectionMasterDto>(data, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (sectiondto == null) return new OperationResultDto { Success = false, Message = "Invalid data" };

                        var section = await _context.FacilityMasters.FirstOrDefaultAsync(a => a.Id == id);
                        if (section == null) return new OperationResultDto { Success = false, Message = "Section not found" };

                        section.Name = sectiondto.Label;
                        section.FacilityHead = sectiondto.FacilityHeadId;
                        section.ParentId = sectiondto.ParentId;
                        section.ModifiedBy = sectiondto.ModifiedBy;
                        section.ModifiedOn = DateTime.UtcNow;

                        break;

                    case "ScenarioMaster":
                        var scenariodto = JsonSerializer.Deserialize<ScenarioMasterDto>(data, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (scenariodto == null) return new OperationResultDto { Success = false, Message = "Invalid data" };
                        var scenario = await _context.ScenarioMasters.FirstOrDefaultAsync(a => a.Id == id);
                        if (scenario == null) return new OperationResultDto { Success = false, Message = "Scenario not found" };

                        scenario.ScenarioName = scenariodto.Label;
                        scenario.Description = scenariodto.Description;
                        scenario.ModifiedBy = scenariodto.ModifiedBy;
                        scenario.ModifiedOn = DateTime.UtcNow;
                        break;


                    case "Roles":
                        var roledto = JsonSerializer.Deserialize<RolesMasterDto>(data, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (roledto == null) return new OperationResultDto { Success = false, Message = "Invalid data" };
                        var role = await _context.Roles.FirstOrDefaultAsync(a => a.Id == id);
                        if (role == null) return new OperationResultDto { Success = false, Message = "Role not found" };

                        role.Name = roledto.Label;
                        role.ModifiedBy = roledto.ModifiedBy;
                        role.ModifiedOn = DateTime.UtcNow;
                        break;

                    case "Designation":
                        var designationdto = JsonSerializer.Deserialize<DesignationMasterDto>(data, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (designationdto == null) return new OperationResultDto { Success = false, Message = "Invalid data" };
                        var designation = await _context.Designations.FirstOrDefaultAsync(a => a.Id == id);
                        if (designation == null) return new OperationResultDto { Success = false, Message = "Designation not found" };

                        designation.Name = designationdto.Label;
                        designation.ModifiedBy = designationdto.ModifiedBy;
                        designation.ModifiedOn = DateTime.UtcNow;
                        break;
                    case "Employees":
                        var employeedto = JsonSerializer.Deserialize<EmployeeMasterDto>(data, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                        if (employeedto == null) return new OperationResultDto { Success = false, Message = "Invalid data" };
                        bool exists = await _context.Employees
                        .AnyAsync(r => r.EmployeeCode == employeedto.EmployeeCode && r.UnitId == employeedto.UnitId && r.Id!= id);

                        if (exists)
                           return new OperationResultDto { Success = false, Message = $"Employee code  '{employeedto.EmployeeCode}' already exists." };

                        var employees = await _context.Employees.FirstOrDefaultAsync(a => a.Id == id);
                        if (employees == null) return new OperationResultDto { Success = false, Message = "Employee not found" };
                        // string hashedPassword = DatabaseUtility.HashPassword(employeedto.EmployeeCode);
                        if (employeedto.RoleId > 0)
                        {
                            if (employees.UserId == null || employees.UserId == 0)
                            {

                                // Create new user
                                var newUser = new Users
                                {
                                    RoleId = employeedto.RoleId,
                                    Username = employeedto.EmployeeCode,
                                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(employeedto.EmployeeCode),
                                    IsActive = true,
                                };

                                _context.Users.Add(newUser);
                                await _context.SaveChangesAsync();
                                var userRole = new UserRole
                                {
                                    UserId = newUser.Id,
                                    RoleId = employeedto.RoleId
                                };
                                _context.UserRoles.Add(userRole);
                                await _context.SaveChangesAsync();

                                employees.UserId = newUser.Id;
                            }
                            else
                            {
                                // Update existing user
                                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == employees.UserId);
                                if (existingUser != null)
                                {
                                    existingUser.RoleId = employeedto.RoleId;
                                    existingUser.Username = employeedto.EmployeeCode;
                                    existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(employeedto.EmployeeCode);
                                    existingUser.IsActive = true;
                                    var existingUserRole = await _context.UserRoles
                                        .FirstOrDefaultAsync(ur => ur.UserId == existingUser.Id);

                                    if (existingUserRole != null)
                                    {
                                        // Delete the old mapping
                                        _context.UserRoles.Remove(existingUserRole);
                                        await _context.SaveChangesAsync();
                                    }

                                    // Insert new mapping with updated RoleId
                                    var newUserRole = new UserRole
                                    {
                                        UserId = existingUser.Id,
                                        RoleId = employeedto.RoleId
                                    };
                                    _context.UserRoles.Add(newUserRole);
                                }

                            }
                        }

                        employees.EmployeeCode = employeedto.EmployeeCode;
                        employees.FullName = employeedto.Label;
                        employees.MobileNumber = employeedto.MobileNumber;
                        employees.Email = employeedto.Email;
                        employees.DeptId = employeedto.areaId;
                        employees.Designation = employeedto.DesigId;
                        employees.ModifiedBy = employeedto.ModifiedBy;
                        employees.ModifiedOn = DateTime.UtcNow;

                        break;

                    default:
                        return new OperationResultDto { Success = false, Message = "Unknown master source" };
                }


                await _context.SaveChangesAsync();
                return new OperationResultDto { Success = true, Message = "Updated successfully" };
            }
            catch (Exception ex)
            {
                return new OperationResultDto { Success = false, Message = $"Update failed: {ex.Message}" };
            }
        }
        public async Task<bool> UpdateStatusAsync(StatusUpdateDto dto)
        {
            switch (dto.source)
            {
                case "UnitsMaster":

                    var unit = await _context.UnitsMaster.FirstOrDefaultAsync(a => a.UnitId == dto.id);
                    if (unit == null) return false;

                    unit.IsActive = dto.isActive;
                    unit.ModifiedBy = dto.ModifiedBy;
                    unit.ModifiedOn = DateTime.UtcNow;
                    break;
                case "FacilityMaster-Area":

                    var area = await _context.FacilityMasters.FirstOrDefaultAsync(a => a.Id == dto.id);
                    if (area == null) return false;

                    area.IsActive = dto.isActive;
                    area.ModifiedBy = dto.ModifiedBy;
                    area.ModifiedOn = DateTime.UtcNow;
                    break;
                case "FacilityMaster-Section":

                    var section = await _context.FacilityMasters.FirstOrDefaultAsync(a => a.Id == dto.id);
                    if (section == null) return false;

                    section.IsActive = dto.isActive;
                    section.ModifiedBy = dto.ModifiedBy;
                    section.ModifiedOn = DateTime.UtcNow;
                    break;

                case "ScenarioMaster":

                    var scenario = await _context.ScenarioMasters.FirstOrDefaultAsync(a => a.Id == dto.id);
                    if (scenario == null) return false;

                    scenario.IsActive = dto.isActive;
                    scenario.ModifiedBy = dto.ModifiedBy;
                    scenario.ModifiedOn = DateTime.UtcNow;
                    break;
                case "Roles":

                    var role = await _context.Roles.FirstOrDefaultAsync(a => a.Id == dto.id);
                    if (role == null) return false;

                    role.IsActive = dto.isActive;
                    role.ModifiedBy = dto.ModifiedBy;
                    role.ModifiedOn = DateTime.UtcNow;
                    break;
                case "Designation":

                    var designation = await _context.Designations.FirstOrDefaultAsync(a => a.Id == dto.id);
                    if (designation == null) return false;

                    designation.IsActive = dto.isActive;
                    designation.ModifiedBy = dto.ModifiedBy;
                    designation.ModifiedOn = DateTime.UtcNow;
                    break;
                case "Employees":

                    var employee = await _context.Employees.FirstOrDefaultAsync(a => a.Id == dto.id);
                    if (employee == null) return false;

                    employee.IsActive = dto.isActive;
                    employee.ModifiedBy = dto.ModifiedBy;
                    employee.ModifiedOn = DateTime.UtcNow;
                    break;


                default:
                    return false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> CheckHODEHSAsync(int configElementId, int unitId)
        {
            var valueIds = await _context.GeneralConfigElementValues
                .Where(g => g.ConnfigElementId == configElementId
                            && g.UnitId == unitId
                            && g.IsActive == true)
                .Select(g => g.ValueId)
                .ToListAsync();

            // Return as comma-separated string
            return string.Join(",", valueIds);
        }

        public async Task<List<DropdownOptionDto>> GetMenusAsync()
        {
            return await _context.Menus
    .Where(m => m.Route != null && m.IsActive == true)
    .OrderBy(m => m.SortOrder) // ✅ Order ascending
    .Select(m => new DropdownOptionDto
    {
        Id = m.Id,
        Label = m.Title,
        ParentId = m.ParentId,
    })
    .ToListAsync();
        }
        public async Task AssignMenusToRoleAsync(RoleMenuAssignmentDto dto)
        {
            try
            {
                // Remove existing mappings for the role
                var existingMappings = await _context.RoleMenus
                    .Where(rm => rm.RoleId == dto.RoleId)
                    .ToListAsync();

                _context.RoleMenus.RemoveRange(existingMappings);

                // Add new mappings
                var now = DateTime.UtcNow;
                var newMappings = dto.MenuIds.Select(menuId => new RoleMenu
                {
                    RoleId = dto.RoleId,
                    MenuId = menuId,
                    SubmittedBy = dto.SubmittedBy,
                    SubmittedOn = now,
                    IsActive = true,
                    UnitId = dto.UnitId
                });

                await _context.RoleMenus.AddRangeAsync(newMappings);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the error or rethrow with more context
                throw new Exception("Error while assigning menus to role: " + ex.Message, ex);
            }
        }
        public async Task<Dictionary<string, PaginatedResult<PendingTaskDto>>> GetFiredrillDashboardAsync( PendingTaskFilterDto dto)
        {
            var result = new Dictionary<string, PaginatedResult<PendingTaskDto>>();

            var hodAreaIds = await _context.FacilityMasters
                .Where(f => f.FacilityHead == dto.userId && f.UnitId == dto.unitId)
                .Select(f => f.Id)
                .ToListAsync();

            var hodehsUserIds = await _context.GeneralConfigElementValues
                .Where(c => c.ConnfigElementId == 1 && c.UnitId == dto.unitId)
                .Select(c => c.ValueId)
                .ToListAsync();

            bool isHODEHS = hodehsUserIds.Contains(dto.userId);

            // ✅ DRAFTS
            var draftsQuery = _context.FireDrills
                .Where(f => f.SubmittedBy == dto.userId && f.EntryStatus == "inprogress" && f.UnitId == dto.unitId)
                .Select(f => new PendingTaskDto
                {
                    RefNo = f.RefNo,
                    Area = _context.FacilityMasters.Where(fm => fm.Id == f.Facility1Id).Select(fm => fm.Name).FirstOrDefault() ?? "Unknown",
                    TaskDetails = _context.ScenarioMasters.Where(fm => fm.Id == f.ScenarioId).Select(fm => fm.ScenarioName).FirstOrDefault() ?? "Unknown",
                    Url = $"/submit-emergency/{f.FireDrillId}?mode=edit&RefNo={f.RefNo}&entryStatus={f.EntryStatus}&status=submit&prepage=dashboard"
                });

            int totalDrafts = await draftsQuery.CountAsync();
            var drafts = await draftsQuery.ToListAsync();
            if (drafts.Any())
            {
                result["Draft"] = new PaginatedResult<PendingTaskDto>
                {
                    Data = drafts,
                    TotalRecords = totalDrafts,
                   
                };
            }



            // ✅ REVIEWS
            var reviewsQuery = _context.FireDrills
                .Where(f => f.EntryStatus == "complete" && f.IsReview == null && f.UnitId == dto.unitId && hodAreaIds.Contains(f.Facility1Id))
                .Select(f => new PendingTaskDto
                {
                    RefNo = f.RefNo,
                    Area = _context.FacilityMasters.Where(fm => fm.Id == f.Facility1Id).Select(fm => fm.Name).FirstOrDefault() ?? "Unknown",
                    TaskDetails = _context.ScenarioMasters.Where(fm => fm.Id == f.ScenarioId).Select(fm => fm.ScenarioName).FirstOrDefault() ?? "Unknown",
                    Url = $"/submit-emergency/{f.FireDrillId}?mode=edit&RefNo={f.RefNo}&entryStatus={f.EntryStatus}&status=review&prepage=dashboard"
                });

            int totalReviews = await reviewsQuery.CountAsync();
            var reviews = await reviewsQuery.ToListAsync();
            if (reviews.Any())
            {
                result["Review"] = new PaginatedResult<PendingTaskDto>
                {
                    Data = reviews,
                    TotalRecords = totalReviews
                };
            }

            // ✅ RELEASED
            if (isHODEHS)
            {
                var releasedQuery = _context.FireDrills
                    .Where(f => f.EntryStatus == "complete" && f.UnitId == dto.unitId && f.IsReview == true && (f.IsReleased == false || f.IsReleased == null))
                    .Select(f => new PendingTaskDto
                    {
                        RefNo = f.RefNo,
                        Area = _context.FacilityMasters.Where(fm => fm.Id == f.Facility1Id).Select(fm => fm.Name).FirstOrDefault() ?? "Unknown",
                        TaskDetails = _context.ScenarioMasters.Where(fm => fm.Id == f.ScenarioId).Select(fm => fm.ScenarioName).FirstOrDefault() ?? "Unknown",
                        Url = $"/submit-emergency/{f.FireDrillId}?mode=edit&RefNo={f.RefNo}&entryStatus={f.EntryStatus}&status=released&prepage=dashboard"
                    });

                int totalReleased = await releasedQuery.CountAsync();
                var released = await releasedQuery.ToListAsync();    //.Skip((dto.pageNumber ) * dto.pageSize).Take(dto.pageSize)
                if (released.Any())
                {
                    result["Released"] = new PaginatedResult<PendingTaskDto>
                    {
                        Data = released,
                        TotalRecords = totalReleased,
                    };
                }
            }

            // ✅ INPROGRESS TASKS
            var inprogressQuery = (
                from assign in _context.taskAssgntoUsers
                join task in _context.Tasks on assign.TaskId equals task.TaskId
                join rec in _context.Recommendations on task.TaskCreatedForId equals rec.RecommendationId
                join drill in _context.FireDrills on rec.FireDrillId equals drill.FireDrillId
                join fac in _context.FacilityMasters on assign.EmpDeptId equals fac.Id into facJoin
                from fac in facJoin.DefaultIfEmpty()
                where assign.UserTaskStatusId == 3 && assign.EmployeeId == dto.userId && drill.UnitId == dto.unitId
                select new PendingTaskDto
                {
                    RefNo = drill.RefNo,
                    FireDrillId = drill.FireDrillId,
                    taskId = task.TaskId,
                    taskCreatedForId = task.TaskCreatedForId,
                    TaskDetails = task.TaskDetails ?? "Unknown",
                    TargetDate = task.TargetDate,
                    Remarks = task.Remarks ?? "Unknown",
                    TaskStatusId = task.TaskStatusId,
                    Area = fac.Name
                });

            int totalInprogress = await inprogressQuery.CountAsync();
            var inprogress = await inprogressQuery.ToListAsync();
            if (inprogress.Any())
            {
                result["Inprogress"] = new PaginatedResult<PendingTaskDto>
                {
                    Data = inprogress,
                    TotalRecords = totalInprogress,
                };
            }

            // ✅ ASSIGN TASKS
            var assignQuery = (
                from assign in _context.taskAssgntoUsers
                join task in _context.Tasks on assign.TaskId equals task.TaskId
                join rec in _context.Recommendations on task.TaskCreatedForId equals rec.RecommendationId
                join drill in _context.FireDrills on rec.FireDrillId equals drill.FireDrillId
                join fac in _context.FacilityMasters on assign.EmpDeptId equals fac.Id into facJoin
                from fac in facJoin.DefaultIfEmpty()
                where assign.UserTaskStatusId == 2 && assign.EmployeeId == dto.userId && drill.UnitId == dto.unitId
                select new PendingTaskDto
                {
                    RefNo = drill.RefNo,
                    FireDrillId = drill.FireDrillId,
                    taskId = task.TaskId,
                    taskCreatedForId = task.TaskCreatedForId,
                    TaskDetails = task.TaskDetails ?? "Unknown",
                    TargetDate = task.TargetDate,
                    Remarks = task.Remarks ?? "",
                    TaskStatusId = task.TaskStatusId,
                    Area = fac.Name
                });

            int totalAssign = await assignQuery.CountAsync();
            var assignTasks = await assignQuery.ToListAsync();
            if (assignTasks.Any())
            {
                result["Assign"] = new PaginatedResult<PendingTaskDto>
                {
                    Data = assignTasks,
                    TotalRecords = totalAssign,
                };
            }

            // ✅ TASK APPROVAL
            var approvalQuery = (
                from assign in _context.taskAssgntoUsers
                join task in _context.Tasks on assign.TaskId equals task.TaskId
                join rec in _context.Recommendations on task.TaskCreatedForId equals rec.RecommendationId
                join drill in _context.FireDrills on rec.FireDrillId equals drill.FireDrillId
                join fac in _context.FacilityMasters on assign.EmpDeptId equals fac.Id into facJoin
                from fac in facJoin.DefaultIfEmpty()
                where assign.UserTaskStatusId == 1 && hodAreaIds.Contains(assign.EmpDeptId ?? 0) && rec.ActionStatusId != 1 && drill.UnitId == dto.unitId
                select new PendingTaskDto
                {
                    RefNo = drill.RefNo,
                    FireDrillId = drill.FireDrillId,
                    taskId = task.TaskId,
                    taskCreatedForId = task.TaskCreatedForId,
                    TaskDetails = task.TaskDetails ?? "Unknown",
                    TargetDate = task.TargetDate,
                    Remarks = task.Remarks ?? "",
                    TaskStatusId = task.TaskStatusId,
                    ApprovalStatusId = rec.ActionStatusId,
                    Area = fac.Name
                });

            int totalApproval = await approvalQuery.CountAsync();
            var approvals = await approvalQuery.ToListAsync();
            if (approvals.Any())
            {
                result["Task Approval"] = new PaginatedResult<PendingTaskDto>
                {
                    Data = approvals,
                    TotalRecords = totalApproval
                };
            }

            return result;
        }

        //      public async Task<Dictionary<string, List<PendingTaskDto>>> GetFiredrillDashboardAsync(int userId, int unitId)
        //      {
        //          var result = new Dictionary<string, List<PendingTaskDto>>();

        //          // Get area IDs where user is HOD
        //          var hodAreaIds = await _context.FacilityMasters
        //              .Where(f => f.FacilityHead == userId && f.UnitId == unitId)
        //              .Select(f => f.Id)
        //              .ToListAsync();

        //          var UserhodAreaIds = await _context.FacilityMasters
        //              .Where(f => f.FacilityHead == userId && f.UnitId == unitId)
        //              .Select(f => f.Id)
        //              .ToListAsync();

        //          // Get all HODEHS user IDs from config (ConfigElementId = 1)
        //          var hodehsUserIds = await _context.GeneralConfigElementValues
        //              .Where(c => c.ConnfigElementId == 1 && c.UnitId == unitId)
        //              .Select(c => c.ValueId)
        //              .ToListAsync();

        //          bool isHODEHS = hodehsUserIds.Contains(userId);

        //          // DRAFTS: Submitted by the user & InProgress
        //          var drafts = await _context.FireDrills
        //              .Where(f => f.SubmittedBy == userId && f.EntryStatus == "inprogress" && f.UnitId == unitId)
        //              .Select(f => new PendingTaskDto
        //              {
        //                  RefNo = f.RefNo,
        //                  Area = _context.FacilityMasters
        //                          .Where(fm => fm.Id == f.Facility1Id)
        //                          .Select(fm => fm.Name)
        //                          .FirstOrDefault() ?? "Unknown",
        //                  TaskDetails = _context.ScenarioMasters
        //                          .Where(fm => fm.Id == f.ScenarioId)
        //                          .Select(fm => fm.ScenarioName)
        //                          .FirstOrDefault() ?? "Unknown",
        //                  Url = $"/submit-emergency/{f.FireDrillId}?mode=edit&RefNo={f.RefNo}&entryStatus={f.EntryStatus}&status=submit&prepage=dashboard"
        //              })
        //              .ToListAsync();

        //          if (drafts.Any()) result["Draft"] = drafts;

        //          // REVIEWS: Complete & not reviewed & user is HOD of that area
        //          var reviews = await _context.FireDrills
        //              .Where(f => f.EntryStatus == "complete" && f.IsReview == null && f.UnitId == unitId && hodAreaIds.Contains(f.Facility1Id))
        //              .Select(f => new PendingTaskDto
        //              {
        //                  RefNo = f.RefNo,
        //                  Area = _context.FacilityMasters
        //                          .Where(fm => fm.Id == f.Facility1Id)
        //                          .Select(fm => fm.Name)
        //                          .FirstOrDefault() ?? "Unknown",
        //                  TaskDetails = _context.ScenarioMasters
        //                          .Where(fm => fm.Id == f.ScenarioId)
        //                          .Select(fm => fm.ScenarioName)
        //                          .FirstOrDefault() ?? "Unknown",
        //                  Url = $"/submit-emergency/{f.FireDrillId}?mode=edit&RefNo={f.RefNo}&entryStatus={f.EntryStatus}&status=review&prepage=dashboard"
        //              })
        //              .ToListAsync();

        //          if (reviews.Any()) result["Review"] = reviews;

        //          // RELEASED: Reviewed but not released & user is HODEHS
        //          if (isHODEHS)
        //          {
        //              var released = await _context.FireDrills
        //  .Where(f => f.EntryStatus == "complete" && f.UnitId == unitId && f.IsReview == true && (f.IsReleased == false || f.IsReleased == null))
        //  .Select(f => new PendingTaskDto
        //  {
        //      RefNo = f.RefNo,
        //      Area = _context.FacilityMasters
        //          .Where(fm => fm.Id == f.Facility1Id)
        //          .Select(fm => fm.Name)
        //          .FirstOrDefault() ?? "Unknown",
        //      TaskDetails = _context.ScenarioMasters
        //                          .Where(fm => fm.Id == f.ScenarioId)
        //                          .Select(fm => fm.ScenarioName)
        //                          .FirstOrDefault() ?? "Unknown",
        //      Url = $"/submit-emergency/{f.FireDrillId}?mode=edit&RefNo={f.RefNo}&entryStatus={f.EntryStatus}&status=released&prepage=dashboard"
        //  })
        //  .ToListAsync();

        //              if (released.Any()) result["Released"] = released;
        //          }
        //          var taskInprogress = await (
        // from assign in _context.taskAssgntoUsers
        // join task in _context.Tasks on assign.TaskId equals task.TaskId
        // join rec in _context.Recommendations on task.TaskCreatedForId equals rec.RecommendationId
        // join drill in _context.FireDrills on rec.FireDrillId equals drill.FireDrillId
        // join fac in _context.FacilityMasters on assign.EmpDeptId equals fac.Id into facJoin
        // from fac in facJoin.DefaultIfEmpty()
        // where assign.UserTaskStatusId == 3 && assign.EmployeeId == userId && drill.UnitId == unitId

        // select new PendingTaskDto
        // {
        //     RefNo = drill.RefNo,
        //     FireDrillId = drill.FireDrillId,
        //     taskId = task.TaskId,
        //     taskCreatedForId = task.TaskCreatedForId,
        //     TaskDetails = task.TaskDetails ?? "Unknown",
        //     TargetDate = task.TargetDate,
        //     Remarks = task.Remarks ?? "Unknown",
        //     TaskStatusId = task.TaskStatusId,
        //     Area = fac.Name,

        // }).ToListAsync();

        //          if (taskInprogress.Any())
        //          {
        //              result["Inprogress"] = taskInprogress;
        //          }

        //          var taskAssign = await (
        //from assign in _context.taskAssgntoUsers
        //join task in _context.Tasks on assign.TaskId equals task.TaskId
        //join rec in _context.Recommendations on task.TaskCreatedForId equals rec.RecommendationId
        //join drill in _context.FireDrills on rec.FireDrillId equals drill.FireDrillId
        //join fac in _context.FacilityMasters on assign.EmpDeptId equals fac.Id into facJoin
        //from fac in facJoin.DefaultIfEmpty()
        //where assign.UserTaskStatusId == 2 && assign.EmployeeId == userId && drill.UnitId == unitId

        //select new PendingTaskDto
        //{
        //    RefNo = drill.RefNo,
        //    FireDrillId = drill.FireDrillId,
        //    taskId = task.TaskId,
        //    taskCreatedForId = task.TaskCreatedForId,
        //    TaskDetails = task.TaskDetails ?? "Unknown",
        //    TargetDate = task.TargetDate,
        //    Remarks = task.Remarks ?? "",
        //    TaskStatusId = task.TaskStatusId,
        //    Area = fac.Name,
        //}).ToListAsync();

        //          if (taskAssign.Any())
        //          {
        //              result["Assign"] = taskAssign;
        //          }
        //          var taskApproval = await (
        //from assign in _context.taskAssgntoUsers
        //join task in _context.Tasks on assign.TaskId equals task.TaskId
        //join rec in _context.Recommendations on task.TaskCreatedForId equals rec.RecommendationId
        //join drill in _context.FireDrills on rec.FireDrillId equals drill.FireDrillId
        //join fac in _context.FacilityMasters on assign.EmpDeptId equals fac.Id into facJoin
        //from fac in facJoin.DefaultIfEmpty()
        //where assign.UserTaskStatusId == 1 && hodAreaIds.Contains(assign.EmpDeptId ?? 0) && rec.ActionStatusId != 1 && drill.UnitId == unitId

        //select new PendingTaskDto
        //{
        //    RefNo = drill.RefNo,
        //    FireDrillId = drill.FireDrillId,
        //    taskId = task.TaskId,
        //    taskCreatedForId = task.TaskCreatedForId,
        //    TaskDetails = task.TaskDetails ?? "Unknown",
        //    TargetDate = task.TargetDate,
        //    Remarks = task.Remarks ?? "",
        //    TaskStatusId = task.TaskStatusId,
        //    ApprovalStatusId = rec.ActionStatusId,
        //    Area = fac.Name,

        //}).ToListAsync();

        //          if (taskApproval.Any())
        //          {
        //              result["Task Approval"] = taskApproval;
        //          }

        //          return result;
        //      }

    }
}

