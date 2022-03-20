using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Options;

using Xunit;

using OwlDocs.Data;
using OwlDocs.Data.Repositories;
using OwlDocs.Domain;
using OwlDocs.Domain.DocumentService;
using OwlDocs.Models;
using OwlDocs.Models.Options;
using Markdig;

namespace OwlDocs.Tests
{
    public class DocumentServiceTests
    {
        private readonly IDocumentService _dbDocSvc;
        private readonly IDocumentService _fileDocSvc;
        private string _temp;

        public DocumentServiceTests()
        {
            _temp = Path.Combine(Environment.CurrentDirectory, "temp");
            DeleteTempFiles();

            var repo = new SqliteRepository("Data Source=.\\test.db");
            repo.EnsureCreated().GetAwaiter().GetResult();
            DbInitializer.InitializeDatabase(repo).GetAwaiter().GetResult();

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
        public async Task ShouldCreateDbFileDocument()
        {
            var dbDoc = new Document();
            dbDoc.ParentId = 1; // root is parent
            dbDoc.Type = (int)DocumentType.File;
            dbDoc.Name = "TestInsert.md";

            var createdDoc = await _dbDocSvc.CreateDocument(dbDoc);

            var getDoc = _dbDocSvc.GetDocumentByPath(createdDoc.Path);

            Assert.NotNull(getDoc);
        }

        [Fact]
        public async Task ShouldCreateDbFolderDocument()
        {
            // create folder
            var folderDoc = new Document();
            folderDoc.ParentId = 1;
            folderDoc.Type = (int)DocumentType.Directory;
            folderDoc.Name = "NewFolder";

            await _dbDocSvc.CreateDocument(folderDoc);
            var createdFolder = await _dbDocSvc.GetDocumentByPath("/NewFolder");

            // create child in folder
            var childDoc = new Document();
            childDoc.ParentId = createdFolder.Id;
            childDoc.Type = (int)DocumentType.File;
            childDoc.Name = "Child.md";

            await _dbDocSvc.CreateDocument(childDoc);
            var createdChild = await _dbDocSvc.GetDocumentByPath("/NewFolder/Child.md");

            Assert.NotNull(createdChild);
        }

        [Fact]
        public async Task ShouldCreateDbImageDocument()
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "TestAssets", "tengyart-CQQHCHRludA-unsplash.jpg");
            var image = await File.ReadAllBytesAsync(imagePath);

            var imageDoc = new Document();
            imageDoc.Name = "Test.jpg";
            imageDoc.Type = (int)DocumentType.Image;
            imageDoc.ParentId = 1; // root
            imageDoc.Data = image;

            await _dbDocSvc.CreateDocument(imageDoc);
            var createdImage = await _dbDocSvc.GetDocumentImage("/Test.jpg");

            Assert.NotNull(createdImage?.Data);
        }

        [Fact]
        public async Task DuplicateDbFileShouldThrowDuplicateDocumentException()
        {
            var dbDoc = new Document();
            dbDoc.ParentId = 1; // root is parent
            dbDoc.Type = (int)DocumentType.File;
            dbDoc.Name = "TestDuplicate.md";

            await _dbDocSvc.CreateDocument(dbDoc);

            var duplicate = new Document();
            duplicate.ParentId = 1; // root is parent
            duplicate.Type = (int)DocumentType.File;
            duplicate.Name = "TestDuplicate.md";

            await Assert.ThrowsAsync<DuplicateDocumentException>(async () => {
                await _dbDocSvc.CreateDocument(duplicate);
            });
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
            {
                try
                {
                    Directory.Delete(_temp, true);
                }
                catch (Exception ex)
                {

                }
            }

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
