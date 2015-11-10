using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace RenoRator.Models
{
    [MetadataType(typeof(JobMetaData))]
    public partial class Job
    {
        public class JobMetaData
        {
            [Display(Name = "Start Date")]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
            public object startDate;

            [Display(Name = "End Date")]
            [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
            public object endDate;

            [Display(Name = "Price Range")]
            public object priceRangeID;

            [Display(Name = "Description")]
            public object description;

            [Display(Name = "Title")]
            public object title;

            [Display(Name = "Customer")]
            public object userID;

            [Display(Name = "Contractor")]
            public object contractorUserID;

            [Display(Name = "Tags")]
            public object tags;

        }

        public static Job getByID(int id)
        {
            var db = new renoRatorDBEntities();
            return db.Jobs.Where(j => j.jobID == id).FirstOrDefault();
        }

        public static IQueryable<Job> getJobsByContractorID(int id)
        {
            renoRatorDBEntities _db = new renoRatorDBEntities();
            return from j in _db.Jobs
                   where j.contractorUserID == id
                   orderby j.jobID
                   select j;
        }

        public static bool deleteJobForContractor(int id, int contractorID)
        {
            renoRatorDBEntities _db = new renoRatorDBEntities();
            var job = _db.Jobs.Where(j => j.jobID == id && contractorID == j.contractorUserID).FirstOrDefault();
            if(job != null)
                _db.Jobs.DeleteObject(job);
            return _db.SaveChanges() > 0;
        }

        public void addGallery()
        {
            try
            {
                renoRatorDBEntities db = new renoRatorDBEntities();
                Job job = db.Jobs.FirstOrDefault(a => a.jobID == this.jobID);
                job.galleryID = this.galleryID;
                db.SaveChanges();
            }
            catch { }
        }

        public void Save()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
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
            db.AddToJobs(this);
            db.SaveChanges();
        }
    }
}