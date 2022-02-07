using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using OwlDocs.Domain.DocumentService;
using OwlDocs.Domain.DocumentCache;
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
        private readonly IDocumentCache _docCache;

        public ImageController(IDocumentService docSvc, IDocumentCache cache)
        {
            _docSvc = docSvc;
            _docCache = cache;
        }


        [Route("")]
        [HttpPost]
        [Authorize(Policy = Policies.DocumentWritersPolicy)]
        public async Task<IActionResult> CreateImage(Document document)
        {
            if (Request.Form.Files != null)
            {
                // validate file types

                using var memoryStream = new MemoryStream();
                await Request.Form.Files[0].CopyToAsync(memoryStream);
                document.Data = memoryStream.ToArray();
                document.Name = Request.Form.Files[0].FileName;
                document.Type = (int)DocumentType.Image;

                await _docSvc.CreateDocument(document);
                _docCache.Tree = await _docSvc.GetDocumentTree();
            }
            else
            {
                // return error
                return BadRequest("Image file not found");
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

            if (imageDoc.Type == (int)DocumentType.Image && imageDoc.Data != null)
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
