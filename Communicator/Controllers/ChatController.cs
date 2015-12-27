using Communicator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Communicator.Controllers
{
    [Authorize]
    public class ChatController : BaseController
    {

        // GET: Chat
        public ActionResult Index()
        {
            var context = new ApplicationDbContext();
            ViewBag.UsersList = context.Users.ToList();

            return View();
        }
    }
}