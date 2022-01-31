using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using Markdig;

using OwlDocs.Models;
using OwlDocs.Data;
using Microsoft.Extensions.Configuration;

namespace OwlDocs.Domain.DocumentService
{
    public class DbDocumentService : IDocumentService
    {
        private readonly OwlDocsContext _dbContext;
        private readonly MarkdownPipeline _pipeline;
        private readonly IConfiguration _config;

        public DbDocumentService(OwlDocsContext dbContext, MarkdownPipeline pipeline, IConfiguration config)
        {
            _dbContext = dbContext;
            _pipeline = pipeline;
            _config = config;
        }

        public async Task<Document> CreateDocument(Document newDocument)
        {
            // get parent path
            var list = await _dbContext.Documents.Where(d => d.Id == newDocument.ParentId).ToListAsync();
            /*.Select(o => o.Path)
            .FirstAsync();*/
            string parentPath = list.First().Path;

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
            var duplicate = await _dbContext.Documents.FirstOrDefaultAsync(d => d.Path == newDocument.Path);

            if (duplicate != null)
            {
                throw new Exception($"An Item Already Exists With a Path: {newDocument.Path}");
            }

            // insert
            var val = await _dbContext.Documents.AddAsync(newDocument);
            await _dbContext.SaveChangesAsync();

            return val.Entity;
        }


        public async Task<int> DeleteDocument(Document document)
        {
            // check that document exists
            var entity = await _dbContext.Documents.FindAsync(document.Id);

            if (entity == null)
            {
                throw new Exception("Document Not Found");
            }

            // check for children
            var children = await _dbContext.Documents.Where(d => d.ParentId == document.Id).ToListAsync();

            // recursively delete children
            if (children != null)
            {
                foreach(var child in children)
                {
                    await DeleteDocument(child);
                }
            }

            // delete
            _dbContext.Documents.Remove(entity);
            return await _dbContext.SaveChangesAsync();
        }


        public async Task<Document> GetDocumentById(int id)
        {
            var document = await _dbContext.Documents.FindAsync(id);

            return document;
        }


        public async Task<Document> GetDocumentByPath(string path)
        {
            var document = await _dbContext.Documents.Where(d => d.Path == path).FirstAsync();

            return document;
        }

        public async Task<Document> GetDocumentImage(string path)
        {
            var imageDoc = await GetDocumentByPath(FormatPath(path));

            return imageDoc;
        }

        public async Task<DocumentTree> GetDocumentTree()
        {
            var docs = await _dbContext.Documents.OrderBy(d => d.ParentId).Select(s => new DocumentTree()
            {
                Id = s.Id,
                ParentId = s.ParentId,
                Name = s.Name,
                Path = s.Path,
                Type = s.Type
            }).ToListAsync();                

            // get root item
            DocumentTree documentTree = null;

            // For each document, find it's parent in the tree and add to document tree
            foreach(var item in docs)
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

        public async Task<int> UpdateDocument(Document document)
        {
            // check if parent changed, if so, move item
            if (document.ParentPath != null &&
                document.Path.Remove(document.Path.LastIndexOf("/")) != document.ParentPath)
            {
                var newPath = document.ParentPath + "/" + document.Name;

                // check for duplicate
                var duplicate = await _dbContext.Documents.FirstOrDefaultAsync(d => d.Path == newPath);

                if (duplicate != null)
                {
                    throw new Exception("Item with the path " + newPath + " already exists.");
                }

                // get new parent directory
                var newParent = await _dbContext.Documents.FirstAsync(d => d.Id == document.ParentId);

                // update document
                var updatedDoc = await _dbContext.Documents.FirstOrDefaultAsync(d => d.Id == document.Id);
                updatedDoc.Path = newPath;
                updatedDoc.ParentPath = newParent.Path;
                updatedDoc.ParentId = newParent.Id;

                // get elements that are children of the updated document
                var children = await _dbContext.Documents.Where(d => d.ParentId == updatedDoc.Id).ToListAsync();

                // update children
                if (children != null && children.Count > 0)
                {
                    foreach (var child in children)
                    {
                        // create doc object for updating
                        var updateChild = new Document();
                        updateChild.Id = child.Id;
                        updateChild.Path = child.Path;
                        updateChild.Name = child.Name;
                        updateChild.ParentId = updatedDoc.Id;
                        updateChild.ParentPath = updatedDoc.Path;
                        updateChild.Type = updatedDoc.Type;

                        await UpdateDocument(updateChild);
                    }
                }


                return await _dbContext.SaveChangesAsync();
            }


            // get entity from db
            var entity = await _dbContext.Documents.FirstAsync(i => i.Id == document.Id);

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


        private static string FormatPath(string path)
        {
            if (path[0] != '/')
            {
                path = "/" + path;
            }

            if (path[path.Length - 1] == '/')
            {
                path = path.Remove(path.Length - 1);
            }

            return path;
        }
    }
}
