using Npgsql;

namespace Apontamento.DataBase
{
    public static class DbConnectionFactory
    {
        public static NpgsqlConnection Create()
        {
            var settings = DataBaseSettings.Instance;
            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
                settings.RefreshConnectionString();

            return new NpgsqlConnection(settings.ConnectionString);
        }
    }
}
