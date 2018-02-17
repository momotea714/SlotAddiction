using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace SlotAddiction.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Select(DateTime date)
        {
            //ここで呼び出し
            var objList = new List<object>();
            objList.Add(new { id = 1, name = "hoge" });
            objList.Add(new { id = 2, name = "foo" });

            //response
            var obj = new { status = "OK", data = objList };
            return Json(obj, JsonRequestBehavior.AllowGet);
        }

    }
}