using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlDocs.Domain
{
    public class DuplicateDocumentException : Exception
    {
        public DuplicateDocumentException()
        {

        }

        public DuplicateDocumentException(string message)
            : base(message)
        {

        }

        public DuplicateDocumentException(string message, Exception inner)
            : base(message, inner)
        {

        }
    }
}
