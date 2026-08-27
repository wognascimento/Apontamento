using System;

namespace Apontamento.DataBase.Model
{
    public class FuncionarioMeiModel
    {
        public int? codfun { get; set; }
        public string? setor { get; set; }
        public string? nome_apelido { get; set; }
        public DateTime? admissao { get; set; }
        public DateTime? demissao { get; set; }
        public string? local_galpao { get; set; } = "MEI";
    }
}
