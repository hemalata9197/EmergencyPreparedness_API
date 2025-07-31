using System.Security.Claims;
using System;
using EmergencyManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace EmergencyManagement.Middleware
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;


        public PermissionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, EHSDbContext dbContext)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userId = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var userRoles = await dbContext.UserRoles
                    .Where(ur => ur.UserId == userId)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();

                var path = context.Request.Path.Value?.ToLower() ?? "";

                //var matchedPermission = await dbContext.Permissions
                //    .Where(p => path.Contains(p.Name.ToLower()))
                //    .Select(p => p.Id)
                //    .ToListAsync();

                //var allowed = await dbContext.RolePermissions
                //    .Where(rp => userRoles.Contains(rp.RoleId) && matchedPermission.Contains(rp.PermissionId))
                //    .AnyAsync();

                //if (!allowed)
                //{
                //    context.Response.StatusCode = 403;
                //    await context.Response.WriteAsync("Forbidden: Permission Denied");
                //    return;
                //}
            }

            await _next(context);
        }
    }
}
