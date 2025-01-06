namespace SSMSTools.Managers.Interfaces
{
    public interface IConfigurationManager
    {
        T GetConfiguration<T>(string filePath) where T : new();
        void SaveConfiguration<T>(string fileName, T content);
    }
}
