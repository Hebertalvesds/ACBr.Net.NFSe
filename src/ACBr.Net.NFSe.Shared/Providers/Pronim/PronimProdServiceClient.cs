using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace ACBr.Net.NFSe.Providers.Pronim
{
    class PronimProdServiceClient : NFSeServiceClient<IPronimProdService>, IPronimProdService, IPronimServiceClient
    {
        
        #region Constructors
        public PronimProdServiceClient(ProviderPronim provider, TipoUrl tipoUrl) : base(provider, tipoUrl)
        {
            Endpoint.Binding = CustomBinding(Endpoint.Binding);
        }
        #endregion Constructors

        private BasicHttpBinding CustomBinding(System.ServiceModel.Channels.Binding bindingOverride)
        {
            
            BasicHttpBinding cb = new BasicHttpBinding(BasicHttpSecurityMode.None);
            
            cb.Name = bindingOverride.Name;
            cb.Namespace = bindingOverride.Namespace;
            cb.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.Certificate;
            
            return cb;
            
        }

        #region Fields
        #endregion Fields

        #region Methods
        public string RecepcionarLoteRps(string cabecalho, string xmlEnvio)
        {
            return ((IPronimProdService)this).RecepcionarLoteRps(cabecalho, xmlEnvio);
        }

        public string ConsultarLoteRps(string cabecalho, string xmlConsulta)
        {
            return ((IPronimProdService)this).RecepcionarLoteRps(cabecalho, xmlConsulta);
        }
        #endregion Methods

        #region Interface Methods
        string IPronimProdService.RecepcionarLoteRps(string cabecalho, string xmlEnvio)
        {
            return Channel.RecepcionarLoteRps(cabecalho, xmlEnvio);
        }

        string IPronimProdService.ConsultarLoteRps(string cabecalho, string xmlConsulta)
        {
            return Channel.ConsultarLoteRps(cabecalho, xmlConsulta);
        }
        #endregion Interface Methods
    }
}
