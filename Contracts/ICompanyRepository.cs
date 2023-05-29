using Entities.Models;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
	public interface ICompanyRepository
	{
		Task<IEnumerable<Company>> GetAllCompaniesAsync(bool trackChanges);
		Task<Company> GetCompanyAsync(Guid companyId, bool trackChanges);
		void CreateCompany(Company company);
		Task<IEnumerable<Company>> GetByIdsAsync(IEnumerable<Guid> ids, bool
	   trackChanges);
		void DeleteCompany(Company company);


		/*ASYNC
		 * The Create and Delete method signatures are left synchronous. That’s 
		because, in these methods, we are not making any changes in the 
		database. All we're doing is changing the state of the entity to Added and 
		Deleted.
		 */
	}
}
