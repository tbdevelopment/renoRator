using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RenoRator.Models;
using RenoRatorLibrary;

namespace RenoRator.Controllers
{
    public class JobAdController : Controller
    {
        renoRatorDBEntities _db;
        //
        // GET: /JobAd/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UploadImages(int id)
        {
            ViewBag.jobAdID = id;
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UploadImages(FormCollection form)
        {
            int galleryid;
            int jobAdId = Convert.ToInt32(form["jobAdID"].ToString());
            JobAd jobAd = JobAd.getSingleJobAd(jobAdId);
            if (form["uploadedImages"] != null && form["uploadedImages"] != "")
            {
                string[] images = form["uploadedImages"].Split('|');
                if (images.Length > 0)
                {
                    galleryid = CreateGallery(images);
                    jobAd.galleryID = galleryid;
                    jobAd.addGallery();
                }
            }

            this.Flash("Your Job Ad was created successfully!", FlashEnum.Success);
            return RedirectToAction("Details", new { id = jobAdId });
        }

        public ActionResult Post()
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User", new { redirectPage = "Post", redirectController = "JobAd" });

            populateDropdowns();        

            return View();
        }

        public class GenericFunctions<T> where T : class
        {
            public static List<T> paginate(renoRatorDBEntities _db, IQueryable<T> query, int pageSize, int? page = null)
            {
                _db = new renoRatorDBEntities();
                var paginatedList = query
                                            .Skip((page ?? 0) * pageSize)
                                            .Take(pageSize)
                                            .ToList();
                return paginatedList;
            }
        }

        public ActionResult Ads(string tag, int? page)
        {

            IQueryable<JobAd> queryableAds;
            if (tag == null || tag == "")
                queryableAds = JobAd.getAllAds(); 
            else
                queryableAds = JobAd.getAllAdsWithTag(tag); 

            var paginatedAds = GenericFunctions<JobAd>.paginate(_db, queryableAds, 10, page);

            List<JobAd> ads = JobAd.getAllAdsAsList();

            Dictionary<string, int> tags = new Dictionary<string, int>();
            foreach (var ad in ads)
            {
                if (ad.tags != null)
                {
                    string[] allTags = ad.tags.Split('|');
                    foreach (string t in allTags)
                        if (t != "")
                            if (!tags.ContainsKey(t))
                                tags[t] = 1;
                            else
                                tags[t]++;
                }
            }
                
            ViewBag.tags = tags;
            ViewBag.tag = tag;
            ViewBag.page = (page ?? 0);
            ViewBag.adsPerPage = 10;
            ViewBag.pages = Math.Ceiling((double)(queryableAds.Count() / 10.0));

            return View(paginatedAds);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
                return RedirectToAction("PageNotFound", "Error", null);
            JobAd ad = JobAd.getAllAds().Where(x => x.jobAdID == id).FirstOrDefault();
            if(ad == null)
                return RedirectToAction("PageNotFound", "Error", null);
            return View(ad);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Post(FormCollection form)
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User", new { redirectPage = "Post", redirectController = "JobAd" });

            _db = new renoRatorDBEntities();
            JobAd newJobAd = new JobAd();
            newJobAd.address = new Address();
            TryUpdateModel(newJobAd, form.ToValueProvider());
            newJobAd.userID = (int)Session["userID"];

            //if (form["uploadedImages"] != null && form["uploadedImages"] != "")
            //{
            //    string[] images = form["uploadedImages"].Split('|');
            //    if (images.Length > 0)
            //        newJobAd.galleryID = CreateGallery(images);
            //}

            if (ModelState.IsValid)
            {
                newJobAd.Save();
                return RedirectToAction("UploadImages", new { id = newJobAd.jobAdID });
                //return RedirectToAction("Details", new { id = newJobAd.jobAdID });
            }

            // Otherwise, reshow form
            TryUpdateModel(newJobAd);
            populateDropdowns();
            return View(newJobAd);
        }

        // takes a string array of file names inside the storagelocation app setting
        // renames them, and creates a new gallery, and returns the id of the new gallery
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
                int[] widthHeight = getNewWidthAndHeight(imgWidth, imgHeight, maxWidth, maxHeight);
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
                } else
                    srcImg.Save(newPath, ImageCodecInfo.GetImageEncoders()[1], encoderParameters);

                Photo p = new Photo();
                p.path = virtualStorageLocation + uniqueName;
                p.thumbPath = virtualStorageLocation + "thumb_" + uniqueName;

                // create a thumbnail
                widthHeight = getNewWidthAndHeight(imgWidth, imgHeight, 96, 54);
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

        public static int[] getNewWidthAndHeight(int imgWidth, int imgHeight, int maxWidth, int maxHeight)
        {
            double ratio;
            double newWidth = 0.0;
            double newHeight = 0.0;
            if (imgWidth > maxWidth)
            {
                newWidth = maxWidth;
                ratio = imgWidth / (double)maxWidth;
                newHeight = imgHeight / ratio;

                if (newHeight > maxHeight)
                {
                    ratio = newHeight / (double)maxHeight;
                    newHeight = maxHeight;
                    newWidth /= ratio;
                }   
            }
            else if (imgHeight > maxHeight)
            {
                newHeight = maxHeight;
                ratio = imgHeight / (double)maxHeight;
                newWidth = imgWidth / ratio;

                if (newWidth > maxWidth)
                {
                    ratio = newWidth / (double)maxWidth;
                    newWidth = maxWidth;
                    newHeight /= ratio;
                }
            }
            return new int[2] { (int)newWidth, (int)newHeight };
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

        [AcceptVerbs(HttpVerbs.Post)]
        public void DeleteMultiple(int[] ids)
        {
            try
            {
                foreach (int id in ids)
                {
                    JobAd ad = new JobAd();
                    ad.deleteSingleAd(id);
                }
            }
            catch 
            {
                this.Flash("Failed to delete one or more Ads.", FlashEnum.Error);
            }
            this.Flash("Deleted Ads Successfully!", FlashEnum.Success);
        }

        public ActionResult DeleteSingle(int id)
        {
            try
            {
                JobAd ad = new JobAd();
                ad.deleteSingleAd(id);
            }
            catch 
            {
                this.Flash("Failed to delete the selected Ad.", FlashEnum.Error);
            }

            this.Flash("Deleted Ad Successfully!", FlashEnum.Success);
            return RedirectToAction("Profile", "User", new { id = Session["userid"] });
        }

        public ActionResult Edit(int id)
        {
            JobAd ad = JobAd.getSingleJobAd(id);
            populateDropdowns();
            if(ad.tags != null)
                ad.tags = ad.tags.Replace('|', ',');
            return View(ad);
        }

        public void deleteSingleImage(int id)
        {
            Photo p = new Photo();
            string storageLocation = HttpContext.Server.MapPath(ConfigurationManager.AppSettings["StorageFolder"]);
            p.deleteSinglePhoto(id, storageLocation);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit(FormCollection form)
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User", new { redirectPage = "Edit", redirectController = "JobAd", id = form["jobAdID"] });

            _db = new renoRatorDBEntities();
            JobAd jobad = JobAd.getSingleJobAd(int.Parse(form["jobAdID"]));
            TryUpdateModel(jobad, form.ToValueProvider());

            if (form["uploadedImages"] != null && form["uploadedImages"] != "")
            {
                string[] images = form["uploadedImages"].Split('|');
                if (images.Length > 0)
                {
                    int gid = UpdateGallery(images, (int)jobad.galleryID);
                    if (jobad.galleryID != gid)
                    {
                        jobad.galleryID = gid;
                        jobad.updateGalleryID();
                    }
                }
            }
            
            if (ModelState.IsValid)
            {
                jobad.update();
                this.Flash("Successfully updated " + jobad.title + ".", FlashEnum.Success);
                return RedirectToAction("Details", new { id = jobad.jobAdID });
            }

            // Otherwise, reshow form
            TryUpdateModel(jobad);
            populateDropdowns();
            return View(jobad);
        }

        public int UpdateGallery(string[] imgs, int galleryid)
        {
            Gallery g = new Gallery();
            g.getGallery(galleryid);
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
                int[] widthHeight = getNewWidthAndHeight(imgWidth, imgHeight, maxWidth, maxHeight);
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
                widthHeight = getNewWidthAndHeight(imgWidth, imgHeight, 96, 54);
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
            return g.updateGallery();
        }
    }
}
