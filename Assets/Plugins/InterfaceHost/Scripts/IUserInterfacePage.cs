using System;

namespace InterfaceHost
{
    public interface IUserInterfacePage<in TArgument> : IDisposable
    {
        void Initialize(TArgument argument);
    }
}