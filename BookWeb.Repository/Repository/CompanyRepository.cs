using BookWeb.Models.Models;
using BookWeb.Repository.Repository.IRepository;
using BookWeb.Service.Context;
using Dapper;

namespace BookWeb.Repository.Repository
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly DapperContext _context;
        public CompanyRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Company> GetCompany(int? Id)
        {
            var query = $"SELECT * " +
                $"FROM Companies where Id = @CompanyId";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QuerySingleAsync<Company>(query, new { CompanyId = Id });
                return result;
            }
        }

        public async Task<IEnumerable<Company>> GetCompanies()
        {
            var query = $"SELECT * FROM Companies";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryAsync<Company>(query);
                return result;
            }
        }

        public async Task<int> InsertCompany(Company company)
        {
            using (var connection = _context.CreateConnection())
            {
                var sql = $"INSERT INTO Companies " +
                    $"(Name, " +
                    $"StreetAddress, " +
                    $"City, " +
                    $"State, " +
                    $"PostalCode, " +
                    $"PhoneNumber) VALUES " +
                    $"(@Name, " +
                    $"@StreetAddress, " +
                    $"@City, " +
                    $"@State, " +
                    $"@PostalCode, " +
                    $"@PhoneNumber); " +
                    $"SELECT CAST(SCOPE_IDENTITY() as int)";
                var result = await connection.QueryAsync<int>(sql, new
                {
                    Name = company.Name,
                    StreetAddress = company.StreetAddress,
                    City = company.City,
                    State = company.State,
                    PostalCode = company.PostalCode,
                    PhoneNumber = company.PhoneNumber,
                });

                return result.FirstOrDefault();

            }
        }

        public async Task<int> UpdateCompany(Company company)
        {
            using (var connection = _context.CreateConnection())
            {
                var sql = $"UPDATE Companies SET " +
                    $"Name = @Name, " +
                    $"StreetAddress = @StreetAddress, " +
                    $"City = @City, " +
                    $"State = @State, " +
                    $"PostalCode = @PostalCode, " +
                    $"PhoneNumber = @PhoneNumber " +
                    $"WHERE Id = @Id; " +
                    "SELECT CAST(SCOPE_IDENTITY() as int)";
                var result = await connection.QueryAsync<int>(sql, new
                {
                    Id = company.Id,
                    Name = company.Name,
                    StreetAddress = company.StreetAddress,
                    City = company.City,
                    State = company.State,
                    PostalCode = company.PostalCode,
                    PhoneNumber = company.PhoneNumber,
                });

                return result.FirstOrDefault();

            }
        }

        public async Task<int> DeleteCompany(int? id)
        {
            string sql = $"DELETE from Companies where Id = @Id";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync(sql, new { Id = id });
                return result;
            }

        }


    }
}
