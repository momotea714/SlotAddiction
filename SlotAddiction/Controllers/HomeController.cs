using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using SlotData.Models;

namespace SlotAddiction.Controllers
{
    public class HomeController : Controller
    {
        #region フィールド
        private readonly Slot _slot = new Slot();
        #endregion

        public ActionResult Index()
        {
            return View();
        }

        //public async Task<JsonResult> Select(DateTime date)
        //{
        //    return await Select(date, null);
        //}

        public async Task<JsonResult> Select(DateTime date, string slotModel)
        {
            //ここで呼び出し
            await _slot.GetSlotDataAsync(date, slotModel);

            //response
            var obj = new { status = "OK", data = _slot.SlotPlayDataCollection };
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
    }
}