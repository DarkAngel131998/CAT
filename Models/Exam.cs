using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CAT.Models
{
    public class Exam
    {
        [Key]
        public int ID { get; set; }
        public string UserID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string QuestionID { get; set; }
        public double Theta { get; set; }
        public double SumI { get; set; }
        public double SumS { get; set; }
        public double SE { get; set; }
        public Boolean Finished { get; set; }
        public Boolean StatusPreviousAnswer { get; set; }
        public User Users { get; set; }

    }
}
