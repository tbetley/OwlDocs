using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Markdig;
using OwlDocs.Domain.Docs;
using OwlDocs.Models;

namespace OwlDocs.Web.Controllers
{
    [Route("Docs")]
    public class DocsController : Controller
    {
        private readonly IDocumentService _docSvc;
        private readonly MarkdownPipeline _markdown;

        public DocsController(IDocumentService docSvc, MarkdownPipeline markdown)
        {
            _docSvc = docSvc;
            _markdown = markdown;
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


            return Redirect("/Docs" + result.Path);
        }

        [Route("")]
        [HttpPut]
        public async Task<IActionResult> UpdateDocument([FromBody] OwlDocument document)
        {
            // validation

            // update
            document.Html = Markdown.ToHtml(document.Markdown, _markdown);

            var result = await _docSvc.UpdateDocument(document);

            return Ok();
        }
    }
}
