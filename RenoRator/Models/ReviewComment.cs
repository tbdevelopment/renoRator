using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RenoRatorLibrary;
using System.Web.Mvc;

namespace RenoRator.Models
{
    [MetadataType(typeof(ReviewCommentMetaData))]
    public partial class ReviewComment
    {
        public class ReviewCommentMetaData
        {
            [ScaffoldColumn(false)]
            public object reviewCommentID { get; private set; }

            [ScaffoldColumn(false)]
            public object reviewID { get; private set; }

            [ScaffoldColumn(false)]
            public object userID { get; private set; }

            [Display(Name = "Comment")]
            [Required(ErrorMessage = "Comment required!")]
            public object comment { get; set; }

        }

        public void Save()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            db.AddToReviewComments(this);
            db.SaveChanges();
        }

    }
}