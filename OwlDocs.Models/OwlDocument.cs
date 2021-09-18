using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlDocs.Models
{ 
    public class OwlDocument
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Path { get; set; }
        public DocumentType Type { get; set; }
        public string Name { get; set; }
        public string Markdown { get; set; }
        public string Html { get; set; }
    }
}
