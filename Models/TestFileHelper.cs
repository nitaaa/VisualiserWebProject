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

            public bool isValid()
            {
                string[] temp = Question.Split(':');
                if (temp[temp.Length - 1].Split(';').Length > 4)
                {
                    return false;
                }
                return true;
            }

            public bool isCorrect()
            {
                return (StudentResponse.Equals(CorrectResponse));
            }

            public void setAnswers()
            {
                string[] temp = Question.Split(':');
                if (temp.Length > 2)
                {
                    string[] text = new string[temp.Length - 2];
                    for (int i = 0; i < text.Length; i++)
                    {
                        text[i] = temp[i];
                    }
                    QuestionText = String.Concat(text);
                } else
                {
                    QuestionText = temp[0];
                }
                Answers = new string[4];
                Answers = temp[temp.Length - 1].Split(';');
                //TODO: Throw error if more than 4 options in Answers
            }

            public string[] getDistractors()
            {
                setAnswers();
                string[] dist = new string[3];
                int j = 0;
                for (int i = 0; i < 4; i++)
                {
                    if (!CorrectResponse.Equals(Answers[i].Trim()))
                    {
                        dist[j] = Answers[i];
                        j++;
                    }
                }
                
                return dist;
            }

            public override string ToString()
            {
                return Question;
            }

            public Question asQuestion()
            {
                //if (!isValid()) return new Question();
                Question q = new Question();
                q.qText = QuestionText;
                q.qCorrectAnswer = CorrectResponse;
                string[] distractors = getDistractors();
                q.qDistractor1 = distractors[0];
                q.qDistractor2 = distractors[1];
                q.qDistractor3 = distractors[2];
                return q;
            }

            public override bool Equals(object obj)
            {
                bool response = true;
                Question thisq = asQuestion();
                Question question = obj as Question;
                thisq.toList();
                question.toList();
                if (QuestionText == question.qText && CorrectResponse == question.qCorrectAnswer)
                {
                    foreach (string s in question.AllDistractors)
                    {
                        if (!thisq.AllDistractors.Contains(s)) response = false;
                    }
                }
                return response;
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