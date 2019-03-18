#region usings

using System;
using Starline.SmartNota.Util.Properties;

#endregion

namespace Starline.SmartNota.Util
{
  #region class TSettings
  /// <summary>
  /// </summary>
  public class TSettings
  {
    #region static bool LogEnabled
    /// <summary>
    /// </summary>
    public static bool LogEnabled
    {
      get { return Settings.Default.LogEnabled; }
      set
      {
        Settings.Default.LogEnabled = value;
        SaveSettings();
      }
    }
    #endregion

    #region static string LogFileName
    /// <summary>
    /// </summary>
    public static string LogFileName
    {
      get { return Settings.Default.LogFileName; }
      set
      {
        Settings.Default.LogFileName = value;
        SaveSettings();
      }
    }
    #endregion

    #region static string Password
    /// <summary>
    /// </summary>
    public static string Password
    {
      get { return Settings.Default.Password; }
      set
      {
        Settings.Default.Password = value;
        SaveSettings();
      }
    }
    #endregion

    #region static string UrlRequest
    /// <summary>
    /// </summary>
    public static string UrlRequest
    {
      get { return Settings.Default.UrlRequest; }
      set
      {
        Settings.Default.UrlRequest = value;
        SaveSettings();
      }
    }
    #endregion

    #region static string UrlResponse
    /// <summary>
    /// </summary>
    public static string UrlResponse
    {
      get { return Settings.Default.UrlResponse; }
      set
      {
        Settings.Default.UrlResponse = value;
        SaveSettings();
      }
    }
    #endregion

    #region static string User
    /// <summary>
    /// </summary>
    public static string User
    {
      get { return Settings.Default.User; }
      set
      {
        Settings.Default.User = value;
        SaveSettings();
      }
    }
    #endregion

    #region static string DefaultNamespace
    /// <summary>
    /// </summary>
    public static string DefaultNamespace
    {
      get { return Settings.Default.DefaultNamespace; }
      set
      {
        Settings.Default.DefaultNamespace = value;
        SaveSettings();
      }
    }
    #endregion


    #region void SaveSettings()
    /// <summary>
    /// </summary>
    private static void SaveSettings()
    {
      Settings.Default.Save();
    }
    #endregion
  }
  #endregion
}