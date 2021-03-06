using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OwlDocs.Domain.DocumentService;
using OwlDocs.Domain.DocumentCache;
using OwlDocs.Models;
using OwlDocs.Web.Authorization;

using System.IO;
using Microsoft.Extensions.Logging;

using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;

namespace OwlDocs.Web.Controllers
{
    [Authorize(Policy = Policies.DocumentReadersPolicy)]
    [Route("Docs")]
    public class DocsController : Controller
    {
        private readonly IDocumentService _docSvc;
        private readonly ILogger<DocsController> _logger;
        private readonly IDocumentCache _cache;

        public DocsController(IDocumentService docSvc, ILogger<DocsController> logger, IDocumentCache cache)
        {
            _docSvc = docSvc;
            _logger = logger;
            _cache = cache;
        }


        [Route("{*path}")]
        [HttpGet]
        public async Task<IActionResult> Document(string path)
        {          
            try
            {
                var doc = await _docSvc.GetDocumentByPath("/" + path);

                if (doc == null)
                    return NotFound();

                if (doc.Type == (int)DocumentType.Directory)
                {
                    return RedirectToAction("Error", "Home", new Error { ExceptionMessage = "Cannot Navigate to a Directory" });
                }

                ViewData["Path"] = doc.Path;

                return View(doc);
            }
            catch (Exception e)
            {
                return RedirectToAction("Error", "Home", new Error { ExceptionMessage = e.Message });
            }            
        }

        [Route("")]
        [HttpPost]
        public async Task<IActionResult> CreateDocument(Document document)
        {
            // validation

            // insert
            Document result;
            try
            {
                result = await _docSvc.CreateDocument(document);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Create Document Error");

                return RedirectToAction("Error", "Home", new Error { ExceptionMessage = e.Message });
            }

            // Update file tree
            _cache.Tree = await _docSvc.GetDocumentTree();

            if (document.Type == (int)DocumentType.File)
            {
                return Redirect("/Docs" + result.UriPath);
            }
            else
            {               
                HttpContext.Request.Headers.TryGetValue("Referer", out var referrer);
                var url = referrer.First();
                return Redirect(url);
            }
        }


        /// <summary>
        /// API ENDPOINT
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        [Route("")]
        [HttpPut]        
        public async Task<IActionResult> UpdateDocument([FromBody] Document document)
        {
            if (document == null)
            {
                _logger.LogError("Update Document Error, document is null");

                return BadRequest("Error, document is null or incorrect format");
            }

            try
            {
                var result = await _docSvc.UpdateDocument(document);                
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Update Document Error");

                return BadRequest("Error" + e.Message);
            }

            // Update document tree
            _cache.Tree = await _docSvc.GetDocumentTree();

            return Ok();
        }


        [Route("delete")]
        [HttpPost]
        public async Task<IActionResult> DeleteDocument(Document document)
        {
            // TODO IMPLEMENT DELETE ACTION
            try
            {
                await _docSvc.DeleteDocument(document);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error Deleting Document");
                throw new Exception("Error Deleting Document");
            }

            // Update tree
            _cache.Tree = await _docSvc.GetDocumentTree();

            // redirect to a view
            return Redirect(HttpContext.Request.Headers["Referer"]);
        }
    }
}
