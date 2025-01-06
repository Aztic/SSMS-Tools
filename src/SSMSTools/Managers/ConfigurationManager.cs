using System;
using System.IO;
using Newtonsoft.Json;
using SSMSTools.Managers.Interfaces;

namespace SSMSTools.Managers
{
    internal class ConfigurationManager: IConfigurationManager
    {
        private const string FolderName = nameof(SSMSTools);

        private string GetBaseFolder()
        {
            var appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            var appFolder = Path.Combine(appDataFolder, FolderName);

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            return appFolder;
        }

        private string GetConfigurationFolder()
        {
            var configurationDirectory = Path.Combine(GetBaseFolder(), "Configuration");

            if (!Directory.Exists(configurationDirectory))
            {
                Directory.CreateDirectory(configurationDirectory);
            }

            return configurationDirectory;
        }

        public T GetConfiguration<T>(string fileName) where T : new()
        {
            var filePath = Path.Combine(GetConfigurationFolder(), fileName);

            if (!File.Exists(filePath))
            {
                return new T();
            }

            var fileContent = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<T>(fileContent);
        }

        public void SaveConfiguration<T>(string fileName, T content)
        {
            var filePath = Path.Combine(GetConfigurationFolder(), fileName);

            using (var outputFile = new StreamWriter(filePath))
            {
                outputFile.Write(JsonConvert.SerializeObject(content));
            }
        }
    }
}
