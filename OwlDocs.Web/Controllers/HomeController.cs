using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OwlDocs.Domain.Docs;

namespace OwlDocs.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IDocumentService _docSvc;

        public HomeController(IDocumentService docSvc)
        {
            _docSvc = docSvc;
        }


        [Route("/")]
        public async Task<IActionResult> Index()
        {
            var tree = await _docSvc.GetDocumentTree();

            return View(tree);
        }

        [Route("/privacy")]
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
