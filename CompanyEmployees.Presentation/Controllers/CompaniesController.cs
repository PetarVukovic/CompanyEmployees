using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects;
namespace CompanyEmployees.Presentation.Controllers
{

	/*
	 * f you remember, we configured versioning to use 1.0 as a default API 
		version (opt.AssumeDefaultVersionWhenUnspecified = true;). Therefore, if a client 
		doesn’t state the required version, our API will use this one:
	 */

	[Route( "api/[controller]" )]
	[ApiController]
	public class CompaniesController : ControllerBase
	{
		private readonly IServiceManager _service;

		public CompaniesController( IServiceManager service ) =>
			_service = service;


		[HttpOptions]
		public IActionResult GetCompaniesOptions()
		{
			Response.Headers.Add( "Allow", "GET, OPTIONS, POST" );
			return Ok();
		}



		[HttpGet( Name = "GetCompanies" )]
		public async Task<IActionResult> GetCompanies()
		{
			var companies = await
			_service.CompanyService.GetAllCompaniesAsync( trackChanges: false );
			return Ok( companies );
		}


		[HttpGet( "{id:guid}", Name = "CompanyById" )]
		//[ResponseCache(Duration =60)]
		[HttpCacheExpiration(CacheLocation =CacheLocation.Public,MaxAge =65)]
		[HttpCacheValidation(MustRevalidate =false)]
		public async Task<IActionResult> GetCompany( Guid id )
		{
			var company = await _service.CompanyService.GetCompanyAsync( id, trackChanges: false );
			return Ok( company );
		}

		[HttpPost( Name = "CreateCompany" )]
		public async Task<IActionResult> CreateCompany( [FromBody] CompanyForCreationDto company )
		{
			if ( company is null )
				return BadRequest( "CompanyForCreationDto object is null" );
			if ( !ModelState.IsValid )
				return UnprocessableEntity( ModelState );
			var createdCompany = await _service.CompanyService.CreateCompanyAsync( company );
			return CreatedAtRoute( "CompanyById", new { id = createdCompany.Id },
			createdCompany );
		}


		/*ArrayModelBinder konverzija stringova i guida
		 * Our ArrayModelBinder will be triggered before an action executes. It 
			will convert the sent string parameter to the IEnumerable<Guid> type, 
			and then the action will be executed:
		 */

		[HttpGet( "collection/({ids})", Name = "CompanyCollection" )]
		public async Task<IActionResult> GetCompanyCollection( [ModelBinder( BinderType = typeof( ArrayModelBinder ) )] IEnumerable<Guid> ids )
		{
			var companies = await _service.CompanyService.GetByIdsAsync( ids, trackChanges: false );
			return Ok( companies );
		}


		[HttpPost( "collection" )]
		public async Task<IActionResult> CreateCompanyCollection
		( [FromBody] IEnumerable<CompanyForCreationDto> companyCollection )
		{
			var result = await
			_service.CompanyService.CreateCompanyCollectionAsync( companyCollection );
			return CreatedAtRoute( "CompanyCollection", new { result.ids },
			result.companies );
		}


		[HttpDelete( "{id:guid}" )]
		public async Task<IActionResult> DeleteCompany( Guid id )
		{
			await _service.CompanyService.DeleteCompanyAsync( id, trackChanges: false );
			return NoContent();
		}


		[HttpPut( "{id:guid}" )]
		public async Task<IActionResult> UpdateCompany( Guid id, [FromBody] CompanyForUpdateDto
		company )
		{
			if ( company is null )
				return BadRequest( "CompanyForUpdateDto object is null" );
			await _service.CompanyService.UpdateCompanyAsync( id, company, trackChanges: true );
			return NoContent();
		}



	}

}
