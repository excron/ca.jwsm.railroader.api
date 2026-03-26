using System;
using System.Collections.Generic;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebInfrastructureSnapshot
    {
        public WebInfrastructureSnapshot(
            DateTimeOffset capturedAtUtc,
            IReadOnlyList<WebSwitchSnapshot> switches,
            IReadOnlyList<WebSignalSnapshot> signals,
            IReadOnlyList<WebCouplerSnapshot> couplers)
        {
            CapturedAtUtc = capturedAtUtc;
            Switches = switches ?? Array.Empty<WebSwitchSnapshot>();
            Signals = signals ?? Array.Empty<WebSignalSnapshot>();
            Couplers = couplers ?? Array.Empty<WebCouplerSnapshot>();
        }

        public DateTimeOffset CapturedAtUtc { get; }

        public IReadOnlyList<WebSwitchSnapshot> Switches { get; }

        public IReadOnlyList<WebSignalSnapshot> Signals { get; }

        public IReadOnlyList<WebCouplerSnapshot> Couplers { get; }
    }
}
