using Communicator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Communicator.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (User != null)
            {
                var context = new ApplicationDbContext();
                var username = User.Identity.Name;

                if (!string.IsNullOrEmpty(username))
                {
                    var user = context.Users.SingleOrDefault(u => u.UserName == username);
                    ViewData.Add("FirstName", user.FirstName);
                    ViewData.Add("LastName", user.LastName);
                }
            }
            base.OnActionExecuted(filterContext);
        }
        public BaseController()
        { }
    }
}