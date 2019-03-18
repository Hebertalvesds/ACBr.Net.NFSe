using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml.Linq;
using Starline.SmartNota.Util.NFSe.WS.SJC.PedidoEnvioRPS;
using Starline.SmartNota.Util.NFSe.WS.SJC.PedidoConsultaRPSNumero;

namespace Starline.SmartNota.Util
{
  #region class TRps
  /// <summary>
  /// 
  /// </summary>
  public class TRps
  {
    #region Properties

    /// <summary>
    /// 
    /// </summary>
    public XDocument Rps { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public string FRpsSent { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public string Place { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string ID { get; set; }

    /// <summary>
    /// 
    /// </summary>
    private XNamespace FDefaultNamespace;

    #endregion

    #region Constructor
    /// <summary>
    /// 
    /// </summary>
    /// <param name="rps"></param>
    public TRps(XDocument rps)
    {
      FDefaultNamespace = TSettings.DefaultNamespace;
      SetProperties(rps);
    }
    #endregion

    #region void SetProperties(XDocument rps)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="rps"></param>
    private void SetProperties(XDocument rps)
    {
      if (rps == null)
      {
        return;
      }

      Rps = rps;

      SetPlace();
      SetID();
      SetName();
    }
    #endregion

    #region void SetPlace()
    /// <summary>
    /// 
    /// </summary>
    private void SetPlace()
    {
      Place = Rps.Descendants(FDefaultNamespace + "NumeroLoteSJC").Count() > 0 ? "SJC" : String.Empty;
    }
    #endregion

    #region void SetName()
    /// <summary>
    /// 
    /// </summary>
    private void SetName()
    {
      var name = String.Empty;

      try
      {
        switch (Place)
        {
          case "SJC":
            name = Rps.Descendants(FDefaultNamespace + "Tomador").Descendants(FDefaultNamespace + "Nome").First().Value;
            break;
          default:
            name = String.Empty;
            break;
        }
      }
      catch{}

      Name = name;
    }
    #endregion

    #region void SetID()
    /// <summary>
    /// 
    /// </summary>
    private void SetID()
    {
      var id = String.Empty;

      try
      {
        switch (Place)
        {
          case "SJC":
            id = Rps.Descendants(FDefaultNamespace + "NumeroLoteSJC").First().Value;
            Rps.Descendants(FDefaultNamespace + "NumeroLoteSJC").Remove();
            break;
          default:
            id = String.Empty;
            break;
        }
      }
      catch { }

      ID = id;
    }
    #endregion

    #region bool TryProcess(Uri responseUrl, X509Certificate2 certificate, out string message)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="responseUrl"></param>
    /// <param name="certificate"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public bool TryProcess(Uri responseUrl, X509Certificate2 certificate,  out string message)
    {
      SetRpsNumber();

      var rpsResponse = SendRps(certificate);
      var success = ParseXResponse(rpsResponse, out message);
      var responseUri = GetResponseUri(responseUrl, success);

      PostRpsResponse(responseUri, rpsResponse);

      return success;
    }
    #endregion

    #region Uri GetResponseUri(Uri responseUrl, bool success)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="responseUrl"></param>
    /// <param name="success"></param>
    /// <returns></returns>
    private Uri GetResponseUri(Uri responseUrl, bool success)
    {
      var responseUri = new UriBuilder(responseUrl);

      var query = responseUri.Query;
      if (!String.IsNullOrWhiteSpace(query))
      {
        query = responseUri.Query.Substring(1) + "&";
      }
      query = query + "numeroLote=" + ID + "&sucesso=" + success;

      responseUri.Query = query;

      return responseUri.Uri;
    }
    #endregion

    #region void SetRpsNumber()
    /// <summary>
    /// 
    /// </summary>
    private void SetRpsNumber()
    {
      var rpsNumber = RequestNextRpsNumber();

      var numberNodes = Rps.Descendants(FDefaultNamespace + "Numero");
      if (numberNodes.Count() == 0)
      {
        TLog.Write("Xml tag 'numero' not found at Smart RPS - xml: " + Rps.ToString(SaveOptions.DisableFormatting));
        throw new ApplicationException("O xml do Smart com o RPS para fazer a conversão para nota fiscal está inválido. Tag 'numero' não encontrada.");
      }

      var numberNode = numberNodes.First();
      numberNode.SetValue(rpsNumber);
    }
    #endregion

    #region int RequestNextRpsNumber()
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private int RequestNextRpsNumber()
    {
      var service = new PedidoConsultaRPSNumeroService();
      var response = service.PedidoConsultaRPSNumero(TSettings.User, TSettings.Password);
      var number = ExtractRpsNumber(response);
      //var number = 1;

      return number;
    }
    #endregion

    #region string RequestNextRpsNumber(string user, string password)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="user"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static string RequestNextRpsNumber(string user, string password)
    {
      var service = new PedidoConsultaRPSNumeroService();
      var response = service.PedidoConsultaRPSNumero(user, password);

      return response;
    }
    #endregion

    #region int ExtractRpsNumber(string xml)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="xml"></param>
    /// <returns></returns>
    private int ExtractRpsNumber(string xml)
    {
      XDocument xDocument;

      if (String.IsNullOrWhiteSpace(xml))
      {
        TLog.Write("ConsultaRPSNumero xml is blank.");
        throw new ApplicationException("O serviço de buscar o número do próximo RPS retornou branco.");
      }

      try
      {
        xDocument = XDocument.Parse(xml);
      }
      catch (Exception ex)
      {
        TLog.Write("Exception trying to parse the ConsultaRPSNumero xml response - xml: " + xml + " - Exception: " + ex + "");
        throw new ApplicationException("O serviço de buscar o número do próximo RPS retornou um XML inválido.");
      }

      var successNode = xDocument.Descendants("Sucesso");
      if (successNode.Count() == 0)
      {
        TLog.Write("Xml tag 'Sucesso' not found at ConsultaRPSNumero xml response - xml: " + xml);
        throw new ApplicationException("O serviço de buscar o número do próximo RPS retornou um XML inválido. Tag 'Sucesso' não encontrada.");
      }

      if (successNode.First().Value == "false")
      {
        var errorMessage = String.Empty;

        var messageNode = xDocument.Descendants("Descricao");
        if (messageNode.Count() > 0)
        {
          errorMessage = messageNode.First().Value;
        }

        TLog.Write("The ConsultaRPSNumero response xml, returned false - xml: " + xml);
        throw new ApplicationException("O serviço de buscar o número do próximo RPS retornou o erro: '" + errorMessage + "'");
      }

      var numberNode = xDocument.Descendants("Numero");
      if (numberNode.Count() == 0)
      {
        TLog.Write("Xml tag 'Numero' not found at ConsultaRPSNumero xml response - xml: " + xml);
        throw new ApplicationException("O serviço de buscar o número do próximo RPS retornou um XML inválido. Tag 'Numero' não encontrada.");
      }

      int number;
      var numberValue = numberNode.First().Value;

      if (String.IsNullOrWhiteSpace(numberValue))
      {
        TLog.Write("The ConsultaRPSNumero xml response, returned a blank value - xml: " + xml);
        throw new ApplicationException("O serviço de buscar o número do próximo RPS retornou um valor em branco.");
      }

      if (!int.TryParse(numberValue, out number))
      {
        TLog.Write("The ConsultaRPSNumero xml response, returned an invalid number - xml: " + xml);
        throw new ApplicationException("O serviço de buscar o número do próximo RPS retornou um número inválido. Numero: " + numberValue);
      }

      return number;
    }
    #endregion

    #region bool ParseXResponse(string xmlResponse, out string message)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="xmlResponse"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private bool ParseXResponse(string xmlResponse, out string message)
    {
      XDocument xDocument;

      if (String.IsNullOrWhiteSpace(xmlResponse))
      {
        message = "O serviço de converter o RPS em nota fiscal retornou branco.";
        TLog.Write("PedidoEnvioRPS xml response is blank. - xml sent: " + FRpsSent);
        return false;
      }

      try
      {
        xDocument = XDocument.Parse(xmlResponse);
      }
      catch (Exception ex)
      {
        message = "O serviço de converter o RPS em nota fiscal retornou um XML inválido.";
        TLog.Write("Exception trying to parse the PedidoEnvioRPS xml response - xml: " + xmlResponse + " - xml sent: " + FRpsSent + " - Exception: " + ex + "");
        return false;
      }

      var successNode = xDocument.Descendants("Sucesso");
      if (successNode.Count() == 0)
      {
        message = "O serviço de converter o RPS em nota fiscal retornou um XML inválido. Tag 'Sucesso' não encontrada.";
        TLog.Write("Xml tag 'Sucesso' not found at PedidoEnvioRPS xml response - xml: " + xmlResponse + " - xml sent: " + FRpsSent);
        return false;
      }

      if (successNode.First().Value == "false")
      {
        var errorMessage = String.Empty;

        var messageNode = xDocument.Descendants("Descricao");
        if (messageNode.Count() > 0)
        {
          errorMessage = messageNode.First().Value;
        }

        message = "O serviço de converter o RPS em nota fiscal retornou um XML retornou o erro: '" + errorMessage + "'";
        TLog.Write("The PedidoEnvioRPS response xml, returned false - xml: " + xmlResponse + " - xml sent: " + FRpsSent);
        return false;
      }

      var nfseNode = xDocument.Descendants("ChaveNFe");
      if (nfseNode.Count() == 0)
      {
        message = "O serviço de converter o RPS em nota fiscal retornou um XML inválido. Tag 'ChaveNFe' não encontrada.";
        TLog.Write("Xml tag 'ChaveNFe' not found at PedidoEnvioRPS xml response - xml: " + xmlResponse + " - xml sent: " + FRpsSent);
        return false;
      }

      var numberNode = nfseNode.Descendants("Numero");
      if (numberNode.Count() == 0)
      {
        message = "O serviço de converter o RPS em nota fiscal retornou um XML inválido. Tag 'Numero' não encontrada.";
        TLog.Write("Xml tag 'Numero' not found at PedidoEnvioRPS xml response - xml: " + xmlResponse + " - xml sent: " + FRpsSent);
        return false;
      }

      int number;
      var numberValue = numberNode.First().Value;

      if (String.IsNullOrWhiteSpace(numberValue))
      {
        message = "O serviço de converter o RPS em nota fiscal retornou um valor em branco.";
        TLog.Write("The PedidoEnvioRPS xml response, returned a blank value - xml: " + xmlResponse + " - xml sent: " + FRpsSent);
        return false;
      }

      if (!int.TryParse(numberValue, out number))
      {
        message = "O serviço de converter o RPS em nota fiscal retornou um número inválido para a nota fiscal. Numero: " + numberValue;
        TLog.Write("The PedidoEnvioRPS xml response, returned an invalid number ChaveNFe/Numero - xml: " + xmlResponse + " - xml sent: " + FRpsSent);
        return false;
      }

      message = number.ToString();
      return true;
    }
    #endregion

    #region string SendRps(X509Certificate2 certificate)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="certificate"></param>
    /// <returns></returns>
    private string SendRps(X509Certificate2 certificate)
    {
      var FRpsSent = TSJCSigner.Sign(FDefaultNamespace + "RPS", Rps, certificate);
      var service = new PedidoEnvioRPSService();
      var response = service.PedidoEnvioRPS(TSettings.User, TSettings.Password, FRpsSent);

      return response;
    }
    #endregion

    #region void PostRpsResponse(Uri responseUrl, string xml)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="responseUrl"></param>
    /// <param name="xml"></param>
    private void PostRpsResponse(Uri responseUrl, string xml)
    {
      TWeb.PostRps(responseUrl, xml, ID);
    }
    #endregion

    #region static string Assina(XName Tag, X509Certificate2 certificado, XDocument Rps)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="responseUrl"></param>
    /// <param name="xml"></param>
    public static string Assina(XName Tag, X509Certificate2 certificado, XDocument Rps)
    {
        var rpsSigned = TSJCSigner.Sign(Tag, Rps, certificado);
        return rpsSigned.ToString();
    }
    public static string AssinaTag(XName Tag, X509Certificate2 certificado, XDocument Rps)
    {
        var rpsSigned = TSJCSigner.SignTag(Tag, Rps, certificado);
        return rpsSigned.ToString();
    }
        #endregion

        #region static TRps[] Load(Uri requestUrl)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requestUrl"></param>
        /// <returns></returns>
        public static TRps[] Load(Uri requestUrl)
    {
      var rps = new List<TRps>();
      var response = TWeb.FetchRps(requestUrl);

      if (String.IsNullOrWhiteSpace(response))
      {
        return rps.ToArray();
      }

      var xRequests = TString.ConvertToXArray(response);

      rps.AddRange(xRequests.Select(request => new TRps(request)));

      return rps.ToArray();
    }
    #endregion
  }
  #endregion
}