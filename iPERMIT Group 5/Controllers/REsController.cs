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
    public class REsController : Controller
    {
        private Group5_iPERMITDBEntities db = new Group5_iPERMITDBEntities();

        // GET: REs
        public ActionResult Index()
        {
            return View(db.RE.ToList());
        }

        // GET: REs/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RE rE = db.RE.Find(id);
            if (rE == null)
            {
                return HttpNotFound();
            }
            return View(rE);
        }

        // GET: REs/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: REs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,contactPersonName,password,createdDate,email,organizationName,organizationAddress")] RE rE)
        {
            if (ModelState.IsValid)
            {
                db.RE.Add(rE);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(rE);
        }

        // GET: REs/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RE rE = db.RE.Find(id);
            if (rE == null)
            {
                return HttpNotFound();
            }
            return View(rE);
        }

        // POST: REs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,contactPersonName,password,createdDate,email,organizationName,organizationAddress")] RE rE)
        {
            if (ModelState.IsValid)
            {
                db.Entry(rE).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(rE);
        }

        // GET: REs/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RE rE = db.RE.Find(id);
            if (rE == null)
            {
                return HttpNotFound();
            }
            return View(rE);
        }

        // POST: REs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            RE rE = db.RE.Find(id);
            db.RE.Remove(rE);
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
