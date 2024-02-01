using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apontamento.DataBase.Model
{
    [Keyless]
    [Table("funcionarios_ativos_geral", Schema = "ht")]
    public class FuncionarioAtivoModel
    {
        public long? codfun { get; set; }
        public string? nome_apelido { get; set; } 
        public DateTime? data_admissao { get; set; } 
        public DateTime? data_demissao { get; set; } 
        public string? rg { get; set; } 
        public string? cpf { get; set; } 
        public string? pis { get; set; } 
        public string? nome_rh { get; set; } 
        public string? apelido { get; set; } 
        public string? setor { get; set; } 
        public string? sub_setor { get; set; } 
        public string? funcao { get; set; } 
        public string? empresa { get; set; } 
        public string? ativo { get; set; } 
        public string? portaria { get; set; } 
        public string? local_galpao { get; set; } 
        public DateTime? data_nascimento { get; set; } 
        public string? sexo { get; set; } 
        public long? matricula { get; set; } 
        public string? cod_cracha { get; set; } 
        public long? cracha { get; set; } 
        public long? foto { get; set; } 
        public long? idade { get; set; } 
        public string? cadastrado_por { get; set; } 
        public DateTime? datacadastro { get; set; } 
        public TimeOnly? horacadastro { get; set; } 
        public string? cpd { get; set; } 
        public string? ncamiseta { get; set; } 
        public double? ncalcado { get; set; } 
        public string? barcode { get; set; }
    }
}
