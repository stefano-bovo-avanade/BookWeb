using BookWeb.Service.Context;
using BookWeb.Models.Models;
using Dapper;
using System.Data;

namespace BookWeb.Repository.Repository
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DapperContext _context;
        public CategoryRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<Category> GetCategory(int? Id)
        {
            var query = $"SELECT Name, DisplayOrder FROM {Category.F_CATEGORY} where {Category.F_ID} = @{Category.F_ID}";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QuerySingleAsync<Category>(query, new { @Id = Id });
                return result;
            }
        }

        public async Task<IEnumerable<Category>> GetCategories()
        {
            var query = $"SELECT * FROM {Category.F_CATEGORY}";

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.QueryAsync<Category>(query);
                return result.ToList();
            }
        }

        public async Task<int> InsertCategory(Category category)
        {
            using (var connection = _context.CreateConnection())
            {
                
                var sql = $"INSERT INTO {Category.F_CATEGORY} ({Category.F_NOME}, {Category.F_DISPLAYORDER}) VALUES " +
                    $"(@{Category.F_NOME}, @{Category.F_DISPLAYORDER}); SELECT CAST(SCOPE_IDENTITY() as int)";
                var result = await connection.QueryAsync<int>(sql,
                    new { @Name = category.Name, @DisplayOrder = category.DisplayOrder });
                
                return result.FirstOrDefault();
                
            }
        }

        public async Task<int> UpdateCategory(Category category)
        {

            using (var connection = _context.CreateConnection())
            {

                var sql = $"UPDATE {Category.F_CATEGORY} SET {Category.F_NOME} = @{Category.F_NOME}, " +
                    $"{Category.F_DISPLAYORDER} = @{Category.F_DISPLAYORDER} WHERE {Category.F_ID} = @{Category.F_ID}; " +
                    "SELECT CAST(SCOPE_IDENTITY() as int)";
                var result = await connection.QueryAsync<int>(sql,
                    new { Name = category.Name, DisplayOrder = category.DisplayOrder, categoryId = category.Id });

                return result.FirstOrDefault();

            }
        }

        public async Task<int> DeleteCategory(int? id)
        {
            var p = new DynamicParameters();
            p.Add("id", id);
            p.Add("returnValue", dbType:DbType.Int32, direction: ParameterDirection.ReturnValue);

            using (var connection = _context.CreateConnection())
            {
                var result = await connection.ExecuteAsync("[dbo].[DeleteCategory]", p, commandType: CommandType.StoredProcedure);
                var returnValue = p.Get<int>("returnValue");
                return returnValue;
            }
        }
    }
}
