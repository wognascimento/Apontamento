using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apontamento.DataBase.Model
{
    [Table("t_tarefas_projetos", Schema = "ht")]
    public class TarefaProjetosModel
    {
        [Key]
        public long codigo_tarefa { get; set; }
        public string identificacao { get; set; }
        public string atividade { get; set; }
        public string tipo_custo { get; set; }
        public string? descricao_tipo_custo { get; set; }
        public string descricao_atividade { get; set; }
        public string? observao_obrigatoria { get; set; }
        public string centro_custo { get; set; }
        public string? departamento { get; set; }
        public string inativo { get; set; }
    }
}
