using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OwlDocs.Web.Options;

namespace OwlDocs.Web.Authorization
{
    public class DocumentWritersHandler : AuthorizationHandler<DocumentWritersRequirement>
    {
        private readonly AuthOptions _authOptions;

        public DocumentWritersHandler(IOptions<AuthOptions> authOptions)
        {
            _authOptions = authOptions.Value;
        }


        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            DocumentWritersRequirement requirement)
        {
            if (_authOptions.Type == AuthorizationType.Anonymous)
            {
                context.Succeed(requirement);
            }
            else if (_authOptions.Type == AuthorizationType.ActiveDirectory)
            {
                foreach (var group in _authOptions.DocumentWriters)
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
