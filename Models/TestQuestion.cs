//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VisualiserWebProject.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    //Questions and answers from a specific test
    public partial class TestQuestion
    {
        public TestQuestion(int questionID, int testID)
        {
            QuestionID = questionID;
            TestID = testID;
            correctSelected = 0;
            qD1Selected = 0;
            qD2Selected = 0;
            qD3Selected = 0;

        }
        public TestQuestion() { }

        public int QuestionID { get; set; }
        public int TestID { get; set; }
        //number of times the correct answer was selected
        [Display(Name = "Correct Option Selected")]
        public int correctSelected { get; set; }
        [Display(Name = "Distractor 1 Selected")]
        public Nullable<int> qD1Selected { get; set; }
        [Display(Name = "Distractor 2 Selected")]
        public Nullable<int> qD2Selected { get; set; }
        [Display(Name = "Distractor 3 Selected")]
        public Nullable<int> qD3Selected { get; set; }
        public decimal markAllocation { get; set; }
        [Display(Name = "Difficulty Index")]
        public decimal difficultyIndex { get; set; }
        [Display(Name = "Discrimination Index")]
        public decimal discriminationIndex { get; set; }
    
        public virtual Question Question { get; set; }
        public virtual Test Test { get; set; }

        public int questionCount()
        {
            return (int)(correctSelected + qD1Selected + qD2Selected + qD3Selected);
        }
    }
}
