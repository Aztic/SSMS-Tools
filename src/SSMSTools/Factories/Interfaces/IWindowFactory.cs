namespace SSMSTools.Factories.Interfaces
{
    public interface IWindowFactory
    {
        T CreateWindow<T>() where T : class;
    }
}
