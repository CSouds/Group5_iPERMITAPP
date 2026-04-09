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
    public class PermitsController : Controller
    {
        private Group5_iPERMITDBEntities db = new Group5_iPERMITDBEntities();

        // GET: Permits
        public ActionResult Index()
        {
            var permit = db.Permit.Include(p => p.EO).Include(p => p.RE).Include(p => p.PermitRequest);
            return View(permit.ToList());
        }

        // GET: Permits/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Permit permit = db.Permit.Find(id);
            if (permit == null)
            {
                return HttpNotFound();
            }
            return View(permit);
        }

        // GET: Permits/Create
        public ActionResult Create()
        {
            ViewBag.issuedBy_EO_ID = new SelectList(db.EO, "ID", "Name");
            ViewBag.issuedTo_RE_ID = new SelectList(db.RE, "ID", "contactPersonName");
            ViewBag.relatedTo_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription");
            return View();
        }

        // POST: Permits/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "PermitID,issueDate,issuedBy_EO_ID,issuedTo_RE_ID,relatedTo_requestNo")] Permit permit)
        {
            if (ModelState.IsValid)
            {
                db.Permit.Add(permit);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.issuedBy_EO_ID = new SelectList(db.EO, "ID", "Name", permit.issuedBy_EO_ID);
            ViewBag.issuedTo_RE_ID = new SelectList(db.RE, "ID", "contactPersonName", permit.issuedTo_RE_ID);
            ViewBag.relatedTo_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription", permit.relatedTo_requestNo);
            return View(permit);
        }

        // GET: Permits/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Permit permit = db.Permit.Find(id);
            if (permit == null)
            {
                return HttpNotFound();
            }
            ViewBag.issuedBy_EO_ID = new SelectList(db.EO, "ID", "Name", permit.issuedBy_EO_ID);
            ViewBag.issuedTo_RE_ID = new SelectList(db.RE, "ID", "contactPersonName", permit.issuedTo_RE_ID);
            ViewBag.relatedTo_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription", permit.relatedTo_requestNo);
            return View(permit);
        }

        // POST: Permits/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "PermitID,issueDate,issuedBy_EO_ID,issuedTo_RE_ID,relatedTo_requestNo")] Permit permit)
        {
            if (ModelState.IsValid)
            {
                db.Entry(permit).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.issuedBy_EO_ID = new SelectList(db.EO, "ID", "Name", permit.issuedBy_EO_ID);
            ViewBag.issuedTo_RE_ID = new SelectList(db.RE, "ID", "contactPersonName", permit.issuedTo_RE_ID);
            ViewBag.relatedTo_requestNo = new SelectList(db.PermitRequest, "requestNo", "activityDescription", permit.relatedTo_requestNo);
            return View(permit);
        }

        // GET: Permits/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Permit permit = db.Permit.Find(id);
            if (permit == null)
            {
                return HttpNotFound();
            }
            return View(permit);
        }

        // POST: Permits/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            Permit permit = db.Permit.Find(id);
            db.Permit.Remove(permit);
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
