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
    public class RequestStatusController : Controller
    {
        private Group5_iPERMITDBEntities db = new Group5_iPERMITDBEntities();

        // GET: RequestStatus
        public ActionResult Index()
        {
            var requestStatus = db.RequestStatus.Include(r => r.PermitRequest);
            return View(requestStatus.ToList());
        }

        // GET: RequestStatus/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequestStatus requestStatus = db.RequestStatus.Find(id);
            if (requestStatus == null)
            {
                return HttpNotFound();
            }
            return View(requestStatus);
        }

        // GET: RequestStatus/Create
        public ActionResult Create()
        {
            ViewBag.PermitRequest_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription");
            return View();
        }

        // POST: RequestStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "RequestStatusID,permitRequestStatus,date,description,PermitRequest_requestNo,updatedBy_ID,updatedByType")] RequestStatus requestStatus)
        {
            if (ModelState.IsValid)
            {
                db.RequestStatus.Add(requestStatus);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.PermitRequest_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription", requestStatus.PermitRequest_requestNo);
            return View(requestStatus);
        }

        // GET: RequestStatus/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequestStatus requestStatus = db.RequestStatus.Find(id);
            if (requestStatus == null)
            {
                return HttpNotFound();
            }
            ViewBag.PermitRequest_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription", requestStatus.PermitRequest_requestNo);
            return View(requestStatus);
        }

        // POST: RequestStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "RequestStatusID,permitRequestStatus,date,description,PermitRequest_requestNo,updatedBy_ID,updatedByType")] RequestStatus requestStatus)
        {
            if (ModelState.IsValid)
            {
                db.Entry(requestStatus).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.PermitRequest_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription", requestStatus.PermitRequest_requestNo);
            return View(requestStatus);
        }

        // GET: RequestStatus/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RequestStatus requestStatus = db.RequestStatus.Find(id);
            if (requestStatus == null)
            {
                return HttpNotFound();
            }
            return View(requestStatus);
        }

        // POST: RequestStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            RequestStatus requestStatus = db.RequestStatus.Find(id);
            db.RequestStatus.Remove(requestStatus);
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
