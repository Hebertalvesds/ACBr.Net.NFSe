using System.ServiceModel;

namespace ACBr.Net.NFSe.Providers.GinfesV2
{   
    [ServiceContract(Namespace = "https://producao.ginfes.com.br")]
    internal interface IGinfesV2ProdService
    {
        [OperationContract(Action = "", ReplyAction = "*")]
        [DataContractFormat(Style = OperationFormatStyle.Rpc)]
        [return: MessageParameter(Name = "return")]
        string ConsultarLoteRps([MessageParameter(Name = "arg0")]string cabecalho, [MessageParameter(Name = "arg1")]string dados);

        [OperationContract(Action = "", ReplyAction = "*")]
        [DataContractFormat(Style = OperationFormatStyle.Rpc)]
        [return: MessageParameter(Name = "return")]
        string RecepcionarLoteRps([MessageParameter(Name = "arg0")]string cabecalho, [MessageParameter(Name = "arg1")]string dados);
    }
}
