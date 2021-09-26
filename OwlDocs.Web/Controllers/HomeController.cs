using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OwlDocs.Models;
using OwlDocs.Domain.Docs;
using Microsoft.AspNetCore.Diagnostics;

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
            return View();
        }

        [Route("/privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("/error")]
        public IActionResult Error()
        {
            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

            var viewModel = new Error
            {
                RequestId = HttpContext.TraceIdentifier,
                ExceptionMessage = exceptionHandlerFeature?.Error?.Message
            };

            return View(viewModel);
        }
    }
}
