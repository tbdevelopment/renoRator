using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RenoRatorLibrary;
using System.Web.Mvc;

namespace RenoRator.Models
{
    [MetadataType(typeof(ReviewMetaData))]
    public partial class Review
    {
        public double[][] ratings;

        public class ReviewMetaData
        {
            [ScaffoldColumn(false)]
            public object reviewID { get; private set; }

            [ScaffoldColumn(false)]
            public object userID { get; private set; }

            [ScaffoldColumn(false)]
            public object jobID { get; private set; }

            [ScaffoldColumn(false)]
            public object contractorUserID { get; private set; }

            [ScaffoldColumn(false)]
            public object galleryID { get; private set; }

            [Required(ErrorMessage = "Review required!")]
            [Display(Name = "Review")]
            public object review1;
        }
        
        public void Save()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            db.AddToReviews(this);

            foreach(double[] r in this.ratings) {
                ReviewRating rating = new ReviewRating();
                rating.reviewID = this.reviewID;
                rating.ratingQuestionID = Convert.ToInt32(r[0]);
                rating.rating = r[1];
                db.AddToReviewRatings(rating);
            }
            
            db.SaveChanges();
        }

        public List<RatingQuestion> getQuestions() {
            renoRatorDBEntities db = new renoRatorDBEntities();
            List<RatingQuestion> result = db.RatingQuestions.ToList();
            return result;
        }
    }
}