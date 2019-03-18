using System.ServiceModel;

namespace ACBr.Net.NFSe.Providers.Pronim
{
    [ServiceContract(Namespace = "http://www.abrasf.org.br/nfse.xsd")]
    internal interface IPronimProdService
    {
        [OperationContract(Action = "", ReplyAction = "*")]
        [DataContractFormat(Style = OperationFormatStyle.Rpc)]
        [return: MessageParameter(Name = "return")]
        string RecepcionarLoteRps([MessageParameter(Name = "arg0")]string arg0, [MessageParameter(Name = "arg1")]string arg1);
    }
}
