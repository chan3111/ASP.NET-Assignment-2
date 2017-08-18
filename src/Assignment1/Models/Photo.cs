using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment2.Models
{
    public class Photo
    {
        [Key]
        public int PhotoId
        {
            get;
            set;
        }

        [ForeignKey("BlogPost")]
        public int BlogPostId
        {
            get;
            set;
        }

        [StringLength(255)]
        public string Filename
        {
            get;
            set;
        }

        [StringLength(2048)]
        public string Url
        {
            get;
            set;
        }
    }
}
