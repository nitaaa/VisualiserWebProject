using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VisualiserWebProject.Models
{
    public class QuestionFrequency
    {
        public QuestionFrequency() { }

        public QuestionFrequency(string text, int? nrSelected)
        {
            this.text = text;
            if (nrSelected != null) { this.nrSelected = 0; } 
            else this.nrSelected = (int)nrSelected;
        }

        public string text;
        public int nrSelected;
    }
}