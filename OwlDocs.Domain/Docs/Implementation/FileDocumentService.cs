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
        private readonly List<string> AcceptedFileTypes;

        public FileDocumentService(MarkdownPipeline pipeline, IConfiguration config)
        {
            _root = new DirectoryInfo(config["DocumentProviderSettings:DirectoryRoot"]);
            AcceptedFileTypes = config.GetSection("AcceptedFileTypes").Get<List<string>>();
            _pipeline = pipeline;
        }

        public async Task<OwlDocument> CreateDocument(OwlDocument newDocument)
        {
            var document = new OwlDocument();

            var relPath = Path.Combine(newDocument.ParentPath, newDocument.Name);
            document.Path = relPath;
            relPath = relPath.Remove(0, 1);

            var absPath = Path.Combine(_root.FullName, relPath);

            // check if file already exists at path
            if (newDocument.Type == DocumentType.File)
            {               
                if (File.Exists(absPath))
                {
                    throw new Exception($"A File Already Exists With a Path: {absPath}");
                }

                using var fs = File.Create(absPath);
                await fs.DisposeAsync();
            }
            else if (newDocument.Type == DocumentType.Image)
            {
                if (File.Exists(absPath))
                {
                    throw new Exception($"A File Already Exists with a Path: {absPath}");
                }

                await File.WriteAllBytesAsync(absPath, newDocument.Data);

            }
            else if (newDocument.Type == DocumentType.Directory)
            {
                if (Directory.Exists(absPath))
                {
                    throw new Exception($"A Folder Already Exists With a Path: {absPath}");
                }

                // throws if not successful
                var dirInfo = Directory.CreateDirectory(absPath);
            }
            
            return document;
        }

        public Task<int> DeleteDocument(OwlDocument document)
        {
            var path = Path.Combine(_root.FullName, document.Path.Remove(0,1));

            if (document.Type == DocumentType.File || document.Type == DocumentType.Image)
            {
                File.Delete(path);
            }
            else if (document.Type == DocumentType.Directory)
            {
                // recursively delete folder/files
                Directory.Delete(path, true);
            }
            else
            {
                throw new Exception("File Type Cannot Be Deleted");
            }
            
            return Task.FromResult(0);
        }

        public Task<OwlDocument> GetDocumentById(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<OwlDocument> GetDocumentByPath(string path)
        {
            path = path.Remove(0, 1);
            var file = new FileInfo(Path.Combine(_root.FullName, path));

            var document = new OwlDocument(file, _root.FullName);
            
            if (AcceptedFileTypes.Contains(file.Extension.ToLower()))
            {
                document.Type = DocumentType.Image;
            }
            else if (file.Extension == ".md")
            {
                document.Markdown = await File.ReadAllTextAsync(file.FullName);
                document.Html = Markdown.ToHtml(document.Markdown);
            }
            
            return document;
        }

        public async Task<OwlDocument> GetDocumentImage(string path)
        {
            var fullPath = Path.Combine(_root.FullName, FormatPath(path));
            FileInfo imageFile = new FileInfo(fullPath);

            if (!imageFile.Exists)
            {
                return null;
            }

            var doc = new OwlDocument();
            doc.Name = imageFile.Name;
            doc.Data = await File.ReadAllBytesAsync(fullPath);
            doc.Type = DocumentType.Image;

            return doc;
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
            // check if parent changed, if so, move item
            if (document.ParentPath != null &&
                document.Path.Remove(document.Path.LastIndexOf("/")) != document.ParentPath)
            {
                var destinationPath = Path.Combine(_root.FullName, document.ParentPath.Remove(0,1));
                var docPath = Path.Combine(_root.FullName, document.Path.Remove(0, 1));
                if (document.Type == DocumentType.File || document.Type == DocumentType.Image)
                {
                    File.Move(docPath, Path.Combine(destinationPath, document.Name));
                }
                else if (document.Type == DocumentType.Directory)
                {
                    Directory.Move(docPath, Path.Combine(destinationPath, document.Name));
                }

                return 0;
            }
            

            // update text 
            var relPath = document.Path.Remove(0, 1);
            var path = Path.Combine(_root.FullName, relPath).Replace('/', '\\');

            // Get Current Doc file info
            var oldFile = new FileInfo(path);

            // rename file if name changed
            if (oldFile.Name != document.Name)
            {
                var newPath = Path.Combine(path.Remove(path.LastIndexOf('\\')), document.Name);
                oldFile.MoveTo(newPath);

                path = newPath;
            }

            await File.WriteAllTextAsync(path, document.Markdown);

            return 0;
        }


        private void WalkFiles(DocumentTree tree, DirectoryInfo root)
        {
            var allFiles = root.GetFiles();
            var mdFiles = allFiles.Where(f => f.Extension.ToLower() == ".md");
            var imageFiles = allFiles.Where(f => AcceptedFileTypes.Contains(f.Extension.ToLower()));

            var dirs = root.GetDirectories();

            // add each file to doc tree
            foreach (var file in mdFiles)
            {
                var newDoc = new DocumentTree()
                {
                    Name = file.Name,
                    Path = file.FullName.Replace(_root.FullName, "").Replace("\\", "/"),
                    Type = DocumentType.File
                };

                tree.Children.Add(newDoc);
            }

            // add each image to doc tree
            foreach (var image in imageFiles)
            {
                var newDoc = new DocumentTree()
                {
                    Name = image.Name,
                    Path = image.FullName.Replace(_root.FullName, "").Replace("\\", "/"),
                    Type = DocumentType.Image
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


        private static string FormatPath(string path)
        {
            if (path[0] == '/')
            {
                path = path.Remove(0);
            }

            if (path[path.Length - 1] == '/')
            {
                path = path.Remove(path.Length - 1);
            }

            return path;
        }
    }


}
