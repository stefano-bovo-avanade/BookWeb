using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;

namespace BookWeb.Service.Context
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string? _connectionString;
        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }


        public IDbConnection CreateConnection()
        {
            var builder = new SqlConnectionStringBuilder(_connectionString);
            return new SqlConnection(builder.ConnectionString);
        }

    }
}
