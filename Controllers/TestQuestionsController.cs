using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VisualiserWebProject.Models;
using EntityState = System.Data.EntityState;

namespace VisualiserWebProject.Controllers
{
    public class TestQuestionsController : Controller
    {
        private QuizVisualiserDatabaseEntities db = new QuizVisualiserDatabaseEntities();

        // GET: TestQuestions
        public ActionResult Index()
        {
            var testQuestions = db.TestQuestions.Include(t => t.Question).Include(t => t.Test);
            return View(testQuestions.ToList());
        }

        // GET: TestQuestions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TestQuestion testQuestion = db.TestQuestions.Find(id);
            if (testQuestion == null)
            {
                return HttpNotFound();
            }
            return View(testQuestion);
        }

        // GET: TestQuestions/Create
        public ActionResult Create()
        {
            ViewBag.QuestionID = new SelectList(db.Questions, "QuestionID", "qText");
            ViewBag.TestID = new SelectList(db.Tests, "TestID", "testTitle");
            return View();
        }

        // POST: TestQuestions/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "QuestionID,TestID,correctSelected,qD1Selected,qD2Selected,qD3Selected,markAllocation,difficultyIndex,discriminationIndex")] TestQuestion testQuestion)
        {
            if (ModelState.IsValid)
            {
                db.TestQuestions.Add(testQuestion);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.QuestionID = new SelectList(db.Questions, "QuestionID", "qText", testQuestion.QuestionID);
            ViewBag.TestID = new SelectList(db.Tests, "TestID", "testTitle", testQuestion.TestID);
            return View(testQuestion);
        }

        // GET: TestQuestions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TestQuestion testQuestion = db.TestQuestions.Find(id);
            if (testQuestion == null)
            {
                return HttpNotFound();
            }
            ViewBag.QuestionID = new SelectList(db.Questions, "QuestionID", "qText", testQuestion.QuestionID);
            ViewBag.TestID = new SelectList(db.Tests, "TestID", "testTitle", testQuestion.TestID);
            return View(testQuestion);
        }

        // POST: TestQuestions/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "QuestionID,TestID,correctSelected,qD1Selected,qD2Selected,qD3Selected,markAllocation,difficultyIndex,discriminationIndex")] TestQuestion testQuestion)
        {
            if (ModelState.IsValid)
            {
                db.Entry(testQuestion).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.QuestionID = new SelectList(db.Questions, "QuestionID", "qText", testQuestion.QuestionID);
            ViewBag.TestID = new SelectList(db.Tests, "TestID", "testTitle", testQuestion.TestID);
            return View(testQuestion);
        }

        // GET: TestQuestions/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TestQuestion testQuestion = db.TestQuestions.Find(id);
            if (testQuestion == null)
            {
                return HttpNotFound();
            }
            return View(testQuestion);
        }

        // POST: TestQuestions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TestQuestion testQuestion = db.TestQuestions.Find(id);
            db.TestQuestions.Remove(testQuestion);
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
