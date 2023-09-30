using Antlr.Runtime.Misc;
using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Security.Policy;
using System.Text;
using System.Web;
using System.Web.Mvc;
using VisualiserWebProject.Models;
using static NPOI.HSSF.Util.HSSFColor;
//using EntityState = System.Data.EntityState;

namespace VisualiserWebProject.Controllers
{
    public class TestQuestionsController : Controller
    {
        private QuizVisualiserDatabaseEntities db = new QuizVisualiserDatabaseEntities();

        // GET: TestQuestions
        public ActionResult Index()
        {
            
            var testQuestions = db.TestQuestions.Include(t => t.Question).Include(t => t.Test).Include(t => t.Test.Module);
            
            return View(testQuestions.ToList());
        }

        // GET: TestQuestions/Details/5
        public ActionResult Details(int Qid, int Tid)
        {
            TestQuestion testQuestion = db.TestQuestions.Where(t => t.TestID == Tid && t.QuestionID == Qid).FirstOrDefault();
            if (testQuestion == null)
            {
                return HttpNotFound();
            }
            string[] labels = { testQuestion.Question.qCorrectAnswer, testQuestion.Question.qDistractor1.Trim(),
                testQuestion.Question.qDistractor2.Trim(), testQuestion.Question.qDistractor3.Trim() };
            int[] data = {testQuestion.correctSelected,testQuestion.qD1Selected.GetValueOrDefault(0),
                testQuestion.qD2Selected.GetValueOrDefault(0), testQuestion.qD3Selected.GetValueOrDefault(0) };
            var jLabels = JsonConvert.SerializeObject(labels);
            var jdata = JsonConvert.SerializeObject(data);

            ViewBag.DifInd = (testQuestion.difficultyIndex * 100).ToString() + "%";
            ViewBag.DiscInd = (testQuestion.discriminationIndex * 100).ToString() + "%";
            // Difficulty Index:
            // 0.95 - 1.0 = Too easy(not doing much good to differentiate examinees, which is really the purpose of assessment)
            // 0.60 - 0.95 = Typical
            // 0.40 - 0.60 = Hard
            // < 0.40 = Too hard(consider that a 4 option multiple choice has a 25 % chance of pure guessing)
            switch (double.Parse(testQuestion.difficultyIndex.ToString()))
            {
                case double d when d < 0.40: 
                    ViewBag.Difficulty = "Too hard";
                    ViewBag.DifBGColor = "#ce8888";
                    ViewBag.DifColor = "#9c1111";
                    break;
                case double d when d < 0.60 && d >= 0.40:
                    ViewBag.Difficulty = "Hard";
                    ViewBag.DifBGColor = "#fdcd80";
                    ViewBag.DifColor = "#fa9a00";
                    break;
                case double d when d < 0.95 && d >= 0.60:
                    ViewBag.Difficulty = "Typical";
                    ViewBag.DifBGColor = "#83d98a";
                    ViewBag.DifColor = "#058610";
                    break;
                case double d when d >= 0.95:
                    ViewBag.Difficulty = "Too Easy";
                    ViewBag.DifBGColor = "#ce8888";
                    ViewBag.DifColor = "#9c1111";
                    break;
                default:
                    ViewBag.Difficulty = "Unknown";
                    ViewBag.DifBGColor = "#878787";
                    ViewBag.DifColor = "#222222";
                    break;
            }

            //Discrimination Index
            //0.20 + = Good item; smarter examinees tend to get the item correct
            //0.10 - 0.20 = OK item; but probably review it
            //0.0 - 0.10 = Marginal item quality; should probably be revised or replaced
            //<0.0 = Terrible item; replace it
            switch (double.Parse(testQuestion.discriminationIndex.ToString()))
            {
                case double d when d < 0.0:
                    ViewBag.Discrimination = "Remove";
                    ViewBag.DiscBGColor = "#ce8888";
                    ViewBag.DiscColor = "#9c1111";
                    ViewBag.DiscInd = "0%";
                    break;
                case double d when d < 0.10 && d >= 0.0:
                    ViewBag.Discrimination = "Revise";
                    ViewBag.DiscBGColor = "#fdcd80";
                    ViewBag.DiscColor = "#fa9a00";
                    break;
                case double d when d < 0.20 && d >= 0.10:
                    ViewBag.Discrimination = "Marginal";
                    ViewBag.DiscBGColor = "#fdcd80";
                    ViewBag.DiscColor = "#fa9a00";
                    break;
                case double d when d >= 0.20:
                    ViewBag.Discrimination = "Good";
                    ViewBag.DiscBGColor = "#83d98a";
                    ViewBag.DiscColor = "#058610";
                    break;
                default:
                    ViewBag.Discrimination = "Unknown";
                    ViewBag.DiscBGColor = "#878787";
                    ViewBag.DiscColor = "#222222";
                    break;
            }

            ViewBag.JLabels = jLabels;
            ViewBag.JData = jdata;
            
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
