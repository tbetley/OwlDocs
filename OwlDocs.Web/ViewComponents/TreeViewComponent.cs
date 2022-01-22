using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using OwlDocs.Domain.DocumentService;
using OwlDocs.Domain.DocumentCache;

namespace OwlDocs.Web.ViewComponents
{
    public class TreeViewComponent : ViewComponent
    {
        private readonly IDocumentService _docSvc;
        private IDocumentCache _cache;

        public TreeViewComponent(IDocumentService docSvc, IDocumentCache cache)
        {
            _docSvc = docSvc;
            _cache = cache;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (_cache.Tree == null || _cache.Tree.Children.Count == 0)
            {
                _cache.Tree = await _docSvc.GetDocumentTree();
            }

            return View("SidebarTree", _cache.Tree);
        }
    }
}
