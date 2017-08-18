using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment2.Models
{
    public class BlogPost
    {
        [Key]
        public int BlogPostId
        {
            get;
            set;
        }

        [ForeignKey("User")]
        public int UserId
        {
            get;
            set;
        }

        [Required]
        [StringLength(200)]
        public string Title
        {
            get;
            set;
        }

        [Required]
        [StringLength(400)]
        public string ShortDescription
        {
            get;
            set;
        }

        [Required]
        [StringLength(4000)]
        public string Content
        {
            get;
            set;
        }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Posted
        {
            get;
            set;
        }

        [Required]
        public bool IsAvailable
        {
            get;
            set;
        }

        public User User { get; set; }

        public ICollection<Photo> Photos { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }
}
