﻿using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;

namespace RenoRator.Controllers
{
    public class FileErrorController : ApiController
    {
        // GET api/fileerror
        public HttpResponseMessage Get(string f)
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
            response.Content = new StreamContent(
                new FileStream(HttpContext.Current.Server.MapPath("/img/image-uploader/file-error-icon.png"),
                    FileMode.Open, FileAccess.Read));
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");
            return response;
        }
    }
}
