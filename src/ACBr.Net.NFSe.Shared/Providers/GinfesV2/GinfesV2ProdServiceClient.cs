using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;

namespace ACBr.Net.NFSe.Providers.GinfesV2
{
    internal sealed class GinfesV2ProdServiceClient : NFSeServiceClient<IGinfesV2ProdService>, IGinfesV2ProdService, IGinfesV2ServiceClient
    {
        public GinfesV2ProdServiceClient(ProviderGinfesV2 provider, TipoUrl tipoUrl) : base(provider, tipoUrl) { }

        public string ConsultarLoteRps([MessageParameter(Name = "arg0")] string cabecalho, [MessageParameter(Name = "arg1")] string dados)
        {
            return ((IGinfesV2ProdService)this).ConsultarLoteRps(cabecalho, dados);
        }

        public string RecepcionarLoteRps([MessageParameter(Name = "arg0")] string cabecalho, [MessageParameter(Name = "arg1")] string dados)
        {
            return ((IGinfesV2ProdService)this).RecepcionarLoteRps(cabecalho, dados);
        }
    }
}
