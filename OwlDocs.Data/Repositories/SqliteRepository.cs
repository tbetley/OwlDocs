using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;
using OwlDocs.Models;
using OwlDocs.Data.Extensions;

namespace OwlDocs.Data.Repositories
{
    public class SqliteRepository : ISqliteRepository
    {
        private readonly string _connectionString;

        public SqliteRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> AnyDocuments()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT COUNT(*) FROM Documents;
            ";

            var count = await command.ExecuteScalarAsync();

            return (long)count > 0;
        }

        public async Task<Document> CreateDocument(Document document)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                INSERT INTO Documents (ParentId, Path, ParentPath, Type, Name, Markdown, Html, Data)
                VALUES ($parentId, $path, $parentPath, $type, $name, $markdown, $html, $data);
            ";

            var parameters = new SqliteParameter[]
            {
                new SqliteParameter("$parentId", document.ParentId == null ? DBNull.Value : document.ParentId),
                new SqliteParameter("$path", document.Path),
                new SqliteParameter("$parentPath", document.ParentPath == null ? DBNull.Value : document.ParentPath),
                new SqliteParameter("$type", document.Type),
                new SqliteParameter("$name", document.Name == null ? DBNull.Value : document.Name),
                new SqliteParameter("$markdown", document.Markdown == null ? DBNull.Value : document.Markdown),
                new SqliteParameter("$html", document.Html == null ? DBNull.Value : document.Html),
                new SqliteParameter("$data", document.Data == null ? DBNull.Value : document.Data)
            };

            command.Parameters.AddRange(parameters);

            var result = await command.ExecuteScalarAsync();

            // get data
            return document;
        }

        public async Task<int> EnsureCreated()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();
            
            var command = connection.CreateCommand();
            command.CommandText =
            @"
                CREATE TABLE IF NOT EXISTS 'Documents' (
                    'Id' INTEGER NOT NULL CONSTRAINT 'PK_Documents' PRIMARY KEY AUTOINCREMENT,
                    'ParentId' INTEGER NULL,
                    'Path' TEXT NULL,
                    'ParentPath' TEXT NULL,
                    'Type' INTEGER NOT NULL,
                    'Name' TEXT NULL,
                    'Markdown' TEXT NULL,
                    'Html' TEXT NULL,
                    'Data' BLOB NULL
                );
            ";

            var retVal = await command.ExecuteNonQueryAsync();

            return retVal;
        }

        public async Task<Document> GetDocumentByPath(string path)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT Id, ParentId, Path, ParentPath, Type, Name, Markdown, Html, Data
                FROM Documents
                WHERE Path = $path
            ";

            command.Parameters.Add(new SqliteParameter("$path", path));

            using var reader = await command.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                await reader.ReadAsync(); 
                var document = new Document();
                document.Id = reader.GetInt32("Id");
                document.ParentId = reader.GetInt32("ParentId");
                document.Path = reader.GetValueOrDefault<string>("Path");
                document.ParentPath = reader.GetValueOrDefault<string>("ParentPath");
                document.Type = reader.GetInt32("Type");
                document.Name = reader.GetString("Name");
                document.Markdown = reader.GetValueOrDefault<string>("Markdown");
                document.Html = reader.GetValueOrDefault<string>("Html");
                document.Data = reader.GetValueOrDefault<byte[]>("Data");

                return document;
            }
            else
            {
                return null;                
            }
        }

        public async Task<Document> GetDocumentById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT Id, ParentId, Path, ParentPath, Type, Name, Markdown, Html, Data
                FROM Documents
                WHERE Id = $id
            ";

            command.Parameters.Add(new SqliteParameter("$id", id));

            using var reader = await command.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                await reader.ReadAsync();
                var document = new Document();
                document.Id = reader.GetInt32("Id");

                var parentId = reader.GetValueOrDefault<long?>("ParentId");
                document.ParentId = parentId == null ? null : (int)parentId;
                
                document.Path = reader.GetValueOrDefault<string>("Path");
                document.ParentPath = reader.GetValueOrDefault<string>("ParentPath");
                document.Type = reader.GetInt32("Type");
                document.Name = reader.GetValueOrDefault<string>("Name");
                document.Markdown = reader.GetValueOrDefault<string>("Markdown");
                document.Html = reader.GetValueOrDefault<string>("Html");
                document.Data = reader.GetValueOrDefault<byte[]>("Data");

                return document;
            }
            else
            {
                return null;
            }
        }

        public async Task<List<Document>> GetDocumentsByParentId(int parentId)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT Id, ParentId, Path, ParentPath, Type, Name, Markdown, Html, Data
                FROM Documents
                WHERE ParentId = $parentId
            ";

            command.Parameters.Add(new SqliteParameter("$parentId", parentId));

            using var reader = await command.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                var documents = new List<Document>();

                while (await reader.ReadAsync())
                {
                    var document = new Document();
                    document.Id = reader.GetInt32("Id");

                    var pId = reader.GetValueOrDefault<long?>("ParentId");
                    document.ParentId = pId == null ? null : (int)pId;

                    document.Path = reader.GetValueOrDefault<string>("Path");
                    document.ParentPath = reader.GetValueOrDefault<string>("ParentPath");
                    document.Type = reader.GetInt32("Type");
                    document.Name = reader.GetValueOrDefault<string>("Name");
                    document.Markdown = reader.GetValueOrDefault<string>("Markdown");
                    document.Html = reader.GetValueOrDefault<string>("Html");
                    document.Data = reader.GetValueOrDefault<byte[]>("Data");
                    
                    documents.Add(document);
                }
                
                return documents;
            }
            else
            {
                return null;
            }
        }

        public async Task<Document> UpdateDocument(Document document)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                UPDATE Documents SET
                ParentId = $parentId, 
                 Path = $path, 
                 ParentPath = $parentPath, 
                 Type = $type, 
                 Name = $name, 
                 Markdown = $markdown, 
                 Html = $html, 
                 Data = $data
                WHERE Id = $id;
            ";

            var parameters = new SqliteParameter[]
            {
                new SqliteParameter("$parentId", document.ParentId == null ? DBNull.Value : document.ParentId),
                new SqliteParameter("$path", document.Path),
                new SqliteParameter("$parentPath", document.ParentPath == null ? DBNull.Value : document.ParentPath),
                new SqliteParameter("$type", document.Type),
                new SqliteParameter("$name", document.Name == null ? DBNull.Value : document.Name),
                new SqliteParameter("$markdown", document.Markdown == null ? DBNull.Value : document.Markdown),
                new SqliteParameter("$html", document.Html == null ? DBNull.Value : document.Html),
                new SqliteParameter("$data", document.Data == null ? DBNull.Value : document.Data),
                new SqliteParameter("$id", document.Id)
            };

            command.Parameters.AddRange(parameters);

            var result = await command.ExecuteScalarAsync();

            // get data
            return document;
        }

        public async Task<List<DocumentTree>> GetOrderedTree()
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                SELECT Id, ParentId, Path, Type, Name
                FROM Documents
                ORDER BY ParentId
            ";

            using var reader = await command.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                var documents = new List<DocumentTree>();

                while (await reader.ReadAsync())
                {
                    var document = new DocumentTree();
                    document.Id = reader.GetInt32("Id");
                    var parentId = reader.GetValueOrDefault<long?>("ParentId");
                    document.ParentId = parentId == null ? null : (int)parentId;
                    document.Path = reader.GetValueOrDefault<string>("Path");
                    document.Type = reader.GetInt32("Type");
                    document.Name = reader.GetValueOrDefault<string>("Name");

                    documents.Add(document);
                }

                return documents;
            }
            else
            {
                return null;
            }
        }

        public async Task<int> DeleteDocumentById(int id)
        {
            using var connection = new SqliteConnection(_connectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText =
            @"
                DELETE FROM Documents WHERE Id = $id;
            ";

            command.Parameters.Add(new SqliteParameter("$id", id));

            await command.ExecuteScalarAsync();

            return 0;
        }
    }
}
