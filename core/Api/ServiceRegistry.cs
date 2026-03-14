using System;
using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Abstractions.Api;

namespace Ca.Jwsm.Railroader.Api.Core.Api
{
    public sealed class ServiceRegistry : IServiceRegistry
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void Register<TService>(TService instance) where TService : class
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            _services[typeof(TService)] = instance;
        }

        public void Unregister<TService>() where TService : class
        {
            _services.Remove(typeof(TService));
        }

        public bool TryGet<TService>(out TService service) where TService : class
        {
            if (_services.TryGetValue(typeof(TService), out var instance))
            {
                service = (TService)instance;
                return true;
            }

            service = null;
            return false;
        }

        public TService GetRequired<TService>() where TService : class
        {
            if (TryGet<TService>(out var service))
            {
                return service;
            }

            throw new InvalidOperationException("Service is not registered: " + typeof(TService).FullName);
        }
    }
}
