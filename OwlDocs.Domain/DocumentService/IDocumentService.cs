using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OwlDocs.Models;

namespace OwlDocs.Domain.DocumentService
{
    public interface IDocumentService
    {
        Task<DocumentTree> GetDocumentTree();
        Task<Document> GetDocumentByPath(string path);
        Task<Document> GetDocumentById(int id);
        Task<Document> CreateDocument(Document newDocument);
        Task<Document> GetDocumentImage(string path);
        Task<int> UpdateDocument(Document document);
        Task<int> DeleteDocument(Document document);
    }
}
