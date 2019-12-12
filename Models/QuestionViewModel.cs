using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAT.Models
{
    public class QuestionViewModel
    {
        public int QuestionStt { get; set; }
        public int IdQuestion { get; set; }
        public string ContentQuestion { get; set; }
        public int IdExam { get; set; }
        public List<AnswerViewModel> Answers { get; set; }
        public string Finished { get; set; }
        public long EndDate { get; set; }
        public double grade { get; set; }

    }

    public class AnswerViewModel
    {
        public int IdAnswer { get; set; }
        public string ContentAnswer { get; set; }
    }
}
