using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;

namespace RenoRator.Models
{
    [MetadataType(typeof(GalleryMetaData))]
    public partial class Gallery
    {
        public class GalleryMetaData
        {
        }

        public void getGallery(int id)
        {
            try
            {
                renoRatorDBEntities db = new renoRatorDBEntities();
                Gallery g = db.Galleries.FirstOrDefault(gal => gal.galleryID == id);
                this.galleryID = g.galleryID;
                this.photos = g.photos;
                this.name = g.name;
            }
            catch { }
        }

        public int updateGallery()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            Gallery g = db.Galleries.FirstOrDefault(gal => gal.galleryID == this.galleryID);
            if (g == null)
                g = new Gallery();
            int gid = g.galleryID;
            //g.photos = this.photos;
            g.name = this.name;
            if (gid == 0)
                db.AddToGalleries(g);
            db.SaveChanges();
            int newid = g.galleryID;

            foreach (Photo p in this.photos)
            {
                Photo photo = new Photo();
                photo.galleryID = newid;
                photo.path = p.path;
                photo.thumbPath = p.thumbPath;
                photo.description = p.description;
                db.AddToPhotos(photo);
                db.SaveChanges();
            }
            return newid;
        }
    }
}