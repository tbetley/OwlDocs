using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OwlDocs.Models;

namespace OwlDocs.Data.Repositories
{
    public interface ISqliteRepository
    {
        Task<Document> GetDocumentByPath(string path);
        Task<Document> GetDocumentById(int id);
        Task<List<Document>> GetDocumentsByParentId(int parentId);
        Task<Document> CreateDocument(Document document);
        Task<Document> UpdateDocument(Document document);
        Task<int> DeleteDocumentById(int id);
        Task<List<DocumentTree>> GetOrderedTree();
        Task<bool> AnyDocuments();
        Task<int> EnsureCreated();
    }
}
