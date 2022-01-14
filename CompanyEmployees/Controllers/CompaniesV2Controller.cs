using Contracts;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    // ServiceExtensions => ConfigureApiVersioning
    // deprecated =  Old Version of the api method
   // [ApiVersion("2.0", Deprecated = true)]
    [Route("api/companies")] // => header check apiversioning options
    //[Route("api/{v:apiversion}/companies")] => query instead of header
    [ApiExplorerSettings(GroupName = "v2")] // Swagger UI v2
    [ApiController]
    public class CompaniesV2Controller : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        public CompaniesV2Controller(IRepositoryManager repository)
        {
            _repository = repository;
        }
        [HttpGet]
        public async Task<IActionResult> GetCompanies([FromRoute]CompanyParameters companyParameters)
        {
            var companies = await _repository.Company.GetAllCompaniesAsync(companyParameters, trackChanges:
           false);
            return Ok(companies);
        }
    }
}
