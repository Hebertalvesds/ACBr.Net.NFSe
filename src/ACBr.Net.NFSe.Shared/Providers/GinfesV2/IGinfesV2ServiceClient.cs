using System;

namespace ACBr.Net.NFSe.Providers.GinfesV2
{
    internal interface IGinfesV2ServiceClient : IDisposable
    {
        string ConsultarLoteRps(string cabecalho, string dados);

        string RecepcionarLoteRps(string cabeçalho, string dados);

    }
}
