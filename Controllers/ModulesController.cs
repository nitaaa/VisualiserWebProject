using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VisualiserWebProject.Models;

namespace VisualiserWebProject.Controllers
{
    public class ModulesController : Controller
    {
        private QuizVisualiserDatabaseEntities db = new QuizVisualiserDatabaseEntities();

        // GET: Modules
        public ActionResult Index(string sortOrder, string searchString, string filter)
        {
            ViewBag.CodeSortParm = String.IsNullOrEmpty(sortOrder) ? "code_desc" : "";
            ViewBag.NameSortParm = sortOrder == "Name" ? "name_desc" : "Name";
            var modules = db.Modules.Include(m => m.User);

            if (!String.IsNullOrEmpty(searchString))
            {
                modules = modules.Where(s => s.moduleName.Contains(searchString) || s.moduleCode.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(filter))
            {
                modules = db.Modules.Where(o => o.moduleCode == filter).Include(m => m.User);
            }

            switch (sortOrder)
            {
                case "code_desc":
                    modules = modules.OrderByDescending(s => s.moduleCode);
                    break;
                case "Name":
                    modules = modules.OrderBy(s => s.moduleName);
                    break;
                case "name_desc":
                    modules = modules.OrderByDescending(s => s.moduleName);
                    break;
                default:
                    modules = modules.OrderBy(s => s.moduleCode);
                    break;
            }
            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "moduleCode");

            return View(modules.ToList());
        }

        // GET: Modules/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Module module = db.Modules.Find(id);
            if (module == null)
            {
                return HttpNotFound();
            }
            return View(module);
        }

        // GET: Modules/Create
        public ActionResult Create()
        {
            //getting the full name of the user
            db.Configuration.ProxyCreationEnabled = false;
            List<SelectListItem> userNames = new List<SelectListItem>();
            foreach (User u in db.Users)
            {
                SelectListItem item = new SelectListItem();
                item.Value = u.UserID.ToString();
                item.Text = u.userFirstName + " " + u.userLastName;
                userNames.Add(item);
            }
            ViewBag.UserID = new SelectList(userNames, "Value", "Text");
            return View();
        }

        // POST: Modules/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ModuleID,UserID,moduleCode,moduleName")] Module module)
        {
            module.archived = false;
            if (ModelState.IsValid)
            {
                db.Modules.Add(module);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(db.Users, "UserID", "userFirstName", module.UserID);
            return View(module);
        }

        // GET: Modules/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Module module = db.Modules.Find(id);
            if (module == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserID = new SelectList(db.Users, "UserID", "userFirstName", module.UserID);
            return View(module);
        }

        // POST: Modules/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ModuleID,UserID,moduleCode,moduleName,archived")] Module module)
        {
            if (ModelState.IsValid)
            {
                db.Entry(module).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.Users, "UserID", "userFirstName", module.UserID);
            return View(module);
        }

        // GET: Modules/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Module module = db.Modules.Find(id);
            if (module == null)
            {
                return HttpNotFound();
            }
            return View(module);
        }

        // POST: Modules/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Module module = db.Modules.Find(id);
            db.Modules.Remove(module);
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
