using System;
using System.Web;
using System.Web.Mvc;

namespace Fleet_WorkShop.Controllers
{
    public class HomeController : Controller
    {
        public string UserName { get; set; }

        public ActionResult Index()
        {
            if (Session["Employee_Id"] == null)
                return RedirectToAction("Login", "Account");
            ViewBag.Email = UserName;
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
        public ActionResult LogOff()
        {
            Session.RemoveAll();
            Session.Abandon();
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.Now.AddHours(-1));
            Response.Cache.SetNoStore();
            //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Login", "Account");
        }
    }
}