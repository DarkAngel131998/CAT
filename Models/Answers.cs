using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Models
{
    public class Answers
    {
        [Key]
        public int ID { get; set; }
        public int QuestionID { get; set; }
        public string Content { get; set; }
        public Boolean RightAnswer { get; set; }

        public Questions Questions { get; set; }
    }
}
