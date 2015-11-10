using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RenoRatorLibrary;
using System.Web.Mvc;

namespace RenoRator.Models
{
    [MetadataType(typeof(JobAdMetaData))]
    public partial class JobAd 
    {
        public class JobAdMetaData
        {
            [ScaffoldColumn(false)]
            public object jobAdID { get; private set; }

            [ScaffoldColumn(false)]
            public object addressID { get; private set; }

            [ScaffoldColumn(false)]
            public object galleryID { get; private set; }

            [ScaffoldColumn(false)]
            public object userID { get; private set; }

            [ScaffoldColumn(false)]
            [Required(ErrorMessage="Price range required!")]
            public object priceRangeID { get; private set; }

            [ScaffoldColumn(false)]
            public object active { get; private set; }

            [Display(Name = "Title")]
            [Required(ErrorMessage = "Title required!")]
            public object title { get; set; }

            [Display(Name = "Description")]
            [Required(ErrorMessage = "Description required!")]
            public object description { get; set; }

            [Display(Name = "Tags")]
            public object tags { get; set; }

            [DateValidation()]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
            [Display(Name = "Target End Date")]
            public object targetEndDate { get; set; }
        }

        public static IQueryable<JobAd> getAllAds()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return (from j in db.JobAds where j.active == true orderby j.jobAdID select j);
        }

        public static IQueryable<JobAd> getAllAdsWithUserID(int user)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return (from j in db.JobAds where j.active == true && j.userID == user orderby j.jobAdID select j);
        }

        public static List<JobAd> getAllAdsAsList()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return (from j in db.JobAds where j.active == true orderby j.jobAdID select j).ToList();
        }

        public static IQueryable<JobAd> getAllAdsWithTag(string tag)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return (from j in db.JobAds where j.active == true && j.tags != null && j.tags.Contains(tag) orderby j.jobAdID select j);
        }

        public static JobAd getSingleJobAd(int id)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return db.JobAds.FirstOrDefault(j => j.jobAdID == id);
        }


        public void Save()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            this.address.city = db.Cities.Where(c => c.cityID == this.address.cityID).FirstOrDefault();
            this.address.country = "Canada";
            this.address.postalCode = this.address.postalCode.Replace(" ","").Replace("-","").Trim();
            List<string> tagList = new List<string>();
            if (this.tags != null)
            {
                foreach (String tag in this.tags.Split(','))
                    tagList.Add(tag.Trim().ToLower());
                this.tags = string.Join("|", tagList);
            }
            this.active = true;
            db.AddToJobAds(this);
            db.SaveChanges();
        }

        public void addGallery()
        {
            try
            {
                renoRatorDBEntities db = new renoRatorDBEntities();
                JobAd ad = db.JobAds.FirstOrDefault(a => a.jobAdID == this.jobAdID);
                ad.galleryID = this.galleryID;
                db.SaveChanges();                
            }
            catch{ }
        }

        public void updateGalleryID()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            JobAd ad = db.JobAds.FirstOrDefault(a => a.jobAdID == this.jobAdID);
            ad.galleryID = this.galleryID;
            db.SaveChanges();
        }

        public int update()
        {
            try
            {
                renoRatorDBEntities db = new renoRatorDBEntities();
                JobAd ad = db.JobAds.FirstOrDefault(a => a.jobAdID == this.jobAdID);
                this.address.city = db.Cities.Where(c => c.cityID == this.address.cityID).FirstOrDefault();
                this.address.country = "Canada";
                this.address.postalCode = this.address.postalCode.Replace(" ", "").Replace("-", "").Trim();
                List<string> tagList = new List<string>();
                if (this.tags != null)
                {
                    foreach (String tag in this.tags.Split(','))
                        tagList.Add(tag.Trim().ToLower());
                    this.tags = string.Join("|", tagList);
                }
                this.targetEndDate = this.targetEndDate.Date;

                ad.title = this.title;
                ad.address.city = this.address.city;
                ad.address.postalCode = this.address.postalCode;
                ad.targetEndDate = this.targetEndDate;
                ad.tags = this.tags;
                ad.address.addressLine1 = this.address.addressLine1;
                ad.address.addressLine2 = this.address.addressLine2;
                ad.priceRangeID = this.priceRangeID;
                ad.description = this.description;
                //ad.galleryID = this.galleryID;

                int rowsUpdated = db.SaveChanges();
                return rowsUpdated;
            }
            catch { return 0; }
            
            
        }

        public void deleteSingleAd(int adID)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            JobAd deleteAd = db.JobAds.FirstOrDefault(a => a.jobAdID == adID);

            try
            {
                deleteAd.active = false;
                db.SaveChanges();
            }
            catch { }
        }
    }
}