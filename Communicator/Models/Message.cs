using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Communicator.Models
{
    public enum MessageFlagType
    {
        MESSAGE_WAITING_FOR_DELIVER,
        MESSAGE_SENDED,
        MESSAGE_DELIVERED
    }

    public class Message
    {

        public Message()
        {
            Content = null;
            UserIdTo = null;
            UserIdFrom = null;
            DateTimeSended = DateTime.Now;
            IpAddress = null;
            Token = null;
            Flag = MessageFlagType.MESSAGE_WAITING_FOR_DELIVER;

        }
        public Message(string _Content,
                       string _UserIdTo,
                       string _UserIdFrom,
                       DateTime _DateTimeSended,
                       string _IpAddress,
                       string _Token,
                       MessageFlagType _Flag)
        {
            Content = _Content;
            UserIdTo = _UserIdTo;
            UserIdFrom = _UserIdFrom;
            DateTimeSended = _DateTimeSended;
            IpAddress = _IpAddress;
            Token = _Token;
            Flag = _Flag;
        }

        public override string ToString()
        {
            return ( "{Id = " + Id + "\n" +
                     " UserIdFrom = " + UserIdFrom + "\n" +
                     " UserIdTo = " + UserIdTo + "\n" +
                     " DateTimeSended = " + DateTimeSended + "\n" +
                     " IpAddress = " + IpAddress + "\n" +
                     " Token = " + Token + "\n" +
                     " Flag = " + Flag + " }");
        }

        public int Id { get; private set; }
        public string Content { get; private set; }
        public string UserIdTo { get; private set; }
        public string UserIdFrom { get; private set; }
        public DateTime DateTimeSended { get; private set; }
        public string IpAddress { get; private set; }
        public string Token { get; private set; }
        public MessageFlagType Flag { get; set; }
    }

    public class DefaultConnection : DbContext
    {
        public DbSet<Message> Messages { get; set; }
    }
}