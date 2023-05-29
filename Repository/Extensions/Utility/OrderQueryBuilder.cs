using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Extensions.Utility
{
	public static  class OrderQueryBuilder
	{
		public static string CreateOrderQuery<T>(string orderByQueryString)
		{
			//Next, we are splitting our query string to get the individual fields:
			//Example: If orderByQueryString is "age,name,city", the orderParams array will contain three elements: "age", "name", and "city".
			var orderParams = orderByQueryString.Trim().Split( ',' );


			/*We’re also using a bit of reflection to prepare the list of PropertyInfo
			objects that represent the properties of our Employee class. We need
			them to be able to check if the field received through the query string
			exists in the Employee class.Example: If the Employee class has properties like Id, Name, Age, and City,
			the propertyInfos array will contain PropertyInfo objects for these properties.*/

			var propertyInfos = typeof( Employee ).GetProperties( BindingFlags.Public | BindingFlags.Instance );

			var orderQueryBuilder = new StringBuilder();
			/*
			
			That prepared, we can actually run through all the parameters and check 
			for their existence:
			 */
			foreach ( var param in orderParams )
			{
				//Ako nema takvog propertia preskoci taj loop
				if ( string.IsNullOrWhiteSpace( param ) )
					continue;

				/*The element is split at the first space character to extract
				 * the property name from the query string (e.g., "age desc" would yield "age").
				 */
				var propertyFromQueryName = param.Split( " " )[0];

				/*
				 * The code attempts to find the corresponding PropertyInfo object
				 * from the propertyInfos array using case-insensitive property name matching.
				 */
				var objectProperty = propertyInfos.FirstOrDefault( pi =>
				pi.Name.Equals( propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase ) );

				if ( objectProperty == null )
					continue;

				/*
				 * check if our 
				parameter contains “desc” at the end of the string. We use that to decide 
				how we should order our property:Example: If orderParams contains ["age desc", "name"], a
				nd the Employee class has properties "Age" and "Name", the orderQueryBuilder will be appended with "Age descending,Name ascending,".
				 */
				var direction = param.EndsWith( " desc" ) ? "descending" : "ascending";

				orderQueryBuilder.Append( $"{objectProperty.Name.ToString()} {direction}," );

			}

			/*
			 * Now that we’ve looped through all the fields, we are just removing excess 
				commas and doing one last check to see if our query indeed has 
				something in it:
			 */
			var orderQuery = orderQueryBuilder.ToString().TrimEnd( ',', ' ' );

			return orderQuery;
		}
	}
}
