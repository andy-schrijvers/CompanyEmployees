using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyEmployees.ActionFilters
{
    public class ValidationCompanyExistsAttribute : IAsyncActionFilter
    {
        private readonly IRepositoryManager _repoManager;
        private readonly ILoggerManager _logger;

        public ValidationCompanyExistsAttribute(IRepositoryManager repoManager, ILoggerManager logger)
        {
            _repoManager = repoManager;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var trackChanges = context.HttpContext.Request.Method.Equals("PUT");
            var companyId = (Guid)context.ActionArguments["companyId"];
            var company = await _repoManager.Company.GetCompanyAsync(companyId, trackChanges);

            if (company == null)
            {
                _logger.LogInfo($"Company with id: {companyId} doesn't exist in the database."); 
                context.Result = new NotFoundResult();
            }

            else
            {
                context.HttpContext.Items.Add("company", company);

                await next();
            }
        }
    }
}
