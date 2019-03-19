using ACBr.Net.Core.Extensions;
using ACBr.Net.NFSe.Nota;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace ACBr.Net.NFSe
{
    public static class XmlConstruct
    {
        public static string GetCPF_CNPJ(this XElement element)
        {
            if (element == null) return string.Empty;

            return (element.ElementAnyNs("Cnpj")?.GetValue<string>() ?? element.ElementAnyNs("Cpf")?.GetValue<string>()) ?? string.Empty;
        }

        public static string GetCpfCnpj(XElement root)
        {
            if (root == null) throw new Exception("O elemento raíz não está presente no arquivo");
            var cpfCnpj = GetElementChild(root, "CpfCnpj");
            return cpfCnpj.ElementAnyNs("Cpf").Value ?? cpfCnpj.ElementAnyNs("Cnpj").Value ?? "";
        }

        public static XElement GetElementInfo(XElement root)
        {
            if (root == null) throw new Exception("O elemento raíz não está presente no arquivo");

            return root.ElementAnyNs("InfRps") ?? root.ElementAnyNs("InfDeclaracaoPrestacaoServico") ?? null;
        }

        public static XElement GetElementChild(XElement root, string child)
        {
            if (root == null) throw new Exception("O elemento raíz nao está presente no arquivo");

            return root.ElementAnyNs(child) ?? null;
        }

        public static XElement GetElementChild(XElement root, XElement alternative, string child)
        {
            if (root == null) throw new Exception("O elemento raíz não estão presente.");

            return root.ElementAnyNs(child) ?? alternative.ElementAnyNs(child) ?? null;
        }

        public static decimal GetDecimalElement(XElement xElement, string child, bool obrigatoria = false)
        {
            NumberStyles styles = NumberStyles.Currency | NumberStyles.AllowDecimalPoint;
            NumberFormatInfo infoFormat = new CultureInfo("en-US", false).NumberFormat;
            infoFormat.NumberDecimalSeparator = ".";
            IFormatProvider culture = new CultureInfo("en-GB");

            if (xElement == null) return 0;

            //HasElementException(xElement, child);

            string value = xElement.ElementAnyNs(child)?.GetValue<string>() ?? "";

            if (value.IsEmpty() && obrigatoria) throw new Exception("A tag: " + child + " é de Presença Obrigatória");

            return Decimal.Parse(value, styles, culture);
        }

        public static string GetStringElement(XElement xElement, string child, bool obrigatoria = false)
        {
            if (xElement == null) return "";

            //HasElementException(xElement, child);

            string value = xElement.ElementAnyNs(child)?.GetValue<string>() ?? "";

            if (value.IsEmpty() && obrigatoria) throw new Exception("A tag: " + child + " é de Presença Obrigatória");

            return value;
        }

        public static int GetIntElement(XElement xElement, string child, bool obrigatoria = false)
        {
            if (xElement == null) return 0;

            //HasElementException(xElement, child);

            int value = xElement.ElementAnyNs(child)?.GetValue<int>() ?? 0;

            if (value == 0 && obrigatoria) throw new Exception("Zero não é um valor permitido para a tag: " + child);

            return value;
        }

        public static NFSeSimNao GetIncentivoFiscalCultural(XElement xElement)
        {
            if (xElement == null) return NFSeSimNao.Nao;

            int value = xElement.ElementAnyNs("IncentivoFiscal")?.GetValue<int>() ?? xElement.ElementAnyNs("IncentivadorCultural")?.GetValue<int>() ?? 0;

            switch (value)
            {
                case 1:
                    return NFSeSimNao.Sim;
                case 2:
                    return NFSeSimNao.Nao;
                default:
                    return NFSeSimNao.Nao;
            }
        }

        public static ExigibilidadeIss GetExigibilidadeISS(XElement xElement)
        {
            if (xElement == null) return ExigibilidadeIss.Exigivel;

            int value = xElement.ElementAnyNs("ExigibilidadeISS")?.GetValue<int>() ?? 0;

            ExigibilidadeIss exigibilidade;

            switch (value)
            {
                case 1:
                    exigibilidade = ExigibilidadeIss.Exigivel;
                    break;
                case 2:
                    exigibilidade = ExigibilidadeIss.NaoIncidencia;
                    break;
                case 3:
                    exigibilidade = ExigibilidadeIss.Isencao;
                    break;
                case 4:
                    exigibilidade = ExigibilidadeIss.Exportacao;
                    break;
                case 5:
                    exigibilidade = ExigibilidadeIss.Imunidade;
                    break;
                case 6:
                    exigibilidade = ExigibilidadeIss.SuspensaDecisaoJudicial;
                    break;
                case 7:
                    exigibilidade = ExigibilidadeIss.SuspensaProcessoAdministrativo;
                    break;
                default:
                    exigibilidade = ExigibilidadeIss.Exigivel;
                    break;
            }

            return exigibilidade;
        }

        public static TipoRps GetTipo(int value)
        {
            return (value == 1) ? TipoRps.RPS : (value == 2) ? TipoRps.NFConjugada : TipoRps.Cupom;
        }

        public static NFSeSimNao GetSimNao(int value)
        {
            switch (value)
            {
                case 1:
                    return NFSeSimNao.Sim;
                case 2:
                    return NFSeSimNao.Nao;
                default:
                    return NFSeSimNao.Nao;

            }
        }

        public static NaturezaOperacao GetNaturezaOperacao(int value)
        {
            switch (value)
            {
                case 1:
                    return NaturezaOperacao.NT01;
                case 2:
                    return NaturezaOperacao.NT02;
                case 3:
                    return NaturezaOperacao.NT03;
                case 4:
                    return NaturezaOperacao.NT04;
                case 5:
                    return NaturezaOperacao.NT05;
                case 6:
                    return NaturezaOperacao.NT06;
                default:
                    return NaturezaOperacao.NT01;
            }
        }

        public static ResponsavelRetencao GetResponsavelRetencao(int value)
        {
            switch (value)
            {
                case 1:
                    return ResponsavelRetencao.Tomador;
                case 2:
                    return ResponsavelRetencao.Prestador;
                default:
                    return ResponsavelRetencao.Prestador;
            }
        }

        private static void HasElementException(XElement xElement, string child)
        {
            if (xElement.ElementAnyNs(child) == null) throw new Exception("Elemento raíz " + xElement.Name + " não contém o filho " + child);
        }

        public static bool NodeExists(XElement root, string child)
        {
            if (root.ElementAnyNs(child) == null) return false;
            else return true;
        }

    }
}
