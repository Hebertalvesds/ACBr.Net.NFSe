using System.Security.Cryptography.X509Certificates;
using System.ServiceModel.Security;
namespace ACBr.Net.NFSe.Providers.Pronim
{
    class PronimHomoServiceClient : NFSeServiceClient<IPronimHomoService>, IPronimHomoService, IPronimServiceClient
    {
        #region Constructors
        public PronimHomoServiceClient(ProviderPronim provider, TipoUrl tipoUrl) : base(provider, tipoUrl) {
            ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = X509CertificateValidationMode.PeerOrChainTrust;
            ClientCredentials.ServiceCertificate.Authentication.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.Offline;

        }
        #endregion Constructors

        #region Methods
        public string RecepcionarLoteRps(string cabecalho, string dados)
        {
            return ((IPronimHomoService)this).RecepcionarLoteRps(cabecalho, dados);
        }

        #endregion Methods

        #region Interface Methods
        string IPronimHomoService.RecepcionarLoteRps(string arg0, string arg1)
        {
            return Channel.RecepcionarLoteRps(arg0, arg1);
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
