using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RenoRator.Models;
using RenoRatorLibrary;

namespace RenoRator.Controllers
{
    public class MessageController : Controller
    {
        //
        // GET: /Message/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult New()
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User", new { redirectPage = "New", redirectController = "Message" });
            
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult New(FormCollection form)
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User", new { redirectPage = "New", redirectController = "Message" });

            message newMsg = new message();
            TryUpdateModel(newMsg, form.ToValueProvider());
            newMsg.senderID = (int)Session["userID"];
            
            if (ModelState.IsValid)
            {
                newMsg.Save();
                this.Flash("Your message has been sent successfully!", FlashEnum.Success);
                return RedirectToAction("Received", "Message");
            }

            // Otherwise, reshow form
            TryUpdateModel(newMsg);
            return View(newMsg);

        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult NewAppointment(FormCollection form)
        {
            message newMsg = new message();
            newMsg.is_read = false;
            newMsg.msgBody = form["msgBody"];
            newMsg.msgSubject = form["msgSubject"];
            newMsg.receiverID = Convert.ToInt32(form["receiverID"]);
            newMsg.senderID = (int)Session["userID"];

            if (ModelState.IsValid)
            {
                newMsg.Save();
                this.Flash("Your appointment message has been sent successfully!", FlashEnum.Success);
                return RedirectToAction("Details", "JobAd", new { @id = form["jobadID"] });
            }

            this.Flash("Your appointment message failed to send. Please try again later.", FlashEnum.Error);
            return RedirectToAction("Details", "JobAd", new { @id = form["jobadID"] });
        }

        public class GenericFunctions<T> where T : class
        {
            public static List<T> paginate(IQueryable<T> query, int pageSize, int? page = null)
            {
                var paginatedList = query
                                            .Skip((page ?? 0) * pageSize)
                                            .Take(pageSize)
                                            .ToList();
                return paginatedList;
            }
        }

        public ActionResult Received(int? page)
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User", new { redirectPage = "Received", redirectController = "Message" });

            int user_id = (int)Session["userID"];
            var msgs = message.getAllMessages().Where(m => m.receiverID == user_id).Where(m => m.deletedByReceiver == false).OrderByDescending(m => m.msgDate);
            List<message> receivedMsgs = GenericFunctions<message>.paginate(msgs, 5, page);

            ViewBag.page = (page ?? 0);
            ViewBag.messagesPerPage = 5;
            ViewBag.numMessages = receivedMsgs.Count();
            ViewBag.pages = Math.Ceiling((double)(message.getAllMessages().Where(m => m.receiverID == user_id).Where(m => m.deletedByReceiver == false).Count() / 5.0));

            return View(receivedMsgs);
        }

        public ActionResult Sent(int? page)
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User", new { redirectPage = "Sent", redirectController = "Message" });

            int user_id = (int)Session["userID"];

            List<message> sentMsgs = GenericFunctions<message>.paginate(message.getAllMessages().Where(m => m.senderID == user_id).Where(m => m.deletedBySender == false).OrderByDescending(m => m.msgDate), 5, page);

            ViewBag.page = (page ?? 0);
            ViewBag.messagesPerPage = 5;
            ViewBag.numMessages = sentMsgs.Count();
            ViewBag.pages = Math.Ceiling((double)(message.getAllMessages().Where(m => m.senderID == user_id).Where(m => m.deletedBySender == false).Count() / 5.0));
            
            return View(sentMsgs);
        }

        public ActionResult View(int id)
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User", new { redirectPage = "Received", redirectController = "Message" });

            message msg = new message().getSingleMessage(id);
            if(msg != null)
                if (msg.senderID != Convert.ToInt32(Session["userID"]) && msg.receiverID != Convert.ToInt32(Session["userID"]))
                    return RedirectToAction("Received", "Message");
            msg.setMessageRead(Convert.ToInt32(Session["userID"]), id);
            
            return View(msg);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Delete(int id)
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User", new { redirectPage = "Received", redirectController = "Message" });

            try
            {
            message msg = new message();
            msg.deleteSingleMessage(Convert.ToInt32(Session["userID"]), id);
            }
            catch
            {
                this.Flash("Failed to delete your message.", FlashEnum.Error);
                return RedirectToAction("Received", "Message");
            }

            this.Flash("Your message has been deleted successfully!", FlashEnum.Success);
            return RedirectToAction("Received", "Message");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult DeleteMultiple(int[] ids)
        {
            if (Session["userID"] == null)
                return RedirectToAction("Login", "User", new { redirectPage = "Received", redirectController = "Message" });

            try
            {
            foreach (int id in ids)
            {
               message msg = new message();
                if (msg != null)
                    msg.deleteSingleMessage(Convert.ToInt32(Session["userID"]), id);
            }
            }
            catch
            {
                this.Flash("Failed to delete 1 or more messages.", FlashEnum.Error);
                return RedirectToAction("Received", "Message");
            }

            this.Flash("Your messages have been deleted successfully!", FlashEnum.Success);
            return RedirectToAction("Received", "Message");
        }

        public int GetNewMessageCount()
        {
            try
            {
                int userid = (int)Session["userID"];
                int messageCount = message.getAllMessages().Where(m => m.receiverID == userid && m.is_read == false && m.deletedByReceiver == false).Count();
                return messageCount;
            }
            catch
            {
                return 0;
            }
        }
    }

}
