//This code is an example of a custom model binder for an IEnumerable type.
//It is used to bind a parameter of type IEnumerable to a model in an ASP.NET Core MVC application
//. The c first checks if the parameter is of type IEnumerable,
//then extracts the value from the ValueProvider. It then checks if the value is null or empty, and
// if not, it uses reflection to get the type the IEnumerable and
// creates an array of objects from the provided value. Finally,
// it creates an arra the IEnumerable
// type and copies the values from the object array to the
// IEnumerable array and assigns it to the binContext.dingy of ofode

using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel;
using System.Reflection;

namespace CompanyEmployees.Presentation;

public class ArrayModelBinder : IModelBinder
{
	/*
	 * Ali dobivamo poruku 415 Unsupported Media Type. Ovo je
		jer naš API ne može vezati parametar tipa niza na
		IEnumerable<Guid> argument u radnji GetCompanyCollection.
	 */
	public Task BindModelAsync( ModelBindingContext bindingContext )
	{
		/*
		 * We are creating a model binder for the IEnumerable type. Therefore, we 
			have to check if our parameter is the same type.
		 */
		if ( !bindingContext.ModelMetadata.IsEnumerableType )
		{
			/*This statement sets the Result property of the bindingContext object to a ModelBindingResult object with a Failed status.
			//This indicates that the model binding process
			// has failed and that the model binding result is not valid.
			// This could be due to an invalid input or a in the binding process.n error*/
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



