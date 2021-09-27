using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OwlDocs.Domain.Docs;
using OwlDocs.Models;
using System.IO;

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
            OwlDocument result;
            try
            {
                result = await _docSvc.CreateDocument(document);

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

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

        [Route("Image")]
        [HttpPost]
        public async Task<IActionResult> CreateImage(OwlDocument document)
        {
            if (Request.Form.Files != null)
            {
                using var memoryStream = new MemoryStream();
                await Request.Form.Files[0].CopyToAsync(memoryStream);
                document.Data = memoryStream.ToArray();
                document.Name = Request.Form.Files[0].FileName;

                await _docSvc.CreateDocument(document);
            }
            return Ok();
        }

        /// <summary>
        /// API ENDPOINT
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        [Route("")]
        [HttpPut]
        public async Task<IActionResult> UpdateDocument([FromBody] OwlDocument document)
        {
            if (document == null)
                return BadRequest("Invalid Document Input");

            try
            {
                var result = await _docSvc.UpdateDocument(document);
                
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return Ok();
        }


        [Route("delete")]
        [HttpPost]
        public async Task<IActionResult> DeleteDocument(OwlDocument document)
        {
            // TODO IMPLEMENT DELETE ACTION
            try
            {
                await _docSvc.DeleteDocument(document);
            }
            catch (Exception e)
            {
                // log

                // throw for error handling middleware
                throw new Exception("Error Deleting Document");
            }

            // redirect to a view
            return Redirect(HttpContext.Request.Headers["Referer"]);
        }
    }
}
