using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OwlDocs.Models;

namespace OwlDocs.Domain.Docs
{
    public interface IDocumentService
    {
        Task<DocumentTree> GetDocumentTree();
        OwlDocument GetDocument(string path);
    }
}
