using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apontamento.DataBase.Model
{
    [Table("t_data_planejamento", Schema = "ht")]
    public class DataPlanejamentoModel
    {
        [Key]
        public DateTime? data { get; set; }
        public int? semana { get; set; }
        public string? dia { get; set; }
        public string? mes { get; set; }
        public string? producao { get; set; }
        public double? ht { get; set; }
        public double? hp { get; set; }
        public int? classif { get; set; }
        public int? ordem { get; set; }
        public string? controle_folha { get; set; }
    }
}
