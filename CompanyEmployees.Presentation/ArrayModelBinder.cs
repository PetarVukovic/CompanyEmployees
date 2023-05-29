using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Presentation
{
	public class ArrayModelBinder : IModelBinder
	{
		public Task BindModelAsync(ModelBindingContext bindingContext)
		{
			/*
			 * We are creating a model binder for the IEnumerable type. Therefore, we 
				have to check if our parameter is the same type.
			 */
			if ( !bindingContext.ModelMetadata.IsEnumerableType)
			{
				bindingContext.Result = ModelBindingResult.Failed();
				return Task.CompletedTask;
			}
			/*
			 * Zatim ekstrahiramo vrijednost (niz GUID-ova odvojen zarezima) pomoću
				ValueProvider.GetValue() izraz. Budući da je to niz tipa, mi
				samo provjeri je li null ili prazan. Ako jest, vraćamo null kao rezultat
				jer imamo nultu provjeru u našoj akciji u kontroleru. Ako nije,
				idemo dalje.
			 */
			var providedValue = bindingContext.ValueProvider.GetValue( bindingContext.ModelName ).ToString();

			if ( string.IsNullOrEmpty( providedValue ) )
			{
				bindingContext.Result = ModelBindingResult.Success( null );
				return Task.CompletedTask;
			}

			/*
			 * In the genericType variable, with the reflection help, we store the type 
				the IEnumerable consists of
			 */

			var genericType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];


			var converter = TypeDescriptor.GetConverter( genericType );

			var objectArray = providedValue.Split( new[] { "," },
		   StringSplitOptions.RemoveEmptyEntries )
			.Select( x => converter.ConvertFromString( x.Trim() ) )
			.ToArray();

			/*
			  * Nakon toga kreiramo niz tipa objekt (objectArray) koji se sastoji
			svih GUID vrijednosti koje smo poslali API-ju i zatim stvorimo niz od
			GUID vrste (guidArray), kopirajte sve vrijednosti iz objectArray u
			guidArray i dodijelite ga bindingContext-u.
			  */
			var guidArray = Array.CreateInstance( genericType, objectArray.Length );

			objectArray.CopyTo( guidArray, 0 );
			bindingContext.Model = guidArray;
			bindingContext.Result = ModelBindingResult.Success( bindingContext.Model );
			return Task.CompletedTask;



		}
	}
}
