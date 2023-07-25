
using ExcelDataReader;
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
    public class MainController : Controller
    {
        // GET: Main
        //[Authorize]
        public ActionResult Dashboard()
        {
            return View();
        }

        // GET: Main/AddNewTest
        public ActionResult AddNewTest()
        {
            return View();
        }

        // Post: Main/AddNewTest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNewTest(object sender)
        {
            //retrieve file from form
            var File = this.Request.Files["testReport"];
            //TODO: save file to temporary storage (but how)


            return View();
        }

        public void ReadTestFile(String filePath)
        {
            int nrRows, nrColumns, curRow,grade;
            int columnNumber = 0;
            List<string> columns = new List<string>();
            List<TestFileHelper> testReport = new List<TestFileHelper>();

            //Using the Excel Data Reader Package
            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx, *.xlsb)
                using (var reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration()
                {
                    AutodetectSeparators = new char[] {'|'}
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
                                //for the remainder of the columns, the format will be |Question n|Response n|
                                while (columnNumber <= nrColumns)
                                {
                                    QuestionResponseFileHelper qrsf = new QuestionResponseFileHelper();
                                    qrsf.Question = reader.GetString(columnNumber);
                                    columnNumber++;
                                    qrsf.Response = reader.GetString(columnNumber);
                                    columnNumber++;
                                    //TODO: Check if response is correct - part of item analysis
                                    testline.QuestionResult.Add(qrsf);
                                }
                                testReport.Add(testline);
                            } else grade = int.Parse(reader.GetString(9).Split('/')[2]);
                        }
                    } while (reader.NextResult());

                    //TODO: Create new Test object then add to DB

                }
            }
        }

        public void ReadQuestionFile(string filePath)
        {
            //TODO: Read Question File (csv most likely - to check with Marinda)
            //TODO: QuestionHelper Model


            //TODO: Create new Question object then add to DB
        }
    }
}