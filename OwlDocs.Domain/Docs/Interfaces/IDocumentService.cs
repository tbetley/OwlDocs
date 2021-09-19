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
        Task<OwlDocument> GetDocumentByPath(string path);
        Task<OwlDocument> GetDocumentById(int id);
        Task<OwlDocument> CreateDocument(OwlDocument newDocument);
        Task<int> UpdateDocument(OwlDocument document);
    }
}
