#region usings

using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Collections.Generic;

#endregion

namespace Starline.SmartNota.Util
{
  #region class TSJCSigner
  /// <summary>
  /// 
  /// </summary>
  internal class TSJCSigner
  {
    #region Constants

    private const LoadOptions SafeLoadOptions = LoadOptions.PreserveWhitespace;
    private const SaveOptions SafeSaveOptions = SaveOptions.DisableFormatting;

    #endregion

    #region static XDocument Sign(XName tag, XDocument document, X509Certificate2 certificate)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="document"></param>
    /// <param name="certificate"></param>
    /// <returns></returns>
    public static string Sign(XName tag, XDocument document, X509Certificate2 certificate)
    {
        XmlDocument doc = new XmlDocument();


        doc.LoadXml(document.ToString());


        // Create an XML declaration. 
        XmlDeclaration xmldecl;
        xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
        xmldecl.Encoding = "UTF-8";
        xmldecl.Standalone = "yes";

        // Add the new node to the document.
        XmlElement root = doc.DocumentElement;
        doc.InsertBefore(xmldecl, root);

        var signature = GetSignature(doc, certificate);


        XmlElement XmlDigitalSignature = signature.GetXml();

        doc.DocumentElement.AppendChild(doc.ImportNode(XmlDigitalSignature, true));

        return doc.InnerXml;
            
    }
        #endregion

        #region static XDocument SignTag(XName tag, XDocument document, X509Certificate2 certificate)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="document"></param>
        /// <param name="certificate"></param>
        /// <returns></returns>
        public static string SignTag(XName tag, XDocument document, X509Certificate2 certificate)
        { 
            var tagsToSign = "";
            XElement xtagsToSignParent = null;
            XElement xtagsToSign = null;

            foreach (XElement element in document.Descendants(tag))
            {
                tagsToSign = element.ToString();
                xtagsToSign = element;
                xtagsToSignParent = element.Parent;
            }

            if(string.IsNullOrEmpty(tagsToSign))
            {
                throw new Exception(string.Format("Tag '{0}' não encontrada", tagsToSign.ToString()));
            }

            xtagsToSign.Remove();

            XmlDocument doc = new XmlDocument();
            
            doc.LoadXml(tagsToSign.ToString());

            // Create an XML declaration. 
            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
            xmldecl.Encoding = "UTF-8";
            xmldecl.Standalone = "yes";

            // Add the new node to the document.
            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmldecl, root);

            var signature = GetSignature(doc, certificate);

            XmlElement XmlDigitalSignature = signature.GetXml();

            var docOriginal = doc.OuterXml;
            var docAss = XmlDigitalSignature.OuterXml;
            var docOriginalAss = docOriginal + docAss;

            XmlNode assignedNode = doc.ImportNode(XmlDigitalSignature, true);

            doc.DocumentElement.AppendChild(assignedNode);

            //xtagsToSignParent.Add(XElement.Parse(doc.DocumentElement.OuterXml));

            xtagsToSignParent.Add(XElement.Parse(docOriginal));
            xtagsToSignParent.Add(XElement.Parse(docAss));

            string textDocument = document.ToString();
            return textDocument;
            //return document.ToString().Replace(xtagsToSign.ToString(), assignedNode.ToString());

            //return doc.InnerXml;

        }
        #endregion


        private static XmlNamespaceManager GetNameSpaceManager(XmlDocument xDoc) { XmlNamespaceManager nsm = new XmlNamespaceManager(xDoc.NameTable); XPathNavigator RootNode = xDoc.CreateNavigator(); RootNode.MoveToFollowing(XPathNodeType.Element); IDictionary<string, string> NameSpaces = RootNode.GetNamespacesInScope(XmlNamespaceScope.All); foreach (KeyValuePair<string, string> kvp in NameSpaces) { nsm.AddNamespace(kvp.Key, kvp.Value); } return nsm; }

        #region static SignedXml GetSignature(XElement originalDocument, X509Certificate2 certificate)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="originalDocument"></param>
        /// <param name="certificate"></param>
        /// <returns></returns>
        private static SignedXml GetSignature(XmlDocument document, X509Certificate2 certificate)
    {      
      if (document.DocumentElement == null)
      {
        throw new InvalidOperationException("Invalid XML document; no root element found.");
      }
      
      var signedXml = new SignedXml(document);
      //signedXml.SignedInfo.CanonicalizationMethod = SignedXml.XmlDsigC14NTransformUrl;
      signedXml.SigningKey = certificate.PrivateKey;
      signedXml.KeyInfo = GetCertificateKeyInfo(certificate);
      signedXml.AddReference(GetSignatureReference(document));

      signedXml.ComputeSignature();

      return signedXml;
    }
    #endregion

    #region static XmlDocument GetXmlDocument(XNode originalDocument)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="originalDocument"></param>
    /// <returns></returns>
    private static XmlDocument GetXmlDocument(XNode originalDocument)
    {
      var document = new XmlDocument
      {
        PreserveWhitespace = false
      };

      document.LoadXml(originalDocument.ToString(SafeSaveOptions));

      return document;
    }
    #endregion

    #region static KeyInfo GetCertificateKeyInfo(X509Certificate certificate)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="certificate"></param>
    /// <returns></returns>
    private static KeyInfo GetCertificateKeyInfo(X509Certificate certificate)
    {
      var certificateKeyInfo = new KeyInfo();
      certificateKeyInfo.AddClause(new KeyInfoX509Data(certificate));

      return certificateKeyInfo;
    }
    #endregion

    #region static Reference GetSignatureReference()
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private static Reference GetSignatureReference(XmlDocument document)
    {
            Reference signatureReference;

            //if ((document.OuterXml.ToString().Contains("00203629000562") || document.OuterXml.ToString().Contains("00203629000309")) && document.OuterXml.ToString().Contains("LoteRps"))
            //{

                var tagsToSign = document.GetElementsByTagName("LoteRps");
                if(tagsToSign.Count == 0)
                {
                    tagsToSign = document.GetElementsByTagName("InfDeclaracaoPrestacaoServico");
                }   
                var id = "";

                foreach (XmlElement infNFe in tagsToSign)
                {
                    if (infNFe.HasAttribute("id"))
                        id = infNFe.Attributes.GetNamedItem("id").Value;
                    else if (infNFe.HasAttribute("Id"))
                        id = infNFe.Attributes.GetNamedItem("Id").Value;
                    else if (infNFe.HasAttribute("ID"))
                        id = infNFe.Attributes.GetNamedItem("ID").Value;
                    else if (infNFe.HasAttribute("iD"))
                        id = infNFe.Attributes.GetNamedItem("iD").Value;
                    else
                        throw new Exception("Tag não tem atributo Id");
                }

               signatureReference = new Reference("#" + id);
            //}
            //else
            //{
            //   signatureReference = new Reference("");

            //}



      signatureReference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
      signatureReference.AddTransform(new XmlDsigC14NTransform());

      return signatureReference;
    }
    #endregion

    #region static void AddSignature(XElement element, SignedXml signedXml)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="element"></param>
    /// <param name="signedXml"></param>
    private static void AddSignature(XElement element, SignedXml signedXml)
    {
      var signatureXml = signedXml.GetXml().OuterXml;
      var signatureElement = XElement.Parse(signatureXml, SafeLoadOptions);

      if (element.Parent != null)
      {
        element.Parent.Add(signatureElement);
      }
    }
    #endregion
  }
  #endregion
}