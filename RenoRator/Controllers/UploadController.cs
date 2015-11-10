using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http;
using MultipleFileUpload.Mvc.Models;
using Newtonsoft.Json;
using WebApiContrib.Formatting;
using RenoRator.Models;
using System.Web.SessionState;


namespace RenoRator.Controllers
{
    public class UploadController : ApiController, IRequiresSessionState
    {

        public string StorageFolder
        {
            get
            {
                return HttpContext
                    .Current
                    .Server
                    .MapPath(ConfigurationManager.AppSettings["StorageFolder"]);
            }
        }

        public bool IsIE9AndLower
        {
            get
            {
                return HttpContext.Current.Request.Browser.Browser.ToUpper() == "IE"
                    && (int.Parse(HttpContext.Current.Request.Browser.Version.Substring(0, 1)) <= 9);
            }
        }

        // GET api/upload
        public HttpResponseMessage Get()
        {
            //return ListCurrentFiles();
            return new HttpResponseMessage();
        }

        // POST api/upload
        public HttpResponseMessage Post()
        {
            return UploadFile();
        }

        // PUT api/upload
        public HttpResponseMessage Put()
        {
            return UploadFile();
        }

        // DELETE api/upload
        public void Delete(string f)
        {
            DeleteFile(f);
        }

        #region helpers

        // Fix for IE
        private HttpResponseMessage ObjectToHttpResponseMessage(object o)
        {
            return IsIE9AndLower
                ? ControllerContext.Request.CreateResponse(HttpStatusCode.OK
                                                            , JsonConvert.SerializeObject(o)
                                                            , new PlainTextFormatter())
                : ControllerContext.Request.CreateResponse(HttpStatusCode.OK, o);
        }

        // list current files
        private HttpResponseMessage ListCurrentFiles()
        {
            FileStatus[] statuses =
                (from file in new DirectoryInfo(StorageFolder).GetFiles("*", SearchOption.TopDirectoryOnly)
                 where !file.Attributes.HasFlag(FileAttributes.Hidden)
                 select new FileStatus(file)).ToArray();
            return ObjectToHttpResponseMessage(statuses);
        }

        // upload file
        private HttpResponseMessage UploadFile()
        {
            List<FileStatus> statuses = new List<FileStatus>();
            HttpRequestHeaders headers = Request.Headers;
            IEnumerable<string> values;
            HttpContext context = HttpContext.Current;

            if (!headers.TryGetValues("X-File-Name", out values))
                UploadWholeFile(context, statuses);
            else
                UploadPartialFile(values.FirstOrDefault(), context, statuses);

            return ObjectToHttpResponseMessage(statuses);
        }

        // Upload entire file
        private void UploadWholeFile(HttpContext context, List<FileStatus> statuses)
        {
            for (int i = 0; i < context.Request.Files.Count; i++)
            {
                HttpPostedFile file = context.Request.Files[i];

                string fileName = IsIE9AndLower
                    ? file.FileName.Split(new char[] { '\\' }).Last()
                    : file.FileName;

                FileStatus status = new FileStatus(file.FileName, file.ContentLength);

                try
                {
                    string fullName = Path.Combine(StorageFolder, Path.GetFileName(fileName));      
                    file.SaveAs(fullName);                    
                }
                catch (Exception e)
                {
                    status.error = e.Message;
                }

                statuses.Add(status);
            }
        }

        // Upload partial file
        private void UploadPartialFile(string fileName, HttpContext context, List<FileStatus> statuses)
        {
            string fullName = Path.Combine(StorageFolder, Path.GetFileName(fileName));
            FileStatus status = new FileStatus(new FileInfo(fullName));
            const int chunkSize = 1024;

            try
            {
                if (context.Request.Files.Count != 1)
                {
                    throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");
                }

                Stream inputStream = context.Request.Files[0].InputStream;

                using (FileStream fileStream = new FileStream(fullName, FileMode.Append, FileAccess.Write))
                {
                    byte[] buffer = new byte[chunkSize];
                    int l = inputStream.Read(buffer, 0, chunkSize);

                    while ((l = inputStream.Read(buffer, 0, chunkSize)) > 0)
                    {
                        fileStream.Write(buffer, 0, l);
                    }

                    fileStream.Flush();
                }
            }
            catch (Exception e)
            {
                status.error = e.Message;
            }

            statuses.Add(status);
        }

        // Delete file from the server
        private void DeleteFile(string f)
        {
            string fullPath = Path.Combine(StorageFolder, f);
            // delete the files
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        #endregion
    }
}
