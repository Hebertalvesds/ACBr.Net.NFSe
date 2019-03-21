using System;

namespace ACBr.Net.NFSe.Providers.Pronim
{
    internal interface IPronimServiceClient : IDisposable
    {
        string RecepcionarLoteRps(string cabecalho, string dados);
        string ConsultarLoteRps(string cabecalho, string dados);
    }
}
