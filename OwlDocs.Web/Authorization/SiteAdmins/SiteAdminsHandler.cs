using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OwlDocs.Web.Options;

namespace OwlDocs.Web.Authorization
{
    public class SiteAdminsHandler : AuthorizationHandler<SiteAdminsRequirement>
    {
        private readonly AuthOptions _authOptions;

        public SiteAdminsHandler(IOptions<AuthOptions> authOptions)
        {
            _authOptions = authOptions.Value;
        }


        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, SiteAdminsRequirement requirement)
        {
            if (_authOptions.Type == AuthorizationType.Anonymous)
            {
                context.Succeed(requirement);
            }
            else if (_authOptions.Type == AuthorizationType.ActiveDirectory)
            {
                foreach (var group in _authOptions.SiteAdmins)
                {
                    if (context.User.IsInRole(group))
                    {
                        context.Succeed(requirement);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
