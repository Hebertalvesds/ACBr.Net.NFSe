namespace ACBr.Net.NFSe.Providers.Pronim
{
    class PronimHomoServiceClient : NFSeServiceClient<IPronimHomoService>, IPronimHomoService, IPronimServiceClient
    {
        #region Constructors
        public PronimHomoServiceClient(ProviderPronim provider, TipoUrl tipoUrl) : base(provider, tipoUrl) { }
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
    }
}
