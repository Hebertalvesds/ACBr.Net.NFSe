using System;

namespace ACBr.Net.NFSe.Providers.Vitoria
{
    internal interface IVitoriaServiceCliente : IDisposable
    {
        string RecepcionarLoteRps(string cabecalho, string dados);
        string ConsultarLoteRps(string cabecalho, string dados);
    }
}
