using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RenoRator.Models;

namespace RenoRator.Controllers
{
    public class ReviewController : Controller
    {
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult New(FormCollection form)
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User", new { id = Convert.ToInt32(form["contractorUserID"]) });
            
            Review newReview = new Review();
            newReview.userID = (int)Session["userID"];
            newReview.contractorUserID = Convert.ToInt32(form["contractorUserID"]);
            newReview.review1 = form["review1"];
            
            List<RatingQuestion> questions = newReview.getQuestions();
            newReview.ratings = new double[questions.Count()][];
            int count = 0;
            foreach (RatingQuestion q in questions) {
                newReview.ratings[count] = new double[2] {q.ratingQuestionID, Convert.ToDouble(form["rating"+count])};
                count++;
            }

            if (ModelState.IsValid) {
                newReview.Save();
                return RedirectToAction("Profile", "User", new { id = Convert.ToInt32(form["contractorUserID"]) });
            }
            
            // Otherwise, reshow form
            TryUpdateModel(newReview);
            return View(newReview);
        }


        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult PostComment(FormCollection form)
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User", new { id = Convert.ToInt32(form["contractorUserID"]) });

            ReviewComment comment = new ReviewComment();
            comment.userID = (int)Session["userID"];
            comment.reviewID = Convert.ToInt32(form["reviewID"]);
            comment.comment = form["comment"];
                        
            if (ModelState.IsValid)
                comment.Save();

            // Otherwise, reshow form
            return RedirectToAction("Review", "User", new { id = comment.reviewID });
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult PostReviewHelpful(int reviewId, bool good)
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User");

            ReviewHelpful helpful = new ReviewHelpful();
            helpful.userID = (int)Session["userID"];
            helpful.reviewID = Convert.ToInt32(reviewId);
            helpful.isGood = good;

            if (ModelState.IsValid)
                helpful.Save();
                
            return RedirectToAction("Profile", "User", new { id = Convert.ToInt32(Session["userID"]) });
        }
    }
}
