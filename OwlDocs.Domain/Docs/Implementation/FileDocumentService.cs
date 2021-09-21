using OwlDocs.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

using Markdig;

namespace OwlDocs.Domain.Docs
{
    public class FileDocumentService : IDocumentService
    {
        private readonly DirectoryInfo _root;
        private readonly MarkdownPipeline _pipeline;

        public FileDocumentService(MarkdownPipeline pipeline, IConfiguration config)
        {
            _root = new DirectoryInfo(config["DocumentProviderSettings:DirectoryRoot"]);
            _pipeline = pipeline;
        }

        public async Task<OwlDocument> CreateDocument(OwlDocument newDocument)
        {
            var document = new OwlDocument();

            var relPath = newDocument.ParentPath + "/" + newDocument.Name;
            document.Path = relPath;
            relPath = relPath.Remove(0, 1);            

            var absPath = Path.Combine(_root.FullName, relPath);
            using var fs = File.Create(absPath);
            await fs.DisposeAsync();

            return document;
        }

        public Task<OwlDocument> GetDocumentById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<OwlDocument> GetDocumentByPath(string path)
        {
            path = path.Remove(0, 1);
            var file = new FileInfo(Path.Combine(_root.FullName, path));

            var document = new OwlDocument();
            document.Name = file.Name;
            document.Markdown = await File.ReadAllTextAsync(file.FullName);
            document.Html = Markdown.ToHtml(document.Markdown);
            document.Path = file.FullName.Replace(_root.FullName, "").Replace("\\", "/");

            return document;
        }

        public async Task<DocumentTree> GetDocumentTree()
        {
            var rootDocument = new DocumentTree()
            { 
               Path = "/"
            };

            WalkFiles(rootDocument, _root);

            await Task.Delay(100);

            return rootDocument;
        }

        public async Task<int> UpdateDocument(OwlDocument document)
        {
            // write text to file 
            var relPath = document.Path.Remove(0, 1);
            var path = Path.Combine(_root.FullName, relPath);

            await File.WriteAllTextAsync(path, document.Markdown);

            return 0;
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
                    Path = file.FullName.Replace(_root.FullName, "").Replace("\\", "/"),
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
                    Path = dir.FullName.Replace(_root.FullName, "").Replace("\\", "/"),
                    Type = DocumentType.Directory
                };

                WalkFiles(newDoc, dir);

                tree.Children.Add(newDoc);
            }
        }
    }
}
