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
using System.Web.Script.Serialization;

namespace Communicator.Hubs
{
    public class GroupManager
    {
        private List<string> ActiveGroups = new List<string>();

        public void Add(string GroupName)
        {
            if (!CheckIfGroupIsActive(GroupName))
            {
                ActiveGroups.Add(GroupName);
            }
        }

        public void Remove(string GroupName)
        {
            ActiveGroups.Remove(GroupName);
        }

        public bool CheckIfGroupIsActive(string GroupName)
        {
            string FoundGroup = ActiveGroups.Find(x => x == GroupName);
            return FoundGroup != null ? true : false;
        }

        public List<string> GetList()
        {
            return ActiveGroups;
        }
    }

    public class ChatHub : Hub
    {
        private DefaultConnection Datebase = new DefaultConnection();
        static private GroupManager GroupManagerObject = new GroupManager();

        private string GetCallerGroupName()
        {
            return Context.User.Identity.GetUserId<string>();
        }


        public override async Task OnConnected()
        {
#if DEBUG
            var id = Context.ConnectionId;
            Debug.WriteLine("OnConnected: " + id.ToString());
#endif

            await Groups.Add(Context.ConnectionId, GetCallerGroupName());
            GroupManagerObject.Add(GetCallerGroupName());
            AddUserActive(GetCallerGroupName());

            await base.OnConnected();
        }


        public override async Task OnDisconnected(bool StopCalled)
        {
#if DEBUG
            var id = Context.ConnectionId;
            Debug.WriteLine("OnDisconnected: " + id.ToString());
#endif
            GroupManagerObject.Remove(GetCallerGroupName());
            RemoveUserActive(GetCallerGroupName());
            await Groups.Remove(Context.ConnectionId, GetCallerGroupName());
            

            await base.OnDisconnected(StopCalled);
        }

        public override Task OnReconnected()
        {
#if DEBUG
            var id = Context.ConnectionId;
            Debug.WriteLine("OnReconnected: " + id.ToString());
#endif

            return base.OnReconnected();
        }

        public void GetToken()
        {
            byte[] Time = BitConverter.GetBytes(DateTime.UtcNow.ToBinary());
            byte[] Key = Guid.NewGuid().ToByteArray();
            string Token = Convert.ToBase64String(Time.Concat(Key).ToArray());
            Clients.Caller.ServerMessage("Your token: " + Token, DateTime.Now);
            Clients.Caller.GetToken(Token);
        }

        public void GetHistory()
        {
            string UserIdFrom = Context.User.Identity.GetUserId<string>();

            var messages = from m 
                           in Datebase.Messages
                           where m.UserIdTo == UserIdFrom && m.Flag == MessageFlagType.MESSAGE_WAITING_FOR_DELIVER
                           select m;

            foreach (var item in messages)
            {
                Clients.Group(item.UserIdTo).message(item.UserIdTo,item.UserIdFrom, item.Content, item.DateTimeSended);
                item.Flag = MessageFlagType.MESSAGE_SENDED;
            }

            Datebase.SaveChangesAsync();

        }

        public void GetActiveUsers()
        {
            var jsonSerialiser = new JavaScriptSerializer();
            var jsonList = jsonSerialiser.Serialize(GroupManagerObject.GetList());
            Clients.Caller.setActiveUsers(jsonList);
        }

        public void RemoveUserActive(string InactiveUser)
        {
            foreach (var item in GroupManagerObject.GetList())
            {
                Clients.Group(item).disableActiveUser(InactiveUser);
            }

        }

        public void AddUserActive(string ActiveUser)
        {
            foreach (var item in GroupManagerObject.GetList())
            {
                Clients.Group(item).enableActiveUser(ActiveUser);
            }
        }

        public void GetContacts()
        {
            var context = new ApplicationDbContext();
            var users = from u
                        in context.Users
                        select u;

            foreach (var user in users)
            {
                Clients.Caller.setContact(user.Id,user.UserName);
            }
        }


        public void Send(string Message,
                         string UserIdTo,
                         string Token)
        {
            var UserIdFrom = Context.User.Identity.GetUserId<string>();
            bool IsUserToActive = GroupManagerObject.CheckIfGroupIsActive(UserIdTo);
            MessageFlagType Flag = MessageFlagType.MESSAGE_WAITING_FOR_DELIVER;

            if (IsUserToActive)
            {
                Flag = MessageFlagType.MESSAGE_SENDED;
            }

            try
            {

                Message SendedMessage = new Message(Message,
                                                    UserIdTo,
                                                    UserIdFrom,
                                                    DateTime.Now,
                                                    Context.Request.Environment["server.RemoteIpAddress"].ToString(),
                                                    Token,
                                                    Flag);
#if DEBUG
                Debug.WriteLine(SendedMessage.ToString());
#endif

                Datebase.Messages.Add(SendedMessage);
                Datebase.SaveChangesAsync();


                Clients.Caller.message(UserIdTo,UserIdFrom, Message, DateTime.Now);

                if (IsUserToActive)
                {
                    Clients.Group(UserIdTo).message(UserIdTo,UserIdFrom, Message, DateTime.Now);
                }

            }
            catch (Exception exception)
            {
                Clients.Group(GetCallerGroupName()).ServerMessage(exception.Message,
                                                                  DateTime.Now);
            }
            
        }


    }
}