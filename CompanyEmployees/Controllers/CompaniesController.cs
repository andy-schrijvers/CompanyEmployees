using AutoMapper;
using CompanyEmployees.ActionFilters;
using CompanyEmployees.Extensions;
using CompanyEmployees.ModelBinders;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    // [ApiVersion("1.0")]
    [Route("api/companies")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v1")] // Swagger UI v1
    //[ResponseCache(CacheProfileName = "120secondsDuration")] //provided by Marvin.Cache.Headers library
    public class CompaniesController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public CompaniesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpOptions]
        public IActionResult GetCompaniesOptions()
        {
            Response.Headers.Add("Allow", "GET, OPTIONS, POST");
            return Ok();
        }

        /// <summary>
        /// Gets the list of all companies
        /// </summary>
        /// <param name="companyParameters"></param>
        /// <returns>The companies list</returns>
        /// <response code="200">The companies list</response>
        /// <response code="401">Unauthorized</response>      
        [HttpGet(Name = "GetCompanies"), Authorize(Roles = "Manager")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> GetCompanies([FromQuery]CompanyParameters companyParameters)
        {
            var companies = await _repository.Company.GetAllCompaniesAsync(companyParameters, trackChanges: false);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(companies.Metadata));

            var companiesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);

            return Ok(companiesDto);          
        }

        /// <summary>
        /// Gets a company by the companyId
        /// </summary>
        /// <param name="companyId"></param>
        /// <returns>The companie with a specific companyId</returns>
        [HttpGet("{companyId:guid}", Name = "CompanyById")]
        [ServiceFilter(typeof(ValidationCompanyExistsAttribute))]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 60)]
        [HttpCacheValidation(MustRevalidate = false)] // this libraries validation doesnt work
                                                      // the following are free and do work but are no that simple to implement
                                                      // CDN (Content Delivery Network) is easy to implement but it comes with a cost €€€
                                                      //  Varnish - https://varnish-cache.org/
                                                      //  Apache Traffic Server - https://trafficserver.apache.org/
                                                      //  Squid - http://www.squid-cache.org/
                                                      //[ResponseCache(Duration = 60)]
        public IActionResult GetCompany([FromRoute]Guid companyId)
        {

            var company = HttpContext.Items["company"];

                var companyDto = _mapper.Map<CompanyDto>(company);
                return Ok(companyDto);
            
        }
        
        [HttpGet("collection/{ids}", Name = "CompanyCollection")]
        public async Task<IActionResult> GetCompanyCollection([ModelBinder(BinderType = typeof(ArrayModelBinder))]IEnumerable<Guid> ids, [FromQuery]CompanyParameters companyParameters)
        {
            if (ids == null)
            {
                _logger.LogError("Parameter ids is null"); 
                return BadRequest("Parameter ids is null");
            }

            var companies = await _repository.Company.GetCompaniesByIdsAsync(ids, companyParameters, false);

            if (ids.Count() != companies.Count())
            {
                _logger.LogError("Some ids are not valid in a collection"); 
                return NotFound();
            }

            HttpContext.Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(companies.Metadata));

            var companiesToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            return Ok(companiesToReturn);
        }

        /// <summary>
        /// Creates a newly created company
        /// </summary>
        /// <param name="company"></param>
        /// <returns>A newly created company</returns>
        /// <response code="201">Returns the newly created company</response>
        /// <response code="400">If the item is null</response>
        /// <response code="422">If the model is invalid</response>
        [HttpPost("CreateCompany")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ProducesResponseType(201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(422)]
        public async Task<IActionResult> CreateCompany([FromBody]CompanyForCreationDto company)
        {        
            var companyEntity = _mapper.Map<Company>(company);

            _repository.Company.CreateCompany(companyEntity);

            await _repository.SaveAsync();

            var companyToReturn = _mapper.Map<CompanyDto>(companyEntity);

            return CreatedAtRoute("CompanyById", new { companyId = companyToReturn.Id }, companyToReturn);
        }

        [HttpPost("collection")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateCompany([FromBody] IEnumerable<CompanyForCreationDto> companyCollectionRequest)
        {
            var companies = _mapper.Map<IEnumerable<Company>>(companyCollectionRequest);

            foreach (var company in companies)
            {
                _repository.Company.CreateCompany(company);
            }

            await _repository.SaveAsync();

            var companyCollectionToReturn = _mapper.Map<IEnumerable<CompanyDto>>(companies);
            var ids = string.Join(',', companyCollectionToReturn.Select(c => c.Id));

            return CreatedAtRoute("CompanyCollection", new { ids = ids }, companyCollectionToReturn);
        }

        [HttpDelete("{companyId:guid}")]
        [ServiceFilter(typeof(ValidationCompanyExistsAttribute))]
        [ProducesResponseType(204)]
        [ProducesResponseType(500)]
        public async Task<IActionResult> DeleteCompany([FromRoute]Guid companyId)
        {
            var company = HttpContext.Items["company"] as Company;

            _repository.Company.DeleteCompany(company);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPut("{companyId:guid}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidationCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateCompany([FromRoute]Guid companyId, [FromBody]CompanyForUpdateDto companyRequest)
        {
            var companyEntity = HttpContext.Items["company"] as Company;

            _mapper.Map(companyRequest, companyEntity);
            await _repository.SaveAsync();

            return NoContent();
        }

    }
}
