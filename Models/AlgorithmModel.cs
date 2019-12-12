using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAT.Models
{
    public class AlgorithmModel
    {
        public double Theta { get; set; }
        public double SumI { get; set; }
        public double SumS { get; set; }
        public double SE { get; set; }
        public int IdQuestion { get; set; }
        public bool finished { get; set; }
    }
}
