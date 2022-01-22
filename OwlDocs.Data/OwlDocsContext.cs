using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using OwlDocs.Models;

namespace OwlDocs.Data
{
    /// <summary>
    /// Main class that coordinates EF Core functionality for the OwlDocs data model.
    /// </summary>
    public class OwlDocsContext : DbContext
    {
        public OwlDocsContext(DbContextOptions<OwlDocsContext> options) : base(options)
        {

        }

        public DbSet<Document> Documents { get; set; }

    }
}
