using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OwlDocs.Domain.Docs;
using OwlDocs.Models;

namespace OwlDocs.Web.Controllers
{
    [Route("Docs")]
    public class DocsController : Controller
    {
        private readonly IDocumentService _docSvc;

        public DocsController(IDocumentService docSvc)
        {
            _docSvc = docSvc;
        }


        [Route("{*path}")]
        [HttpGet]
        public async Task<IActionResult> Document(string path)
        {
            var document = await _docSvc.GetDocumentByPath("/" + path);

            if (document.Type == DocumentType.Directory)
            {
                return BadRequest();
            }

            ViewData["Path"] = document.Path;

            return View(document);
        }

        [Route("")]
        [HttpPost]
        public async Task<IActionResult> CreateDocument(OwlDocument document)
        {
            // validation

            // insert
            var result = await _docSvc.CreateDocument(document);

            if (document.Type == DocumentType.File)
            {
                return Redirect("/Docs" + result.Path);
            }
            else
            {               
                HttpContext.Request.Headers.TryGetValue("Referer", out var referrer);
                var url = referrer.First();
                return Redirect(url);
            }
        }

        [Route("")]
        [HttpPut]
        public async Task<IActionResult> UpdateDocument([FromBody] OwlDocument document)
        {
            // validation

            // update
            var result = await _docSvc.UpdateDocument(document);

            return Ok();
        }

        [Route("delete")]
        [HttpPost]
        public async Task<IActionResult> DeleteDocument(OwlDocument document)
        {
            // TODO IMPLEMENT DELETE ACTION
            return Ok();
        }
    }
}
