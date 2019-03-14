using System;
using System.Collections.Generic;
using System.Text;

namespace ACBr.Net.NFSe.Providers.GinfesV2
{
    internal sealed class GinfesV2HomoServiceClient : NFSeServiceClient<IGinfesV2HomoService>, IGinfesV2HomoService, IGinfesV2ServiceClient
    {
        #region Constructors
        public GinfesV2HomoServiceClient(ProviderGinfesV2 provider, TipoUrl tipoUrl) : base(provider, tipoUrl) { }
        #endregion Constructors
        public string ConsultarLoteRps(string cabecalho, string dados)
        {
            return ((IGinfesV2HomoService)this).ConsultarLoteRps(cabecalho, dados);
        }

        public string RecepcionarLoteRps(string cabecalho, string dados)
        {
            return ((IGinfesV2HomoService)this).RecepcionarLoteRps(cabecalho, dados);
        }
    }
}
