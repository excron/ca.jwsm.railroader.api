using System;
using System.Collections.Generic;
using Ca.Jwsm.Railroader.Api.Abstractions.Common;
using Ca.Jwsm.Railroader.Api.Trains.Contracts;
using Ca.Jwsm.Railroader.Api.Trains.Models;

namespace Ca.Jwsm.Railroader.Api.Host.Services
{
    public sealed class ControlRequestService : IControlRequestService
    {
        private readonly List<KeyValuePair<string, Func<ControlRequest, ControlRequestResult>>> _interceptors =
            new List<KeyValuePair<string, Func<ControlRequest, ControlRequestResult>>>();
        private readonly object _sync = new object();

        public Result RegisterInterceptor(string id, Func<ControlRequest, ControlRequestResult> interceptor)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Result.Failure("Interceptor id is required.");
            }

            if (interceptor == null)
            {
                return Result.Failure("Interceptor delegate is required.");
            }

            lock (_sync)
            {
                foreach (var registration in _interceptors)
                {
                    if (string.Equals(registration.Key, id, StringComparison.OrdinalIgnoreCase))
                    {
                        return Result.Failure("A control interceptor with the same id is already registered.");
                    }
                }

                _interceptors.Add(new KeyValuePair<string, Func<ControlRequest, ControlRequestResult>>(id, interceptor));
            }

            return Result.Success();
        }

        public ControlRequestResult Evaluate(ControlRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            lock (_sync)
            {
                var current = ControlRequestResult.PassThrough(request);
                foreach (var registration in _interceptors)
                {
                    current = registration.Value(request);
                    if (current != null && current.IsBlocked)
                    {
                        return current;
                    }
                }

                return current;
            }
        }
    }
}
