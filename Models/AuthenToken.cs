using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAT.Models
{
    public class AuthenToken
    {
        public string Id { get; set; }
        public string AuthToken { get; set; }
        public string UserName { get; set; }
        public Boolean Continued { get; set; }
    }
}
