using System;

namespace SimpleIoC
{
    public interface IContainerScope : IDisposable
    {
        int id { get; }
    }
}