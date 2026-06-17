using System;
using System.Configuration;
using System.Linq;
using System.Windows;

namespace LetterClashClient.Services {
  public static class LanguageManager {
    private const string DefaultLanguage = "ES";
    public static string CurrentLanguage { get; private set; } = DefaultLanguage;

    public static void InitializeLanguage() {
      string savedLanguage = DefaultLanguage;
      try {
        savedLanguage = ConfigurationManager.AppSettings["Language"];
        if (string.IsNullOrEmpty(savedLanguage)) {
          savedLanguage = DefaultLanguage;
        }
      } catch {
        savedLanguage = DefaultLanguage;
      }

      SetLanguage(savedLanguage, persist: false);
    }

    public static void SetLanguage(string langCode, bool persist = true) {
      if (string.IsNullOrEmpty(langCode)) return;

      var appResources = Application.Current.Resources;

      // Find and remove any existing Lang resource dictionary
      var existingLang = appResources.MergedDictionaries
          .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Themes/Lang"));

      if (existingLang != null) {
        appResources.MergedDictionaries.Remove(existingLang);
      }

      // Create and load the new Language dictionary
      var newLang = new ResourceDictionary {
        Source = new Uri($"/LetterClashClient;component/Themes/Lang{langCode}.xaml", UriKind.RelativeOrAbsolute)
      };

      // Add the language dictionary to resources
      appResources.MergedDictionaries.Add(newLang);
      CurrentLanguage = langCode;

      if (persist) {
        try {
          var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
          if (config.AppSettings.Settings["Language"] == null) {
            config.AppSettings.Settings.Add("Language", langCode);
          } else {
            config.AppSettings.Settings["Language"].Value = langCode;
          }
          config.Save(ConfigurationSaveMode.Modified);
          ConfigurationManager.RefreshSection("appSettings");
        } catch (Exception ex) {
          Console.WriteLine($"Error saving language configuration: {ex.Message}");
        }
      }
    }
  }
}
