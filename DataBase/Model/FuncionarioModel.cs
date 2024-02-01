using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apontamento.DataBase.Model
{
    [Table("funcionarios", Schema = "ht")]
    public class FuncionarioModel
    {
        [Key]
        public long? codfun { get; set; }
        public string? nome_apelido { get; set; }
        public DateTime? data_admissao { get; set; }
        public DateTime? data_demissao { get; set; }
        public string? rg { get; set; }
        public string? nome_rh { get; set; }
        public string? apelido { get; set; }
        public string? setor { get; set; }
        public string? funcao { get; set; }
        public string? empresa { get; set; }
        public string? ativo { get; set; }
        public string? obs { get; set; }
        public string? portaria { get; set; }
        public string? impresso { get; set; }
        public string? local_galpao { get; set; }
        public DateTime? data_nascimento { get; set; }
        public string? sexo { get; set; }
        public long? matricula { get; set; }
        public int? cracha { get; set; }
        public int? foto { get; set; }
        public int? idade { get; set; }
        public string? cadastrado_por { get; set; }
        public DateTime? datacadastro { get; set; }
        public TimeOnly? horacadastro { get; set; }
        public string? cpd { get; set; }
        public string? ncamiseta { get; set; }
        public double? ncalcado { get; set; }
        public string? cpf { get; set; }
        public DateTime? data_altera { get; set; }
        public string? alterado_por { get; set; }
        public string? pis { get; set; }
        public string? transferido { get; set; }
        public string? contratacao { get; set; }
        public string? tipo_depto { get; set; }
        public string? sub_depto { get; set; }
        public string? exibir_furo { get; set; }
        public long? codigo_setor { get; set; }
        public string? telefone_residencial { get; set; }
        public string? telefone_celular { get; set; }
        public string? status_cipa { get; set; }
        public DateTime? data_cipa { get; set; }
        public string? ocultar_dados { get; set; }
        public int? prazo_contrato { get; set; }
        public int? prazo_renova_contrato { get; set; }
        public string? contrato_fixo_temp { get; set; }
        public long? codigo_anterior { get; set; }
        public string? agencia { get; set; }
        public string? sub_setor { get; set; }
        public string? cidade { get; set; }
        public string? local { get; set; }
    }
}
