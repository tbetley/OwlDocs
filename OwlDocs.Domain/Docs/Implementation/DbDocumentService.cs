using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using OwlDocs.Models;
using OwlDocs.Data;

namespace OwlDocs.Domain.Docs
{
    public class DbDocumentService : IDocumentService
    {
        private readonly OwlDocsContext _dbContext;

        public DbDocumentService(OwlDocsContext dbContext)
        {
            _dbContext = dbContext;
        }

        public OwlDocument GetDocument(string path)
        {
            throw new NotImplementedException();
        }

        public async Task<DocumentTree> GetDocumentTree()
        {
            var docTrees = await _dbContext.OwlDocuments
                .Select(s => new DocumentTree()
                {
                    Id = s.Id,
                    ParentId = s.ParentId,
                    Name = s.Name,
                    Path = s.Path,
                    Type = s.Type
                })
                .OrderBy(s => s.ParentId)
                .ToListAsync();

            // get root item
            DocumentTree documentTree = null;

            foreach(var item in docTrees)
            {
                // base case, set root
                if (item.ParentId == null)
                {
                    documentTree = item;
                }
                // find parent
                else
                {
                    var parentNode = FindParent(documentTree, (int)item.ParentId);

                    if (parentNode != null) // parent found
                    {
                        parentNode.Children.Add(item);
                    }
                }
            }

            return documentTree;
        }


        private DocumentTree FindParent(DocumentTree root, int parentId)
        {
            // base case
            if (root.Id == parentId)
                return root;

            if (root.Children.Count > 0)
            {
                foreach (var child in root.Children)
                {
                    var parent = FindParent(child, parentId);

                    if (parent != null)
                        return parent;
                }

                return null;
            }
            else
            {
                return null;
            }
        }
    }
}
