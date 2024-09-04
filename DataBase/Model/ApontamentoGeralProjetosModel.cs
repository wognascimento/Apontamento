using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apontamento.DataBase.Model
{
    [Keyless]
    [Table("qry_apontamento_geral_projetos", Schema = "ht")]
    public class ApontamentoGeralProjetosModel
    {
        public string? nome_func { get; set; }
        public string? departamento { get; set; }
        public int? semana { get; set; }
        public DateTime? data { get; set; }
        public string? desc_atividade { get; set; }
        public string? cliente_tema { get; set; }
        public string? observacao { get; set; }
        public double? hora_trabalhada { get; set; }
        public string? centro_custo { get; set; }
        public string? tipo_custo { get; set; }
        public string? descricao_tipo_custo { get; set; }
    }
}
