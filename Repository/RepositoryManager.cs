using Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
	public sealed class RepositoryManager:IRepositoryManager
	{
		private readonly RepositoryContext _repositoryContext;
		private readonly Lazy<ICompanyRepository> _companyRepository;
		private readonly Lazy<IEmployeeRepository> _employeeRepository;

		public RepositoryManager(RepositoryContext repositoryContext)
		{
			_repositoryContext = repositoryContext;
			_companyRepository = new Lazy<ICompanyRepository>( () => new CompanyRepository( _repositoryContext ) );
			_employeeRepository=new Lazy<IEmployeeRepository>(()=>new EmployeeRepository( _repositoryContext ) );
		}

		/*Lazy class
		 * This means that our repository instances 
			are only going to be created when we access them for the first time, and 
			not before that
		 * U C#, lijeni objekt je objekt čija je izrada odgođena do prve upotrebe.
		 * Lijena inicijalizacija objekta znači da se njegovo stvaranje odgađa do prve upotrebe1.
		 * Klasa Lazy<T> pojednostavljuje posao izvođenja lijene inicijalizacije i instanciranja objekata.
		 * Inicijaliziranjem objekata na lijen način, možete izbjeći da ih uopće morate kreirati ako nikada nisu potrebni, 
		 * ili možete odgoditi njihovu inicijalizaciju dok im se prvi put ne pristupi.
		 */

		public ICompanyRepository Company => _companyRepository.Value;

		public IEmployeeRepository Employee => _employeeRepository.Value;


		/*await
		 Using the await keyword is not mandatory, though. Of course, if we don’t 
		use it, our SaveAsync() method will execute synchronously — and that is 
		not our goal here.
		 */
		public async Task  SaveAsync()=>await _repositoryContext.SaveChangesAsync();

		
	}
}
