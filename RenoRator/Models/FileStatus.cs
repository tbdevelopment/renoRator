using System.IO;
using System.Web;

namespace MultipleFileUpload.Mvc.Models
{
    public class FileStatus
    {

        #region [ Constructors ]

        public FileStatus()
        {
        }

        public FileStatus(FileInfo fileInfo)
        {
            SetValues(HttpUtility.UrlEncode(fileInfo.Name), (int)fileInfo.Length);
        }

        public FileStatus(string fileName, int fileLength)
        {
            SetValues(HttpUtility.UrlEncode(fileName), fileLength);
        }

        #endregion

        #region [ Properties ]

        public string group { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int size { get; set; }
        public string progress { get; set; }
        public string url { get; set; }
        public string complete_url { get; set; }
        public string error_url { get; set; }
        public string delete_url { get; set; }
        public string delete_type { get; set; }
        public string error { get; set; }

        #endregion

        #region [ Helpers ]

        private void SetValues(string fileName, int fileLength)
        {
            name = fileName;
            type = "image/png";
            size = fileLength;
            progress = "1.0";
            url = "../api/upload?f=" + fileName;
            complete_url = "../api/filecomplete?f=" + fileName;
            error_url = "../api/fileerror?f=" + fileName;
            delete_url = "../api/upload?f=" + fileName;
            delete_type = "DELETE";
        }

        #endregion

    }
}