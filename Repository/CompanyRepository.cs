using Contracts;
using Entities;
using Entities.Models;
using Entities.RequestFeatures;
using Microsoft.EntityFrameworkCore;
using Repository.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repository
{
    public class CompanyRepository : RepositoryBase<Company>, ICompanyRepository
    {

        public CompanyRepository(RepositoryContext context) : base(context)
        {
            
        }

        public async Task<PagedList<Company>> GetAllCompaniesAsync(CompanyParameters companyParameters, bool trackChanges)
        {
            var companies = await FindAll(trackChanges)
            .Sort(companyParameters.OrderBy)
            .Skip((companyParameters.PageNumber - 1) * companyParameters.PageSize)
            .Take(companyParameters.PageSize)
            .ToListAsync();
            
            var count = await FindAll(trackChanges).CountAsync();

            return new PagedList<Company>(companies, count, companyParameters.PageNumber, companyParameters.PageSize);
        }

        public async Task<PagedList<Company>> GetCompaniesByIdsAsync(IEnumerable<Guid> ids, CompanyParameters companyParameters, bool trackChanges)
        {
            var companies = await FindByCondition(c => ids.Contains(c.Id), trackChanges)
                .OrderBy(c => c.Name)
                .Skip((companyParameters.PageNumber - 1) * companyParameters.PageSize)
                .Take(companyParameters.PageSize)
                .ToListAsync();

            var count = await FindByCondition(c => ids.Contains(c.Id), trackChanges).CountAsync();

            return new PagedList<Company>(companies, count, companyParameters.PageNumber, companyParameters.PageSize);
        }
      
        public async Task<Company> GetCompanyAsync(Guid companyId, bool trackChange) => 
            await FindByCondition(c => c.Id == companyId, trackChange).SingleOrDefaultAsync();

        public void CreateCompany(Company company) => Create(company);

        public void DeleteCompany(Company company) => Delete(company);

    }
}
