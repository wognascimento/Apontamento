using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apontamento.DataBase.Model
{
    [Table("t_funcionario_proj", Schema = "ht")]
    public class FuncionarioProjetosModel
    {
        [Key]
        public long? cod_func { get; set; }
        public string? nome_func { get; set; }
        public string? funcao_func { get; set; }
        public string? nick_name_func { get; set; }
        public string? tipo_contrato { get; set; }
        public double? horas_minimas { get; set; }
        public DateTime? incio_ferias { get; set; }
        public DateTime? termino_ferias { get; set; }
        public DateTime? data_demissao { get; set; }
        public string? departamento { get; set; }
        public string? ferias_apontadas { get; set; }
        public string? cracha { get; set; }

        // Relacionamento um-para-muitos com DataPlanProjetoModel
        public ICollection<DataPlanProjetoModel> PlanosProjetos { get; set; }
    }
}
