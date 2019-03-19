using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;

namespace Starline.SmartNota.Util
{
  #region class TWeb
  /// <summary>
  /// 
  /// </summary>
  public class TWeb
  {
    #region static string FetchRps(Uri url)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    public static string FetchRps(Uri url)
    {
      try
      {
        var webClient = new WebClient();
        byte[] bytesWebResponse = webClient.DownloadData(url);
        var webResponse = UTF8Encoding.UTF8.GetString(bytesWebResponse);
        return webResponse;
      }
      catch (WebException ex)
      {
        TLog.Write("WebException trying to fetch the rps at URL: '" + url + "' - WebException: " + ex);

        throw new Exception(
          "Oops!! O tempo limite de conexão ao serviço para carregar as RPSs expirou. Por favor, verifique sua internet e tente novamente..." +
          Environment.NewLine + "Erro: " + ex.Message, ex);
      }
      catch (Exception ex)
      {
        TLog.Write("Exception trying to fetch the rps at URL: '" + url + "' - Exception: " + ex);

        throw new ApplicationException(
          "Oops!! Ocorreu algum erro ao tentar conectar ao serviço para carregar as RPSs. Por favor, verifique sua internet e tente novamente..." +
          Environment.NewLine + "Erro: " + ex.Message, ex);
      }
    }
    #endregion

    #region static void PostRps(Uri url, string xml, string id)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="xml"></param>
    /// <param name="id"></param>
    public static void PostRps(Uri url, string xml, string id)
    {
      var parameters = new NameValueCollection
                         {
                           {"numeroLote", id},
                           {"xml", xml}
                         };

      try
      {
        var webClient = new WebClient();
        webClient.UploadValues(url, "POST", parameters);
      }
      catch (WebException ex)
      {
        TLog.Write("WebException trying to post the rps xml response at URL: '" + url + "' - xml: '" + xml + "' - WebException: " + ex);

        throw new ApplicationException(
          "Oops!! O tempo limite de conexão ao serviço de notificar o estado dos RPSs no Smart expirou. Por favor, verifique sua internet e tente novamente..." +
          Environment.NewLine + "Erro: " + ex.Message, ex);
      }
      catch(Exception ex)
      {
        TLog.Write("Exception trying to post the rps xml response at URL: '" + url + "' - xml: '" + xml + "' - Exception: " + ex);

        throw new ApplicationException(
          "Oops!! Ocorreu algum erro ao tentar conectar ao serviço de notificar o estado dos RPSs no Smart. Por favor, verifique sua internet e tente novamente..." +
          Environment.NewLine + "Erro: " + ex.Message, ex);
      }
    }
    #endregion

    #region static void PostFile(Uri url, string fileName)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="url"></param>
    /// <param name="fileName"></param>
    public static void PostFile(Uri url, string fileName)
    {
      try
      {
        var webClient = new WebClient();
        webClient.UploadFile(url, fileName);
      }
      catch (WebException ex)
      {
        TLog.Write("WebException trying to post the log file at URL: '" + url + "' - logFileName: '" + fileName + "' - WebException: " + ex);

        throw new ApplicationException(
          "Oops!! O tempo limite de conexão ao serviço para enviar o arquivo de log expirou. Por favor, verifique sua internet e tente novamente..." +
          Environment.NewLine + "Erro: " + ex.Message, ex);
      }
      catch (Exception ex)
      {
        TLog.Write("Exception trying to post the log file at URL: '" + url + "' - logFileName: '" + fileName + "' - Exception: " + ex);

        throw new ApplicationException(
          "Oops!! Ocorreu algum erro ao tentar conectar ao para enviar o arquivo de log. Por favor, verifique sua internet e tente novamente..." +
          Environment.NewLine + "Erro: " + ex.Message, ex);
      }
    }
    #endregion

    #region static bool TryParseUrl(string s, out Uri url)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    public static bool TryParseUrl(string s, out Uri url)
    {
      url = null;
      if (String.IsNullOrWhiteSpace(s))
      {
        return false;
      }

      try
      {
        var uri = new UriBuilder(s);
        url = uri.Uri;
        return true;
      }
      catch
      {
        return false;
      }
    }
    #endregion
  }
  #endregion
}
