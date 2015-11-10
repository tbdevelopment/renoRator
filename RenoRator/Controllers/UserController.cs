using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RenoRator.Models;
using System.Security.Cryptography;
using RenoRatorLibrary;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.Web.Script.Serialization;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace RenoRator.Controllers
{
    public class UserController : Controller
    {
        //
        // GET: /User/
        renoRatorDBEntities _db;

        public ActionResult Home()
        {
            return View();
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Register()
        {
            _db = new renoRatorDBEntities();
            ViewBag.userTypes = new SelectList(_db.UserTypes.ToArray(), "userTypeID", "description");
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Register(FormCollection form)
        {
            _db = new renoRatorDBEntities();
            User newUser = new User();
            ViewBag.userTypes = new SelectList(_db.UserTypes.ToArray(), "userTypeID", "description"); 
            int userTypeID = 1;
            int.TryParse(form["userTypeValue"], out userTypeID);
            newUser.userTypeID = userTypeID;

            TryUpdateModel(newUser, form.ToValueProvider());
            if (newUser.company == null)
                newUser.company = "";

            if (newUser.HasDuplicateEmail())
            {
                this.Flash("Email already exists in the database.", FlashEnum.Warning);
                ViewBag.userTypeID = form["userTypeValue"];
                newUser.email = "";
                newUser.emailConfirm = "";
                return View(newUser);
            }

            if (ModelState.IsValid)
            {
                newUser.Save();
                Session["userID"] = newUser.userID;
                HttpContext.Session["userID"] = newUser.userID;
                HttpContext.Session["username"] = newUser.fname + " " + newUser.lname;
                HttpContext.Session["usertype"] = newUser.userTypeID;
                if (newUser.profilePhotoID != null)
                    HttpContext.Session["profilepic"] = newUser.photo.thumbPath;
                else
                    HttpContext.Session["profilepic"] = "/public/img/icons/user.png";
                this.Flash("Welcome " + newUser.fname + " " + newUser.lname + ". Your regisration was successful", FlashEnum.Success);
                return RedirectToAction("Home");
            }

            ViewBag.userTypeID = form["userTypeValue"];

            // Otherwise, reshow form
            this.Flash("Please make sure to fill in all required fields.", FlashEnum.Warning);
            return View(newUser);

        }

        public ActionResult Login()
        {
            return View();
        }

        private class GenericFunctions<T> where T : class
        {
            public static List<T> paginate(IEnumerable<T> query, int pageSize, int? page = null)
            {
                var paginatedList = query
                                            .Skip((page ?? 0) * pageSize)
                                            .Take(pageSize)
                                            .ToList();
                return paginatedList;
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Contractors(FormCollection form)
        {
            User u = new User();
            List<User> users = u.getAllContractors(form);

            ViewBag.cities = new SelectList(new City().getAllCities().ToArray(), "cityID", "city1");
            ViewBag.provinces = new SelectList(new Province().getAllProvinces().ToArray(), "provinceID", "province1");
            ViewBag.fname = form["fname"];
            ViewBag.lname = form["lname"];
            ViewBag.city = form["city"];
            ViewBag.province = form["province"];
            ViewBag.company = form["company"];

            ViewBag.page = 0;
            ViewBag.numAds = 0;
            ViewBag.adsPerPage = 0;
            ViewBag.pages = 0;
            return View(users);
        }

        public ActionResult Contractors(int? page)
        {
            User u = new User();
            List<User> users = GenericFunctions<User>.paginate(u.getAllContractors(), 10, page);

            ViewBag.cities = new SelectList(new City().getAllCities().ToArray(), "cityID", "city1");
            ViewBag.provinces = new SelectList(new Province().getAllProvinces().ToArray(), "provinceID", "province1");
            ViewBag.fname = "";
            ViewBag.lname = "";
            ViewBag.company = "";
            ViewBag.city = 0;
            ViewBag.province = 0;

            ViewBag.page = (page ?? 0);
            ViewBag.numAds = users.Count();
            ViewBag.adsPerPage = 10;
            ViewBag.pages = Math.Ceiling((double)(u.getAllContractors().Count() / 10.0));
            return View(users);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Login(FormCollection form, string redirectPage, string redirectController, int rId = 0)
        {
            _db = new renoRatorDBEntities();
            int uId = tryLogin(form["user_email"].ToString(), form["password"].ToString());
            if (uId > 0)
            {
                HttpContext.Session["userID"] = uId;
                User user = _db.Users.Where(x => x.userID == uId).FirstOrDefault();
                HttpContext.Session["username"] = user.fname + " " + user.lname;
                HttpContext.Session["usertype"] = user.userTypeID;
                if (user.profilePhotoID != null)
                    HttpContext.Session["profilepic"] = user.photo.thumbPath;
                else
                    HttpContext.Session["profilepic"] = "/public/img/icons/user.png";

                if (!string.IsNullOrEmpty(redirectPage) && !String.IsNullOrEmpty(redirectController) && rId > 0)
                    return RedirectToAction(redirectPage, redirectController, new { id = rId });
                else if (!string.IsNullOrEmpty(redirectPage) && !String.IsNullOrEmpty(redirectController))
                    return RedirectToAction(redirectPage, redirectController);
                return RedirectToAction("Home", "User");
            }

            // Otherwise, reshow form
            this.Flash("Username or Password incorrect.", FlashEnum.Error);
            return View();

        }

        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.Session.Abandon();
            //TempData["StatusMessage"] = "You have now been logged out";
            this.Flash("You have now logged out. You will be redirected in a few moments..", FlashEnum.Success);
            Response.AddHeader("REFRESH", "2;URL=Home");
            return View();
        }

        public double RoundToHalf(double x)
        {
            double precision = 2.0 * Math.Pow(10, 0);
            return Math.Ceiling(x * precision) / precision;
        }
        public ActionResult Profile(int id)
        {
            _db = new renoRatorDBEntities();
            User u = _db.Users.Where(x => x.userID == id).FirstOrDefault();
            ViewBag.user = id;

            var citiesList = _db.Cities.ToList();
            SelectList cities = new SelectList(citiesList.ToArray(), "cityID", "city1");
            ViewBag.cities = cities;

            var provinceList = _db.Provinces.ToList();
            SelectList provinces = new SelectList(provinceList.ToArray(), "provinceID", "province1");
            ViewBag.provinces = provinces;

            var phoneTypesList = _db.PhoneTypes.ToList();
            ViewBag.phonetypes = phoneTypesList;

            Review r = new Review();
            ViewBag.ratingQuestions = r.getQuestions();

            List<Review> reviews = u.reviews1.ToList();
            List<ReviewRating> ratings;
            double points = 0;
            int rating_count = 0;

            foreach(Review rev in reviews) {
                ratings = rev.reviewratings.ToList();
                foreach (ReviewRating rr in ratings)
                    points += rr.rating;
                rating_count += ratings.Count();
            }

            if(u.userTypeID == 2) {
                ViewBag.PortfolioJobs = (from j in _db.Jobs
                                         where j.contractorUserID == u.userID && j.galleryID != null
                                         select j).ToList();
            }

            ViewBag.reviews = reviews;
            ViewBag.rating = points / rating_count;

            if(Session["userid"] != null && id == (int)Session["userid"]) {
                List<JobAd> userads = JobAd.getAllAdsWithUserID(id).ToList();
                ViewBag.userads = userads;
            }

            return View(u);
        }

        private static int tryLogin(string email, string password) {
            renoRatorDBEntities _db = new renoRatorDBEntities();
            var user = _db.Users.Where(u => u.email == email).FirstOrDefault();
            if (user != null && user.password == PasswordFunctions.CreateHash(password, user.salt))
                return user.userID;
            return -1;
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditBio(FormCollection form)
        {
            try
            {
                _db = new renoRatorDBEntities();
                int uId = Convert.ToInt32(form["userID"]);
                if (uId == Convert.ToInt32(Session["userID"]))
                {
                    User user = _db.Users.Where(u => u.userID == uId).FirstOrDefault();
                    user.bio = form["bio"];

                    if (ModelState.IsValid)
                        _db.SaveChanges();
                }
            }
            catch
            {
                this.Flash("Something went wrong while saving your bio. Please try again later.", FlashEnum.Error);
                RedirectToAction("Profile", "User", new { id = Convert.ToInt32(Session["userID"]) });
            }

            return RedirectToAction("Profile", "User", new { id = Convert.ToInt32(Session["userID"]) });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditCompany(FormCollection form)
        {
            _db = new renoRatorDBEntities();
            int uId = Convert.ToInt32(form["userID"]);
            if (uId == Convert.ToInt32(Session["userID"]))
            {
                User user = _db.Users.Where(u => u.userID == uId).FirstOrDefault();
                user.company = form["company"];

                if (ModelState.IsValid)
                    _db.SaveChanges();
            }

            return RedirectToAction("Profile", "User", new { id = Convert.ToInt32(Session["userID"]) });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditAddress(FormCollection form)
        {
            _db = new renoRatorDBEntities();
            int uId = Convert.ToInt32(form["userID"]);
            if (uId == Convert.ToInt32(Session["userID"]))
            {
                try
                {
                    User user = _db.Users.Where(u => u.userID == uId).FirstOrDefault();
                    if (user.addressID == null)
                        user.address = new Address();

                    user.address.addressLine1 = form["address.addressLine1"];
                    user.address.addressLine2 = form["address.addressLine2"];
                    user.address.postalCode = form["address.postalCode"].Replace(" ", "");
                    int cityId = Convert.ToInt32(form["address.cityID"]);
                    user.address.city = _db.Cities.Where(c => c.cityID == cityId).FirstOrDefault();
                    user.address.country = "Canada";
                    user.address.city.provinceID = int.Parse(form["address.city.provinceID"]);

                    if (ModelState.IsValid)
                        _db.SaveChanges();
                }
                catch
                {
                    this.Flash("Something went wrong while saving your address. Please try again later.", FlashEnum.Error);
                    return RedirectToAction("Profile", "User", new { id = Convert.ToInt32(Session["userID"]) });
                }
            }

            return RedirectToAction("Profile", "User", new { id = Convert.ToInt32(Session["userID"]) });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddPhones(String[][] forms)
        {
            _db = new renoRatorDBEntities();
            int uId = Convert.ToInt32(Session["userID"]);
            Regex digitsOnly = new Regex(@"[^\d]");   
            Phone phone;
            try 
            {
                foreach ( String[] form in forms )
                {
                    phone = new Phone();
                    phone.userID = uId;

                    if (String.IsNullOrEmpty(form[0]) || String.IsNullOrEmpty(form[1]))
                        return RedirectToAction("Profile", "User", new { id = uId });

                    string phoneNo = digitsOnly.Replace(form[1], "");
                    if (phoneNo == "" || (phoneNo.Length < 10 || phoneNo.Length > 11))
                        return RedirectToAction("Profile", "User", new { id = uId });

                    phone.phoneTypeID = Convert.ToInt32(form[0]);
                    phone.phoneNo = phoneNo;

                    if (ModelState.IsValid)
                    {
                        _db.AddToPhones(phone);
                        _db.SaveChanges();
                    }
                }
            }
            catch
            {
                this.Flash("Something went wrong while adding your new phone. Please try again later.", FlashEnum.Error);
                return RedirectToAction("Profile", "User", new { id = uId });
            }
            return RedirectToAction("Profile", "User", new { id = uId });            
        }
        
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditPhone(FormCollection form)
        {
            _db = new renoRatorDBEntities();
            int uId = Convert.ToInt32(Session["userID"]);
            Regex digitsOnly = new Regex(@"[^\d]");   
            Phone phone;

            if (form["phoneID"] != null)
            {
                try
                {
                    int phoneID = Convert.ToInt32(form["phoneID"]);
                    phone = _db.Phones.Where(p => p.phoneID == phoneID).FirstOrDefault();
                    if (phone.userID == uId)
                    {
                        if (String.IsNullOrEmpty(form["phoneTypeID"]) || String.IsNullOrEmpty(form["phoneNo"]))
                            return RedirectToAction("Profile", "User", new { id = uId });

                        phone.phoneTypeID = Convert.ToInt32(form["phoneTypeID"]);
                        string phoneNo = digitsOnly.Replace(form["phoneNo"], "");

                        if (phoneNo == "")
                        {
                            deletePhoneNo(phoneID);
                            return RedirectToAction("Profile", "User", new { id = uId });
                        }
                        else if (phoneNo.Length < 10 || phoneNo.Length > 11)
                        {
                            return RedirectToAction("Profile", "User", new { id = uId });
                        }

                        phone.phoneNo = phoneNo;

                        if (ModelState.IsValid)
                            _db.SaveChanges();
                    }
                }
                catch
                {
                    this.Flash("Something went wrong while saving your phone data. Please try again later.", FlashEnum.Error);
                    return RedirectToAction("Profile", "User", new { id = uId });
                }
            }
            return RedirectToAction("Profile", "User", new { id = uId });
        }


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeletePhone(int id)
        {
            _db = new renoRatorDBEntities();
            try
            {                
                deletePhoneNo(id);
            }
            catch
            {
                this.Flash("Failed to delete your phone number. Please try again later.", FlashEnum.Error);
            }

            return RedirectToAction("Profile", "User", new { id = Convert.ToInt32(Session["userID"]) });
        }

        public void deletePhoneNo(int id)
        {
            _db = new renoRatorDBEntities();
            int uId = Convert.ToInt32(Session["userID"]);
            Phone phone = _db.Phones.Where(p => p.phoneID == id).FirstOrDefault();
            if (phone != null && phone.userID == uId) 
            {
                _db.Phones.DeleteObject(phone);
                _db.SaveChanges();
            }               
        }


        public void getUsersByQuery(string q)
        {
            if (Session["userID"] == null)
                return;

            q = q.ToLower();
            _db = new renoRatorDBEntities();

            var result = _db.Users
                .Where(u => (u.fname + " " + u.lname).Contains(q))
                .OrderBy(u => u.fname).ThenBy(u => u.lname)
                .Select(u => new
                {
                    id = u.userID,
                    name = u.fname + " " + u.lname + "(" + u.email + ")"
                });

            JavaScriptSerializer ser = new JavaScriptSerializer();
            Response.ContentType = "application/json";
            Response.Write(ser.Serialize(result));
            Response.End();
        }

        public ActionResult Review(int id)
        {
            if (id == 0)
                RedirectToAction("Profile", "User", new { id = Convert.ToInt32(Session["userID"]) });

            _db = new renoRatorDBEntities();
            Review review = _db.Reviews.Where(r => r.reviewID == id).FirstOrDefault();
            if (review == null)
                RedirectToAction("Profile", "User", new { id = Convert.ToInt32(Session["userID"]) });
            
            ViewBag.ratingQuestions = review.getQuestions();
            return View(review);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult SelectPhoto(FormCollection form)
        {
            try
            {
                User u = new User();
                int user = Convert.ToInt32(form["userID"]);
                int photo = Convert.ToInt32(form["selectedPhoto"]);
                Session["profilepic"] = u.updateProfilePicture(user, photo);

                this.Flash("Profile Picture updated successfully!", FlashEnum.Success);
                return RedirectToAction("Profile", new { @id = user });
            }
            catch
            {
                this.Flash("Failed to update your profile picture. Please try again.", FlashEnum.Error);
                return RedirectToAction("ProfilePicture");
            }
        }

        public ActionResult ProfilePicture()
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User", new { redirectPage = "ProfilePicture", redirectController = "User" });

            User u = new User();
            u.setUser((int)Session["userID"]);
            
            return View(u);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult ProfilePicture(FormCollection form)
        {
            User u = new User();
            u.setUser((int)Session["userID"]);

            int galleryid;
            if (form["uploadedImages"] != null && form["uploadedImages"] != "")
            {
                string[] images = form["uploadedImages"].Split('|');
                if (images.Length > 0 && u.profileGalleryID == null)
                {
                    galleryid = CreateGallery(images);
                    u.profileGalleryID = galleryid;
                    u.addProfileGallery();
                }
                else if (images.Length > 0)
                {
                    int gid = UpdateGallery(images, (int)u.profileGalleryID);
                    if (u.profileGalleryID != gid)
                    {
                        u.profileGalleryID = gid;
                        u.updateGalleryID();
                    }
                }
            }

            this.Flash("Your picture uploaded successfully!", FlashEnum.Success);
            return View(u);
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

                int maxHeight = 180;
                int maxWidth = 180;
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

                Bitmap thumb = new Bitmap(newWidth, newHeight);
                using (Graphics gr = Graphics.FromImage(thumb))
                {
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    gr.DrawImage(srcImg, new Rectangle(0, 0, newWidth, newHeight));
                }

                thumb.Save(storageLocation + "thumb_" + uniqueName, ImageCodecInfo.GetImageEncoders()[1], encoderParameters);
                thumb.Dispose();

                // delete the original
                srcImg.Dispose();
                System.IO.File.Delete(fullPath);

                // add the photo to the gallery
                g.photos.Add(p);
            }
            return g.updateGallery();
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

                int maxHeight = 180;
                int maxWidth = 180;
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

                Bitmap thumb = new Bitmap(newWidth, newHeight);
                using (Graphics gr = Graphics.FromImage(thumb))
                {
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    gr.DrawImage(srcImg, new Rectangle(0, 0, newWidth, newHeight));
                }

                thumb.Save(storageLocation + "thumb_" + uniqueName, ImageCodecInfo.GetImageEncoders()[1], encoderParameters);
                thumb.Dispose();

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

        private int[] getNewWidthAndHeight(int imgWidth, int imgHeight, int maxWidth, int maxHeight)
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

    }
}

