using OwlDocs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlDocs.Domain.Docs
{
    public class FileDocumentService : IDocumentService
    {
        private readonly DirectoryInfo _root;

        public FileDocumentService(string root)
        {
            _root = new DirectoryInfo(root);
        }

        public Task<OwlDocument> CreateDocument(OwlDocument newDocument)
        {
            throw new NotImplementedException();
        }

        public Task<OwlDocument> GetDocumentById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<OwlDocument> GetDocumentByPath(string path)
        {
            throw new NotImplementedException();
        }

        public async Task<DocumentTree> GetDocumentTree()
        {
            var rootDocument = new DocumentTree()
            { 
               Path = _root.FullName
            };

            WalkFiles(rootDocument, _root);

            await Task.Delay(100);

            return rootDocument;
        }

        public Task<int> UpdateDocument(OwlDocument document)
        {
            throw new NotImplementedException();
        }

        
        private void WalkFiles(DocumentTree tree, DirectoryInfo root)
        {
            var files = root.GetFiles("*.md");
            var dirs = root.GetDirectories();

            // add each file to doc tree
            foreach (var file in files)
            {
                var newDoc = new DocumentTree()
                {
                    Name = file.Name,
                    Path = file.FullName.Replace(_root.FullName, ""),
                    Type = DocumentType.File                
                };

                tree.Children.Add(newDoc);
            }

            // add each directory to doctree and go through children
            foreach (var dir in dirs)
            {
                var newDoc = new DocumentTree()
                {
                    Name = dir.Name,
                    Path = dir.FullName.Replace(_root.FullName, ""),
                    Type = DocumentType.Directory
                };

                WalkFiles(newDoc, dir);

                tree.Children.Add(newDoc);
            }
        }
    }
}
