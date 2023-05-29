using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Entities.ErrorModels
{
	public class ErrorDetails
	{
		/*
		  We are going to use this class for the details of our error message.
		 */
		public int StatusCode { get; set; }
		public string? Message { get; set; }
		public override string ToString()=>JsonSerializer.Serialize(this);
	}
}
