using OwlDocs.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlDocs.Domain.DocumentCache
{
    public class DocumentCache : IDocumentCache
    {
        public DocumentTree Tree { get; set; }
    }
}
