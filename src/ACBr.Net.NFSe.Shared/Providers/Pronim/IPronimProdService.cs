using System.ServiceModel;

namespace ACBr.Net.NFSe.Providers.Pronim
{
    [ServiceContract(Namespace = "http://tempuri.org/")]
    internal interface IPronimProdService
    {

        [OperationContract(Action = "http://tempuri.org/INFSEGeracao/RecepcionarLoteRps")]
        [DataContractFormat(Style = OperationFormatStyle.Document)]
        string RecepcionarLoteRps(string cabecalho, string xmlEnvio);

        [OperationContract(Action = "http://tempuri.org/INFSEConsultas/ConsultarLoteRps")]
        [DataContractFormat(Style = OperationFormatStyle.Document)]
        string ConsultarLoteRps(string cabecalho, string xmlConsulta);

    }
}
