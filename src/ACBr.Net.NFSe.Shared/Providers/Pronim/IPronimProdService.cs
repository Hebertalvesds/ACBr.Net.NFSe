using System.ServiceModel;

namespace ACBr.Net.NFSe.Providers.Pronim
{

    [ServiceContract(Namespace = "http://tempuri.org/")]
    internal interface IPronimProdService
    {
        
        [OperationContract(Action = "http://tempuri.org/INFSEGeracao/RecepcionarLoteRps", ReplyAction = "http://tempuri.org/INFSEGeracao/RecepcionarLoteRpsResponse")]
        [DataContractFormat(Style = OperationFormatStyle.Document)]
        [return: MessageParameter(Name = "return")]
        string RecepcionarLoteRps([MessageParameter(Name = "arg0")]string arg0, [MessageParameter(Name = "arg1")]string arg1);

        
    }
}
