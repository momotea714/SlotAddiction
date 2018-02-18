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
    public class TempoController : Controller
    {
        private SlotAddictionDBContext db = new SlotAddictionDBContext();

        // GET: Tempo
        public ActionResult Index()
        {
            return View(db.Tempos.ToList());
        }

        // GET: Tempo/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tempo tempo = db.Tempos.Find(id);
            if (tempo == null)
            {
                return HttpNotFound();
            }
            return View(tempo);
        }

        // GET: Tempo/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Tempo/Create
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,StoreURL,StoreName,SlotMachineStartNo,SlotMachineEndNo")] Tempo tempo)
        {
            if (ModelState.IsValid)
            {
                db.Tempos.Add(tempo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tempo);
        }

        // GET: Tempo/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tempo tempo = db.Tempos.Find(id);
            if (tempo == null)
            {
                return HttpNotFound();
            }
            return View(tempo);
        }

        // POST: Tempo/Edit/5
        // 過多ポスティング攻撃を防止するには、バインド先とする特定のプロパティを有効にしてください。
        // 詳細については、https://go.microsoft.com/fwlink/?LinkId=317598 を参照してください。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,StoreURL,StoreName,SlotMachineStartNo,SlotMachineEndNo")] Tempo tempo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tempo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(tempo);
        }

        // GET: Tempo/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tempo tempo = db.Tempos.Find(id);
            if (tempo == null)
            {
                return HttpNotFound();
            }
            return View(tempo);
        }

        // POST: Tempo/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tempo tempo = db.Tempos.Find(id);
            db.Tempos.Remove(tempo);
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
