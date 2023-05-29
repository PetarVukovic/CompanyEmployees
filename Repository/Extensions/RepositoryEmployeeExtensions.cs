using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Xml.Linq;
using Repository.Extensions.Utility;

namespace Repository.Extensions
{
	public static class RepositoryEmployeeExtensions
	{
		public static IQueryable<Employee> FilterEmployees(this IQueryable<Employee> employees,uint minAge,uint maxAge) =>
			employees.Where(e=>e.Age>=minAge&& e.Age<=maxAge);	

		public static IQueryable<Employee> Search(this IQueryable<Employee> employees,string searchTerm)
		{
			if ( string.IsNullOrWhiteSpace( searchTerm ) ) 
				return employees;

			var lowerCaseTerm=searchTerm.Trim().ToLower();

			return employees.Where(e=>e.Name.ToLower().Contains(lowerCaseTerm));

		}

		public static IQueryable<Employee> Sort(this IQueryable<Employee> employees, string orderByQueryString)
		{
			/*We begin by executing some basic check against the orderByQueryString. 
				If it is null or empty, we just return the same collection ordered by name.
			 */

			if ( string.IsNullOrWhiteSpace(orderByQueryString))
				return employees.OrderBy(e=>e.Name);

			/* So, we are extracting a logic that can be reused in the CreateOrderQuery<T> method.
			 */

			var orderQuery = OrderQueryBuilder.CreateOrderQuery<Employee>( orderByQueryString );

			if ( string.IsNullOrWhiteSpace( orderQuery ) )
				return employees.OrderBy( e => e.Name );

			/*
			 * At this point, the orderQuery variable should contain the “Name 
			ascending, DateOfBirth descending” string. That means it will order 
			our results first by Name in ascending order, and then by DateOfBirth in 
			descending order.
			 */
			return employees.OrderBy( orderQuery );


		}
	}
}
