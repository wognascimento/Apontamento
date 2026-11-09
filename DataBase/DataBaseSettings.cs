using System;
using System.Collections.Specialized;
using System.Configuration;

namespace Apontamento.DataBase
{
    public sealed class DataBaseSettings
    {
        private static readonly DataBaseSettings instance = new();
        private const string EnvHost = "APONTAMENTO_DB_HOST";
        private const string EnvDatabase = "APONTAMENTO_DB_NAME";
        private const string EnvUsername = "APONTAMENTO_DB_USER";
        private const string EnvPassword = "APONTAMENTO_DB_PASSWORD";
        private const string DefaultUpdateUrl = "https://atualizasig.cipolatti.com.br/downloads/apontamento/version.json";

        public string? Host { get; set; }
        public string? Database { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ConnectionString { get; set; }
        public NameValueCollection? AppSetting { get; set; }
        public string CaminhoSistema { get; set; } = @"C:\SIG\Apontamento S.I.G\";
        public string? UpdateInfoUrl { get; set; }
        public static DataBaseSettings Instance => instance;

        public void LoadFromConfiguration()
        {
            AppSetting = ConfigurationManager.GetSection("appSettings") as NameValueCollection;

            Host = ReadSetting("DatabaseHost", EnvHost, "192.168.0.23");
            Database = ReadSetting("Database", EnvDatabase, DateTime.Now.Year.ToString());
            Username = ReadSetting("Username", EnvUsername, Environment.UserName);
            Password = ReadSetting("Password", EnvPassword, null);
            CaminhoSistema = ReadSetting("SystemPath", null, CaminhoSistema);
            UpdateInfoUrl = ReadSetting("UpdateInfoUrl", "APONTAMENTO_UPDATE_URL", DefaultUpdateUrl).Trim();
            if (Uri.TryCreate(UpdateInfoUrl, UriKind.Absolute, out var url)
                && url.Host == "192.168.0.49"
                && string.Equals(url.AbsolutePath, "/downloads/apontamento/version.json", StringComparison.OrdinalIgnoreCase))
                UpdateInfoUrl = DefaultUpdateUrl;

            RefreshConnectionString();
        }

        public void RefreshConnectionString()
        {
            if (string.IsNullOrWhiteSpace(Host))
                throw new InvalidOperationException("Host do banco de dados nao configurado.");

            if (string.IsNullOrWhiteSpace(Database))
                throw new InvalidOperationException("Nome do banco de dados nao configurado.");

            if (string.IsNullOrWhiteSpace(Username))
                throw new InvalidOperationException("Usuario do banco de dados nao configurado.");

            if (string.IsNullOrWhiteSpace(Password))
                throw new InvalidOperationException("Senha padrao do banco de dados nao configurada. Configure APONTAMENTO_DB_PASSWORD ou a chave Password.");

            ConnectionString = $"Host={Host};Database={Database};Username={Username};Password={Password}";
        }

        private string ReadSetting(string key, string? environmentVariable, string? defaultValue)
        {
            if (!string.IsNullOrWhiteSpace(environmentVariable))
            {
                var environmentValue = Environment.GetEnvironmentVariable(environmentVariable);
                if (!string.IsNullOrWhiteSpace(environmentValue))
                    return environmentValue;
            }

            var configValue = AppSetting?[key];
            return string.IsNullOrWhiteSpace(configValue) ? defaultValue ?? string.Empty : configValue;
        }
    }
}
