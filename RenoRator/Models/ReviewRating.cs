using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RenoRatorLibrary;
using System.Web.Mvc;

namespace RenoRator.Models
{
    [MetadataType(typeof(ReviewRatingMetaData))]
    public partial class ReviewRating
    {
        public class ReviewRatingMetaData
        {
            [ScaffoldColumn(false)]
            public object reviewRatingID { get; private set; }

            [ScaffoldColumn(false)]
            public object reviewID { get; private set; }

            [ScaffoldColumn(false)]
            public object ratingQuestionID { get; private set; }

            [Display(Name = "Rating")]
            [Required(ErrorMessage = "Rating required!")]
            public object rating { get; set; }

        }
    }
}