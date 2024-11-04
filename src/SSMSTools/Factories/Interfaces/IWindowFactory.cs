namespace SSMSTools.Factories.Interfaces
{
    internal interface IWindowFactory
    {
        T CreateWindow<T>() where T : class;
    }
}
