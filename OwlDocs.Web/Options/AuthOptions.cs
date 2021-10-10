using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace OwlDocs.Web.Options
{
    public class AuthOptions
    {
        public const string Authorization = "Authorization";
        public AuthorizationType Type { get; set; }

        public const string DocumentReaderPolicy = "DocumentReaderPolicy";
        public List<string> DocumentReaders { get; set; }

        public const string DocumentWritersPolicy = "DocumentWritersPolicy";
        public List<string> DocumentWriters { get; set; }

        public const string SiteAdminPolicy = "SiteAdminPolicy";
        public List<string> SiteAdmins { get; set; }
    }

    public enum AuthorizationType { Anonymous, ActiveDirectory }
}
