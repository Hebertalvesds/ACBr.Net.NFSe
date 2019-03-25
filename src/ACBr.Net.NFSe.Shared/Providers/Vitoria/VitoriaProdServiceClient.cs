using System;
using System.Collections.Generic;
using System.Text;

namespace ACBr.Net.NFSe.Providers.Vitoria
{
    class VitoriaProdServiceClient : NFSeServiceClient<IVitoriaProdService>, IVitoriaProdService, IVitoriaServiceCliente
    {
        #region Constructor
        public VitoriaProdServiceClient(ProviderVitoria provider, TipoUrl tipoUrl) : base(provider, tipoUrl)
        {

        }
        #endregion

        #region Methods
        public string RecepcionarLoteRps(string cabecalho, string xmlEnvio)
        {
            return ((IVitoriaProdService)this).RecepcionarLoteRps(cabecalho, xmlEnvio);
        }

        public string ConsultarLoteRps(string cabecalho, string xmlConsulta)
        {
            return ((IVitoriaProdService)this).ConsultarLoteRps(cabecalho, xmlConsulta);
        }
        #endregion Methods

        #region InterfaceMethods
        string IVitoriaProdService.RecepcionarLoteRps(string cabecalho, string xmlEnvio)
        {
            return Channel.RecepcionarLoteRps(cabecalho, xmlEnvio);
        }

        string IVitoriaProdService.ConsultarLoteRps(string cabecalho, string xmlConsulta)
        {
            return Channel.ConsultarLoteRps(cabecalho, xmlConsulta);
        }
        #endregion
    }
}
