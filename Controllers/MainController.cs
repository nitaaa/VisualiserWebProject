using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace VisualiserWebProject.Controllers
{
    public class MainController : Controller
    {
        // GET: Main
        public ActionResult Dashboard()
        {
            return View();
        }
    }
}