using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OwlDocs.Models;

namespace OwlDocs.Data
{
    public class DbInitializer
    {
        public static void InitializeDatabase(OwlDocsContext context)
        {
            if (context.Documents.Any())
            {
                return;
            }

            var documents = new Document[]
            {
                new Document
                {
                    ParentId = null,
                    Path = "/",
                    Type = DocumentType.Root,
                    Name = null,
                    Markdown = null,
                    Html = null
                },
                new Document
                {
                    ParentId = 1,
                    Path = "/Test1.md",
                    Type = DocumentType.File,
                    Name = "Test1.md",
                    Markdown = "#Test Markdown File\n**Hello there**\nThis is a test.",
                    Html = null
                },
                new Document
                {
                    ParentId = 1,
                    Path = "/TestFolder",
                    Type = DocumentType.Directory,
                    Name = "TestFolder",
                    Markdown = null,
                    Html = null
                },
                new Document
                {
                    ParentId = 3,
                    Path = "/TestFolder/test2.md",
                    Type = DocumentType.File,
                    Name = "test2.md",
                    Markdown = "#Test Markdown File\n**Hello there**\nThis is a test2.",
                    Html = null
                },
                new Document
                {
                    ParentId = 1,
                    Path = "/test3.md",
                    Type = DocumentType.File,
                    Name = "test3.md",
                    Markdown = "#Test Markdown File\n**Hello there**\nThis is a test2.",
                    Html = null
                },
                new Document
                {
                    ParentId = 3,
                    Path = "/TestFolder/AnotherFolder",
                    Type = DocumentType.Directory,
                    Name = "AnotherFolder",
                    Markdown = null,
                    Html = null
                },
                new Document
                {
                    ParentId = 6,
                    Path = "/TestFolder/AnotherFolder/test4.md",
                    Type = DocumentType.File,
                    Name = "test4.md",
                    Markdown = "#Test Markdown File\n**Hello there**\nThis is a test4.",
                    Html = null
                }
            };

            context.Documents.AddRange(documents);
            context.SaveChanges();
        }
    }
}
