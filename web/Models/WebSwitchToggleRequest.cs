namespace Ca.Jwsm.Railroader.Api.Web.Models
{
    public sealed class WebSwitchToggleRequest
    {
        public string NodeId { get; set; } = string.Empty;

        public bool Thrown { get; set; }
    }
}
