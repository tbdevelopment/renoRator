using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace RenoRator.Models
{
    [MetadataType(typeof(PhotoMetaData))]
    public partial class Photo
    {
        public class PhotoMetaData
        {
        }

        public void deleteSinglePhoto(int photoid, string folderLocation)
        {
            try
            {
                renoRatorDBEntities db = new renoRatorDBEntities();
                Photo p = db.Photos.FirstOrDefault(ph => ph.photoID == photoid);
                string path = folderLocation + p.path.Substring(16);
                string thumbPath = folderLocation + p.thumbPath.Substring(16);
                // delete actual file from server
                System.IO.File.Delete(path);
                System.IO.File.Delete(thumbPath);

                db.DeleteObject(p);
                db.SaveChanges();
            }
            catch { }
        }
    }
}