using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CAT.Models
{
    public class Questions
    {
        [Key]
        public int ID { get; set; }
        public string Content { get; set; }
        public double a { get; set; }
        public double b { get; set; }
        public double c { get; set; }

        public List<Answers> Answers { get; set; }
    }
}
