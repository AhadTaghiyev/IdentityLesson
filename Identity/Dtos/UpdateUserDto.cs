using System;
namespace Identity.Dtos
{
	public class UpdateUserDto
	{
		public string Email { get; set; } = null!;
		public string? OldPassword { get; set; } 
		public string? NewPassword { get; set; } 
    }
}

