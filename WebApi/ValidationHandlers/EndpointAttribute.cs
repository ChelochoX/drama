using Microsoft.AspNetCore.Authorization;

namespace WebApi.ValidationHandlers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class EndpointAttribute : AuthorizeAttribute
{
    public string Name { get; }

    public EndpointAttribute(string name) : base("Endpoint")
    {
        Name = name;
    }
}