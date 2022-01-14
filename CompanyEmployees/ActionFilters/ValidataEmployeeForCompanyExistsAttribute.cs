using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.ActionFilters
{
    public class ValidataEmployeeForCompanyExistsAttribute : IAsyncActionFilter
    {
        private readonly IRepositoryManager _repoManager;
        private readonly ILoggerManager _logger;

        public ValidataEmployeeForCompanyExistsAttribute(IRepositoryManager repoManager, ILoggerManager logger)
        {
            _repoManager = repoManager;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;
            var trackingChanges = (method.Equals("PUT") || method.Equals("PATCH")) ? true : false;

            var companyId = (Guid)context.ActionArguments["companyId"];
            var company = await _repoManager.Company.GetCompanyAsync(companyId, trackingChanges);

            if (company == null) 
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database."); 
                context.Result = new NotFoundResult(); 
                return; 
            }

            var employeeId = (Guid)context.ActionArguments["employeeId"];
            var employee = await _repoManager.Employee.GetEmployeeForCompanyAsync(companyId, employeeId, trackingChanges);

            if (employee == null)
            {
                _logger.LogInfo($"Employee with id: {employeeId} doesn't exist in the database.");
                context.Result = new NotFoundResult();
                return;
            }

            else
            {
                context.HttpContext.Items.Add("employee", employee);
                await next();
            }
        }

    }
}
