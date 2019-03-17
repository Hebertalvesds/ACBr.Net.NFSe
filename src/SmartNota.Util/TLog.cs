using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Starline.SmartNota.Util
{
  #region class TLog
  /// <summary>
  /// 
  /// </summary>
  public class TLog
  {
    #region Fields

    /// <summary>
    /// 
    /// </summary>
    private static bool FPathChecked;

    /// <summary>
    /// 
    /// </summary>
    private static string FLogFileNameFormattedAux;

    #endregion

    #region static string FLogFileNameFormatted
    /// <summary>
    /// 
    /// </summary>
    private static string FLogFileNameFormatted
    {
      get
      {
        if (FLogFileNameFormattedAux == null)
        {
          FLogFileNameFormattedAux = FormatLogFileName(TSettings.LogFileName);
        }

        return FLogFileNameFormattedAux;
      }
    }
    #endregion

    #region static void CheckPath()
    /// <summary>
    /// 
    /// </summary>
    private static void CheckPath()
    {
      if  (!FPathChecked)
      {
        Directory.CreateDirectory(Path.GetDirectoryName(FLogFileNameFormatted));
        FPathChecked = true;
      }
    }
    #endregion

    #region static void Write(string text)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    public static void Write(string text)
    {
      CheckPath();

      if (!TSettings.LogEnabled)
      {
        return;
      }

      var textToLog = new StringBuilder();
      textToLog.Append(DateTime.Now.ToString("yyMMddHHmmss") + '\t');
      textToLog.Append(text.Replace(Environment.NewLine, " "));
      textToLog.Append(Environment.NewLine);

      File.AppendAllText(FLogFileNameFormatted, textToLog.ToString());
    }
    #endregion

    #region static string FormatLogFileName(string logFileName)
    /// <summary>
    /// 
    /// </summary>
    /// <param name="logFileName"></param>
    /// <returns></returns>
    public static string FormatLogFileName(string logFileName)
    {
      var formatedLogFileName = logFileName;
      var dateTime = DateTime.Now;

      formatedLogFileName = formatedLogFileName.Replace("yy", dateTime.ToString("yy"));
      formatedLogFileName = formatedLogFileName.Replace("MM", dateTime.ToString("MM"));
      formatedLogFileName = formatedLogFileName.Replace("dd", dateTime.ToString("dd"));

      return formatedLogFileName;
    }
    #endregion
  }
  #endregion
}
