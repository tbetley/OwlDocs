using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Options;

using Xunit;

using OwlDocs.Data.Repositories;
using OwlDocs.Domain.DocumentService;
using OwlDocs.Models;
using OwlDocs.Models.Options;
using Markdig;

namespace OwlDocs.Tests
{
    public class DocumentIntegrationTests
    {
        private readonly IDocumentService _dbDocSvc;
        private readonly IDocumentService _fileDocSvc;
        private string _temp;

        public DocumentIntegrationTests()
        {
            _temp = Path.Combine(Environment.CurrentDirectory, "temp");
            DeleteTempFiles();

            var repo = new SqliteRepository("Data Source=.\\test.db");
            repo.EnsureCreated().GetAwaiter().GetResult();

            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            // create temporary directory
            Directory.CreateDirectory(_temp);

            // create document options
            var documentOptions = new DocumentOptions();
            documentOptions.DirectoryRoot = _temp;
            documentOptions.AcceptedTextFileTypes = new List<string>()
            {
                ".md",
                ".txt"
            };
            documentOptions.AcceptedImageFileTypes = new List<string>()
            {
                ".png",
                ".gif",
                ".jpg",
                ".jpeg"
            };
            var options = Options.Create(documentOptions);
            
            _dbDocSvc = new DbDocumentService(pipeline, repo);
            _fileDocSvc = new FileDocumentService(pipeline, options);
        }

        [Fact]
        public async Task ShouldCreateDbNewDocument()
        {
            // arange 
            var dbDoc = new Document();
            dbDoc.Id = 1;
            dbDoc.ParentId = null;
            dbDoc.Path = "/";
            dbDoc.ParentPath = null;
            dbDoc.Type = (int)DocumentType.Root;
            dbDoc.Name = null;
            dbDoc.Markdown = null;
            dbDoc.Html = null;
            dbDoc.Data = null;

            // act
            await _dbDocSvc.CreateDocument(dbDoc);
            var newDoc = await _dbDocSvc.GetDocumentByPath(dbDoc.Path);

            // assert
            Assert.Equal(newDoc.Id, dbDoc.Id);
        }

        [Fact]
        public async Task ShouldCreateFileNewDocument()
        {
            // arange 
            var dbDoc = new Document();
            dbDoc.Id = 1;
            dbDoc.ParentId = null;
            dbDoc.Path = null;
            dbDoc.ParentPath = "/";
            dbDoc.Type = (int)DocumentType.File;
            dbDoc.Name = "test.md";
            dbDoc.Markdown = null;
            dbDoc.Html = null;
            dbDoc.Data = null;

            // act
            var createdDoc = await _fileDocSvc.CreateDocument(dbDoc);

            var doc = await _fileDocSvc.GetDocumentByPath(createdDoc.Path);

            Assert.NotNull(doc);
        }

        private void DeleteTempFiles()
        {
            // delete temp folder
            if (Directory.Exists(_temp))
                Directory.Delete(_temp, true);

            // delete temp database
            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "test.db");
            if (File.Exists(dbPath))
            {
                try
                {
                    File.Delete(dbPath);
                }
                catch (Exception ex)
                {
                    
                }
            }
        }

    }
}
