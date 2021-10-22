using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwlDocs.Web.Authorization
{
    public static class Policies
    {
        public const string DocumentReadersPolicy = "DocumentReadersPolicy";
        public const string DocumentWritersPolicy = "DocumentWritersPolicy";
        public const string SiteAdminsPolicy = "SiteAdminsPolicy";
    }
}
