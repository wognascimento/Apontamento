using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apontamento.DataBase.Model
{
    [Keyless]
    [Table("qry_ht_furo_geral", Schema = "ht")]
    public class QryHtFuroGeralModel
    {
        public long? codfun { get; set; }
        public string? setor { get; set; } 
        public string? nome_apelido { get; set; } 
        public DateTime? data_ideal { get; set; } 
        public double? ht_ideal { get; set; } 
        public double? ht_real { get; set; } 
        public double? ht_furo { get; set; } 
        public string? local_galpao { get; set; }
    }
}
