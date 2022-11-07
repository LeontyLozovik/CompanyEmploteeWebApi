using Contracts;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public class RepositoryManager : IRepositoryManager
    {
        private RepositoryContext _repositoryContext;
        private ICompanyRepository _companyRepository;
        private IEmployeeRepository _employeeRepository;

        public RepositoryManager(RepositoryContext repositoryContext)
        { 
            _repositoryContext = repositoryContext; 
        }
        public ICompanyRepository CompanyRepository
        {
            get 
            { 
                if (_companyRepository == null) 
                    _companyRepository = new CompanyReposirory(_repositoryContext); 

                return _companyRepository; 
            }
        }
        public IEmployeeRepository EmployeeRepository
        {
            get 
            { 
                if (_employeeRepository == null) 
                    _employeeRepository = new EmployeeReposirory(_repositoryContext); 

                return _employeeRepository; 
            }
        }
        public Task SaveAsync() => _repositoryContext.SaveChangesAsync();
    }
}
