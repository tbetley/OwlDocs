using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OwlDocs.Domain.Docs;
using OwlDocs.Data;
using OwlDocs.Models;
using System.IO;
using Microsoft.AspNetCore.Authorization;

using OwlDocs.Web.Authorization;

namespace OwlDocs.Web.Controllers
{
    [Authorize(Policy = Policies.DocumentReadersPolicy)]
    [Route("Images")]
    public class ImageController : Controller
    {
        private readonly IDocumentService _docSvc;

        public ImageController(IDocumentService docSvc)
        {
            _docSvc = docSvc;
        }


        [Route("")]
        [HttpPost]
        [Authorize(Policy = Policies.DocumentWritersPolicy)]
        public async Task<IActionResult> CreateImage(OwlDocument document)
        {
            if (Request.Form.Files != null)
            {
                // validate file types

                using var memoryStream = new MemoryStream();
                await Request.Form.Files[0].CopyToAsync(memoryStream);
                document.Data = memoryStream.ToArray();
                document.Name = Request.Form.Files[0].FileName;
                document.Type = DocumentType.Image;

                await _docSvc.CreateDocument(document);
            }
            return Redirect(Request.Headers["Referer"]);
        }

        [Route("{*path}")]
        [HttpGet]
        public async Task<IActionResult> GetImage(string path)
        {
            var imageDoc = await _docSvc.GetDocumentImage(path);

            if (imageDoc == null)
            {
                return NotFound();
            }

            if (imageDoc.Type == DocumentType.Image && imageDoc.Data != null)
            {
                return File(imageDoc.Data, "image/png");
            }
            else
            {
                return BadRequest();
            }

        }
    }
}
