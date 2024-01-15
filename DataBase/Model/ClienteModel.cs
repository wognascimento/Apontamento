﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apontamento.DataBase.Model
{
    [Table("clientes", Schema = "comercial")]
    public class ClienteModel
    {
        [Key]
        public string? sigla { get; set; }
        public string? grupo { get; set; }
        public string? nome { get; set; }
        public string? tipo { get; set; }
        public string? internacional { get; set; }
        public string? rsocial { get; set; }
        public string? nome_fantasia { get; set; }
        public string? endereco { get; set; }
        public string? cidade  { get; set; }
        public string? bairro { get; set; }
        public string? est { get; set; }
        public int? cep { get; set; }
        public string? cep_internacional { get; set; }
        public string? praca { get; set; }
        public string? regiao { get; set; }
        public string? pais { get; set; }
        public string? ddi  { get; set; }
        public string? ddd { get; set; }
        public int? fone1  { get; set; }
        public int? fone2 { get; set; }
        public int? fax { get; set; }
        public string? cnpj { get; set; }
        public string? inscestad { get; set; }
        public DateTime? aniversario { get; set; }
        public int? abl { get; set; }
        public int? lojas { get; set; }
        public string? abrasce { get; set; }
        public string? website { get; set; }
        public string? email { get; set; }
        public string? inativo { get; set; }
        public string? obsinatvo { get; set; }
        public DateTime? atualizacao { get; set; }
        public string? respatualizacao { get; set; }
        public int? pisos { get; set; }
        public string? reg_atend { get; set; }
        public string? obs { get; set; }
        public int? fluxo { get; set; }
        public string? referencia_cliente { get; set; }
        public string? publico { get; set; }
        public double? area_construida { get; set; }
        public double? publico_mensal { get; set; }
        public double? qtdpiso { get; set; }
        public double? qtdvao { get; set; }
        public double? qtdcorredores { get; set; }
        public double? area_praca_principal { get; set; }
        public double? area_praca_aliment { get; set; }
        public double? lojas_ancora { get; set; }
        public string? ccm { get; set; }
        public string? opiniao { get; set; }
        public string? publico_classe { get; set; }
        public string? publico_sexo { get; set; }
        public string? publico_fluxo { get; set; }
        public double? distancia { get; set; }
        public int? totvs { get; set; }
        public string? aux { get; set; }
        public int? id_cliente { get; set; }
        public string? cnpj_retencao { get; set; }
    }
}
