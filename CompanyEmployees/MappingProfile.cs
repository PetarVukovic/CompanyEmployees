using AutoMapper;
using Entities.Models;
using Shared.DataTransferObjects;

namespace CompanyEmployees
{
	public class MappingProfile:Profile
	{
		public MappingProfile()
		{
			/*
			 * Because 
				we have the FullAddress property in our DTO record, which contains 
				both the Address and the Country from the model class, we have to 
				specify additional mapping rules with the ForMember method.
			 */
			CreateMap<Company, CompanyDto>()
				.ForMember( "FullAdresss",
				opt => opt.MapFrom( x => string.Join( ' ', x.Address, x.Country ) ) );

			CreateMap<Employee, EmployeeDto>();

			CreateMap<CompanyForCreationDto, Company>();

			CreateMap<EmployeeForCreationDto, Employee>();

			/*
			 * The ReverseMap method is also going to configure this rule to execute 
				reverse mapping if we ask for it.
			 */
			CreateMap<EmployeeForUpdateDto, Employee>().ReverseMap();

			CreateMap<CompanyForUpdateDto, Company>();

			CreateMap<UserForRegistrationDto, User>();


		}

	}
}
