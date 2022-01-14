using AutoMapper;
using CompanyEmployees.ActionFilters;
using Contracts;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace CompanyEmployees.Controllers
{
    [Route("api/{v:apiversion}/companies/{companyId:guid}/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IDataShaper<EmployeeDto> _dataShaper;

        public EmployeesController(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IDataShaper<EmployeeDto> dataShaper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _dataShaper = dataShaper;
        }

        [HttpGet]
        [HttpHead]
        [ServiceFilter(typeof(ValidationCompanyExistsAttribute))]
        public async Task<IActionResult> GetAllEmployees([FromRoute]Guid companyId, [FromQuery]EmployeeParameters employeeParameters)
        {
            if (!employeeParameters.ValidAgeRange)
            {
                return BadRequest("Max age can't be greater than min age.");
            }

            var company = HttpContext.Items["company"] as Company;

            var employeesFromDb = await _repository.Employee.GetEmployeesForCompanyAsync(company.Id, employeeParameters, false);

            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(employeesFromDb.Metadata));

            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);

            return Ok(_dataShaper.ShapeData(employeesDto, employeeParameters.Fields));
        }

        [HttpGet("{employeeId:guid}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployee([FromRoute]Guid companyId, [FromRoute]Guid employeeId)
        {
            var company = await _repository.Company.GetCompanyAsync(companyId, false);
            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeFromDb = await _repository.Employee.GetEmployeeForCompanyAsync(companyId, employeeId, false);

            if (employeeFromDb == null)
            {
                _logger.LogInfo($"Employee with id: {employeeId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeDto = _mapper.Map<EmployeeDto>(employeeFromDb);

            return Ok(employeeDto);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidationCompanyExistsAttribute))]
        public async Task<IActionResult> CreateEmployee([FromRoute]Guid companyId, [FromBody] EmployeeForCreationDto employeeRequest)
        {
            var company = HttpContext.Items["company"];

            var employee = _mapper.Map<Employee>(employeeRequest);

            _repository.Employee.CreateEmployeeForCompany(companyId, employee);
            await _repository.SaveAsync();

            var employeeToReturn = _mapper.Map<EmployeeDto>(employee);

            return CreatedAtRoute("GetEmployeeForCompany", new { companyId = companyId, employeeId = employeeToReturn.Id }, employeeToReturn);

        }

        [HttpDelete("{employeeId:guid}")]
        [ServiceFilter(typeof(ValidataEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployee([FromRoute]Guid companyId, [FromRoute]Guid employeeId)
        {
            var employeeEntity = HttpContext.Items["employee"] as Employee;

            _repository.Employee.DeleteEmployee(employeeEntity);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPut("{employeeId:guid}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidataEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateEmployee([FromRoute]Guid companyId, [FromRoute]Guid employeeId, [FromBody]EmployeeForUpdateDto employee)
        {
            var employeeEntity = HttpContext.Items["employee"] as Employee;

            _mapper.Map(employee, employeeEntity);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{employeeId:guid}")]
        [ServiceFilter(typeof(ValidataEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany([FromRoute]Guid companyId, [FromRoute]Guid employeeId, [FromBody]JsonPatchDocument<EmployeeForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                _logger.LogError("patchDoc object sent from client is null."); 
                return BadRequest("patchDoc object is null");
            }

            var employeeEntity = HttpContext.Items["employee"] as Employee;

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);

            patchDoc.ApplyTo(employeeToPatch, ModelState);

            TryValidateModel(employeeToPatch);

            if (!ModelState.IsValid)
            {
                _logger.LogError("Invalid model state for the patch document"); 
                return UnprocessableEntity(ModelState);
            }

            _mapper.Map(employeeToPatch, employeeEntity);

            await _repository.SaveAsync();

            return NoContent();
        }
    }
}


