using System;
using System.Collections.Generic;

namespace api.premier.Models
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string MotherLastName { get; set; }
        public string MobilePhone { get; set; }
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
