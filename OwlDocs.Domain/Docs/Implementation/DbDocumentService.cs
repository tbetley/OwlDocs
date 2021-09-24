using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Markdig;

using OwlDocs.Models;
using OwlDocs.Data;

namespace OwlDocs.Domain.Docs
{
    public class DbDocumentService : IDocumentService
    {
        private readonly OwlDocsContext _dbContext;
        private readonly MarkdownPipeline _pipeline;

        public DbDocumentService(OwlDocsContext dbContext, MarkdownPipeline pipeline)
        {
            _dbContext = dbContext;
            _pipeline = pipeline;
        }

        public async Task<OwlDocument> CreateDocument(OwlDocument newDocument)
        {
            // get parent path
            var parentPath = await _dbContext.OwlDocuments
                .Where(d => d.Id == newDocument.ParentId)
                .Select(o => o.Path)
                .FirstAsync();
            

            // set new path
            if (parentPath == "/")
            {
                newDocument.Path = parentPath + newDocument.Name;
            }
            else
            {
                newDocument.Path = parentPath + "/" + newDocument.Name;
            }

            // validate that a document does not exist with that same path
            var duplicate = await _dbContext.OwlDocuments.FirstOrDefaultAsync(d => d.Path == newDocument.Path);

            if (duplicate != null)
            {
                throw new Exception($"An Item Already Exists With a Path: {newDocument.Path}");
            }

            // insert
            var val = await _dbContext.OwlDocuments.AddAsync(newDocument);
            await _dbContext.SaveChangesAsync();

            return val.Entity;
        }


        public async Task<int> DeleteDocument(OwlDocument document)
        {
            // check that document exists
            var entity = await _dbContext.OwlDocuments.FindAsync(document.Id);

            if (entity == null)
            {
                throw new Exception("Document Not Found");
            }

            // check for children
            var children = await _dbContext.OwlDocuments.Where(d => d.ParentId == document.Id).ToListAsync();

            // recursively delete children
            if (children != null)
            {
                foreach(var child in children)
                {
                    await DeleteDocument(child);
                }
            }

            // delete
            _dbContext.OwlDocuments.Remove(entity);
            return await _dbContext.SaveChangesAsync();
        }


        public async Task<OwlDocument> GetDocumentById(int id)
        {
            var document = await _dbContext.OwlDocuments.FindAsync(id);

            return document;
        }


        public async Task<OwlDocument> GetDocumentByPath(string path)
        {
            var document = await _dbContext.OwlDocuments.Where(d => d.Path == path).FirstAsync();

            return document;
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

            // For each document, find it's parent in the tree and add to document tree
            foreach(var item in docTrees)
            {
                // base case, set root (first item)
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

        public async Task<int> UpdateDocument(OwlDocument document)
        {
            // get entity from db
            var entity = await _dbContext.OwlDocuments.FirstAsync(i => i.Id == document.Id);

            // update values
            entity.Html = Markdown.ToHtml(document.Markdown, _pipeline);
            entity.Markdown = document.Markdown;

            // save changes
            return await _dbContext.SaveChangesAsync();
        }


        /// <summary>
        /// Returns the document with given parent id if it exists in the root document.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
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
