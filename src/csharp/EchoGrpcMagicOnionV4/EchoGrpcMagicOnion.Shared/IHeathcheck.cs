using MagicOnion;
using System;

namespace EchoGrpcMagicOnion.Shared
{
    public interface IHealthzService : IService<IHealthzService>
    {
        UnaryResult<int> Readiness();
    }
}
