namespace ACBr.Net.NFSe.Providers.Pronim
{
    class PronimProdServiceClient : NFSeServiceClient<IPronimProdService>, IPronimProdService, IPronimServiceClient
    {
        #region Constructors
        public PronimProdServiceClient(ProviderPronim provider, TipoUrl tipoUrl) : base(provider, tipoUrl) { }
        #endregion Constructors

        #region Methods
        public string RecepcionarLoteRps(string cabecalho, string dados)
        {
            return ((IPronimProdService)this).RecepcionarLoteRps(cabecalho, dados);
        }

        #endregion Methods

        #region Interface Methods
        string IPronimProdService.RecepcionarLoteRps(string arg0, string arg1)
        {
            return Channel.RecepcionarLoteRps(arg0, arg1);
        }
        #endregion Interface Methods
    }
}
