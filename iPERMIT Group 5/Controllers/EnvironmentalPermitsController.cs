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
    public class EnvironmentalPermitsController : Controller
    {
        private Group5_iPERMITDBEntities db = new Group5_iPERMITDBEntities();

        // GET: EnvironmentalPermits
        public ActionResult Index()
        {
            return View(db.EnvironmentalPermits.ToList());
        }

        // GET: EnvironmentalPermits/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EnvironmentalPermits environmentalPermits = db.EnvironmentalPermits.Find(id);
            if (environmentalPermits == null)
            {
                return HttpNotFound();
            }
            return View(environmentalPermits);
        }

        // GET: EnvironmentalPermits/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EnvironmentalPermits/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "permitID,permitName,permitFee,description")] EnvironmentalPermits environmentalPermits)
        {
            if (ModelState.IsValid)
            {
                db.EnvironmentalPermits.Add(environmentalPermits);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(environmentalPermits);
        }

        // GET: EnvironmentalPermits/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EnvironmentalPermits environmentalPermits = db.EnvironmentalPermits.Find(id);
            if (environmentalPermits == null)
            {
                return HttpNotFound();
            }
            return View(environmentalPermits);
        }

        // POST: EnvironmentalPermits/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "permitID,permitName,permitFee,description")] EnvironmentalPermits environmentalPermits)
        {
            if (ModelState.IsValid)
            {
                db.Entry(environmentalPermits).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(environmentalPermits);
        }

        // GET: EnvironmentalPermits/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            EnvironmentalPermits environmentalPermits = db.EnvironmentalPermits.Find(id);
            if (environmentalPermits == null)
            {
                return HttpNotFound();
            }
            return View(environmentalPermits);
        }

        // POST: EnvironmentalPermits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            EnvironmentalPermits environmentalPermits = db.EnvironmentalPermits.Find(id);
            db.EnvironmentalPermits.Remove(environmentalPermits);
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
