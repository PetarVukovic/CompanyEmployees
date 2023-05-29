using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
	public interface IRepositoryManager
	{
		ICompanyRepository Company { get; }
		IEmployeeRepository Employee { get; }

		 
		/* We have the Create,
			Update, and Delete methods in the RepositoryBase class, but they
		won’t make any change in the database until we call the SaveChanges
		method.Our repository manager class will handle that as well.*/
		
		Task SaveAsync();
	}
}
