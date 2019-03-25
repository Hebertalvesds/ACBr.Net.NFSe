using System;
using System.Collections.Generic;
using System.Text;

namespace ACbr.Net.Storage.Models
{
    public sealed class Prestador
    {
        public int Id { get; set; }
        public string NomeConfig { get; set; }
        public string CnpjCpf { get; set; }
        public string RazaoSocial { get; set; }
        public string NomeFantasia { get; set; }
        public int Telefone { get; set; }
        public int Cep { get; set; }
        public string Endereco { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Uf { get; set; }
        public int CodCidade { get; set; }
        public int CodSiaf { get; set; }

        public string CertPxsPath { get; set; }
        public string Senha { get; set; }
        public string Serial { get; set; }
        public string XmlListaPath { get; set; }
        public string XmlProcessaPath { get; set; }

        public string Ambiente { get; set; }
    }
}

