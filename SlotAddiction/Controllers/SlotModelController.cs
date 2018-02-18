using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SlotAddiction.DataBase;
using SlotAddiction.Models;

namespace SlotAddiction.Controllers
{
    public class SlotModelController : Controller
    {
        private SlotAddictionDBContext db = new SlotAddictionDBContext();

        // GET: SlotModel
        public ActionResult Index()
        {
            return View(db.SlotModels.ToList());
        }

        // GET: SlotModel/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SlotModel slotModel = db.SlotModels.Find(id);
            if (slotModel == null)
            {
                return HttpNotFound();
            }
            return View(slotModel);
        }

        // GET: SlotModel/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SlotModel/Create
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,SlotModelName,SlotModelShortName,ThroughType")] SlotModel slotModel)
        {
            if (ModelState.IsValid)
            {
                db.SlotModels.Add(slotModel);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(slotModel);
        }

        // GET: SlotModel/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SlotModel slotModel = db.SlotModels.Find(id);
            if (slotModel == null)
            {
                return HttpNotFound();
            }
            return View(slotModel);
        }

        // POST: SlotModel/Edit/5
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,SlotModelName,SlotModelShortName,ThroughType")] SlotModel slotModel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(slotModel).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(slotModel);
        }

        // GET: SlotModel/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SlotModel slotModel = db.SlotModels.Find(id);
            if (slotModel == null)
            {
                return HttpNotFound();
            }
            return View(slotModel);
        }

        // POST: SlotModel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SlotModel slotModel = db.SlotModels.Find(id);
            db.SlotModels.Remove(slotModel);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
