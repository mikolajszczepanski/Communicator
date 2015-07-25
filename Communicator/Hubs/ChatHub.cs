using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Communicator.Hubs
{
    public class ChatHub : Hub
    {
        /*public void Hello()
        {
            Clients.All.hello();
        }*/

        public void Send(string name, string message)
        {
            
            string dateTime = DateTime.Now.Hour.ToString().PadLeft(2,'0') + 
                        ":" + DateTime.Now.Minute.ToString().PadLeft(2, '0');

            // Call the broadcastMessage method to update clients.
            Clients.All.broadcastMessage(name, message, dateTime);
        }
    }
}