using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.ValidationHandlers;

public abstract class AttributeAuthorizationHandler<TRequirement, TAttribute> : AuthorizationHandler<TRequirement> where TRequirement : IAuthorizationRequirement where TAttribute : Attribute
{

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement)
    {
        IEnumerable<TAttribute> attributes = new List<TAttribute>();
        if ((context.Resource as DefaultHttpContext)?.GetEndpoint() is RouteEndpoint routeEndpoint)
        {
            var actionDescriptor = routeEndpoint.Metadata.OfType<ControllerActionDescriptor>().SingleOrDefault();
            attributes = actionDescriptor?.MethodInfo.GetCustomAttributes(typeof(TAttribute), false).Cast<TAttribute>();
        }
        return HandleRequirementAsync(context, requirement, attributes);
    }

    protected abstract Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement, IEnumerable<TAttribute> attributes);
}