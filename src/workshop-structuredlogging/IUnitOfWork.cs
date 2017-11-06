using System;

namespace workshop_structuredlogging
{
    public interface IUnitOfWork : IDisposable
    {
        void Start();
        void Complete();
    }
}
