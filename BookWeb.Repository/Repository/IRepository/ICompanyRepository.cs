using BookWeb.Models.Models;

namespace BookWeb.Repository.Repository.IRepository
{
    public interface ICompanyRepository
    {
        public Task<Company> GetCompany(int? Id);

        public Task<IEnumerable<Company>> GetCompanies();

        public Task<int> InsertCompany(Company Company);

        public Task<int> UpdateCompany(Company Company);

        public Task<int> DeleteCompany(int? id);
    }
}
