using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apontamento.DataBase.Model
{
    [Table("qry_apontamento_horas", Schema = "ht")]
    public class QryApontamentoHoraModel
    {
        [Key]
        public long? cod_linha { get; set; }
        public long? cod_func { get; set; }
        public string? desc_atividade { get; set; }
        public string? cliente_tema { get; set; }
        public DateTime? data { get; set; }
        public int? semana { get; set; }
        public TimeSpan? hora_inicio { get; set; }
        public TimeSpan? hora_fim { get; set; }
        public TimeSpan? total_hora { get; set; }
        public string? observacao { get; set; }
        public TimeSpan? intervalo { get; set; }
        public double? hora_trabalhada { get; set; }
    }
}
