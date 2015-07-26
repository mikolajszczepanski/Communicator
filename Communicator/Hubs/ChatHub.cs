using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using System.Web.Security;
using Communicator.Models;

namespace Communicator.Hubs
{
    public class ChatHub : Hub
    {
        private DefaultConnection db = new DefaultConnection();


        public void Send(string message,
                         string userIdTo,
                         string userIdFrom,
                         string token)
        {
            try {
                // string name = Membership.GetUser(userIdFrom).UserName; //exception
                string dateTime = DateTime.Now.Hour.ToString().PadLeft(2, '0') +
                        ":" + DateTime.Now.Minute.ToString().PadLeft(2, '0');

                Message SendedMessage = new Message(message,
                                                    userIdTo,
                                                    userIdFrom,
                                                    DateTime.Now,
                                                    null,
                                                    null,
                                                    MessageFlagType.MESSAGE_WAITING_FOR_DELIVER);

                db.Messages.Add(SendedMessage);
                db.SaveChangesAsync();

                Clients.All.broadcastMessage(userIdFrom, message, dateTime);
            }
            catch (NullReferenceException exception)
            {
                Clients.All.broadcastMessage("Server", 
                                             exception.Message + " " + 
                                             exception.StackTrace,DateTime.Now);
            }
            
        }
    }
}