using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace Starline.SmartNota.Util
{
  #region TXDocument
  /// <summary>
  /// 
  /// </summary>
  internal static class TXDocument
  {
    #region static string ToStringWithDeclaration(this XDocument doc)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="doc"></param>
    /// <returns></returns>
    public static string ToStringWithDeclaration(this XDocument doc)
    {
      if (doc == null)
      {
        throw new ArgumentException("doc");
      }

      var builder = new StringBuilder();
      using (TextWriter writer = new StringWriter(builder))
      {
        doc.Save(writer);
      }

      var xml = builder.ToString();
      xml = xml.Replace("utf-16", "utf-8");

      return xml;
    }
    #endregion
  }
  #endregion
}
