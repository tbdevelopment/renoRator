using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RenoRatorLibrary;
using System.Web.Mvc;

namespace RenoRator.Models
{
    [MetadataType(typeof(RatingQuestionMetaData))]
    public partial class RatingQuestion
    {
        public class RatingQuestionMetaData
        {
            [ScaffoldColumn(false)]
            public object ratingQuestionID { get; private set; }

            [Display(Name = "Question")]
            [Required(ErrorMessage = "Question required!")]
            public object question { get; set; }

        }

        public IEnumerable<RatingQuestion> getAllRatingQuestions()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return (from q in db.RatingQuestions select q);
        }
    }
}