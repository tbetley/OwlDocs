using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OwlDocs.Web.Authorization;

namespace OwlDocs.Web.Controllers
{
    [Authorize(Policy = Policies.SiteAdminsPolicy)]
    [Route("admin")]
    public class AdminController : Controller
    {
        [Route("")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
