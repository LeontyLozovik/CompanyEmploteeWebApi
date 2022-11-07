using AutoMapper;
using Contracts;
using Entities;
using Entities.DataTransferObjects;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repository.DataShaping;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.ActionFilter;

namespace WebApi.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IRepositoryManager _repository;
        private readonly ILogger<EmployeeController> _logger;
        private readonly IMapper _mapper;
        private readonly IDataShaper<EmployeeDTO> _dataShaper;

        public EmployeeController(IRepositoryManager repositoryContext, ILogger<EmployeeController> logger,
            IMapper mapper, IDataShaper<EmployeeDTO> dataShaper)
        {
            _repository = repositoryContext;
            _logger = logger;
            _mapper = mapper;
            _dataShaper = dataShaper;
        }

        [HttpGet]
        [HttpHead]
        public async Task<IActionResult> GetEmployees(Guid companyId, [FromQuery]EmployeeParameters employeeParams)
        {
            if (!employeeParams.ValidAgeRange)
                return BadRequest("Max age can't be less than min age.");

            var company = await _repository.CompanyRepository.GetCompanyAsync(companyId, false);
            if (company == null)
            {
                _logger.LogInformation($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employees = await _repository.EmployeeRepository.GetEmployeesAsync(companyId, employeeParams, false);
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(employees.MetaData));
            var employeesDTO = _mapper.Map<IEnumerable<EmployeeDTO>>(employees);
            return Ok(_dataShaper.ShapeData(employeesDTO, employeeParams.Fields));
        }

        [HttpGet("{employeeId}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployee(Guid companyId, Guid employeeId)
        {
            var company = await _repository.CompanyRepository.GetCompanyAsync(companyId, false);
            if (company == null)
            {
                _logger.LogInformation($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employee = await _repository.EmployeeRepository.GetEmployeeAsync(companyId, employeeId, false);
            if (employee == null)
            {
                _logger.LogInformation($"Company with id: {employeeId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeDTO = _mapper.Map<EmployeeDTO>(employee);
            return Ok(employeeDTO);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> CreateEmployeeForCompamy(Guid companyId, [FromBody] EmployeeForCreationDTO employee)
        {
            var company = await _repository.CompanyRepository.GetCompanyAsync(companyId, trackChanges: false);
            if (company == null)
            {
                _logger.LogInformation($"Company with id: {companyId} doesn't exist in the database.");
                return NotFound();
            }

            var employeeEntity = _mapper.Map<Employee>(employee);

            _repository.EmployeeRepository.CreateEmployeeForCompany(companyId, employeeEntity);
            await _repository.SaveAsync();

            var employeeToReturn = _mapper.Map<EmployeeDTO>(employeeEntity);
            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, employeeId = employeeToReturn.Id }, employeeToReturn);
        }

        [HttpDelete("{employeeId}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid employeeId)
        {
            var employeeForCompany = HttpContext.Items["employee"] as Employee;

            _repository.EmployeeRepository.DeleteEmployee(employeeForCompany);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPut("{employeeId}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany(Guid companyId, Guid employeeId,
            [FromBody] EmployeeForUpdateDTO employee)
        {
            var employeeEntity = HttpContext.Items["employee"] as Employee;

            _mapper.Map(employee, employeeEntity);
            await _repository.SaveAsync();

            return NoContent();
        }

        [HttpPatch("{employeeId}")]
        [ServiceFilter(typeof(ValidateEmployeeForCompanyExistsAttribute))]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(Guid companyId, Guid employeeId, 
            [FromBody] JsonPatchDocument<EmployeeForUpdateDTO> patchDoc) 
        { 
            if (patchDoc == null) 
            { 
                _logger.LogError("patchDoc object sent from client is null."); 
                return BadRequest("patchDoc object is null"); 
            } 
            
            var employeeEntity = HttpContext.Items["employee"] as Employee; 
            
            var employeeToPatch = _mapper.Map<EmployeeForUpdateDTO>(employeeEntity); 
            
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
