using NLog;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Drive.Logging;

namespace Drive.WebHost.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            Logging.Logging log = new Logging.Logging(LogManager.GetCurrentClassLogger());
            log.Log.Trace("tracer");
            log.Log.Debug("Debug");
            log.Log.Error("Error");

            log.Log.Info("Info");
            log.Log.Warn("Warn");

            log.Log.Fatal("Fatal");


            return View();
        }
    }
}