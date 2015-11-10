using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using RenoRatorLibrary;
using System.Web.Mvc;

namespace RenoRator.Models
{
    [MetadataType(typeof(MessageMetaData))]
    public partial class message
    {
        public class MessageMetaData
        {

            [Required(ErrorMessage = "Receiver required!")]
            [Display(Name = "To")]
            public object receiverID;

            [Required(ErrorMessage = "Sender required!")]
            [Display(Name = "From")]
            public object senderID;
            
            [Required(ErrorMessage = "Subject required!")]
            [Display(Name = "Subject")]
            public object msgSubject;

            [Required(ErrorMessage = "Message required!")]
            [Display(Name = "Message")]
            public object msgBody;
            
            [Display(Name = "Date")]
            public object msgDate;
            
            [ScaffoldColumn(false)]
            public object deletedBySender;

            [ScaffoldColumn(false)]
            public object deletedByReceiver;
        }

        public static IQueryable<message> getAllMessages()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return (from m in db.messages select m);
        }

        public IEnumerable<message> getAllReceivedMessages(int userID)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return (from m in db.messages where m.receiverID == userID && m.deletedByReceiver == false select m);
        }

        public int countAllReceivedMessages(int userID)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return (from m in db.messages where m.receiverID == userID && m.deletedByReceiver == false select m).Count();
        }

        public IEnumerable<message> getAllReceivedMessagesDesc(int userID)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return (from m in db.messages where m.receiverID == userID && m.deletedByReceiver == false orderby m.msgDate descending select m);
        }

        public IEnumerable<message> getAllSentMessages(int userID)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return (from m in db.messages where m.senderID == userID && m.deletedBySender == false select m);
        }

        public int countAllSentMessages(int userID)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return (from m in db.messages where m.senderID == userID && m.deletedBySender == false select m).Count();
        }

        public IEnumerable<message> getAllSentMessagesDesc(int userID)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return (from m in db.messages where m.senderID == userID && m.deletedBySender == false orderby m.msgDate descending select m);
        }

        public message getSingleMessage(int id)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            return (from m in db.messages select m).FirstOrDefault(m => m.messageID == id);
            
        }

        public void setMessageRead(int userID, int msgID)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            message msg = db.messages.FirstOrDefault(m => m.messageID == msgID);
            if (msg != null)
            {
                if (!msg.is_read && msg.receiverID == userID)
                {
                    msg.is_read = true;
                    db.SaveChanges();
                }
            }
        }

        public void deleteSingleMessage(int userID, int msgID)
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            message deleteMSG = db.messages.FirstOrDefault(m => m.messageID == msgID);
            
            bool is_sender = deleteMSG.senderID == userID;
            bool is_receiver = deleteMSG.receiverID == userID;

            if (!is_sender && !is_receiver)
                return;

            try
            {
                if (is_sender)
                    deleteMSG.deletedBySender = true;
                if (is_receiver)
                    deleteMSG.deletedByReceiver = true;
                db.SaveChanges();
            }
            catch { }
        }

        public void Save()
        {
            renoRatorDBEntities db = new renoRatorDBEntities();
            this.msgDate = DateTime.Now;
            this.deletedByReceiver = false;
            this.deletedBySender = false;
            db.AddTomessages(this);
            db.SaveChanges();
        }
    }
}