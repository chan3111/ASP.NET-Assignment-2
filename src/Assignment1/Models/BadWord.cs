using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Assignment2.Models
{
    public class BadWord
    {
        [Key]
        public int BadWordId
        {
            get;
            set;
        }
        [Required]
        [StringLength(50)]
        public string Word
        {
            get;
            set;
        }
    }
}
