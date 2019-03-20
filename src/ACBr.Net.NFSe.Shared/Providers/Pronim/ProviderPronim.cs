using ACBr.Net.Core.Extensions;
using ACBr.Net.DFe.Core;
using ACBr.Net.DFe.Core.Common;
using ACBr.Net.DFe.Core.Serializer;
using ACBr.Net.NFSe.Configuracao;
using ACBr.Net.NFSe.Nota;
using System;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ACBr.Net.NFSe.Providers.Pronim
{
    internal sealed class ProviderPronim : ProviderBase
    {
        #region Internal Types
        private enum LoadXmlFormato
        {
            Indefinido,
            NFSe,
            Rps
        }
        #endregion Internal Types

        #region Constructors
        public ProviderPronim(ConfigNFSe config, ACBrMunicipioNFSe municipio) : base (config, municipio)
        {
            Name = "Pronim";
        }
        #endregion Constructors

        #region Methods

        

        public override RetornoWebservice Enviar(string lote, string doc)
        {
            var retornoWebservice = new RetornoWebservice();

            if (lote == "") retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "Lote não informado." });

            if (doc == "") retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "RPS não informado." });

            retornoWebservice.XmlEnvio = doc;

            if (Configuracoes.Geral.RetirarAcentos)
            {
                retornoWebservice.XmlEnvio = retornoWebservice.XmlEnvio.RemoveAccent();
            }

            retornoWebservice.XmlEnvio = AssinarMensagemXML(XDocument.Parse(retornoWebservice.XmlEnvio), Certificado).ToString();

            GravarArquivoEmDisco(retornoWebservice.XmlEnvio, $"lote-{lote}-env.xml");

            // Verifica Schema
            ValidarSchema(retornoWebservice, "nfse_v202.xsd");
            if (retornoWebservice.Erros.Any()) return retornoWebservice;

            // Recebe mensagem de retorno
            try
            {
                using (var cliente = GetCliente(TipoUrl.Enviar))
                {
                    retornoWebservice.XmlRetorno = cliente.RecepcionarLoteRps(GerarCabecalho(), retornoWebservice.XmlEnvio);
                }
            }
            catch (Exception ex)
            {
                retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = ex.Message });
                return retornoWebservice;
            }

            GravarArquivoEmDisco(retornoWebservice.XmlRetorno, $"lote-{lote}-ret.xml");

            // Analisa mensagem de retorno
            var xmlRet = XDocument.Parse(retornoWebservice.XmlRetorno);
            MensagemErro(retornoWebservice, xmlRet, "EnviarLoteRpsResposta");
            if (retornoWebservice.Erros.Any()) return retornoWebservice;

            retornoWebservice.NumeroLote = xmlRet.Root?.ElementAnyNs("NumeroLote")?.GetValue<string>() ?? string.Empty;
            retornoWebservice.DataLote = xmlRet.Root?.ElementAnyNs("DataRecebimento")?.GetValue<DateTime>() ?? DateTime.MinValue;
            retornoWebservice.Protocolo = xmlRet.Root?.ElementAnyNs("Protocolo")?.GetValue<string>() ?? string.Empty;
            retornoWebservice.Sucesso = (!string.IsNullOrWhiteSpace(retornoWebservice.NumeroLote));

            if (!retornoWebservice.Sucesso)
                return retornoWebservice;

            return retornoWebservice;

        }

        public override RetornoWebservice Enviar(string lote, NotaFiscalCollection notas)
        {
            var retornoWebservice = new RetornoWebservice();

            if (lote == "")
                retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "Lote não informado." });

            if (notas.Count == 0)
                retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "RPS não informado." });

            if (retornoWebservice.Erros.Count > 0)
                return retornoWebservice;

            var xmlLoteRps = new StringBuilder();

            foreach (var nota in notas)
            {
                var xmlRps = GetXmlRps(nota, false, false, false);
                xmlLoteRps.Append(xmlRps);
                GravarRpsEmDisco(xmlRps, $"Rps-{nota.IdentificacaoRps.DataEmissao:yyyyMMdd}-{nota.IdentificacaoRps.Numero}.xml", nota.IdentificacaoRps.DataEmissao);
            }

            var xmlLote = new StringBuilder();
            xmlLote.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xmlLote.Append("<EnviarLoteRpsEnvio xmlns=\"http://www.abrasf.org.br/nfse.xsd\">");
            xmlLote.Append($"<LoteRps Id=\"{lote}\" versao=\"2.02\">");
            xmlLote.Append($"<NumeroLote>{lote}</NumeroLote>");
            xmlLote.Append($"<CpfCnpj><Cnpj>{Configuracoes.PrestadorPadrao.CpfCnpj.ZeroFill(14)}</Cnpj></CpfCnpj>");
            xmlLote.Append($"<InscricaoMunicipal>{Configuracoes.PrestadorPadrao.InscricaoMunicipal}</InscricaoMunicipal>");
            xmlLote.Append($"<QuantidadeRps>{notas.Count}</QuantidadeRps>");
            xmlLote.Append("<ListaRps>");
            xmlLote.Append(xmlLoteRps);
            xmlLote.Append("</ListaRps>");
            xmlLote.Append("</LoteRps>");
            xmlLote.Append("</EnviarLoteRpsEnvio>");
            retornoWebservice.XmlEnvio = xmlLote.ToString();

            if (Configuracoes.Geral.RetirarAcentos)
            {
                retornoWebservice.XmlEnvio = retornoWebservice.XmlEnvio.RemoveAccent();
            }
            retornoWebservice.XmlEnvio = AssinarMensagemXML(XDocument.Parse(retornoWebservice.XmlEnvio), Certificado).ToString();
            //retornoWebservice.XmlEnvio = XmlSigning.AssinarXml(retornoWebservice.XmlEnvio, "EnviarLoteRpsEnvio", "LoteRps", Certificado);

            GravarArquivoEmDisco(retornoWebservice.XmlEnvio, $"lote-{lote}-env.xml");

            // Verifica Schema
            ValidarSchema(retornoWebservice, "nfse_v202.xsd");
            if (retornoWebservice.Erros.Any()) return retornoWebservice;

            // Recebe mensagem de retorno
            try
            {
                using (var cliente = GetCliente(TipoUrl.Enviar))
                {
                    retornoWebservice.XmlRetorno = cliente.RecepcionarLoteRps(GerarCabecalho(), retornoWebservice.XmlEnvio);
                }
            }
            catch (Exception ex)
            {
                retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = ex.Message });
                return retornoWebservice;
            }

            GravarArquivoEmDisco(retornoWebservice.XmlRetorno, $"lote-{lote}-ret.xml");

            // Analisa mensagem de retorno
            var xmlRet = XDocument.Parse(retornoWebservice.XmlRetorno);
            MensagemErro(retornoWebservice, xmlRet, "EnviarLoteRpsResposta");
            if (retornoWebservice.Erros.Any()) return retornoWebservice;

            retornoWebservice.NumeroLote = xmlRet.Root?.ElementAnyNs("NumeroLote")?.GetValue<string>() ?? string.Empty;
            retornoWebservice.DataLote = xmlRet.Root?.ElementAnyNs("DataRecebimento")?.GetValue<DateTime>() ?? DateTime.MinValue;
            retornoWebservice.Protocolo = xmlRet.Root?.ElementAnyNs("Protocolo")?.GetValue<string>() ?? string.Empty;
            retornoWebservice.Sucesso = (!string.IsNullOrWhiteSpace(retornoWebservice.NumeroLote));

            if (!retornoWebservice.Sucesso)
                return retornoWebservice;

            // ReSharper disable once SuggestVarOrType_SimpleTypes
            foreach (NotaFiscal nota in notas)
            {
                nota.NumeroLote = retornoWebservice.NumeroLote;
            }

            return retornoWebservice;
        }

        public override RetornoWebservice Enviar(int lote, NotaFiscalCollection notas)
        {
            var retornoWebservice = new RetornoWebservice();

            if (lote == 0)
                retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "Lote não informado." });

            if (notas.Count == 0)
                retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = "RPS não informado." });

            if (retornoWebservice.Erros.Count > 0)
                return retornoWebservice;

            var xmlLoteRps = new StringBuilder();

            foreach (var nota in notas)
            {
                var xmlRps = GetXmlRps(nota, false, false, false);
                xmlLoteRps.Append(xmlRps);
                GravarRpsEmDisco(xmlRps, $"Rps-{nota.IdentificacaoRps.DataEmissao:yyyyMMdd}-{nota.IdentificacaoRps.Numero}.xml", nota.IdentificacaoRps.DataEmissao);
            }

            var xmlLote = new StringBuilder();
            xmlLote.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xmlLote.Append("<EnviarLoteRpsEnvio xmlns=\"http://www.abrasf.org.br/nfse.xsd\">");
            xmlLote.Append($"<LoteRps Id=\"LOTE{lote}\" versao=\"2.02\">");
            xmlLote.Append($"<NumeroLote>{lote}</NumeroLote>");
            xmlLote.Append($"<CpfCnpj><Cnpj>{Configuracoes.PrestadorPadrao.CpfCnpj.ZeroFill(14)}</Cnpj></CpfCnpj>");
            xmlLote.Append($"<InscricaoMunicipal>{Configuracoes.PrestadorPadrao.InscricaoMunicipal}</InscricaoMunicipal>");
            xmlLote.Append($"<QuantidadeRps>{notas.Count}</QuantidadeRps>");
            xmlLote.Append("<ListaRps>");
            xmlLote.Append(xmlLoteRps);
            xmlLote.Append("</ListaRps>");
            xmlLote.Append("</LoteRps>");
            xmlLote.Append("</EnviarLoteRpsEnvio>");
            retornoWebservice.XmlEnvio = xmlLote.ToString();

            if (Configuracoes.Geral.RetirarAcentos)
            {
                retornoWebservice.XmlEnvio = retornoWebservice.XmlEnvio.RemoveAccent();
            }
            //retornoWebservice.XmlEnvio = AssinarMensagemXML(XDocument.Parse(retornoWebservice.XmlEnvio), Certificado).ToString();
            retornoWebservice.XmlEnvio = XmlSigning.AssinarXml(retornoWebservice.XmlEnvio, "EnviarLoteRpsEnvio", "LoteRps", Certificado);

            GravarArquivoEmDisco(retornoWebservice.XmlEnvio, $"lote-{lote}-env.xml");

            // Verifica Schema
            ValidarSchema(retornoWebservice, "nfse_v202.xsd");
            if (retornoWebservice.Erros.Any()) return retornoWebservice;

            // Recebe mensagem de retorno
            try
            {
                using (var cliente = GetCliente(TipoUrl.Enviar))
                {
                    retornoWebservice.XmlRetorno = cliente.RecepcionarLoteRps(GerarCabecalho(), retornoWebservice.XmlEnvio);
                }
            }
            catch (Exception ex)
            {
                retornoWebservice.Erros.Add(new Evento { Codigo = "0", Descricao = ex.Message });
                return retornoWebservice;
            }

            GravarArquivoEmDisco(retornoWebservice.XmlRetorno, $"lote-{lote}-ret.xml");

            // Analisa mensagem de retorno
            var xmlRet = XDocument.Parse(retornoWebservice.XmlRetorno);
            MensagemErro(retornoWebservice, xmlRet, "EnviarLoteRpsResposta");
            if (retornoWebservice.Erros.Any()) return retornoWebservice;

            retornoWebservice.NumeroLote = xmlRet.Root?.ElementAnyNs("NumeroLote")?.GetValue<string>() ?? string.Empty;
            retornoWebservice.DataLote = xmlRet.Root?.ElementAnyNs("DataRecebimento")?.GetValue<DateTime>() ?? DateTime.MinValue;
            retornoWebservice.Protocolo = xmlRet.Root?.ElementAnyNs("Protocolo")?.GetValue<string>() ?? string.Empty;
            retornoWebservice.Sucesso = (!string.IsNullOrWhiteSpace(retornoWebservice.NumeroLote));

            if (!retornoWebservice.Sucesso)
                return retornoWebservice;

            // ReSharper disable once SuggestVarOrType_SimpleTypes
            foreach (NotaFiscal nota in notas)
            {
                nota.NumeroLote = retornoWebservice.NumeroLote;
            }

            return retornoWebservice;
        }

        private string GetXmlRps(NotaFiscal nota, bool identado, bool showDeclaration, bool withPrefix)
        {
            var incentivadorCultural = (nota.IncentivadorCultural == NFSeSimNao.Sim) ? 1 : 2;
            string tipoRps;
            switch (nota.IdentificacaoRps.Tipo)
            {
                case TipoRps.RPS:
                    tipoRps = "1";
                    break;
                case TipoRps.NFConjugada:
                    tipoRps = "2";
                    break;
                case TipoRps.Cupom:
                    tipoRps = "3";
                    break;
                default:
                    tipoRps = "0";
                    break;
            }

            string tipoRpsSubstituido;
            switch (nota.RpsSubstituido.Tipo)
            {
                case TipoRps.RPS:
                    tipoRpsSubstituido = "1";
                    break;

                case TipoRps.NFConjugada:
                    tipoRpsSubstituido = "2";
                    break;

                case TipoRps.Cupom:
                    tipoRpsSubstituido = "3";
                    break;

                default:
                    tipoRpsSubstituido = "0";
                    break;
            }

            int exigibilidade= 1;
            switch (nota.Servico.ExigibilidadeIss)
            {
                case ExigibilidadeIss.Exigivel:
                    exigibilidade = 1;
                    break;
                case ExigibilidadeIss.NaoIncidencia:
                    exigibilidade = 2;
                    break;
                case ExigibilidadeIss.Isencao:
                    exigibilidade = 3;
                    break;
                case ExigibilidadeIss.Exportacao:
                    exigibilidade = 4;
                    break;
                case ExigibilidadeIss.Imunidade:
                    exigibilidade = 5;
                    break;
                case ExigibilidadeIss.SuspensaDecisaoJudicial:
                    exigibilidade = 6;
                    break;
                case ExigibilidadeIss.SuspensaProcessoAdministrativo:
                    exigibilidade = 7;
                    break;
            }

            string regimeET, optanteSN;

            if(nota.RegimeEspecialTributacao == RegimeEspecialTributacao.SimplesNacional)
            {
                regimeET = "6";
                optanteSN = "1";
            }
            else
            {
                var regime = (int)nota.RegimeEspecialTributacao;
                regimeET = (regime == 0) ? string.Empty : regime.ToString();
                optanteSN = "2";
            }

            
            var situacao = (nota.Situacao == SituacaoNFSeRps.Normal) ? "1" : "2";
            var issRetido = (nota.Servico.Valores.IssRetidoSimNao == NFSeSimNao.Sim) ? "1" : "2";

            var xmlDoc = new XDocument(new XDeclaration("1.0", "UTF-8", null));

            XNamespace ns = "http://www.abrasf.org.br/nfse.xsd";
            var rps = withPrefix ? new XElement(ns + "Rps", new XAttribute(XNamespace.Xmlns + "xns", ns)) :
                                  new XElement("Rps", new XAttribute(XNamespace.Xmlns + "xns", ns));

            xmlDoc.Add(rps);

            var infoRps = new XElement(ns + "InfDeclaracaoPrestacaoServico", new XAttribute("Id", "RPS" + nota.IdentificacaoRps.Numero));
            rps.Add(infoRps);

            var rpsInfo = new XElement(ns + "Rps");
            infoRps.Add(rpsInfo);

            var ideRps = new XElement(ns + "IdentificacaoRps");
            rpsInfo.Add(ideRps);

            ideRps.AddChild(AdicionarTag(TipoCampo.Int, "", "Numero", ns, 1, 15, Ocorrencia.Obrigatoria, nota.IdentificacaoRps.Numero));
            ideRps.AddChild(AdicionarTag(TipoCampo.Str, "", "Serie", ns, 1, 5, Ocorrencia.Obrigatoria, nota.IdentificacaoRps.Serie));
            ideRps.AddChild(AdicionarTag(TipoCampo.Int, "", "Tipo", ns, 1, 1,Ocorrencia.Obrigatoria, tipoRps));
            rpsInfo.AddChild(AdicionarTag(TipoCampo.Dat, "", "DataEmissao", ns, 20, 20, Ocorrencia.Obrigatoria, nota.IdentificacaoRps.DataEmissao));
            rpsInfo.AddChild(AdicionarTag(TipoCampo.Int, "", "Status", ns, 1, 1, Ocorrencia.Obrigatoria, situacao));

            //infoRps.AddChild(AdicionarTag(TipoCampo.Int, "", "NaturezaOperacao", ns, 1, 1, Ocorrencia.Obrigatoria, naturezaOperacao));
            //infoRps.AddChild(AdicionarTag(TipoCampo.Int, "", "RegimeEspecialTributacao", ns, 1, 1, Ocorrencia.NaoObrigatoria, regimeEspecialTributacao));

            if (!string.IsNullOrWhiteSpace(nota.RpsSubstituido.NumeroRps))
            {
                var rpsSubstituido = new XElement(ns + "RpsSubstituido");

                rpsSubstituido.AddChild(AdicionarTag(TipoCampo.Int, "", "Numero", ns, 1, 15, Ocorrencia.Obrigatoria, nota.RpsSubstituido.NumeroRps));
                rpsSubstituido.AddChild(AdicionarTag(TipoCampo.Int, "", "Serie", ns, 1, 5, Ocorrencia.Obrigatoria, nota.RpsSubstituido.Serie));
                rpsSubstituido.AddChild(AdicionarTag(TipoCampo.Int, "", "Tipo", ns, 1, 1, Ocorrencia.Obrigatoria, tipoRpsSubstituido));

                infoRps.AddChild(rpsSubstituido);
            }

            infoRps.AddChild(AdicionarTag(TipoCampo.Dat, "", "Competencia", ns, 20, 20, Ocorrencia.Obrigatoria, nota.Competencia));

            var servico = new XElement(ns + "Servico");
            infoRps.AddChild(servico);

            var valores = new XElement(ns + "Valores");
            servico.AddChild(valores);

            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorServicos", ns, 1, 15, Ocorrencia.Obrigatoria, nota.Servico.Valores.ValorServicos));

            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorDeducoes", ns, 1, 15, Ocorrencia.Obrigatoria, nota.Servico.Valores.ValorDeducoes));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorPis", ns, 1, 15, Ocorrencia.Obrigatoria, nota.Servico.Valores.ValorPis));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorCofins", ns, 1, 15, Ocorrencia.Obrigatoria, nota.Servico.Valores.ValorCofins));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorInss", ns, 1, 15, Ocorrencia.Obrigatoria, nota.Servico.Valores.ValorInss));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorIr", ns, 1, 15, Ocorrencia.Obrigatoria, nota.Servico.Valores.ValorIr));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorCsll", ns, 1, 15, Ocorrencia.Obrigatoria, nota.Servico.Valores.ValorCsll));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "ValorIss", ns, 1, 15, Ocorrencia.Obrigatoria, nota.Servico.Valores.ValorIss));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "Aliquota", ns, 1, 7, Ocorrencia.Obrigatoria, nota.Servico.Valores.Aliquota));  // Valor Percentual - Exemplos: 1% => 0.01   /   25,5% => 0.255   /   100% => 1

            servico.AddChild(AdicionarTag(TipoCampo.Int, "", "IssRetido", ns, 1, 1, Ocorrencia.Obrigatoria, issRetido));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "DescontoIncondicionado", ns, 1, 15, Ocorrencia.Obrigatoria, nota.Servico.Valores.DescontoIncondicionado));
            valores.AddChild(AdicionarTag(TipoCampo.De2, "", "DescontoCondicionado", ns, 1, 15, Ocorrencia.Obrigatoria, nota.Servico.Valores.DescontoCondicionado));

            /*
            Ocorrencia TipoOcorrenciaValorIss = Ocorrencia.MaiorQueZero;
            if (regimeEspecialTributacao == "2")
            {
                // Se o regime for "Estimativa", obrigatório informar as tags "ValorIss" e "Aliquota"
                TipoOcorrenciaValorIss = Ocorrencia.Obrigatoria;
            }
            */

            servico.AddChild(AdicionarTag(TipoCampo.Str, "", "ItemListaServico", ns, 1, 5, Ocorrencia.Obrigatoria, nota.Servico.ItemListaServico));

            servico.AddChild(AdicionarTag(TipoCampo.StrNumber, "", "CodigoCnae", ns, 1, 7, Ocorrencia.NaoObrigatoria, nota.Servico.CodigoCnae));

            servico.AddChild(AdicionarTag(TipoCampo.Str, "", "CodigoTributacaoMunicipio", ns, 1, 20, Ocorrencia.NaoObrigatoria, nota.Servico.CodigoTributacaoMunicipio));
            servico.AddChild(AdicionarTag(TipoCampo.Str, "", "Discriminacao", ns, 1, 2000, Ocorrencia.Obrigatoria, nota.Servico.Discriminacao));
            servico.AddChild(AdicionarTag(TipoCampo.StrNumber, "", "CodigoMunicipio", ns, 1, 7, Ocorrencia.Obrigatoria, nota.Servico.CodigoMunicipio == 9999999 ? 99999 : nota.Servico.CodigoMunicipio)); // Ginfes: No IBGE, o código de cidade do exterior é 9999999, mas no Ginfes é 99999
            servico.AddChild(AdicionarTag(TipoCampo.Str, "", "CodigoPais", ns, 4, 4, Ocorrencia.NaoObrigatoria, nota.Servico.CodigoPais));
            servico.AddChild(AdicionarTag(TipoCampo.Int, "", "ExigibilidadeISS", ns, 1, 1, Ocorrencia.Obrigatoria, exigibilidade));

            var prestador = new XElement(ns + "Prestador");
            infoRps.AddChild(prestador);

            var cpfcnpj = new XElement(ns + "CpfCnpj");

            prestador.Add(cpfcnpj);
            cpfcnpj.AddChild(AdicionarTagCNPJCPF("", "Cpf", "Cnpj", nota.Prestador.CpfCnpj.ZeroFill(14), ns));
            prestador.AddChild(AdicionarTag(TipoCampo.StrNumber, "", "InscricaoMunicipal", ns, 1, 15, Ocorrencia.Obrigatoria, nota.Prestador.InscricaoMunicipal));

            var tomador = new XElement(ns + "Tomador");
            infoRps.AddChild(tomador);

            if (!nota.Tomador.CpfCnpj.IsEmpty() || !nota.Tomador.InscricaoMunicipal.IsEmpty())
            {
                var ideTomador = new XElement(ns + "IdentificacaoTomador");
                tomador.AddChild(ideTomador);

                if (!nota.Tomador.CpfCnpj.IsEmpty())
                {
                    var cpfCnpj = new XElement(ns + "CpfCnpj");
                    ideTomador.AddChild(cpfCnpj);
                    cpfCnpj.AddChild(AdicionarTagCNPJCPF("", "Cpf", "Cnpj", nota.Tomador.CpfCnpj, ns));
                }

                if (!string.IsNullOrWhiteSpace(nota.Tomador.InscricaoMunicipal))
                    ideTomador.AddChild(AdicionarTag(TipoCampo.StrNumber, "", "InscricaoMunicipal", ns, 1, 15, Ocorrencia.NaoObrigatoria, nota.Tomador.InscricaoMunicipal));
            }

            tomador.AddChild(AdicionarTag(TipoCampo.Str, "", "RazaoSocial", ns, 1, 115, Ocorrencia.NaoObrigatoria, nota.Tomador.RazaoSocial));
            if (!nota.Tomador.Endereco.Logradouro.IsEmpty())
            {
                var endereco = new XElement(ns + "Endereco");
                tomador.AddChild(endereco);

                endereco.AddChild(AdicionarTag(TipoCampo.Str, "", "Endereco", ns, 1, 125, Ocorrencia.NaoObrigatoria, nota.Tomador.Endereco.Logradouro));
                endereco.AddChild(AdicionarTag(TipoCampo.Str, "", "Numero", ns, 1, 10, Ocorrencia.NaoObrigatoria, nota.Tomador.Endereco.Numero.IsEmpty() ? "SN" : nota.Tomador.Endereco.Numero));
                endereco.AddChild(AdicionarTag(TipoCampo.Str, "", "Complemento", ns, 1, 10, Ocorrencia.NaoObrigatoria, nota.Tomador.Endereco.Complemento));
                endereco.AddChild(AdicionarTag(TipoCampo.Str, "", "Bairro", ns, 1, 60, Ocorrencia.NaoObrigatoria, nota.Tomador.Endereco.Bairro));
                endereco.AddChild(AdicionarTag(TipoCampo.StrNumber, "", "CodigoMunicipio", ns, 1, 7, Ocorrencia.NaoObrigatoria, nota.Tomador.Endereco.CodigoMunicipio == 9999999 ? 99999 : nota.Tomador.Endereco.CodigoMunicipio)); // Ginfes: No IBGE, o código de cidade do exterior é 9999999, mas no Ginfes é 99999
                endereco.AddChild(AdicionarTag(TipoCampo.Str, "", "Uf", ns, 2, 2, Ocorrencia.NaoObrigatoria, nota.Tomador.Endereco.Uf));
                endereco.AddChild(AdicionarTag(TipoCampo.StrNumberFill, "", "Cep", ns, 8, 8, Ocorrencia.NaoObrigatoria, nota.Tomador.Endereco.Cep));
            }

            if (!nota.Tomador.DadosContato.Telefone.IsEmpty() || !nota.Tomador.DadosContato.Email.IsEmpty())
            {
                var contato = new XElement(ns + "Contato");
                tomador.AddChild(contato);

                contato.AddChild(AdicionarTag(TipoCampo.StrNumber, "", "Telefone", ns, 1, 11, Ocorrencia.NaoObrigatoria, nota.Tomador.DadosContato.DDD + nota.Tomador.DadosContato.Telefone));
                contato.AddChild(AdicionarTag(TipoCampo.Str, "", "Email", ns, 1, 80, Ocorrencia.NaoObrigatoria, nota.Tomador.DadosContato.Email));
            }

            if (!nota.Intermediario.RazaoSocial.IsEmpty())
            {
                var intServico = new XElement(ns + "IntermediarioServico");
                infoRps.AddChild(intServico);

                intServico.AddChild(AdicionarTag(TipoCampo.Str, "", "RazaoSocial", ns, 1, 115, 0, nota.Intermediario.RazaoSocial));

                var intServicoCpfCnpj = new XElement(ns + "CpfCnpj");
                intServico.AddChild(intServicoCpfCnpj);

                intServicoCpfCnpj.AddChild(AdicionarTagCNPJCPF("", "Cpf", "Cnpj", nota.Intermediario.CpfCnpj, ns));

                intServico.AddChild(AdicionarTag(TipoCampo.StrNumber, "", "InscricaoMunicipal", ns, 1, 15, 0, nota.Intermediario.InscricaoMunicipal));
            }

            if (!nota.ConstrucaoCivil.CodigoObra.IsEmpty())
            {
                var conCivil = new XElement(ns + "ConstrucaoCivil");
                infoRps.AddChild(conCivil);

                conCivil.AddChild(AdicionarTag(TipoCampo.Str, "", "CodigoObra", ns, 1, 15, Ocorrencia.Obrigatoria, nota.ConstrucaoCivil.CodigoObra));
                conCivil.AddChild(AdicionarTag(TipoCampo.Str, "", "Art", ns, 1, 15, Ocorrencia.Obrigatoria, nota.ConstrucaoCivil.ArtObra));
            }

            infoRps.AddChild(AdicionarTag(TipoCampo.Int, "", "OptanteSimplesNacional", ns, 1, 1, Ocorrencia.Obrigatoria, optanteSN));
            infoRps.AddChild(AdicionarTag(TipoCampo.Int, "", "IncentivoFiscal", ns, 1, 1, Ocorrencia.Obrigatoria, incentivadorCultural));

            return xmlDoc.AsString(identado, showDeclaration, Encoding.UTF8);

        }

        private string GerarCabecalho()
        {
            return $"<cabecalho versao=\"2.02\" xmlns=\"http://www.abrasf.org.br/nfse.xsd\">{Environment.NewLine}<versaoDados>2.02</versaoDados>{Environment.NewLine}</cabecalho>";
        }

        private static void MensagemErro(RetornoWebservice retornoWs, XContainer xmlRet, string xmlTag)
        {
            var mensagens = xmlRet?.ElementAnyNs(xmlTag);
            mensagens = mensagens?.ElementAnyNs("ListaMensagemRetorno");
            if (mensagens == null)
                return;

            foreach (var mensagem in mensagens.ElementsAnyNs("MensagemRetorno"))
            {
                var evento = new Evento
                {
                    Codigo = mensagem?.ElementAnyNs("Codigo")?.GetValue<string>() ?? string.Empty,
                    Descricao = mensagem?.ElementAnyNs("Mensagem")?.GetValue<string>() ?? string.Empty,
                    Correcao = mensagem?.ElementAnyNs("Correcao")?.GetValue<string>() ?? string.Empty
                };
                retornoWs.Erros.Add(evento);
            }
        }

        private IPronimServiceClient GetCliente(TipoUrl tipo)
        {
            switch (Configuracoes.WebServices.Ambiente)
            {
                case DFeTipoAmbiente.Homologacao:
                    return new PronimHomoServiceClient(this, tipo);

                case DFeTipoAmbiente.Producao:
                    return new PronimProdServiceClient(this, tipo);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private XDocument AssinarMensagemXML(XDocument mensagemXML, System.Security.Cryptography.X509Certificates.X509Certificate2 certificado)
        {
            var xmlDoc = new XmlDocument();
            var key = new System.Security.Cryptography.RSACryptoServiceProvider();
            var signedDocument = new System.Security.Cryptography.Xml.SignedXml();
            var keyInfo = new System.Security.Cryptography.Xml.KeyInfo();
            xmlDoc.LoadXml(mensagemXML.ToString());


            key = (System.Security.Cryptography.RSACryptoServiceProvider)certificado.PrivateKey;
            keyInfo.AddClause(clause: new System.Security.Cryptography.Xml.KeyInfoX509Data(certificado));
            signedDocument = new System.Security.Cryptography.Xml.SignedXml(xmlDoc);

            signedDocument.SigningKey = key;
            signedDocument.KeyInfo = keyInfo;

            var reference = new System.Security.Cryptography.Xml.Reference();
            reference.Uri = String.Empty;

            reference.AddTransform(new System.Security.Cryptography.Xml.XmlDsigEnvelopedSignatureTransform());
            reference.AddTransform(transform: new System.Security.Cryptography.Xml.XmlDsigExcC14NTransform(false));

            signedDocument.AddReference(reference);
            signedDocument.ComputeSignature();

            foreach (XmlElement loterps in xmlDoc.GetElementsByTagName("LoteRps"))
            {
                signedDocument.Signature.Id = "ID" + loterps.GetAttribute("Id").ToString();
                reference.Uri = "#" + loterps.GetAttribute("Id").ToString();
            }
            foreach (XmlElement rps in xmlDoc.GetElementsByTagName("Rps"))
            {
                //
            }

            xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(signedDocument.GetXml(), true));

            return XDocument.Parse(xmlDoc.OuterXml);
        }
        #endregion Methods
    }
}
