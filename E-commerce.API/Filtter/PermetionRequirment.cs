using Microsoft.AspNetCore.Authorization;

namespace E_commerce.API.Filtter
{
    public class PermetionRequirment : IAuthorizationRequirement
    {
        public string Permission { get; private set; }
        public PermetionRequirment(string permission)
        {
            Permission = permission;
        }
    }
}
