using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using RenoRator.Models;
using RenoRatorLibrary;

namespace RenoRator.Controllers
{
    public class PortfolioController : Controller
    {

        renoRatorDBEntities _db;
        //
        // GET: /Portfolio/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AddJob()
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User", new { redirectPage = "AddJob", redirectController = "Portfolio" });

            populateDropdowns();
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddJob(FormCollection form)
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User", new { redirectPage = "AddJob", redirectController = "Portfolio" });

            Job newJob = new Job();
            TryUpdateModel(newJob, form.ToValueProvider());
            newJob.contractorUserID = (int)Session["userID"];
            

            if (ModelState.IsValid)
            {
                newJob.Save();
                return RedirectToAction("UploadImages", new { id = newJob.jobID });
            }

            // Otherwise, reshow form
            return View(newJob);

        }

        public ActionResult Edit(int? page)
        {
            // parse for user id
            int contractorID = -1;
            if(Session["userID"] != null) {
                try {
                    contractorID = (int)Session["userID"];
                } catch(Exception ex) { }
            }
            if(contractorID == -1)
                return View();

            IQueryable<Job> jobs = Job.getJobsByContractorID(contractorID);
            var pagedJobs = GenericFunctions<Job>.paginate(jobs, 10, page);
            ViewBag.page = (page ?? 0);
            ViewBag.adsPerPage = 10;
            ViewBag.pages = Math.Ceiling((double)(pagedJobs.Count() / 10.0));
            return View(pagedJobs);
        }

        public ActionResult Delete(int id)
        {
            int userID = -1;
            try
            {
                if (Session["userID"] != null)
                    userID = (int)Session["userID"];
            }
            catch (Exception ex)
            {
                userID = -1;
            }

            bool deleted = false;
            if (id > 0 && userID > 0)
                deleted = Job.deleteJobForContractor(id, userID);

            if (deleted)
                return RedirectToAction("Edit");
            else
                return View();

        }

        public ActionResult UploadImages(int id)
        {
            ViewBag.jobID = id;
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UploadImages(FormCollection form)
        {
            int galleryid;
            int jobID = Convert.ToInt32(form["jobID"].ToString());
            Job job = Job.getByID(jobID);
            if (form["uploadedImages"] != null && form["uploadedImages"] != "")
            {
                string[] images = form["uploadedImages"].Split('|');
                if (images.Length > 0)
                {
                    galleryid = CreateGallery(images);
                    job.galleryID = galleryid;
                    job.addGallery();
                }
            }

            this.Flash("Your Job was created successfully!", FlashEnum.Success);
            return RedirectToAction("Edit");
        }

        public class GenericFunctions<T> where T : class
        {
            public static List<T> paginate(IQueryable<T> query, int pageSize, int? page = null)
            {
                var paginatedList = query.Skip((page ?? 0) * pageSize)
                                         .Take(pageSize)
                                         .ToList();
                return paginatedList;
            }
        }

        public void getPortfolioJobByID(int jobID)
        {
            _db = new renoRatorDBEntities();

            var result = (from j in _db.Jobs
                         where j.jobID == jobID
                          select new portfolioJob
                         {                             
                             jobID = j.jobID,
                             galleryID = j.galleryID,
                             description = j.description,
                             startDate = j.startDate,
                             endDate = j.endDate,
                             minPriceRange = j.pricerange.min,
                             maxPriceRange = j.pricerange.max,
                             contractor = j.contractorUser.fname + " " + j.contractorUser.lname,
                             addressLine1 = j.address.addressLine1,
                             addressLine2 = j.address.addressLine2,
                             city = j.address.city.city1,
                             province = j.address.city.province.province1,
                             postalCode = j.address.postalCode,
                             tags = j.tags,
                             title = j.title
                         }).FirstOrDefault();

            // add the photos
            foreach(var photo in _db.Photos.Where(p => p.galleryID == result.galleryID))
                result.photos.Add( new portfolioPhoto { 
                    photoID = photo.photoID,
                    path = photo.path,
                    thumbPath = photo.thumbPath
                });

            JavaScriptSerializer ser = new JavaScriptSerializer();
            Response.ContentType = "application/json";
            Response.Write(ser.Serialize(result));
            Response.End();
        }

        public class portfolioJob
        {
            public int jobID;
            public int? galleryID;
            public string description;
            public DateTime? startDate;
            public DateTime endDate;
            public int minPriceRange;
            public int maxPriceRange;
            public string contractor;
            public string addressLine1;
            public string addressLine2;
            public string city;
            public string province;
            public string postalCode;
            public string tags;
            public string title;
            public List<portfolioPhoto> photos;

            public portfolioJob ()
            {
                photos = new List<portfolioPhoto>();
            }
        }

        public class portfolioPhoto 
        {
            public int photoID;
            public string path;
            public string thumbPath;
        }

        private void populateDropdowns()
        {
            _db = new renoRatorDBEntities();
            var priceRanges = from range in _db.PriceRanges.ToList()
                              select new { priceRangeID = range.priceRangeID, range = range.min + " - " + range.max };
            SelectList priceranges = new SelectList(priceRanges.ToArray(), "priceRangeID", "range");
            ViewBag.priceranges = priceranges;

            var citiesList = _db.Cities.ToList();
            SelectList cities = new SelectList(citiesList.ToArray(), "cityID", "city1");
            ViewBag.cities = cities;

            var provinceList = _db.Provinces.ToList();
            SelectList provinces = new SelectList(provinceList.ToArray(), "provinceID", "province1");
            ViewBag.provinces = provinces;
        }

        public int CreateGallery(string[] imgs)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            Gallery g = new Gallery();
            foreach (string img in imgs)
            {
                string storageLocation = HttpContext.Server.MapPath(ConfigurationManager.AppSettings["StorageFolder"]);
                string virtualStorageLocation = ConfigurationManager.AppSettings["StorageFolder"];
                string fullPath = storageLocation + img;
                string uniqueName = PhotoFunctions.getUniqueFileName(img);
                string newPath = storageLocation + uniqueName;

                System.Drawing.Image srcImg = System.Drawing.Image.FromFile(fullPath);
                int imgWidth = srcImg.Width;
                int imgHeight = srcImg.Height;

                int maxHeight = 540;
                int maxWidth = 960;
                int[] widthHeight = JobAdController.getNewWidthAndHeight(imgWidth, imgHeight, maxWidth, maxHeight);
                int newWidth = widthHeight[0];
                int newHeight = widthHeight[1];

                EncoderParameters encoderParameters = new EncoderParameters(1);
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Compression, 100);

                if (newWidth > 0 || newHeight > 0)
                {
                    Bitmap newImage = new Bitmap(newWidth, newHeight);
                    using (Graphics gr = Graphics.FromImage(newImage))
                    {
                        gr.SmoothingMode = SmoothingMode.HighQuality;
                        gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        gr.DrawImage(srcImg, new Rectangle(0, 0, newWidth, newHeight));
                    }


                    newImage.Save(newPath, ImageCodecInfo.GetImageEncoders()[1], encoderParameters);
                    newImage.Dispose();
                }
                else
                    srcImg.Save(newPath, ImageCodecInfo.GetImageEncoders()[1], encoderParameters);

                Photo p = new Photo();
                p.path = virtualStorageLocation + uniqueName;
                p.thumbPath = virtualStorageLocation + "thumb_" + uniqueName;

                // create a thumbnail
                widthHeight = JobAdController.getNewWidthAndHeight(imgWidth, imgHeight, 96, 54);
                newWidth = widthHeight[0];
                newHeight = widthHeight[1];

                //System.Drawing.Image i = System.Drawing.Image.FromFile(newPath);
                System.Drawing.Image thumb = srcImg.GetThumbnailImage(newWidth, newHeight, () => false, IntPtr.Zero);
                thumb.Save(storageLocation + "thumb_" + uniqueName, ImageCodecInfo.GetImageEncoders()[1], encoderParameters);

                // delete the original
                srcImg.Dispose();
                System.IO.File.Delete(fullPath);

                // add the photo to the gallery
                g.photos.Add(p);
            }
            db.AddToGalleries(g);
            db.SaveChanges();
            return g.galleryID;
        }

    }
}
    