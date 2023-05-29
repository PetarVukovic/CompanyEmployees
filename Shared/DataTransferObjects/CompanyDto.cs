using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects
{
	/*Record explained
	 * Tip record nam daje lakši način za stvaranje nepromjenjivog
		tip reference u .NET. To znači da record instance property
		vrijednosti se ne mogu promijeniti nakon njegove inicijalizacije. Podaci se prosljeđuju prema vrijednosti
		a jednakost između dva recorda  provjerava se usporedbom vrijednosti
		njihovih svojstava.
			Records can be a valid alternative to classes when we have to send or 
		receive data. The very purpose of a DTO is to transfer data from one part 
		of the code to another, and immutability in many cases is useful.
	 */

	/*DTO
	 * Well, EF Core uses model classes to map them to the tables in the 
database and that is the main purpose of a model class. But as we saw, 
our models have navigational properties and sometimes we don’t want to 
map them in an API response. So, we can use DTO to remove any 
property or concatenate properties into a single property. 
Moreover, there are situations where we want to map all the properties 
from a model class to the result — but still, we want to use DTO instead. 
The reason is if we change the database, we also have to change the 
properties in a model — but that doesn’t mean our clients want the result 
changed. So, by using DTO, the result will stay as it was before the model 
changes. 
	 */
	[Serializable]
	public record CompanyDto
	{
		/*Init
		 * This object is still immutable and init-only properties protect the state of 
			the object from mutation once initialization is finished.To nam je trebalo za xml response u postmanu.
		Da nam compailer nista ne dodaje na ove nase propertije
		 */
		public Guid Id { get; init; }
		public string? Name { get; init; }
		public string? FullAdresss { get; init; }
			
	};
	
}
