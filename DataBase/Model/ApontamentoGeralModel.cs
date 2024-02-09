using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apontamento.DataBase.Model
{
    [Keyless]
    [Table("qry_apontamento_geral", Schema = "ht")]
    public class ApontamentoGeralModel
    {
        public long? codfun  { get; set; }
        public string? nome_apelido  { get; set; }
        public string? setor  { get; set; }
        public long? num_os  { get; set; }
        public string? cliente_os  { get; set; }
        public double? ht  { get; set; }
        public DateTime? data  { get; set; }
        public string? cadastrado_por  { get; set; }
        public DateTime? inclusao  { get; set; }
        public string? alterado_por  { get; set; }
        public DateTime? alteracao  { get; set; }
        public long? cod  { get; set; }
        public double? semana  { get; set; }
        public string? tipo_os  { get; set; }
        public string? descricao_setor  { get; set; }
        public string? descricao_servico  { get; set; }
        public string? orientacao_caminho  { get; set; }
        public string? local_galpao { get; set; }
    }
}
