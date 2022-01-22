using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

namespace OwlDocs.Models
{ 
    public class Document
    {
        public Document(FileInfo file, string rootPath)
        {
            Name = file.Name;
            Path = file.FullName.Replace(rootPath, "").Replace("\\", "/");
        }

        public Document()
        {

        }

        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Path { get; set; }
        public string ParentPath { get; set; }
        public DocumentType Type { get; set; }
        public string Name { get; set; }
        public string Markdown { get; set; }
        public string Html { get; set; }
        public byte[] Data { get; set; }
    }
}
