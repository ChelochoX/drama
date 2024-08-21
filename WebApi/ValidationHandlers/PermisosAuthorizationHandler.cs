using System.Security.Claims;
using Application.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace WebApi.ValidationHandlers;

public class PermisosAuthorizationHandler : AttributeAuthorizationHandler<PermissionAuthorizationRequirement, EndpointAttribute>
{
    private readonly PermisosConfiguration _configuration;

    public PermisosAuthorizationHandler(PermisosConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionAuthorizationRequirement requirement, IEnumerable<EndpointAttribute> attributes)
    {
        foreach (var permissionAttribute in attributes)
        {
            if (!AuthorizeAsync(context.User, permissionAttribute.Name))
            {
                return;
            }
        }
        context.Succeed(requirement);
    }

    private bool AuthorizeAsync(ClaimsPrincipal user, string endpoint)
    {
        string[] permisos = _configuration.Permisos
            .Where(pair => pair.Value.Contains(endpoint))
            .Select(pair => pair.Key)
            .ToArray();
        if (user.Identity.IsAuthenticated && user.HasClaim(e => e.Type == "Permiso"))
        {
            var permisosUser = user.Claims.Where(e => e.Type == "Permiso");
            bool existePermiso = permisos.Intersect(permisosUser.Select(p => p.Value)).Any();
            return existePermiso;
        }
        return false;
    }
}

public class PermissionAuthorizationRequirement : IAuthorizationRequirement;
