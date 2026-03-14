using System;

namespace Ca.Jwsm.Railroader.Api.Abstractions.Diagnostics
{
    public interface IDiagnosticsService
    {
        void RegisterSink(ITraceSink sink);

        void Trace(string source, string message);

        void Trace(string source, Exception exception, string message = null);
    }
}
