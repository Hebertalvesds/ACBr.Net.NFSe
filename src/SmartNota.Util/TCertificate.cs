using System;
using System.Security.Cryptography.X509Certificates;

namespace Starline.SmartNota.Util
{
  #region class TCertificate
  /// <summary>
  /// 
  /// </summary>
  public class TCertificate
  {
    #region static X509Certificate2 PickCertificate()
    /// <summary>
    /// Displays a dialog box for selecting an X.509 certificate from a certificate collection.
    /// </summary>
    /// <returns>An X509Certificate2 object that contains the selected certificate.</returns>
    public static X509Certificate2 PickCertificate()
    {
      const string TITLE = "Starline.SmartNota - Escolha o seu certificado digital";
      const string MESSAGE = "Escolha um certificado digital para ser usado na assinatura e geração da Nota Fiscal e na comunicação com a prefeitura da sua cidade.";

      var certificate = PickCertificate(TITLE, MESSAGE);

      return certificate;
    }
    #endregion

    #region static X509Certificate2 PickCertificate(string title, string message)
    /// <summary>
    /// Displays a dialog box for selecting an X.509 certificate from a certificate collection.
    /// </summary>
    /// <param name="title">The title of the dialog box.</param>
    /// <param name="message">A descriptive message to guide the user. The message is displayed in the dialog box.</param>
    /// <returns>An X509Certificate2 object that contains the selected certificate.</returns>
    public static X509Certificate2 PickCertificate(string title, string message)
    {
      var x509Store = new X509Store(StoreName.My, StoreLocation.CurrentUser);

      try
      {
        x509Store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

        var certificates = X509Certificate2UI.SelectFromCollection(x509Store.Certificates, title, message,
                                                                   X509SelectionFlag.SingleSelection);

        if (certificates.Count == 0)
        {
          throw new NullReferenceException(
            "Oops!! Certificado não informado... Não será possível emitir notas fiscais sem um certificado válido.");
        }

        var certificate = certificates[0];

        return certificate;
      }
      finally
      {
        x509Store.Close();
      }
    }
    #endregion
  }
  #endregion
}
