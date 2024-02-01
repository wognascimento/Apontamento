using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apontamento.DataBase.Model
{
    [Table("ht", Schema = "ht")]
    public class HtModel
    {
        [Key]
        public long? cod { get; set; }
        public DateTime? data { get; set; } 
        public long? num_os { get; set; } 
        public double? tarefas { get; set; } 
        public double? quantidadehorastrabalhadas { get; set; } 
        public double? quantidadeunidadeproduzida { get; set; } 
        public double? semana { get; set; } 
        public string? cadastrado_por { get; set; } 
        public DateTime? inclusao { get; set; } 
        public string? alterado_por { get; set; } 
        public DateTime? alteracao { get; set; } 
        public string? barcode { get; set; } 
        public string? obs { get; set; } 
        public long? tbl_servicos_num_os { get; set; } 
        public long? apontamnetos_num_os { get; set; } 
        public long? codfun { get; set; }
    }
}
