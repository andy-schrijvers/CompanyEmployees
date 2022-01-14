using Contracts;
using Entities;
using System;
using System.Threading.Tasks;

namespace Repository
{
    public class RepositoryManager : IRepositoryManager
    {
        private readonly IEmployeeRepository _employeeRepo;
        private readonly ICompanyRepository _companyRepo;
        private readonly RepositoryContext _context;

        public RepositoryManager(RepositoryContext context)
        {
            _context = context;
        }

        public ICompanyRepository Company => _companyRepo == null ? new CompanyRepository(_context) : _companyRepo;

        public IEmployeeRepository Employee => _employeeRepo == null ? new EmployeeRepository(_context) : _employeeRepo;

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
