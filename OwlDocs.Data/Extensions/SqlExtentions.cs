using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;

namespace OwlDocs.Data.Extensions
{
    public static class SqlExtentions
    {
        public static T GetValueOrDefault<T>(this SqliteDataReader reader, string name)
        {
            if (Convert.IsDBNull(reader[name]))
            {
                return default(T);
            }
            else
            {
                
                return (T)reader[name];
            }
        }
    }
}
