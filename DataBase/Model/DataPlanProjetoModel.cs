using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apontamento.DataBase.Model
{
    [Table("tbldataplanprojetos", Schema = "ht")]
    public class DataPlanProjetoModel
    {
        //[Key, Column(Order = 1)]
        //public long codfun { get; set; }
        [Key, Column(Order = 2)]
        public string dia { get; set; }
        public double hora_minima { get; set; }

        // Chave estrangeira para FuncionarioProjetosModel
        [Key, Column(Order = 1)]
        [ForeignKey("Funcionario")]
        public long codfun { get; set; }
        public FuncionarioProjetosModel Funcionario { get; set; }
    }
}
