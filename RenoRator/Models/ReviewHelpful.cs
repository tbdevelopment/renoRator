using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RenoRatorLibrary;
using System.Web.Mvc;

namespace RenoRator.Models
{
    [MetadataType(typeof(ReviewHelpfulMetaData))]
    public partial class ReviewHelpful
    {
        public class ReviewHelpfulMetaData
        {
            [ScaffoldColumn(false)]
            public object reviewHelpfulID { get; private set; }

            [ScaffoldColumn(false)]
            public object reviewID { get; private set; }

            [ScaffoldColumn(false)]
            public object userID { get; private set; }

            [Display(Name = "Helpful")]
            [Required(ErrorMessage = "Field required!")]
            public object isGood { get; set; }

        }

        public void Save() 
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            db.AddToReviewHelpfuls(this);
            db.SaveChanges();
        }

    }
}