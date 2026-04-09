using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using iPERMIT_Group_5.Models;

namespace iPERMIT_Group_5.Controllers
{
    public class EOsController : Controller
    {
        private Group5_iPERMITDBEntities db = new Group5_iPERMITDBEntities();

        // GET: EOs
        public ActionResult Index()
        {
            return View(db.EO.ToList());
        }

        // GET: EOs/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EO eO = db.EO.Find(id);
            if (eO == null)
            {
                return HttpNotFound();
            }
            return View(eO);
        }

        // GET: EOs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EOs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name")] EO eO)
        {
            if (ModelState.IsValid)
            {
                db.EO.Add(eO);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(eO);
        }

        // GET: EOs/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EO eO = db.EO.Find(id);
            if (eO == null)
            {
                return HttpNotFound();
            }
            return View(eO);
        }

        // POST: EOs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,Name")] EO eO)
        {
            if (ModelState.IsValid)
            {
                db.Entry(eO).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(eO);
        }

        // GET: EOs/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EO eO = db.EO.Find(id);
            if (eO == null)
            {
                return HttpNotFound();
            }
            return View(eO);
        }

        // POST: EOs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            EO eO = db.EO.Find(id);
            db.EO.Remove(eO);
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
