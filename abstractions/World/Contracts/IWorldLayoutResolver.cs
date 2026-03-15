using System;

namespace Ca.Jwsm.Railroader.Api.Abstractions.World.Contracts
{
    public interface IWorldLayoutResolver
    {
        void PrepareWorldApply();

        Type ResolveType(string referenceName);

        bool TryResolveComponentKind(string kind, out Type componentType);
    }
}
