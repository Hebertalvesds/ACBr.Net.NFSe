using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace ACBr.Net.NFSe.Providers.Vitoria
{
    class VitoriaHomServiceCliente : NFSeServiceClient<IVitoriaHomService>, IVitoriaHomService, IVitoriaServiceCliente
    {
        #region Constructor
        public VitoriaHomServiceCliente(ProviderVitoria provider, TipoUrl tipoUrl) : base (provider, tipoUrl)
        {

        }
        #endregion

        #region Methods
        public string RecepcionarLoteRps(string cabecalho, string xmlEnvio)
        {
            return ((IVitoriaHomService)this).RecepcionarLoteRps(cabecalho, xmlEnvio);
        }

        public string ConsultarLoteRps(string cabecalho, string xmlConsulta)
        {
            return ((IVitoriaHomService)this).ConsultarLoteRps(cabecalho, xmlConsulta);
        }
        #endregion Methods

        #region InterfaceMethods
        string IVitoriaHomService.RecepcionarLoteRps(string cabecalho, string xmlEnvio)
        {
            return Channel.RecepcionarLoteRps(cabecalho, xmlEnvio);
        }

        string IVitoriaHomService.ConsultarLoteRps(string cabecalho, string xmlConsulta)
        {
            return Channel.ConsultarLoteRps(cabecalho, xmlConsulta);
        }
        #endregion
    }
}
