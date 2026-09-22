using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace EmployeeManagement.Api.Security;

public sealed class AuthorizationSecurityOperationTransformer
    : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var endpointMetadata =
            context.Description.ActionDescriptor.EndpointMetadata;
        var requiresAuthorization =
            endpointMetadata.OfType<IAuthorizeData>().Any() &&
            !endpointMetadata.OfType<IAllowAnonymous>().Any();

        if (!requiresAuthorization)
        {
            // An empty operation-level requirement overrides the global Bearer requirement.
            operation.Security = [];
        }

        return Task.CompletedTask;
    }
}
