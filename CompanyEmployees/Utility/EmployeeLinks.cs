using Contracts;
using Entities.LinkModels;
using Entities.Models;
using Microsoft.Net.Http.Headers;
using Shared.DataTransferObjects;

namespace CompanyEmployees.Utility
{
	public class EmployeeLinks : IEmployeeLinks
	{
		/*
		 * We are going to use LinkGenerator to generate links for our responses 
		and IDataShaper to shape our data. As you can see, the shaping logic is 
		now extracted from the EmployeeService class, which we will modify a 
		bit later
		 */
		private readonly LinkGenerator _linkGenerator;
		private readonly IDataShaper<EmployeeDto> _dataShaper;

		public EmployeeLinks(LinkGenerator linkGenerator, IDataShaper<EmployeeDto>dataShaper)
		{
			_linkGenerator = linkGenerator;
			_dataShaper = dataShaper;
		}

		/*
		 * 
		 * So, our method accepts four parameters. The employeeDto collection, 
		the fields that are going to be used to shape the previous collection, 
		companyId because routes to the employee resources contain the Id from 
		the company, and httpContext which holds information about media 
		types
		 */
		public LinkResponse TryGenerateLinks(IEnumerable<EmployeeDto> employeesDto, string fields, Guid companyId, HttpContext httpContext)
		{
			var shapedEmployees=ShapeData(employeesDto, fields);	

			if(ShouldGenerateLinks(httpContext))
			{
				return ReturnLinkedEmployees(employeesDto,fields,companyId,httpContext,shapedEmployees);
			}
			return ReturnShapedEmployees( shapedEmployees );
		}

		private LinkResponse ReturnLinkedEmployees(IEnumerable<EmployeeDto> employeesDto, string fields, Guid companyId, HttpContext httpContext, List<Entity> shapedEmployees)
		{
			var employeeDtoList=employeesDto.ToList();

			for(var index=0; index<employeeDtoList.Count; index++)
			{
				var employeeLinks= CreateLinksForEmployee( httpContext, companyId,employeeDtoList[index].Id, fields );

				shapedEmployees[index].Add( "Links", employeeLinks );

			}
			var employeeCollection = new LinkCollectionWrapper<Entity>( shapedEmployees );

			var linkedEmployees = CreateLinksForEmployees( httpContext, employeeCollection );

			return new LinkResponse { HasLinks = true, LinkedEntities = linkedEmployees };
		}

		private List<Link>  CreateLinksForEmployee( HttpContext httpContext, Guid companyId, Guid id, string fields="" )
		{

			/*
			 *  We are creating the links by using the 
				LinkGenerator‘s GetUriByAction method — which accepts 
				HttpContext, the name of the action, and the values that need to be 
				used to make the URL valid. In the case of the EmployeesController, we 
				send the company id, employee id, and fields.

			 */
			var links = new List<Link>
			{
				new Link(_linkGenerator.GetUriByAction(httpContext,"GetEmployeeForCompany",values:new{companyId, id ,fields}),"self","GET"),
				new Link(_linkGenerator.GetUriByAction(httpContext,"DeleteEmployeeForCompany",values:new  { companyId, id }),"delete_company","DELETE"),
				new Link(_linkGenerator.GetUriByAction(httpContext,"UpdateEmployeeForCompany",values:new {companyId,id}),"update_company","PUT"),
				new Link(_linkGenerator.GetUriByAction(httpContext,"PartiallyUpdateEmployeeForCompany",values:new {companyId,id}),"partially_update_employee","PATCH")

			};
			return links;	
		}

		private LinkCollectionWrapper<Entity> CreateLinksForEmployees( HttpContext httpContext, LinkCollectionWrapper<Entity> employeesWrapper )
		{
			employeesWrapper.Links.Add( new Link( _linkGenerator.GetUriByAction( httpContext, "GetEmployeesForCompany", values: new { } ), "self", "GET" ) );
			return employeesWrapper;
		}

		/*
		 * In the ShouldGenerateLinks method, we extract the media type from 
		the httpContext. If that media type ends with hateoas, the method 
		returns true; otherwise, it returns false.
		 */
		private bool ShouldGenerateLinks(HttpContext httpContext)
		{
			var mediaType = (MediaTypeHeaderValue) httpContext.Items["AcceptHeaderMediaType"];

			return mediaType.SubTypeWithoutSuffix.EndsWith( "hateoas", StringComparison.InvariantCultureIgnoreCase );
		}


		/*
		 
		 * The ShapeData method executes data shaping and extracts only the 
		entity part without the Id property
		 */
		private List<Entity> ShapeData(IEnumerable<EmployeeDto> employeesDto, string fields)=>
			_dataShaper.ShapeData(employeesDto, fields)
				.Select(e=>e.Entity).ToList();

		private LinkResponse ReturnShapedEmployees(List<Entity> shapedEmployees) =>
			 new LinkResponse { ShapedEntities = shapedEmployees };

	}
}
