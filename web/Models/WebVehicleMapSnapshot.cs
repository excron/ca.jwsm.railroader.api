using System;
using System.Collections.Generic;

namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebVehicleMapSnapshot
    {
        public WebVehicleMapSnapshot(DateTimeOffset capturedAtUtc, IReadOnlyList<WebVehicleSnapshot> vehicles)
        {
            CapturedAtUtc = capturedAtUtc;
            Vehicles = vehicles ?? Array.Empty<WebVehicleSnapshot>();
        }

        public DateTimeOffset CapturedAtUtc { get; }

        public IReadOnlyList<WebVehicleSnapshot> Vehicles { get; }
    }
}
