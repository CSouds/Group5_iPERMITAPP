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
    public class DecisionsController : Controller
    {
        private Group5_iPERMITDBEntities db = new Group5_iPERMITDBEntities();

        // GET: Decisions
        public ActionResult Index()
        {
            var decision = db.Decision.Include(d => d.EO).Include(d => d.PermitRequest);
            return View(decision.ToList());
        }

        // GET: Decisions/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Decision decision = db.Decision.Find(id);
            if (decision == null)
            {
                return HttpNotFound();
            }
            return View(decision);
        }

        // GET: Decisions/Create
        public ActionResult Create()
        {
            ViewBag.madeBy_EO_ID = new SelectList(db.EO, "ID", "Name");
            ViewBag.relatedTo_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription");
            return View();
        }

        // POST: Decisions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,dateOfDecision,finalDecision,Description,madeBy_EO_ID,relatedTo_requestNo")] Decision decision)
        {
            if (ModelState.IsValid)
            {
                db.Decision.Add(decision);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.madeBy_EO_ID = new SelectList(db.EO, "ID", "Name", decision.madeBy_EO_ID);
            ViewBag.relatedTo_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription", decision.relatedTo_requestNo);
            return View(decision);
        }

        // GET: Decisions/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Decision decision = db.Decision.Find(id);
            if (decision == null)
            {
                return HttpNotFound();
            }
            ViewBag.madeBy_EO_ID = new SelectList(db.EO, "ID", "Name", decision.madeBy_EO_ID);
            ViewBag.relatedTo_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription", decision.relatedTo_requestNo);
            return View(decision);
        }

        // POST: Decisions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,dateOfDecision,finalDecision,Description,madeBy_EO_ID,relatedTo_requestNo")] Decision decision)
        {
            if (ModelState.IsValid)
            {
                db.Entry(decision).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.madeBy_EO_ID = new SelectList(db.EO, "ID", "Name", decision.madeBy_EO_ID);
            ViewBag.relatedTo_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription", decision.relatedTo_requestNo);
            return View(decision);
        }

        // GET: Decisions/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Decision decision = db.Decision.Find(id);
            if (decision == null)
            {
                return HttpNotFound();
            }
            return View(decision);
        }

        // POST: Decisions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Decision decision = db.Decision.Find(id);
            db.Decision.Remove(decision);
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
