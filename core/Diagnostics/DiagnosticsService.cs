using System;
using System.Collections.Generic;
using System.Linq;
using Ca.Jwsm.Railroader.Api.Abstractions.Diagnostics;

namespace Ca.Jwsm.Railroader.Api.Core.Diagnostics
{
    public sealed class DiagnosticsService : IDiagnosticsService
    {
        private readonly List<ITraceSink> _sinks = new List<ITraceSink>();

        public void RegisterSink(ITraceSink sink)
        {
            if (sink == null)
            {
                throw new ArgumentNullException(nameof(sink));
            }

            _sinks.Add(sink);
        }

        public void Trace(string source, string message)
        {
            var line = string.Format("[{0}] {1}", source ?? "api", message ?? string.Empty);

            foreach (var sink in _sinks.ToList())
            {
                sink.Write(source ?? "api", line);
            }
        }

        public void Trace(string source, Exception exception, string message = null)
        {
            var detail = message ?? exception?.Message ?? "Unhandled diagnostic event.";
            var exceptionText = exception == null ? string.Empty : " Exception: " + exception;
            Trace(source, detail + exceptionText);
        }
    }
}
