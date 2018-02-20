using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using SlotAddiction.DataBase;
using SlotData.Models;

namespace SlotAddiction.Controllers
{
    public class HomeController : Controller
    {
        #region フィールド
        private readonly Slot _slot = new Slot();
        private readonly SlotAddictionDBContext _db = new SlotAddictionDBContext();
        #endregion

        #region アクションメソッド
        /// <summary>
        /// 既定ページ
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.SlotModels = _db.SlotModels;
            return View();
        }
        #endregion

        #region API
        /// <summary>
        /// 指定した日付と機種を基に店舗マスタに登録している店舗からスロットの情報を取得する
        /// </summary>
        /// <param name="date"></param>
        /// <param name="slotModel">nullの場合は全取得</param>
        /// <returns></returns>
        public async Task<JsonResult> Select(DateTime date, List<string> slotModel)
        {
            // [] を返せばnullになるとおもったがうまくいかん。
            //ももんご君が直してくれることを期待
            if (slotModel.All(string.IsNullOrEmpty))
            {
                await _slot.GetSlotDataAsync(date);
            }
            else
            {
                await _slot.GetSlotDataAsync(date, slotModel);
            }

            //response
            var obj = new { status = "OK", data = _slot.SlotPlayDataCollection };
            return Json(obj, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}