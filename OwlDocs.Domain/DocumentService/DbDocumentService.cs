using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Markdig;

using OwlDocs.Models;
using OwlDocs.Data;
using OwlDocs.Data.Repositories;

namespace OwlDocs.Domain.DocumentService
{
    public class DbDocumentService : IDocumentService
    {
        private readonly ISqliteRepository _sqliteRepo;
        private readonly MarkdownPipeline _pipeline;

        public DbDocumentService(MarkdownPipeline pipeline, ISqliteRepository sqliteRepo)
        {
            _pipeline = pipeline;
            _sqliteRepo = sqliteRepo;
        }

        public async Task<Document> CreateDocument(Document newDocument)
        {            
            if (newDocument.Type == (int)DocumentType.Root)
            {
                newDocument.Path = "/";
            }          
            else
            {
                var parentDocument = await _sqliteRepo.GetDocumentById((int)newDocument.ParentId);
                var parentPath = parentDocument.Path;
                var parentUriPath = parentDocument.UriPath;

                // set new path
                var uriPath = Uri.EscapeDataString(newDocument.Name);
                if (parentPath == "/")
                {
                    newDocument.Path = parentPath + newDocument.Name;
                    newDocument.UriPath = parentUriPath + uriPath;
                }
                else
                {
                    newDocument.Path = parentPath + "/" + newDocument.Name;
                    newDocument.UriPath = parentUriPath + "/" + uriPath;
                }
            }
            
            // validate that a document does not exist with that same path
            var duplicate = await _sqliteRepo.GetDocumentByPath(newDocument.Path);

            if (duplicate != null)
            {
                throw new Exception($"An Item Already Exists With a Path: {newDocument.Path}");
            }

            // insert            
            var val = await _sqliteRepo.CreateDocument(newDocument);

            return val;
        }


        public async Task<int> DeleteDocument(Document document)
        {
            // check that document exists
            var entity = await _sqliteRepo.GetDocumentById(document.Id);

            if (entity == null)
            {
                throw new Exception("Document Not Found");
            }

            // check for children
            var children = await _sqliteRepo.GetDocumentsByParentId(document.Id);

            // recursively delete children
            if (children != null)
            {
                foreach(var child in children)
                {
                    await DeleteDocument(child);
                }
            }

            // delete
            var val = await _sqliteRepo.DeleteDocumentById(document.Id);

            return 0;
        }


        public async Task<Document> GetDocumentByPath(string path)
        {
            var document = await _sqliteRepo.GetDocumentByPath(path);
            return document;
        }

        public async Task<Document> GetDocumentImage(string path)
        {
            var imageDoc = await GetDocumentByPath(FormatPath(path));
            return imageDoc;
        }

        public async Task<DocumentTree> GetDocumentTree()
        {
            var docs = await _sqliteRepo.GetOrderedTree();

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
                var duplicate = await _sqliteRepo.GetDocumentByPath(newPath);

                if (duplicate != null)
                {
                    throw new Exception("Item with the path " + newPath + " already exists.");
                }

                // get new parent directory
                var newParent = await _sqliteRepo.GetDocumentById((int)document.ParentId);

                // update document
                var updatedDoc = await _sqliteRepo.GetDocumentById(document.Id);
                updatedDoc.Path = newPath;
                updatedDoc.UriPath = newParent.UriPath + "/" + Uri.EscapeDataString(document.Name);
                updatedDoc.ParentPath = newParent.Path;
                updatedDoc.ParentId = newParent.Id;

                await _sqliteRepo.UpdateDocument(updatedDoc);

                // get elements that are children of the updated document
                var children = await _sqliteRepo.GetDocumentsByParentId(updatedDoc.Id);

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

                return 0;
            }

            // get entity from db
            var entity = await _sqliteRepo.GetDocumentById(document.Id);

            // update values
            entity.Html = Markdown.ToHtml(document.Markdown, _pipeline);
            entity.Markdown = document.Markdown;
            
            // check rename
            if (document.Name != entity.Name)
            {
                entity.Name = document.Name;
                entity.Path = document.Path.Remove(document.Path.LastIndexOf('/')) + '/' + document.Name;
                entity.UriPath = entity.UriPath.Remove(entity.UriPath.LastIndexOf('/')) + '/' + Uri.EscapeDataString(document.Name);
            }

            // save changes
            await _sqliteRepo.UpdateDocument(entity);

            return 0;
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
