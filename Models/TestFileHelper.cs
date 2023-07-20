using FileHelpers;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisualiserWebProject.Models
{
    //Excel File 
    [DelimitedRecord("|")]
    public class TestFileHelper
    {
        //Test Report Layout

        public string Surname;
        public string FirstName ;
        public string StudentID;
        public string Email;
        public string State;
        public string Started;
        public string Completed;
        public string Time;
        public string Mark;

        public List<QuestionResponseFileHelper> QuestionResult;
        //Questions
        public class QuestionResponseFileHelper
        {
            public string Question;
            public string Response;
        }


    }
}