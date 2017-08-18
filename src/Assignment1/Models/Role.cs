using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment2.Models
{
    public class Role
    {
        [Key]
        public int RoleId
        {
            get;
            set;
        }

        [StringLength(50)]
        public string Name
        {
            get;
            set;
        }
    }
}
