using Microsoft.AspNetCore.Authorization;

namespace E_commerce.API.Filtter
{
    public class PermetionAuthrizationHandelar: AuthorizationHandler<PermetionRequirment>
    {
        protected override  Task HandleRequirementAsync(AuthorizationHandlerContext context, PermetionRequirment requirement)
        {
            if (context.User == null)
                return Task.CompletedTask;
            var canacess = context.User.Claims.Any(r => r.Type == "Permission" && r.Value == requirement.Permission);

            if (canacess)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}
