using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using VisualiserWebProject.Models;

namespace VisualiserWebProject.Controllers
{
    public class TestsController : Controller
    {
        private QuizVisualiserDatabaseEntities db = new QuizVisualiserDatabaseEntities();

        // GET: Tests
        public ActionResult Index()
        {
            var tests = db.Tests.Include(t => t.Module).Include(t => t.User);
            return View(tests.ToList());
        }

        // GET: Tests/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Test test = db.Tests.Find(id);
            List<TestQuestion> testquestions = db.TestQuestions.Where(t => t.TestID == test.TestID).ToList();
            if (test == null)
            {
                return HttpNotFound();
            }
            return View(test);
        }

        // GET: Tests/Create
        public ActionResult Create()
        {
            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "moduleCode");
            ViewBag.assessor = new SelectList(db.Users, "UserID", "userFirstName");
            return View();
        }

        // POST: Tests/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "TestID,ModuleID,testTitle,testType,totalAttempts,uniqueAttempts,averageMark,testDate,uploadDate,assessor")] Test test)
        {
            if (ModelState.IsValid)
            {
                db.Tests.Add(test);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "moduleCode", test.ModuleID);
            ViewBag.assessor = new SelectList(db.Users, "UserID", "userFirstName", test.assessor);
            return View(test);
        }

        // GET: Tests/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Test test = db.Tests.Find(id);
            if (test == null)
            {
                return HttpNotFound();
            }
            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "moduleCode", test.ModuleID);
            ViewBag.assessor = new SelectList(db.Users, "UserID", "userFirstName", test.assessor);
            return View(test);
        }

        // POST: Tests/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "TestID,ModuleID,testTitle,testType,totalAttempts,uniqueAttempts,averageMark,testMark,testDate,uploadDate,assessor")] Test test)
        {
            if (ModelState.IsValid)
            {
                db.Entry(test).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "moduleCode", test.ModuleID);
            ViewBag.assessor = new SelectList(db.Users, "UserID", "userFirstName", test.assessor);
            return View(test);
        }

        // GET: Tests/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Test test = db.Tests.Find(id);
            if (test == null)
            {
                return HttpNotFound();
            }
            return View(test);
        }

        // POST: Tests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            //TODO untested
            Test test = db.Tests.Find(id);
            foreach (TestQuestion tq in test.TestQuestions)
            {
                db.TestQuestions.Remove(tq);
            }
            db.Tests.Remove(test);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult TestDashboard(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Test test = db.Tests.Find(id);
            if (test == null)
            {
                return HttpNotFound();
            }

            List<TestQuestion> testQuestions = db.TestQuestions.Where(x => x.TestID == test.TestID).ToList();
            ViewBag.TestQuestions = testQuestions;



            ViewBag.PercentPassed = ((double)test.testMark / test.totalAttempts).ToString() + "%";
            ViewBag.AveragePercent = (test.averageMark).ToString() + "%";


            string[] labels = { "Total Attempts", "Unique Attemps" };
            int[] data = { test.totalAttempts, test.uniqueAttempts };
            var jLabels = JsonConvert.SerializeObject(labels);
            var jdata = JsonConvert.SerializeObject(data);
            ViewBag.AttemptsData = jdata;
            ViewBag.AttemptsLabels = jLabels;


            //serialise to json: question number, question difficulty, question discrimination
            //serialise to json: question number, nr attempts, nr correct
            int start = 1;
            int end = testQuestions.Count();

            List<int> questionNr = Enumerable.Range(start, end).ToList();
            //    new List<string>();
            //for (int i = 1; i <= testQuestions.Count(); i++)
            //{
            //    questionNr.Add("Q" + i.ToString());
            //}
            ViewBag.QuestionNr = JsonConvert.SerializeObject(questionNr);

            List<double> qdiff = testQuestions.Select(t => (double)t.difficultyIndex*100).ToList(); 
            List<double> qdisc = testQuestions.Select(t => (double)t.discriminationIndex*100).ToList();
            ViewBag.LQDiff = JsonConvert.SerializeObject(qdiff);
            ViewBag.LQDisc = JsonConvert.SerializeObject(qdisc);

            List<int> nrAttempts = testQuestions.Select(t => t.questionCount()).ToList();
            List<int> nrCorrect = testQuestions.Select(t => t.correctSelected).ToList();
            ViewBag.LNrAttemps = JsonConvert.SerializeObject(nrAttempts);
            ViewBag.LNrCorrect = JsonConvert.SerializeObject(nrCorrect);

            //ViewBag.Jlabels = "";
            //ViewBag.JData = "";
            ViewData["TestQuestions"] = testQuestions;
            return View(test);
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
