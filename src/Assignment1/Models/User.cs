using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment2.Models
{
    public class User
    {
        [Key]
        public int UserId
        {
            get;
            set;
        }

        [ForeignKey("Role")]
        public int RoleId
        {
            get;
            set;
        }

        [Required]
        [StringLength(50)]
        public string FirstName
        {
            get;
            set;
        }

        [Required]
        [StringLength(50)]
        public string LastName
        {
            get;
            set;
        }

        [Required]
        [StringLength(50)]
        public string EmailAddress
        {
            get;
            set;
        }

        [Required]
        [StringLength(50)]
        public string Password
        {
            get;
            set;
        }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DateOfBirth
        {
            get;
            set;
        }

        [Required]
        [StringLength(50)]
        public string City
        {
            get;
            set;
        }

        [Required]
        [StringLength(50)]
        public string Address
        {
            get;
            set;
        }

        [Required]
        [StringLength(50)]
        public string PostalCode
        {
            get;
            set;
        }

        [Required]
        [StringLength(50)]
        public string Country
        {
            get;
            set;
        }

        public virtual Role Role { get; set; }

        public virtual ICollection<BlogPost> BlogPosts { get; set; }
    }
}
