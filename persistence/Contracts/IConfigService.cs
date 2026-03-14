namespace Ca.Jwsm.Railroader.Api.Persistence.Contracts
{
    public interface IConfigService
    {
        bool TryGetJson(string section, out string json);

        void SetJson(string section, string json);
    }
}
