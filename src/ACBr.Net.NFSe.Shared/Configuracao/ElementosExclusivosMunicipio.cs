﻿using ACBr.Net.NFSe.Nota;
using ACBr.Net.NFSe.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace ACBr.Net.NFSe.Configuracao
{
    public static class ArquivosPorMunicipio
    {

        public static NotaFiscal ChamaPreenchimentoDinamico(int codigoMunicipio, XElement rps)
        {
            switch (codigoMunicipio)
            {
                case 3143302:
                    return Pronim(rps);
                case 3118601:
                    return Ginfes(rps);
                default:
                    return null;
            }
        }

        private static NotaFiscal Pronim(XElement rps)
        {
            var nfse = new NotaFiscal();

            var ns = rps.GetDefaultNamespace();
            var rInfo = XmlConstruct.GetElementInfo(rps);
            var rpsAux = XmlConstruct.GetElementChild(rInfo, "Rps");
            var identificacao = XmlConstruct.GetElementChild(rInfo, "IdentificacaoRps");
            var servico = rInfo.Element(ns + "Servico");
            var valores = servico.Element(ns + "Valores");
            var prestador = rInfo.Element(ns + "Prestador");
            var tomador = rInfo.Element(ns + "Tomador");

            #region Rps Identificacao
            nfse.IdentificacaoRps.Numero = XmlConstruct.GetStringElement(identificacao, "Numero", true);
            nfse.IdentificacaoRps.Serie = XmlConstruct.GetStringElement(identificacao, "Serie", true);
            nfse.IdentificacaoRps.Tipo = XmlConstruct.GetTipo(XmlConstruct.GetIntElement(identificacao, "Tipo", true));
            nfse.IdentificacaoRps.DataEmissao = DateTime.Parse(XmlConstruct.GetStringElement(rInfo, "DataEmissao"));
            #endregion Rps Identificacao

            nfse.Competencia = DateTime.Parse(XmlConstruct.GetStringElement(rInfo, "Competencia", true));

            #region Servico
            nfse.Servico.Valores.IssRetidoSimNao = XmlConstruct.GetSimNao(XmlConstruct.GetIntElement(servico, "IssRetido"));
            nfse.Servico.ResponsavelRetencao = XmlConstruct.GetResponsavelRetencao(XmlConstruct.GetIntElement(servico,"ResponsavelRetencao"));
            nfse.Servico.ItemListaServico = XmlConstruct.GetStringElement(servico, "ItemListaServico", true);
            nfse.Servico.CodigoCnae = XmlConstruct.GetStringElement(servico, "CodigoCnae");
            nfse.Servico.CodigoTributacaoMunicipio = XmlConstruct.GetStringElement(servico, "CodigoTributacaoMunicipio");
            nfse.Servico.Discriminacao = XmlConstruct.GetStringElement(servico, "Discriminacao", true);
            nfse.Servico.CodigoMunicipio = XmlConstruct.GetIntElement(servico, "CodigoMunicipio");
            nfse.Servico.CodigoPais = XmlConstruct.GetIntElement(servico, "CodigoPais");
            nfse.Servico.ExigibilidadeIss = XmlConstruct.GetExigibilidadeISS(servico);
            nfse.Servico.MunicipioIncidencia = XmlConstruct.GetIntElement(servico, "MunicipioIncidencia");
            nfse.Servico.NumeroProcesso = XmlConstruct.GetStringElement(servico, "NumeroProcesso");

            #region Valores
            nfse.Servico.Valores.ValorServicos = XmlConstruct.GetDecimalElement(valores, "ValorServicos", true);
            nfse.Servico.Valores.ValorDeducoes = XmlConstruct.GetDecimalElement(valores, "ValorDeducoes");
            nfse.Servico.Valores.ValorPis = XmlConstruct.GetDecimalElement(valores, "ValorPis");
            nfse.Servico.Valores.ValorCofins = XmlConstruct.GetDecimalElement(valores, "ValorCofins");
            nfse.Servico.Valores.ValorInss = XmlConstruct.GetDecimalElement(valores, "ValorInss");
            nfse.Servico.Valores.ValorIr = XmlConstruct.GetDecimalElement(valores, "ValorIr");
            nfse.Servico.Valores.ValorCsll = XmlConstruct.GetDecimalElement(valores, "ValorCsll");
            nfse.Servico.Valores.ValorIss = XmlConstruct.GetDecimalElement(valores, "ValorIss");
            nfse.Servico.Valores.ValorOutrasRetencoes = XmlConstruct.GetDecimalElement(valores, "OutrasRetencoes");
            nfse.Servico.Valores.Aliquota = XmlConstruct.GetDecimalElement(valores, "Aliquota");
            nfse.Servico.Valores.DescontoCondicionado = XmlConstruct.GetDecimalElement(valores, "DescontoCondicionado");
            nfse.Servico.Valores.DescontoIncondicionado = XmlConstruct.GetDecimalElement(valores, "DescontoIncondicionado");
            #endregion Valores

            #endregion Servico

            #region Prestador
            //Dados do Prestador
            nfse.Prestador.CpfCnpj = XmlConstruct.GetCPF_CNPJ(prestador);//RecuperarCpfOuCnpj(prestador, ns);
            nfse.Prestador.InscricaoMunicipal = XmlConstruct.GetStringElement(prestador, "InscricaoMunicipal");
            #endregion Prestador

            #region Tomador
            //Dados do Tomador
            nfse.Tomador.CpfCnpj = XmlConstruct.GetCPF_CNPJ(tomador);
            nfse.Tomador.RazaoSocial = XmlConstruct.GetStringElement(tomador, "RazaoSocial");
            nfse.Tomador.InscricaoMunicipal = XmlConstruct.GetStringElement(tomador, "InscricaoMunicipal");
            #region Tomador Endereco
            var endereco = tomador.Element(ns + "Endereco");

            nfse.Tomador.Endereco.Logradouro = XmlConstruct.GetStringElement(endereco, "Endereco");
            nfse.Tomador.Endereco.Numero = XmlConstruct.GetStringElement(endereco, "Numero");
            nfse.Tomador.Endereco.Complemento = XmlConstruct.GetStringElement(endereco, "Complemento");
            nfse.Tomador.Endereco.Bairro = XmlConstruct.GetStringElement(endereco, "Bairro");
            nfse.Tomador.Endereco.CodigoMunicipio = XmlConstruct.GetIntElement(endereco, "CodigoMunicipio");
            nfse.Tomador.Endereco.Uf = XmlConstruct.GetStringElement(endereco, "Uf") ?? XmlConstruct.GetStringElement(endereco, "Estado") ?? "";
            nfse.Tomador.Endereco.CodigoPais = XmlConstruct.GetIntElement(endereco, "CodigoPais");
            nfse.Tomador.Endereco.Cep = XmlConstruct.GetStringElement(endereco, "Cep");
            #endregion Tomador Endereco

            #region Tomador Contato
            var contato = tomador.Element(ns + "Contato");
            nfse.Tomador.DadosContato.Email = XmlConstruct.GetStringElement(endereco, "Email");
            nfse.Tomador.DadosContato.Telefone = XmlConstruct.GetStringElement(endereco, "Telefone");
            #endregion Tomador Contato
            #endregion Tomador

            nfse.IncentivadorCultural = XmlConstruct.GetIncentivoFiscalCultural(rInfo);

            if(XmlConstruct.NodeExists(rInfo, "Intermediario"))
            {

            }

            if(XmlConstruct.NodeExists(rInfo, "ConstrucaoCivil"))
            {

            }

            return nfse;

        }

        private static NotaFiscal Ginfes(XElement rps)
        {
            var nfse = new NotaFiscal();
            var ns = rps.GetDefaultNamespace();
            var rInfo = XmlConstruct.GetElementInfo(rps);
            var rpsAux = XmlConstruct.GetElementChild(rInfo, "Rps");
            var identificacao = XmlConstruct.GetElementChild(rInfo, "IdentificacaoRps");
            var servico = rInfo.Element(ns + "Servico");
            var valores = servico.Element(ns + "Valores");
            var prestador = rInfo.Element(ns + "Prestador");

            nfse.NaturezaOperacao = XmlConstruct.GetNaturezaOperacao(XmlConstruct.GetIntElement(rInfo, "NaturezaOperacao"));
            nfse.Servico.Valores.BaseCalculo = XmlConstruct.GetDecimalElement(valores, "BaseCalculo", true);
            nfse.Servico.Valores.ValorLiquidoNfse = XmlConstruct.GetDecimalElement(valores, "ValorLiquidoNfse");
            nfse.Servico.Valores.ValorIssRetido = XmlConstruct.GetDecimalElement(valores, "ValorIssRetido");

            return nfse;
        }
    }
}
