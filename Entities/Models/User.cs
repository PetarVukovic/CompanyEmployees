using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
	public class User:IdentityUser
	{
		public string? FirstName { get; set; }

		public string? LastName { get; set;}

		/*
		 * Here we add two additional properties, which we are going to add to the 
			AspNetUsers table
		 */
		public string? RefreshToken { get; set; }

		public DateTime RefreshTokenExpiryTime { get; set; }
	}
}
