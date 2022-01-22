using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OwlDocs.Models;

namespace OwlDocs.Domain.DocumentCache
{
    public interface IDocumentCache
    {
        DocumentTree Tree { get; set; }  
    }
}
