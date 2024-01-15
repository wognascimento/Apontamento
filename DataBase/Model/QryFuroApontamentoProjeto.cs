﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apontamento.DataBase.Model
{
    [Table("qryfurohtprojetos", Schema = "ht")]
    [Keyless]
    public class QryFuroApontamentoProjeto
    {
        public long? codfun { get; set; }
        public DateTime? data { get; set; }
        public int? semana { get; set; }
        public TimeSpan? ht_apontada { get; set; }
        public double? converte { get; set; }
        public string? dia { get; set; }
        public double? hora_minima { get; set; }
        public double? validacao { get; set; }
        public double? verificacao { get; set; }
        public double? validacao_novo { get; set; }
        public double? verificacao_novo { get; set; }
        public double? hora_trabalhada { get; set; }
    }
}
