using System;
using System.ComponentModel.DataAnnotations;

namespace api.premier.Models
{
    public class UserCreateDto
    {
        public int Id { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [Required]
        [StringLength(18, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        public string Password { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string MotherLastName { get; set; }
        public string MobilePhone { get; set; }
        [Required]
        public int RoleId { get; set; }
        public string Avatar { get; set; }
        public bool? Reset { get; set; }
        public bool? Status { get; set; }
        public bool? Send { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UpdateBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string token { get; set; }
    }
}
