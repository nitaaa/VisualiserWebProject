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
    
    //Questions and answers from a specific test
    public partial class TestQuestion
    {
        public TestQuestion(int questionID, int testID)
        {
            QuestionID = questionID;
            TestID = testID;
        }

        public int QuestionID { get; set; }
        public int TestID { get; set; }
        //number of times the correct answer was selected
        public int correctSelected { get; set; }
        public Nullable<int> qD1Selected { get; set; }
        public Nullable<int> qD2Selected { get; set; }
        public Nullable<int> qD3Selected { get; set; }
        public decimal markAllocation { get; set; }
        public decimal difficultyIndex { get; set; }
        public decimal discriminationIndex { get; set; }
    
        public virtual Question Question { get; set; }
        public virtual Test Test { get; set; }
    }
}
