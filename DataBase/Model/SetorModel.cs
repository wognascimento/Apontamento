using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apontamento.DataBase.Model
{
    [Table("tbl_setor", Schema = "producao")]
    public class SetorModel
    {
        [Key]
        public long? codigo_setor { get; set; }
        public string? setor { get; set; }
        public string? localizacao { get; set; }
        public string? galpao { get; set; }
        public string? responsavel { get; set; }
        public string? lider { get; set; }
        public string? alterado_por { get; set; }
        public DateTime? data_altera { get; set; }
        public string? login_resp { get; set; }
        public string? relatorio_noturno { get; set; }
        public string? permissao_vaga { get; set; }
        public string? inativo { get; set; }
    }
}
