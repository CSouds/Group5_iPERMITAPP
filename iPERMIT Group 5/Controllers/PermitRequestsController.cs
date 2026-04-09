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
    public class PermitRequestsController : Controller
    {
        private Group5_iPERMITDBEntities db = new Group5_iPERMITDBEntities();

        // GET: PermitRequests
        public ActionResult Index()
        {
            var permitRequest = db.PermitRequest.Include(p => p.EnvironmentalPermits).Include(p => p.RE);
            return View(permitRequest.ToList());
        }

        // GET: PermitRequests/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PermitRequest permitRequest = db.PermitRequest.Find(id);
            if (permitRequest == null)
            {
                return HttpNotFound();
            }
            return View(permitRequest);
        }

        // GET: PermitRequests/Create
        public ActionResult Create()
        {
            ViewBag.requestedPermit_permitID = new SelectList(db.EnvironmentalPermits, "permitID", "permitName");
            ViewBag.requestedBy_RE_ID = new SelectList(db.RE, "ID", "contactPersonName");
            return View();
        }

        // POST: PermitRequests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "requestNo,dateOfRequest,activityDescription,activityStartDate,activityDuration,permitFee,requestedBy_RE_ID,requestedPermit_permitID")] PermitRequest permitRequest)
        {
            if (ModelState.IsValid)
            {
                db.PermitRequest.Add(permitRequest);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.requestedPermit_permitID = new SelectList(db.EnvironmentalPermits, "permitID", "permitName", permitRequest.requestedPermit_permitID);
            ViewBag.requestedBy_RE_ID = new SelectList(db.RE, "ID", "contactPersonName", permitRequest.requestedBy_RE_ID);
            return View(permitRequest);
        }

        // GET: PermitRequests/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PermitRequest permitRequest = db.PermitRequest.Find(id);
            if (permitRequest == null)
            {
                return HttpNotFound();
            }
            ViewBag.requestedPermit_permitID = new SelectList(db.EnvironmentalPermits, "permitID", "permitName", permitRequest.requestedPermit_permitID);
            ViewBag.requestedBy_RE_ID = new SelectList(db.RE, "ID", "contactPersonName", permitRequest.requestedBy_RE_ID);
            return View(permitRequest);
        }

        // POST: PermitRequests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "requestNo,dateOfRequest,activityDescription,activityStartDate,activityDuration,permitFee,requestedBy_RE_ID,requestedPermit_permitID")] PermitRequest permitRequest)
        {
            if (ModelState.IsValid)
            {
                db.Entry(permitRequest).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.requestedPermit_permitID = new SelectList(db.EnvironmentalPermits, "permitID", "permitName", permitRequest.requestedPermit_permitID);
            ViewBag.requestedBy_RE_ID = new SelectList(db.RE, "ID", "contactPersonName", permitRequest.requestedBy_RE_ID);
            return View(permitRequest);
        }

        // GET: PermitRequests/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            PermitRequest permitRequest = db.PermitRequest.Find(id);
            if (permitRequest == null)
            {
                return HttpNotFound();
            }
            return View(permitRequest);
        }

        // POST: PermitRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            PermitRequest permitRequest = db.PermitRequest.Find(id);
            db.PermitRequest.Remove(permitRequest);
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
