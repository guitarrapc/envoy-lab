using MagicOnion;
using System;

namespace EchoGrpcMagicOnion.Shared
{
    public interface IEchoService : IService<IEchoService>
    {
        UnaryResult<string> EchoAsync(string request);
    }
}
