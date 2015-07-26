using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using System.Web.Security;

namespace Communicator.Hubs
{
    public class ChatHub : Hub
    {
        public void Send(string message,
                         string userIdTo,
                         string userIdFrom,
                         string token)
        {
            try {
                // string name = Membership.GetUser(userIdFrom).UserName; //exception
                string dateTime = DateTime.Now.Hour.ToString().PadLeft(2, '0') +
                        ":" + DateTime.Now.Minute.ToString().PadLeft(2, '0');

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