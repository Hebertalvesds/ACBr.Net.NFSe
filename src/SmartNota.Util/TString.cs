using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Starline.SmartNota.Util
{
  /// <summary>
  /// 
  /// </summary>
  internal class TString
  {
    #region static XDocument[] ConvertToXArray(string str)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static XDocument[] ConvertToXArray(string str)
    {
      string requestXml;
      var xRequests = new List<XDocument>();
      var sr = new StringReader(str);

      while ((requestXml = sr.ReadLine()) != null)
      {
        if (String.IsNullOrWhiteSpace(requestXml)) continue;

        try
        {
          var xRequest = XDocument.Parse(requestXml);
          xRequests.Add(xRequest);
        }
        catch(Exception ex)
        {
          TLog.Write("Exception trying to load RPS to xml - exception: " + ex);
        }
      }

      return xRequests.ToArray();
    }
    #endregion
  }
}
