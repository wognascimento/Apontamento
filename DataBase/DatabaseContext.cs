using Apontamento.DataBase.Model;
using Microsoft.EntityFrameworkCore;
using System;

namespace Apontamento.DataBase
{
    public class DatabaseContext : DbContext
    {
        private DataBaseSettings BaseSettings = DataBaseSettings.Instance;

        static DatabaseContext() => AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            optionsBuilder.UseNpgsql(
                $"host={BaseSettings.Host};" +
                $"user id={BaseSettings.Username};" +
                $"password={BaseSettings.Password};" +
                $"database={BaseSettings.Database};" +
                $"Application Name=SIG Apontamento <{BaseSettings.Database}>;",
                options => { options.EnableRetryOnFailure(); }
                );
            optionsBuilder.EnableSensitiveDataLogging();
        }

        public DbSet<TarefaProjetosModel> TarefaProjetos { get; set; }
        public DbSet<ClienteModel> Clientes { get; set; }
        public DbSet<TemaModel> Temas { get; set; }
        public DbSet<FuncionarioProjetosModel> FuncionarioProjetos { get; set; }
        public DbSet<ApontamentoHoraModel> ApontamentoHoras { get; set; }
        public DbSet<DataPlanejamentoModel> DataPlanejamentos { get; set; }
        public DbSet<QryApontamentoHoraModel> QryApontamentos { get; set; }
        public DbSet<QryFuroApontamentoProjeto> QryFuroApontamentoProjetos { get; set; }
        public DbSet<FuncionarioAtivoModel> FuncionarioAtivos { get; set; }
        public DbSet<FuncionarioModel> Funcionarios { get; set; }
        public DbSet<HtModel> Hts { get; set; }
        public DbSet<QryHtFuroGeralModel> QryHtFuros { get; set; }
    }
}
