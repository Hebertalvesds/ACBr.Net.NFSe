using System.ServiceModel;

namespace ACBr.Net.NFSe.Providers.Vitoria
{
    [ServiceContract(Namespace = "http://www.abrasf.org.br/nfse.xsd")]
    internal interface IVitoriaHomService
    {
        [OperationContract(Action = "http://www.abrasf.org.br/nfse.xsd/RecepcionarLoteRps")]
        [DataContractFormat(Style = OperationFormatStyle.Document)]
        string RecepcionarLoteRps(string cabecalho, string xmlEnvio);

        [OperationContract(Action = "http://www.abrasf.org.br/nfse.xsd/ConsultarLoteRps")]
        [DataContractFormat(Style = OperationFormatStyle.Document)]
        string ConsultarLoteRps(string cabecalho, string xmlConsulta);

    }
}
