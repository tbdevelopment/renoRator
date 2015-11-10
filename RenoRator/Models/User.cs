using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RenoRatorLibrary;
using System.Web.Mvc;

namespace RenoRator.Models
{
    [MetadataType(typeof(UserModel))]
    public partial class User {
        public class UserModel
        {
            [Display(Name = "First Name")]
            [Required(ErrorMessage = "First Name Required!")]
            public object fname { get; set; }

            [Display(Name = "Last Name")]
            [Required(ErrorMessage = "Last Name Required!")]
            public object lname { get; set; }

            [Display(Name = "Email")]
            [RegularExpression("^[_a-z0-9-]+(\\.[_a-z0-9-]+)*@[a-z0-9-]+(\\.[a-z0-9-]+)*(\\.[a-z]{2,4})$", ErrorMessage = "Email Not Valid")]
            [Required(ErrorMessage = "Email Required!")]
            public object email { get; private set; }

            [Required(ErrorMessage = "Password Required!")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public object password { get; set; }

            [Display(Name = "Biography")]
            public object bio { get; set; }

            [Display(Name = "Company")]
            public object company { get; set; }
        }

        [Required]
        [Compare("email")]
        [RegularExpression("^[_a-z0-9-]+(\\.[_a-z0-9-]+)*@[a-z0-9-]+(\\.[a-z0-9-]+)*(\\.[a-z]{2,4})$", ErrorMessage = "Email Not Valid")]
        [Display(Name = "Confirm Email")]
        public string emailConfirm { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("password")]
        [Display(Name = "Confirm Password")]
        public string passwordConfirm { get; set; }

        public IEnumerable<User> getAllContractors()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return (from u in db.Users where u.userTypeID == 2 select u).OrderBy(u => u.userID);
        }

        public bool HasDuplicateEmail()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return (from u in db.Users where u.email.Equals(this.email) select u).Count() > 0;
        }

        public void setUser(int id)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            User user = (from u in db.Users where u.userID == id select u).FirstOrDefault();
            this.userID = user.userID;
            this.usertype = user.usertype;
            this.userTypeID = user.userTypeID;
            this.verified = user.verified;
            this.addressID = user.addressID;
            this.address = user.address;
            this.bio = user.bio;
            this.company = user.company;
            this.email = user.email;
            this.fname = user.fname;
            this.lname = user.lname;
            this.password = user.password;
            this.portfolioGalleryID = user.portfolioGalleryID;
            this.profileGalleryID = user.profileGalleryID;
            this.profilePhotoID = user.profilePhotoID;
            this.gallery = user.gallery;
            this.portfolioGallery = user.portfolioGallery;
        }

        public List<User> getAllContractors(FormCollection form)
        {
            string fname = form["fname"].ToString();
            string lname = form["lname"].ToString();
            string company = form["company"].ToString();
            int city = (form["city"] == "") ? 0 : Convert.ToInt32(form["city"]);
            int province = (form["province"] == "") ? 0 : Convert.ToInt32(form["province"].ToString());
            renoRatorDBEntities db = new renoRatorDBEntities();
            var qry = (from u in db.Users 
                    where u.userTypeID == 2 
                    && u.fname.Contains(fname) 
                    && u.lname.Contains(lname)
                    && u.company.Contains(company)
                    select u);

            if (city > 0 && province > 0)
                return qry.Where(u => u.address.city.cityID == city && u.address.city.province.provinceID == province).OrderBy(u => u.userID).ToList();
            else if (city > 0)
                return qry.Where(u => u.address.city.cityID == city).OrderBy(u => u.userID).ToList();
            else if (province > 0)
                return qry.Where(u => u.address.city.province.provinceID == province).OrderBy(u => u.userID).ToList();
            else
                return qry.OrderBy(u => u.userID).ToList();
        }

        public void addProfileGallery()
        {
            try
            {
                renoRatorDBEntities db = new renoRatorDBEntities();
                User user = db.Users.FirstOrDefault(a => a.userID == this.userID);
                user.profileGalleryID = this.profileGalleryID;
                db.SaveChanges();
            }
            catch { }
        }

        public void updateGalleryID()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            User u = db.Users.FirstOrDefault(a => a.userID == this.userID);
            u.profileGalleryID = this.profileGalleryID;
            db.SaveChanges();
        }

        public string updateProfilePicture(int userID, int photoID)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            User u = db.Users.FirstOrDefault(a => a.userID == userID);
            u.profilePhotoID = photoID;
            db.SaveChanges();
            return u.photo.thumbPath;
        }

        public void Save()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            // salt and hash the password
            string salt = PasswordFunctions.CreateSalt(8);
            this.salt = salt;
            this.password = PasswordFunctions.CreateHash(this.password, salt);
            // add user to the database and save
            db.AddToUsers(this);
            db.SaveChanges();
        }
    }
}