using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.LinkModels;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
	internal sealed class EmployeeService : IEmployeeService
	{

		private readonly IRepositoryManager _repository;
		private readonly ILoggerManager _logger;
		private readonly IMapper _mapper;
		private readonly IEmployeeLinks _employeeLinks;

		public EmployeeService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IEmployeeLinks employeeLinks)
		{
			_repository = repository;
			_logger = logger;
			_mapper = mapper;
			_employeeLinks = employeeLinks;
		}

		public async Task <(LinkResponse linkResponse, MetaData metaData)> GetEmployeesAsync 
			(Guid companyId, LinkParameters linkParams, bool trackChanges)
		{
			if(!linkParams.EmployeeParameters.ValidAgeRange)
				throw new MaxAgeRangeBadRequestException();


			await CheckIfCompanyExists( companyId, trackChanges );

			var employeesWithMetaData = await _repository.Employee
			.GetEmployeesAsync( companyId, linkParams.EmployeeParameters, trackChanges );

			var employeesDto =_mapper.Map<IEnumerable<EmployeeDto>>( employeesWithMetaData );

			var links = _employeeLinks.TryGenerateLinks( employeesDto,linkParams.EmployeeParameters.Fields,companyId, linkParams.Context );

			return (linkResponse: links, metaData: employeesWithMetaData.MetaData);

		}


		public async Task<EmployeeDto> GetEmployeeAsync(Guid companyId, Guid id, bool trackChanges)
		{
			var company = await _repository.Company.GetCompanyAsync( companyId, trackChanges );
			if ( company is null )
				throw new CompanyNotFoundException( companyId );

			var employeeDb = await _repository.Employee.GetEmployeeAsync( companyId, id, trackChanges );
			if ( employeeDb is null )
				throw new EmployeeNotFoundException( id );

			var employee = _mapper.Map<EmployeeDto>( employeeDb );
			return employee;
		}

		public async Task<EmployeeDto> CreateEmployeeForCompanyAsync(Guid companyId,
			EmployeeForCreationDto employeeForCreation, bool trackChanges)
		{
			var company = await _repository.Company.GetCompanyAsync( companyId, trackChanges );
			if ( company is null )
				throw new CompanyNotFoundException( companyId );

			var employeeEntity = _mapper.Map<Employee>( employeeForCreation );

			_repository.Employee.CreateEmployeeForCompany( companyId, employeeEntity );
			await _repository.SaveAsync();

			var employeeToReturn = _mapper.Map<EmployeeDto>( employeeEntity );

			return employeeToReturn;
		}

		public async Task DeleteEmployeeForCompanyAsync(Guid companyId, Guid id, bool trackChanges)
		{
			var company = await _repository.Company.GetCompanyAsync( companyId, trackChanges );
			if ( company is null )
				throw new CompanyNotFoundException( companyId );

			var employeeForCompany = await _repository.Employee.GetEmployeeAsync( companyId, id, trackChanges );
			if ( employeeForCompany is null )
				throw new EmployeeNotFoundException( id );

			_repository.Employee.DeleteEmployee( employeeForCompany );
			await _repository.SaveAsync();
		}

		public async Task UpdateEmployeeForCompanyAsync(Guid companyId, Guid id,
			EmployeeForUpdateDto employeeForUpdate,
			bool compTrackChanges, bool empTrackChanges)
		{
			var company = await _repository.Company.GetCompanyAsync( companyId, compTrackChanges );
			if ( company is null )
				throw new CompanyNotFoundException( companyId );

			var employeeEntity = await _repository.Employee.GetEmployeeAsync( companyId, id, empTrackChanges );
			if ( employeeEntity is null )
				throw new EmployeeNotFoundException( id );

			_mapper.Map( employeeForUpdate, employeeEntity );
			await _repository.SaveAsync();
		}

		public async Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatchAsync
			(Guid companyId, Guid id, bool compTrackChanges, bool empTrackChanges)
		{
			var company = await _repository.Company.GetCompanyAsync( companyId, compTrackChanges );
			if ( company is null )
				throw new CompanyNotFoundException( companyId );

			var employeeEntity = await _repository.Employee.GetEmployeeAsync( companyId, id, empTrackChanges );
			if ( employeeEntity is null )
				throw new EmployeeNotFoundException( companyId );

			var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>( employeeEntity );

			return (employeeToPatch, employeeEntity);
		}

		public async Task SaveChangesForPatchAsync(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)
		{
			_mapper.Map( employeeToPatch, employeeEntity );
			await _repository.SaveAsync();
		}
		private async Task CheckIfCompanyExists(Guid companyId, bool trackChanges)
		{
			var company = await _repository.Company.GetCompanyAsync( companyId, trackChanges );
			if ( company is null )
				throw new CompanyNotFoundException( companyId );
		}
	}
}
