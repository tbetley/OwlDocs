using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using OwlDocs.Domain.Docs;

namespace OwlDocs.Web.ViewComponents
{
    public class TreeViewComponent : ViewComponent
    {
        private readonly IDocumentService _docSvc;

        public TreeViewComponent(IDocumentService docSvc)
        {
            _docSvc = docSvc;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var tree = await _docSvc.GetDocumentTree();

            return View(tree);
        }
    }
}
