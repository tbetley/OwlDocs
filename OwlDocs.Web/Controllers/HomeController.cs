using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OwlDocs.Models;
using OwlDocs.Domain.Docs;
using OwlDocs.Web.Authorization;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace OwlDocs.Web.Controllers
{
    [Authorize(Policy = Policies.DocumentReadersPolicy)]
    public class HomeController : Controller
    {
        private readonly IDocumentService _docSvc;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IDocumentService docSvc, ILogger<HomeController> logger)
        {
            _docSvc = docSvc;
            _logger = logger;
        }


        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("/privacy")]
        public IActionResult Privacy()
        {
            return View();
        }

        [Route("/error")]
        public IActionResult Error(Error error)
        {

            Error viewModel = null;

            if (error.ExceptionMessage != null)
            {
                viewModel = new Error();
                viewModel.ExceptionMessage = error.ExceptionMessage;
                viewModel.RequestId = HttpContext.TraceIdentifier;
            }
            else
            {
                var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();

                if (exceptionHandlerFeature.Error != null)
                {
                    viewModel = new Error();
                    viewModel.ExceptionMessage = exceptionHandlerFeature?.Error?.Message;
                    viewModel.RequestId = HttpContext.TraceIdentifier;
                }                
            }

            if (viewModel != null)
            {
                return View(viewModel);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }
    }
}
