using Microsoft.Data.SqlClient;


namespace SistemaFinanceiro.Database
{
    public static class DbConnection
    {
        private static string 
        connectionString = @"Server=ALFA14707;Database=sistemaFinanceiro;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }
    }
}
