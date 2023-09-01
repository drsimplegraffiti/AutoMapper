using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace Techie.Modal
{
    public class UserModel
    {

        [Required (ErrorMessage = "Code is required")]
        public string Code { get; set; } = string.Empty;

        [Required (ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = "User";

        [Required (ErrorMessage = "Name is required")]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required (ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;


        public string Name { get; set; } = string.Empty;


    }

}