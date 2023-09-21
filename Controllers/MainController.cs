﻿
using ExcelDataReader;
using NPOI.HSSF.Record;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using VisualiserWebProject.Models;
using static System.Net.Mime.MediaTypeNames;
using static VisualiserWebProject.Models.TestFileHelper;
using static VisualiserWebProject.Models.Question;

namespace VisualiserWebProject.Controllers
{
    [Authorize]
    public class MainController : Controller
    {
        private QuizVisualiserDatabaseEntities db = new QuizVisualiserDatabaseEntities();
        private List<TestFileHelper> currentFile;
        private Test currentTest;
        private int maxMark;
        private int nrQuestions;

        // GET: Main
        public ActionResult Dashboard()
        {
            return View();
        }

        // GET: Main/AddNewTest
        public ActionResult AddNewTest()
        {
            TempData.Clear(); //remove any temp data from previous submission if there is any

            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "moduleCode");
            ViewBag.assessor = new SelectList(db.Users, "UserID", "userFirstName");

            return View();
        }

        // Post: Main/AddNewTest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewTest([Bind(Include = "ModuleID,testTitle,testType,testDate")] Test test)
        {
            if (Request.Files.Count < 1)
            {
                throw new ArgumentNullException("No File Specified");
            }
            //retrieve file from form
            TempData["TestReport"] = ReadTestFile(Request.Files[0]);
            TempData["currentTest"] = test;
            TempData["maxMark"] = maxMark;
            TempData["nrQuestions"] = nrQuestions;
                        
            return RedirectToAction("ConfirmSubmission");

        }


        public List<TestFileHelper> ReadTestFile(HttpPostedFileBase inputFile) //String filePath
        {
            int nrRows, nrColumns, curRow;
            int columnNumber = 0;
            List<string> columns = new List<string>();
            List<TestFileHelper> testReport = new List<TestFileHelper>();

            //Using the Excel Data Reader Package
            //FileStream stream = new FileStream(inputFile.FileName, FileMode.Open,FileAccess.Read);
            using (Stream stream = inputFile.InputStream) //System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                using (var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration()
                {
                    AutodetectSeparators = new char[] { '|' }
                }))
                {
                    //TODO: testing, this is assumed functionality
                    //limit number of fields based on the anonimity of student data
                    do
                    {
                        nrRows = reader.RowCount;
                        nrColumns = reader.FieldCount;
                        columnNumber = 0;
                        curRow = 0;
                        while (reader.Read())
                        {
                            if (curRow > 0)
                            {
                                TestFileHelper testline = new TestFileHelper();
                                testline.Surname = reader.GetString(columnNumber);
                                columnNumber++;
                                testline.FirstName = reader.GetString(columnNumber);
                                columnNumber++;
                                testline.StudentID = reader.GetString(columnNumber);
                                columnNumber++;
                                testline.Email = reader.GetString(columnNumber);
                                columnNumber++;
                                testline.State = reader.GetString(columnNumber);
                                columnNumber++;
                                testline.Started = reader.GetString(columnNumber);
                                columnNumber++;
                                testline.Completed = reader.GetString(columnNumber);
                                columnNumber++;
                                //format "x mins y secs"
                                //TODO: write parser to get in proper DateTime format
                                testline.Time = reader.GetString(columnNumber);
                                columnNumber++;
                                testline.Mark = reader.GetString(columnNumber);
                                columnNumber++;

                                testline.QuestionResult = new List<QuestionResponseFileHelper>();
                                //for the remainder of the columns, the format will be
                                //|Question n|Student Response n|Correct Response n|
                                while (columnNumber < nrColumns)
                                {
                                    QuestionResponseFileHelper qrsf = new QuestionResponseFileHelper();
                                    qrsf.Question = reader.GetString(columnNumber);
                                    columnNumber++;
                                    qrsf.StudentResponse = reader.GetString(columnNumber);
                                    columnNumber++;
                                    qrsf.CorrectResponse = reader.GetString(columnNumber);
                                    columnNumber++;
                                    qrsf.setAnswers(); //Data Cleaning
                                    testline.QuestionResult.Add(qrsf);
                                }
                                testReport.Add(testline);
                                columnNumber = 0;
                            }
                            else maxMark = int.Parse(reader.GetString(8).Split('/')[1]);
                            curRow++;
                        }
                    } while (reader.NextResult());

                }
            }
            nrQuestions = testReport.First().QuestionResult.Count;
            return testReport;

        }

        // GET: Main/AddNewTest
        public ActionResult ConfirmSubmission()
        {
            //ViewData["TestReport"] = currentFile;

            //ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "moduleCode");
            //ViewBag.assessor = new SelectList(db.Users, "UserID", "userFirstName");

            //put in test info
            if (TempData.Count < 4)
            {
                throw new Exception("Values Not Found");
            }
            ViewBag.NumberQuestions = TempData["nrQuestions"];
            ViewData["TestReport"] = TempData["TestReport"];


            return View();
        }

        // Post: Main/AddNewTest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitFile()
        {
            //TODO: Data Cleaning - unique attempts, each question and relating answers
            //TODO: ITEM ANALYSIS - average mark, indices - separate method




            //TODO: Create new Test object then add to DB
            //TODO: Error if maxmark is <=0
            currentFile = (List<TestFileHelper>)TempData["TestReport"];

            maxMark = (int)TempData["maxMark"];
            nrQuestions = (int)TempData["nrQuestions"];

            currentTest = (Test)TempData["currentTest"];
            currentTest.uniqueAttempts = currentFile.Distinct().Count(); //select distinct based on student ID
            currentTest.testMark = maxMark;
            currentTest.assessor = int.Parse(Request.Cookies["userID"].Value);

            //total attempts
            //unique attempts
            currentTest.totalAttempts = currentFile.Count();
            currentTest.uniqueAttempts = currentFile.Distinct().Count();

            int testID = addTestToDB(currentTest);
            //average mark
            double totalmarks = 0;
            List<Question> tQuestions = new List<Question>();
            List<TestQuestion> TestQuestions = new List<TestQuestion>();

            foreach (TestFileHelper line in currentFile)
            {
                totalmarks += double.Parse(line.Mark);
                List<QuestionResponseFileHelper> allQ = line.QuestionResult;
                List<TestQuestion> uniqueQuestions = new List<TestQuestion>();
                TestQuestion tq;
                Question curQ = new Question();
                Question dbQ;
                foreach (QuestionResponseFileHelper qr in allQ)
                {
                    curQ.qText = qr.QuestionText;
                    curQ.qCorrectAnswer = qr.CorrectResponse;
                    string[] dist = qr.getDistractors();
                    curQ.qDistractor1 = dist[0];
                    curQ.qDistractor2 = dist[1];
                    curQ.qDistractor3 = dist[2];

                    //check if new question in DB
                    if (db.Questions.Any(o => o.Equals(curQ)))
                    {
                        //not new question - retrieve from DB
                        dbQ = db.Questions.Where(o => o.Equals(curQ)).FirstOrDefault();
                    } else
                    {
                        //new question - add to DB
                        dbQ = addQuestionToDB(curQ);
                    }

                    //check if unique question in test
                    tq = new TestQuestion(dbQ.QuestionID, testID);
                }
                

            }

            //ADDING TO DB
            //db.Tests.Add(test); //check if this still works
            //db.SaveChanges();
            return RedirectToAction("Dashboard"); //TODO: Update to correct page
        }

        //Add Test To DB
        public int addTestToDB(Test test)
        {
            if (ModelState.IsValid)
            {
                db.Tests.Add(test);
                db.SaveChanges();
            }
            int ID = db.Tests.LastOrDefault().TestID;
            return ID;
        }
        //Add Question To DB
        public Question addQuestionToDB(Question question)
        {
            if (ModelState.IsValid)
            {
                db.Questions.Add(question);
                db.SaveChanges();
            }
            return db.Questions.Where(o => o.qText == question.qText).LastOrDefault();
        }

        //Count LOC REGEX: ^(?!(\s*\*))(?!(\s*\-\-\>))(?!(\s*\<\!\-\-))(?!(\s*\n))(?!(\s*\*\/))(?!(\s*\/\*))(?!(\s*\/\/\/))(?!(\s*\/\/))(?!(\s*\}))(?!(\s*\{))(?!(\s(using))).*$
        //https://stackoverflow.com/questions/1244729/how-do-you-count-the-lines-of-code-in-a-visual-studio-solution

        //Assign Module to Lecturer
        // GET: Main/AssignModule
        public ActionResult AssignModule()
        {
            //getting the full names of each user to assign a module to
            db.Configuration.ProxyCreationEnabled = false;
            List<SelectListItem> userNames = new List<SelectListItem>();
            foreach (User u in db.Users)
            {
                SelectListItem item = new SelectListItem();
                item.Value = u.UserID.ToString();
                item.Text = u.userFirstName + " " + u.userLastName;
                userNames.Add(item);
            }
            //viewbag for the dropdown lists
            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "moduleCode");
            ViewBag.assessor = new SelectList(userNames,"Value","Text");
            return View();
        }

        // Post: Main/AssignModule
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignModule(object sender)
        {
            //TODO: Assign module to user -> change the test.assessor to new user
            //add option to remove current lecturer

            return View();
        }
    }
}