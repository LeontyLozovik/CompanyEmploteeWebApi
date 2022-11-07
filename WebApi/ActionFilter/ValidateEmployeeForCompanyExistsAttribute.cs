using Contracts;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace WebApi.ActionFilter
{
    public class ValidateEmployeeForCompanyExistsAttribute : IAsyncActionFilter
    {
        private readonly ILogger<ValidateEmployeeForCompanyExistsAttribute> _logger;
        private readonly IRepositoryManager _repository;
        public ValidateEmployeeForCompanyExistsAttribute(ILogger<ValidateEmployeeForCompanyExistsAttribute> logger, IRepositoryManager repository)
        {
            _logger = logger;
            _repository = repository;
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var method = context.HttpContext.Request.Method;
            var trackChanges = (method.Equals("PUT") || method.Equals("PATCH")) ? true : false;
            var companyId = (Guid)context.ActionArguments["companyId"];
            var company = await _repository.CompanyRepository.GetCompanyAsync(companyId, false);

            if (company == null)
            {
                _logger.LogInformation($"Company with id: {companyId} doesn't exist in the database.");
                context.Result = new NotFoundResult();
                return;
            }

            var employeeId = (Guid)context.ActionArguments["employeeId"];
            var employee = await _repository.EmployeeRepository.GetEmployeeAsync(companyId, employeeId, trackChanges);

            if (employee == null)
            {
                _logger.LogInformation($"Employee with id: {employeeId} doesn't exist in the database.");
                context.Result = new NotFoundResult();
            }
            else 
            {
                context.HttpContext.Items.Add("employee", employee);
                await next();
            }
        }
    }
}
