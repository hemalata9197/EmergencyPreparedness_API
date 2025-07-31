using EmergencyManagement.Data;
using EmergencyManagement.Models.DTOs.Common;
using EmergencyManagement.Models.DTOs.Master;
using EmergencyManagement.Models.Entities;
using EmergencyManagement.Services.Interfaces;
using EmergencyManagement.Token;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EmergencyManagement.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly EHSDbContext _context;
        private readonly JwtSettings _jwtSettings;
        public AuthService(EHSDbContext context, IOptions<JwtSettings> jwtOptions)
        {
            _context = context;
            _jwtSettings = jwtOptions.Value;
        }
        public async Task<AuthResponseDto> AuthenticateAsync(LoginDto request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
    {
                new Claim("employeeId", user.Employee.Id.ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),       // nameid
        new Claim(ClaimTypes.Name, user.Username),                      // name
        new Claim(JwtRegisteredClaimNames.Sub, user.Username)           // sub
    };

            // Add EmployeeId if available
            if (user.Employee != null)
            {
                claims.Add(new Claim("employeeId", user.Employee.Id.ToString()));  // ✅ Custom claim
            }

            // Add Roles
            foreach (var role in user.UserRoles.Select(ur => ur.Role.Name))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                NotBefore = DateTime.UtcNow.AddSeconds(-5),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new AuthResponseDto
            {
                Token = tokenString,
                Username = user.Username,
                IsFirstLogin=user.IsFirstLogin,
                Roles = user.UserRoles.Select(r => r.Role.Name).ToList(),
                EmployeeName = user.Employee?.FullName
            };
        }
        public async Task<UserInfoDto?> GetUserInfoAsync(string username)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) return null;

            // 1. Get directly assigned menu IDs
            var assignedMenuIds = await _context.RoleMenus
                .Where(rm => rm.RoleId == user.RoleId && rm.IsActive)
                .Select(rm => rm.MenuId)
                .ToListAsync();

            // 2. Fetch all menus to traverse parent-child relations
            var allMenus = await _context.Menus
                .Where(m => m.IsActive)
                .ToListAsync();

            // 3. Recursive function to collect parent IDs
            HashSet<int> GetWithParents(IEnumerable<int> menuIds)
            {
                var allIds = new HashSet<int>(menuIds);
                var menuDict = allMenus.ToDictionary(m => m.Id, m => m.ParentId);

                foreach (var id in menuIds)
                {
                    var currentId = id;
                    while (menuDict.ContainsKey(currentId) && menuDict[currentId] != null && !allIds.Contains(menuDict[currentId]!.Value))
                    {
                        var parentId = menuDict[currentId];
                        if (parentId.HasValue)
                        {
                            allIds.Add(parentId.Value);
                            currentId = parentId.Value;
                        }
                        else break;
                    }
                }

                return allIds;
            }

            // 4. Include all parent menus
            var menuIdsWithParents = GetWithParents(assignedMenuIds);

            // 5. Get all required menus for the tree
            var menusToShow = allMenus
                .Where(m => menuIdsWithParents.Contains(m.Id))
                .OrderBy(m => m.SortOrder)
                .ToList();

            // 6. Build tree
            List<MenuDto> BuildMenuTree(List<Menu> menus, int? parentId)
            {
                return menus
                    .Where(m => m.ParentId == parentId)
                    .OrderBy(m => m.SortOrder)
                    .Select(m => new MenuDto
                    {
                        Id = m.Id,
                        Name = m.Title,
                        Route = m.Route,
                        Children = BuildMenuTree(menus, m.Id)
                    })
                    .ToList();
            }

            var menuTree = BuildMenuTree(menusToShow, null);

            return new UserInfoDto
            {
                Username = user.Username,
                Roles = new List<string>(),
                Permissions = new List<string>(),
                Menu = menuTree
            };
        }
        public async Task<object?> GetUnitsByEmployeeIdAsync(int EmpId)
        {
            if (EmpId == 1)
            {
                // ✅ Return all units for Admin (EmpId = 1)
                var allUnits = await _context.UnitsMaster
                    .OrderBy(u => u.UnitId)
                    .Select(u => new
                    {
                        UnitId = u.UnitId,
                        UnitName = u.UnitName
                    }).ToListAsync();

                return allUnits.Cast<object>().ToList();
            }
            else
            {
                // ✅ Return only units assigned to the employee
                var units = await (from emp in _context.Employees
                                   join u in _context.UnitsMaster on emp.UnitId equals u.UnitId
                                   where emp.Id == EmpId
                                   orderby u.UnitId
                                   select new
                                   {
                                       UnitId = u.UnitId,
                                       UnitName = u.UnitName
                                   }).ToListAsync();

                return units.Cast<object>().ToList();
            }
        }
        //    public async Task<AuthResponseDto> AuthenticateAsync(LoginDto request)
        //    {
        //        var user = await _context.Users
        //            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
        //            .Include(u => u.Employee)
        //            .FirstOrDefaultAsync(u => u.Username == request.Username && u.IsActive);

        //        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        //            return null;

        //        var tokenHandler = new JwtSecurityTokenHandler();
        //        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

        //        var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),          // "nameid"
        //    new Claim(ClaimTypes.Name, user.Username),                         // "name"
        //    new Claim(JwtRegisteredClaimNames.Sub, user.Username)              // "sub"
        //};

        //        // Add roles
        //        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        //        foreach (var role in roles)
        //        {
        //            claims.Add(new Claim(ClaimTypes.Role, role)); // "role"
        //        }

        //        var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Name, ClaimTypes.Role);

        //        var tokenDescriptor = new SecurityTokenDescriptor
        //        {
        //            Subject = identity,
        //            NotBefore = DateTime.UtcNow.AddSeconds(-5), // fix: slight skew
        //            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
        //            SigningCredentials = new SigningCredentials(
        //      new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        //            Issuer = _jwtSettings.Issuer,
        //            Audience = _jwtSettings.Audience
        //        };

        //        var token = tokenHandler.CreateToken(tokenDescriptor);
        //        var tokenString = tokenHandler.WriteToken(token);

        //        return new AuthResponseDto
        //        {
        //            Token = tokenString,
        //            Username = user.Username,
        //            Roles = roles,
        //            EmployeeName = user.Employee?.FullName
        //        };
        //    }


        //public async Task<UserInfoDto?> GetUserInfoAsync(string username)
        //{
        //    var user = await _context.Users
        //        .Include(u => u.UserRoles)
        //            .ThenInclude(ur => ur.Role)
        //                .ThenInclude(r => r.RolePermissions)
        //                    .ThenInclude(rp => rp.Permission)
        //        .Include(u => u.UserRoles)
        //            .ThenInclude(ur => ur.Role)
        //                .ThenInclude(r => r.RoleMenus)
        //                    .ThenInclude(rm => rm.Menu)
        //        .FirstOrDefaultAsync(u => u.Username == username);

        //    if (user == null)
        //        return null;

        //    return new UserInfoDto
        //    {
        //        Username = user.Username,
        //        Roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList(),
        //        Permissions = user.UserRoles
        //            .SelectMany(ur => ur.Role.RolePermissions)
        //            .Select(rp => rp.Permission.Name)
        //            .Distinct()
        //            .ToList(),
        //        Menu = user.UserRoles
        //            .SelectMany(ur => ur.Role.RoleMenus)
        //            .Select(rm => new MenuDto
        //            {
        //                Id = rm.Menu.Id,
        //                Name = rm.Menu.Title,
        //                Route = rm.Menu.Route
        //            })
        //            .Distinct()
        //            .ToList()
        //    };
        //}
        //public async Task<UserInfoDto?> GetUserInfoAsync(string username)
        //{
        //    // 1. Get the user
        //    var user = await _context.Users
        //        .FirstOrDefaultAsync(u => u.Username == username);

        //    if (user == null)
        //        return null;

        //    // 2. Get all active menus
        //    var allMenus = await _context.Menus
        //        .Where(m => m.IsActive)
        //        .OrderBy(m => m.SortOrder)
        //        .ToListAsync();

        //    // 3. Build recursive menu tree
        //    List<MenuDto> BuildMenuTree(List<Menu> menus, int? parentId)
        //    {
        //        return menus
        //            .Where(m => m.ParentId == parentId)
        //            .OrderBy(m => m.SortOrder)
        //            .Select(m => new MenuDto
        //            {
        //                Id = m.Id,
        //                Name = m.Title,
        //                Route = m.Route,
        //                Children = BuildMenuTree(menus, m.Id)
        //            })
        //            .ToList();
        //    }
        //    var menuTree = BuildMenuTree(allMenus, null);

        //    Console.WriteLine("DEBUG MENU TREE:");
        //    void PrintMenu(List<MenuDto> menus, string prefix = "")
        //    {
        //        foreach (var m in menus)
        //        {
        //            Console.WriteLine($"{prefix}- {m.Name} ({m.Route})");
        //            if (m.Children != null && m.Children.Any())
        //            {
        //                PrintMenu(m.Children, prefix + "  ");
        //            }
        //        }
        //    }

        //    // 4. Return full user info with menu
        //    return new UserInfoDto
        //    {
        //        Username = user.Username,
        //        Roles = new List<string>(),        // optional, since role isn't required now
        //        Permissions = new List<string>(),  // optional
        //        Menu = BuildMenuTree(allMenus, null)
        //    };


        //}

        //public async Task<UserInfoDto?> GetUserInfoAsync(string username)
        //{
        //    var user = await _context.Users
        //        .Include(u => u.UserRoles)
        //            .ThenInclude(ur => ur.Role)
        //                .ThenInclude(r => r.RolePermissions)
        //                    .ThenInclude(rp => rp.Permission)
        //        .Include(u => u.UserRoles)
        //            .ThenInclude(ur => ur.Role)
        //                .ThenInclude(r => r.RoleMenus)
        //                    .ThenInclude(rm => rm.Menu)
        //                        .ThenInclude(m => m.Children)
        //        .FirstOrDefaultAsync(u => u.Username == username);

        //    if (user == null) return null;

        //    var allMenus = user.UserRoles
        //        .SelectMany(ur => ur.Role.RoleMenus)
        //        .Select(rm => rm.Menu)
        //        .Where(m => m.IsActive)
        //        .Distinct()
        //        .OrderBy(m => m.SortOrder)
        //        .ToList();

        //    List<MenuDto> BuildMenuTree(List<Menu> menus, int? parentId)
        //    {
        //        return menus
        //            .Where(m => m.ParentId == parentId)
        //            .OrderBy(m => m.SortOrder)
        //            .Select(m => new MenuDto
        //            {
        //                Id = m.Id,
        //                Name = m.Title,
        //                Route = m.Route,
        //                Children = BuildMenuTree(menus, m.Id)
        //            })
        //            .ToList();
        //    }

        //    return new UserInfoDto
        //    {
        //        Username = user.Username,
        //        Roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList(),
        //        Permissions = user.UserRoles
        //            .SelectMany(ur => ur.Role.RolePermissions)
        //            .Select(rp => rp.Permission.Name)
        //            .Distinct()
        //            .ToList(),
        //        Menu = BuildMenuTree(allMenus, null)
        //    };
        //}
    }

}
