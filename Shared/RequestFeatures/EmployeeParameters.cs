using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.RequestFeatures
{
	public class EmployeeParameters:RequestParameters
	{

		public EmployeeParameters() => OrderBy = "name";
		
		/*default of uint is 0 
		 */
		public uint MinAge { get; set; } 
		public uint MaxAge { get; set; }=int.MaxValue;

		/*
		 * We’ve also added a simple validation property – ValidAgeRange. Its 
		purpose is to tell us if the max-age is indeed greater than the min-age. If 
		it’s not, we want to let the API user know that he/she is doing something 
		wrong.

		 */
		public bool ValidAgeRange => MaxAge > MinAge;

		//Now we can write queries with searchTerm=”name” in them
		public string? SearchTerm { get; set; }


		

	}
}
