
using ExcelDataReader;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VisualiserWebProject.Models;
using static VisualiserWebProject.Models.TestFileHelper;

namespace VisualiserWebProject.Controllers
{
    [Authorize]
    public class MainController : Controller
    {
        private QuizVisualiserDatabaseEntities db = new QuizVisualiserDatabaseEntities();
        private int maxMark = 0;

        // GET: Main
        public ActionResult Dashboard()
        {
            return View();
        }

        // GET: Main/AddNewTest
        public ActionResult AddNewTest()
        {
            ViewBag.ModuleID = new SelectList(db.Modules, "ModuleID", "moduleCode");
            ViewBag.assessor = new SelectList(db.Users, "UserID", "userFirstName");
            return View();
        }

        // Post: Main/AddNewTest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewTest([Bind(Include = "ModuleID,testTitle,testType,testDate")] Test test)
        {
            //retrieve file from form
            HttpPostedFileBase inputFile = this.Request.Files["testReport"];

            //TODO: Data Cleaning - unique attempts, each question and relating answers

            //TODO: ITEM ANALYSIS - average mark, indices - separate method

            //TODO: Create new Test object then add to DB
            //TODO: Error if maxmark is <=0

            test.uniqueAttempts = 0;
            test.testMark = 0;


            //db.Tests.Add(test);
            //db.SaveChanges();
            return RedirectToAction("Dashboard"); //TODO: Update to correct page
        }

        public List<TestFileHelper> ReadTestFile(HttpPostedFileBase inputFile) //String filePath
        {
            int nrRows, nrColumns, curRow;
            int columnNumber = 0;
            List<string> columns = new List<string>();
            List<TestFileHelper> testReport = new List<TestFileHelper>();

            //Using the Excel Data Reader Package
            using (var stream = inputFile.InputStream) //System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
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
                        columnNumber = 1;
                        curRow = 0;
                        while (reader.Read())
                        {
                            if (curRow > 1)
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
                                //for the remainder of the columns, the format will be
                                //|Question n|Student Response n|Correct Response n|
                                while (columnNumber <= nrColumns)
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
                            }
                            else maxMark = int.Parse(reader.GetString(9).Split('/')[2]);
                        }
                    } while (reader.NextResult());

                }
            }

            //TODO: Show file lines

            return testReport;

        }

        public void ReadQuestionFile(string filePath)
        {
            //TODO: Read Question File (csv most likely - to check with Marinda)
            //TODO: QuestionHelper Model


            //TODO: Create new Question object then add to DB
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