using Contracts;
using Entities;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.EntityFrameworkCore;
using Repository.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Repository
{
    public class EmployeeRepository : RepositoryBase<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(RepositoryContext context) : base(context)
        {

        }

        public async Task<PagedList<Employee>> GetEmployeesForCompanyAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChange)
        {
            #region SMALL AMOUNT OF DATA
            // SMALL AMOUNT OF DATA
            //var list = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChange)
            //    .OrderBy(e => e.Name)
            //    .ToListAsync();

            //return PagedList<Employee>
            //    .ToPagedList(list, employeeParameters.PageNumber, employeeParameters.PageSize); 
            #endregion

            #region LARGE AMOUNT OF DATA
            // LARGE AMOUNT OF DATA
            var employees = await FindByCondition(e => (e.CompanyId.Equals(companyId)), trackChange)
                .FilterEmployees(employeeParameters.MinAge, employeeParameters.MaxAge)
                .Search(employeeParameters.SearchTerm)
                .Sort(employeeParameters.OrderBy)
                .Skip((employeeParameters.PageNumber - 1) * employeeParameters.PageSize)
                .Take(employeeParameters.PageSize)
                .ToListAsync();

            var count = await FindByCondition(e => e.CompanyId.Equals(companyId), trackChange).CountAsync();

            return new PagedList<Employee>(employees, count, employeeParameters.PageNumber, employeeParameters.PageSize); 
            #endregion
        }


        public async Task<Employee> GetEmployeeForCompanyAsync(Guid companyId, Guid employeeId, bool trackChange) =>
            await FindByCondition(e => e.CompanyId.Equals(companyId) && e.Id.Equals(employeeId), trackChange)
            .SingleOrDefaultAsync();

        public void CreateEmployeeForCompany(Guid companyId, Employee employee)
        {
            employee.CompanyId = companyId;
            Create(employee);
        }

        public void DeleteEmployee(Employee employee) => Delete(employee);
    }
}
