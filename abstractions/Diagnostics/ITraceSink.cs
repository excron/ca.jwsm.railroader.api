namespace Ca.Jwsm.Railroader.Api.Abstractions.Diagnostics
{
    public interface ITraceSink
    {
        void Write(string source, string message);
    }
}
