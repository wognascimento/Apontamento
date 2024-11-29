using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apontamento.DataBase.Model
{
    [Keyless]
    [Table("qry_apontamento_geral_projetos", Schema = "ht")]
    public class ApontamentoGeralProjetosModel
    {
        public long? cod_linha { get; set; }
        public long? cod_func { get; set; }
        public string? nome_func { get; set; }
        public string? identificacao { get; set; }
        public string? tipo_custo { get; set; }
        public string? descricao_tipo_custo { get; set; }
        public string? centro_custo { get; set; }
        public string? desc_atividade { get; set; }
        public DateTime? data { get; set; }
        public int? semana { get; set; }
        public string? observacao { get; set; }
        public string? departamento { get; set; }
        public string? cliente_tema { get; set; }
        public double? hora_trabalhada { get; set; }
    }
}
