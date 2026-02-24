using MySqlConnector;
using Microsoft.Extensions.Configuration;
using System.IO;
using System;

namespace SistemaFinanceiro.Database
{
    public static class DbConnection
    {
        public static MySqlConnection GetConnection()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            string connectionString = configuration.GetConnectionString("DefaultConnection");

            return new MySqlConnection(connectionString);
        }
    }
}