namespace Ca.Jwsm.Railroader.Api.Abstractions.Api
{
    public interface IServiceRegistry
    {
        void Register<TService>(TService instance) where TService : class;

        void Unregister<TService>() where TService : class;

        bool TryGet<TService>(out TService service) where TService : class;

        TService GetRequired<TService>() where TService : class;
    }
}
