using System.Security.Cryptography.X509Certificates;
using System.ServiceModel;
using System.ServiceModel.Security;

namespace ACBr.Net.NFSe.Providers.Pronim
{
    class PronimHomoServiceClient : NFSeServiceClient<IPronimHomoService>, IPronimHomoService, IPronimServiceClient
    {
        #region Constructors
        public PronimHomoServiceClient(ProviderPronim provider, TipoUrl tipoUrl) : base(provider, tipoUrl) {
            ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerOrChainTrust;
            ClientCredentials.ServiceCertificate.Authentication.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.Offline;

            //Endpoint.Binding = CustomBinding(Endpoint.Binding);
        }
        #endregion Constructors

        #region Methods

        private BasicHttpBinding CustomBinding(System.ServiceModel.Channels.Binding bindingOverride)
        {

            BasicHttpBinding cb = new BasicHttpBinding(BasicHttpSecurityMode.Transport);

            cb.Name = bindingOverride.Name;
            cb.Namespace = bindingOverride.Namespace;
            cb.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.Certificate;
            cb.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            
            return cb;

        }

        public string RecepcionarLoteRps(string cabecalho, string xmlEnvio)
        {
            return ((IPronimHomoService)this).RecepcionarLoteRps(cabecalho, xmlEnvio);
        }

        public string ConsultarLoteRps(string cabecalho, string xmlConsulta)
        {
            return ((IPronimHomoService)this).ConsultarLoteRps(cabecalho, xmlConsulta);
        }

        #endregion Methods

        #region Interface Methods
        string IPronimHomoService.RecepcionarLoteRps(string cabecalho, string xmlEnvio)
        {
            return Channel.RecepcionarLoteRps(cabecalho, xmlEnvio);
        }

        string IPronimHomoService.ConsultarLoteRps(string cabecalho, string xmlConsulta)
        {
            return Channel.ConsultarLoteRps(cabecalho, xmlConsulta);
        }
        #endregion Interface Methods

        public static bool ValidateServerCertificate(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
                return true;
            throw new System.Exception(sslPolicyErrors.ToString());
            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }
    }
}
