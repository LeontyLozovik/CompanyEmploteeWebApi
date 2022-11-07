using Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;

namespace WebApi.ActionFilter
{
    public class ValidateCompanyExistsAttribute : IAsyncActionFilter
    {
        private readonly IRepositoryManager _repository; 
        private readonly ILogger<ValidateCompanyExistsAttribute> _logger; 
        public ValidateCompanyExistsAttribute(IRepositoryManager repository, ILogger<ValidateCompanyExistsAttribute> logger) 
        { 
            _repository = repository; 
            _logger = logger; 
        }
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) 
        { 
            var trackChanges = context.HttpContext.Request.Method.Equals("PUT"); 
            var companyId = (Guid)context.ActionArguments["companyId"]; 
            var company = await _repository.CompanyRepository.GetCompanyAsync(companyId, trackChanges);
            if (company == null) 
            { 
                _logger.LogInformation($"Company with id: {companyId} doesn't exist in the database."); 
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
