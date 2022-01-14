using Entities.Models;
using Entities.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts
{
    public interface IEmployeeRepository 
    {
        void Create(Employee employee);
        Task<PagedList<Employee>> GetEmployeesForCompanyAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChange);
        Task<Employee> GetEmployeeForCompanyAsync(Guid companyId, Guid employeeId, bool trackChange);
        void CreateEmployeeForCompany(Guid companyId, Employee employee);
        void DeleteEmployee(Employee employee);
    }
}
