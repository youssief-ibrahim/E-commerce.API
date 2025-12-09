using System.Text.RegularExpressions;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace E_commerce.EF.Localization
{
    public class JsonStringLocalizer : IStringLocalizer
    {
        private readonly JsonSerializer  seryalizer =new JsonSerializer();
        public LocalizedString this[string name]
        {
             get
            {
                var value = GetString(name);
                if (string.IsNullOrEmpty(value))
                {
                    return new LocalizedString(name, name, true);
                }
                return new LocalizedString(name, value, false);
            }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var actualValue = this[name];
                return !actualValue.ResourceNotFound
                    ? new LocalizedString(name, string.Format(actualValue.Value, arguments), false)
                    : actualValue;
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            //var filePath = Path.Combine(AppContext.BaseDirectory, "Localization", "Resources", $"{Thread.CurrentThread.CurrentCulture.Name}.json");
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            var filePath = Path.Combine(AppContext.BaseDirectory, "Localization", "Resources", $"{culture}.json");
            using FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using StreamReader streamreader = new StreamReader(stream);
            using JsonTextReader reader = new JsonTextReader(streamreader);
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    var key = reader.Value?.ToString();
                    reader.Read();
                    var value = seryalizer.Deserialize<string>(reader);
                    if (key != null)
                    {
                        yield return new LocalizedString(key, value ?? string.Empty, false);
                    }
                }
            }
        }
        private string GetString(string key)
        {
            var culture = Thread.CurrentThread.CurrentCulture.Name;
            var filePath = Path.Combine(AppContext.BaseDirectory, "Localization", "Resources", $"{culture}.json");
            //var filePath = Path.Combine( AppContext.BaseDirectory,"Localization","Resources",$"{Thread.CurrentThread.CurrentCulture.Name}.json");
            var fullFilePath = Path.GetFullPath(filePath);

            if (File.Exists(fullFilePath))
            {
                var result = GetvaluefromJson(key, fullFilePath);
                return result;
            }

            return string.Empty;
        }
        private string GetvaluefromJson(string propertyName, string Filepath)
        {
            if (string.IsNullOrEmpty(Filepath) || string.IsNullOrEmpty(propertyName))
            {
                return string.Empty;
            }
            using FileStream stream = new FileStream(Filepath, FileMode.Open, FileAccess.Read);

            using StreamReader streamreader = new StreamReader(stream);
            using JsonTextReader reader = new JsonTextReader(streamreader);

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName && reader.Value?.ToString() == propertyName)
                {
                    reader.Read();
                    return seryalizer.Deserialize<string>(reader);
                }
            }
            return string.Empty;
        }

    }
}
