using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.Identity;
using System.Web.Security;
using Communicator.Models;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Communicator.Hubs
{
    public class ChatHub : Hub
    {
        private DefaultConnection db = new DefaultConnection();


        public override async Task OnConnected()
        {
            //uwierzytelnienie

            #if DEBUG
                var id = Context.ConnectionId;
                Debug.WriteLine("OnConnected: " + id.ToString());
            #endif

            await base.OnConnected();
        }


        public override async Task OnDisconnected(bool StopCalled)
        {
            //usuwa z kolekcji polaczenie

            #if DEBUG
                var id = Context.ConnectionId;
                Debug.WriteLine("OnConnected: " + id.ToString());
            #endif
            await base.OnDisconnected(StopCalled);
        }

        public void GetToken()
        {
            byte[] Time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] Key = Guid.NewGuid().ToByteArray();
            string Token = Convert.ToBase64String(Time.Concat(Key).ToArray());
            Clients.Caller.broadcastMessage("Server", "Your token: " + Token, DateTime.Now);
            Clients.Caller.GetToken(Token);
        }


        public void Send(string Message,
                         string UserIdTo,
                         string UserIdFrom,
                         string Token)
        {
         
            try {
                // string name = Membership.GetUser(userIdFrom).UserName; //exception
                string dateTime = DateTime.Now.Hour.ToString().PadLeft(2, '0') +
                        ":" + DateTime.Now.Minute.ToString().PadLeft(2, '0');

                Message SendedMessage = new Message(Message,
                                                    UserIdTo,
                                                    UserIdFrom,
                                                    DateTime.Now,
                                                    null,
                                                    null,
                                                    MessageFlagType.MESSAGE_WAITING_FOR_DELIVER);
                #if DEBUG
                    Debug.WriteLine(SendedMessage.ToString());
                #endif

                db.Messages.Add(SendedMessage);
                db.SaveChangesAsync();

                Clients.All.broadcastMessage(UserIdFrom, Message, dateTime);
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