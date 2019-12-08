using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace CAT.Models
{
    public class User : IdentityUser
    {
        [JsonIgnore]
        public override string PasswordHash { get => base.PasswordHash; set => base.PasswordHash = value; }
        [JsonIgnore]
        public override string SecurityStamp { get => base.SecurityStamp; set => base.SecurityStamp = value; }
        [JsonIgnore]
        public override string ConcurrencyStamp { get => base.ConcurrencyStamp; set => base.ConcurrencyStamp = value; }
        [JsonIgnore]
        public override string NormalizedUserName { get => base.NormalizedUserName; set => base.NormalizedUserName = value; }
        [JsonIgnore]
        public override string NormalizedEmail { get => base.NormalizedEmail; set => base.NormalizedEmail = value; }
        public string FullName { get; set; }
        public List<Exam> Exams { get; set; }
    }
}

