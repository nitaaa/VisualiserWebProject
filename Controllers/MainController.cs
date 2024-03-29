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
using Microsoft.Ajax.Utilities;
using System.Data.Entity.Validation;

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
        private int nrPassed;

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
            HttpPostedFileBase file = Request.Files[0];
            TempData["TestReport"] = ReadTestFile(file);
            TempData["TestFileXSLX"] = file;
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
            nrPassed = 0;
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
                                if (double.Parse(testline.Mark)/maxMark >= 0.5)
                                {
                                    nrPassed++;
                                }
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

                                    if (qrsf.isValid())
                                    {
                                        qrsf.setAnswers(); //Data Cleaning
                                    }
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
            TempData["Passed"] = nrPassed;
            nrQuestions = testReport.First().QuestionResult.Count;
            return testReport;

        }

        // GET: Main/ConfirmSubmission
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
            int nrQuestions = int.Parse(TempData["nrQuestions"].ToString());
            ViewBag.NumberQuestions = nrQuestions;
            currentFile = (List<TestFileHelper>)TempData["TestReport"];
            ViewData["TestReport"] = currentFile;

            TempData["TestReport"] = currentFile;
            TempData["nrQuestions"] = nrQuestions;

            return View();
        }

        // Post: Main/SubmitFile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SubmitFile()
        {
            currentFile = (List<TestFileHelper>)TempData["TestReport"];

            maxMark = (int)TempData["maxMark"];
            nrQuestions = (int)TempData["nrQuestions"];

            currentTest = (Test)TempData["currentTest"];
            currentTest.testMark = maxMark;
            currentTest.assessor = int.Parse(Request.Cookies["userID"].Value);

            //total attempts
            //unique attempts
            currentTest.totalAttempts = currentFile.Count();
            currentTest.uniqueAttempts = currentFile.Select(x => x.StudentID).Distinct().Count(); //changed
            currentTest.uploadDate = DateTime.Now;
            currentTest.averageMark = decimal.Parse("0.0");

            int testID = addTestToDB(currentTest);
            //average mark
            double totalmarks = 0;
            List<Question> tQuestions = new List<Question>();
            List<TestQuestion> TestQuestions = new List<TestQuestion>();            

            foreach (TestFileHelper line in currentFile)
            {
                totalmarks += double.Parse(line.Mark);
                List<QuestionResponseFileHelper> allQ = line.QuestionResult;
                TestQuestion tq;
                Question curQ = new Question();
                Question dbQ;
                foreach (QuestionResponseFileHelper qr in allQ)
                {
                    List<Question> dbQuestions = db.Questions.ToList();
                    if (!qr.isValid())
                    {
                        continue;
                    }
                    curQ = new Question();
                    curQ.qText = qr.QuestionText;
                    curQ.qCorrectAnswer = qr.CorrectResponse;
                    string[] dist = qr.getDistractors();
                    curQ.qDistractor1 = dist[0];
                    curQ.qDistractor2 = dist[1];
                    curQ.qDistractor3 = dist[2];

                    //check if new question in DB
                    if (dbQuestions.Any(o => o.Equals(curQ)))
                    {
                        //not new question - retrieve from DB
                        dbQ = dbQuestions.Where(o => o.Equals(curQ)).FirstOrDefault();
                    } else
                    {
                        //new question - add to DB
                        dbQ = addQuestionToDB(curQ);
                        dbQuestions.Add(dbQ);
                    }

                    //check if unique question in test
                    if (!TestQuestions.Any(o => o.QuestionID == dbQ.QuestionID))
                    {
                        //not unique
                        tq = new TestQuestion(dbQ.QuestionID, testID);
                        TestQuestions.Add(tq); //add to TestQuestions
                    } else
                    {
                        //exists in TestQuestions
                        tq = TestQuestions.Where(o => o.QuestionID == dbQ.QuestionID).FirstOrDefault();
                    }

                    if (qr.isCorrect())
                        tq.correctSelected++;
                    else
                    {
                        //distractor increment
                        if (qr.StudentResponse.Equals(dbQ.qDistractor1.Trim()))
                        {
                            tq.qD1Selected++;
                        } else if (qr.StudentResponse.Equals(dbQ.qDistractor2.Trim()))
                        {
                            tq.qD2Selected++;
                        } else if (qr.StudentResponse.Equals(dbQ.qDistractor3.Trim()))
                        {
                            tq.qD3Selected++;
                        }
                    }
                    
                } //All Questions for one attempt processed
            }//all attempts processed

            #region Item Analysis
            int p27 = (int)Math.Floor(currentTest.totalAttempts * 0.27);
            List<TestFileHelper> upper27 = new List<TestFileHelper>();
            List<TestFileHelper> lower27 = new List<TestFileHelper>();
            List<TestFileHelper> studentResponses = new List<TestFileHelper>(currentFile);

            //get upper 27%
            upper27 = studentResponses.OrderByDescending(o => int.Parse(o.Mark)).Take(p27).ToList();
            //get lower 27%
            lower27 = studentResponses.OrderBy(o => int.Parse(o.Mark)).Take(p27).ToList();
            double averageMark = (totalmarks / currentTest.totalAttempts) /  maxMark * 100; // changed
            foreach (TestQuestion testQuestion in TestQuestions)
            {
                int questionCount = testQuestion.questionCount();
                double difficultyIndex = (double)testQuestion.correctSelected/(double)questionCount;
                double discriminationIndex = 0;
                int upper27Correct = 0;
                int lower27Correct = 0;
                Question question;
                //using currentFile to get the top 27% of student and nr correct 
                //and  ^^^     bottom      ^^^
                foreach (TestFileHelper student in upper27)
                {
                    testQuestion.Question = db.Questions.Where(obj => obj.QuestionID == testQuestion.QuestionID).FirstOrDefault();
                    List<QuestionResponseFileHelper> sQuestions = student.QuestionResult.Where(ob => ob.isValid() && ob.asQuestion().Equals(testQuestion.Question)).ToList();
                    upper27Correct += sQuestions.Where(o => o.isCorrect()).Count();
                }

                foreach (TestFileHelper student in lower27)
                {
                    testQuestion.Question = db.Questions.Where(obj => obj.QuestionID == testQuestion.QuestionID).FirstOrDefault();
                    List<QuestionResponseFileHelper> sQuestions = student.QuestionResult.Where(ob => ob.isValid() && ob.asQuestion().Equals(testQuestion.Question)).ToList();
                    lower27Correct += sQuestions.Where(o => o.isCorrect()).Count();
                }

                discriminationIndex = ((double)(upper27Correct - lower27Correct)) / questionCount;
                testQuestion.difficultyIndex = Decimal.Parse(difficultyIndex.ToString());
                testQuestion.discriminationIndex = Decimal.Parse(discriminationIndex.ToString());
                testQuestion.markAllocation = 1;

                addTestQuestionToDB(testQuestion);
            }
            #endregion


            //YOU MADE CHANGES - testMark now NRPASSED
            currentTest.averageMark = (Decimal)averageMark;
            currentTest.testMark = (int)TempData["Passed"];
            try
            {
                currentTest.File = (HttpPostedFileBase)TempData["TestFileXSLX"];
                if (ModelState.IsValid)
                {
                    db.Entry(currentTest).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {

                throw;
            }
            return RedirectToAction("TestDashboard","Tests", new { id = currentTest.TestID }); //TODO: Update to correct page
        }

        //Add Test To DB
        public int addTestToDB(Test test)
        {
            if (ModelState.IsValid)
            {
                test.File = (HttpPostedFileBase)TempData["TestFileXSLX"];
                try
                {
                    db.Tests.Add(test);
                    db.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Console.WriteLine("- Property: \"{0}\", Value: \"{1}\", Error: \"{2}\"",
                                ve.PropertyName,
                                eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName),
                                ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            } else
            {
                throw new Exception("Error in Item Analysis, adding Test to database");
            }
            return test.TestID;
        }

        //Add Question To DB
        public Question addQuestionToDB(Question question)
        {
            question.toList();
            if (ModelState.IsValid)
            {
                db.Questions.Add(question);
                db.SaveChanges();
            } else
            {
                throw new Exception("Error in Item Analysis, adding Question to database");
            }
            return question;
        }

        //Add TestQuestion To DB
        public void addTestQuestionToDB(TestQuestion tq)
        {
            if (ModelState.IsValid)
            {
                db.TestQuestions.Add(tq);
                db.SaveChanges();
            } else
            {
                throw new Exception("Error in Item Analysis, adding TestQuestion to database");
            }
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