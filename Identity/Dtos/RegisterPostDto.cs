using System;
using System.ComponentModel.DataAnnotations;

namespace Identity.Dtos
{
	public class RegisterPostDto
	{
		[EmailAddress]
		public string Email { get; set; }
		public string Password { get; set; }
		[Compare("Password")]
		public string ConfirmPassword { get; set; }
    }
}

