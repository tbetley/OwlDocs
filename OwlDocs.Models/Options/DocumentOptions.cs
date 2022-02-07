using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace OwlDocs.Models.Options
{
    public class DocumentOptions
    {
        public const string DocumentSettings = "DocumentSettings";

        // providers
        public const string Database = "Database";
        public const string File = "File";

        // database types
        public const string Sqlite = "Sqlite";

        [Required]
        public string Provider { get; set; }
        public string DatabaseType { get; set; }
        
        public string DirectoryRoot { get; set; }

        [Required]
        public List<string> AcceptedTextFileTypes { get; set; }
        [Required]
        public List<string> AcceptedImageFileTypes { get; set; }
    }
}
