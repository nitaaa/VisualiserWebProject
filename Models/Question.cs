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

    public partial class Question
    {
        public Question()
        {
            this.TestQuestions = new HashSet<TestQuestion>();
            this.Topics = new HashSet<Topic>();
        }
        //Autogenerated | Annotation - Key (uniquely identify entity)
        [Key]
        public int QuestionID { get; set; }

        [Display(Name = "Question: ")]
        [Required(ErrorMessage = "Please enter the question.", AllowEmptyStrings = false)]
        [StringLength(255)]
        public string qText { get; set; }

        [Display(Name = "Correct Answer: ")]
        [Required(ErrorMessage = "Please enter the correct answer to the question.", AllowEmptyStrings = false)]
        [StringLength(255)]
        public string qCorrectAnswer { get; set; }

        [Display(Name = "Distractor 1: ")]
        [Required(ErrorMessage = "Please enter an incorrect answer to the question.", AllowEmptyStrings = false)]
        [StringLength(255)]
        public string qDistractor1 { get; set; }

        [Display(Name = "Distractor 2: ")]
        [Required(ErrorMessage = "Please enter an incorrect answer to the question.", AllowEmptyStrings = false)]
        [StringLength(255)]
        public string qDistractor2 { get; set; }

        [Display(Name = "Distractor 3: ")]
        [Required(ErrorMessage = "Please enter an incorrect answer to the question.", AllowEmptyStrings = false)]
        [StringLength(255)]
        public string qDistractor3 { get; set; }
    
        public virtual ICollection<TestQuestion> TestQuestions { get; set; }
        public virtual ICollection<Topic> Topics { get; set; }

        public List<String> AllDistractors;

        private void toList()
        {
            AllDistractors = new List<string>();
            AllDistractors.Add(qDistractor1);
            AllDistractors.Add(qDistractor2);
            AllDistractors.Add(qDistractor3);
        }

        public override bool Equals(object obj)
        {
            bool response = true;
            this.toList();
            Question q2 = obj as Question;
            q2.toList();
            if (qText == q2.qText && qCorrectAnswer == q2.qCorrectAnswer)
            {
                foreach (string s in q2.AllDistractors)
                {
                    if (!this.AllDistractors.Contains(s)) response = false;
                }
            }
            return response;
        }
    }
}
