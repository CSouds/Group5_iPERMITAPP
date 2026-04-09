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
    public class PaymentsController : Controller
    {
        private Group5_iPERMITDBEntities db = new Group5_iPERMITDBEntities();

        // GET: Payments
        public ActionResult Index()
        {
            var payment = db.Payment.Include(p => p.OPS_CPP).Include(p => p.PermitRequest);
            return View(payment.ToList());
        }

        // GET: Payments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payment.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        // GET: Payments/Create
        public ActionResult Create()
        {
            ViewBag.approvedBy_OPS_ID = new SelectList(db.OPS_CPP, "ID", "Name");
            ViewBag.PermitRequest_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription");
            return View();
        }

        // POST: Payments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PaymentID,paymentDate,paymentMethod,isApproved,PermitRequest_requestNo,approvedBy_OPS_ID")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                db.Payment.Add(payment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.approvedBy_OPS_ID = new SelectList(db.OPS_CPP, "ID", "Name", payment.approvedBy_OPS_ID);
            ViewBag.PermitRequest_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription", payment.PermitRequest_requestNo);
            return View(payment);
        }

        // GET: Payments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payment.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            ViewBag.approvedBy_OPS_ID = new SelectList(db.OPS_CPP, "ID", "Name", payment.approvedBy_OPS_ID);
            ViewBag.PermitRequest_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription", payment.PermitRequest_requestNo);
            return View(payment);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PaymentID,paymentDate,paymentMethod,isApproved,PermitRequest_requestNo,approvedBy_OPS_ID")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(payment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.approvedBy_OPS_ID = new SelectList(db.OPS_CPP, "ID", "Name", payment.approvedBy_OPS_ID);
            ViewBag.PermitRequest_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription", payment.PermitRequest_requestNo);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payment.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Payment payment = db.Payment.Find(id);
            db.Payment.Remove(payment);
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
