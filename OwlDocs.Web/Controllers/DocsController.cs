using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OwlDocs.Web.Controllers
{
    [Route("Docs")]
    public class DocsController : Controller
    {
        [Route("{*path}")]
        public IActionResult Index(string path)
        {
            return View();
        }
    }
}
