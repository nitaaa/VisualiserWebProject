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
            public string StudentResponse;
            public string CorrectResponse;

            public string QuestionText;
            public string[] Answers;

            public bool isCorrect()
            {
                return (StudentResponse.Equals(CorrectResponse));
            }

            public void setAnswers()
            {
                string[] temp = Question.Split(':');
                QuestionText = temp[0];
                Answers = new string[4];
                Answers = temp[1].Split(';');
                //TODO: Throw error if more than 4 options in Answers
            }

            public override string ToString()
            {
                return Question;
            }
        }

        public override bool Equals(object obj)
        {
            if (((TestFileHelper)obj).StudentID == StudentID)
            return true;
            else return false;
        }
    }
}