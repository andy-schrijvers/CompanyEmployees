using Entities.Models;
using Entities.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Contracts
{
    public interface ICompanyRepository 
    {
        Task<PagedList<Company>> GetAllCompaniesAsync(CompanyParameters companyParameters, bool trackChanges);
        Task<PagedList<Company>> GetCompaniesByIdsAsync(IEnumerable<Guid> ids, CompanyParameters companyParameters, bool trackChanges);
        Task<Company> GetCompanyAsync(Guid companyId, bool trackChange);

        void CreateCompany(Company company);
        void DeleteCompany(Company company);
    }
}
