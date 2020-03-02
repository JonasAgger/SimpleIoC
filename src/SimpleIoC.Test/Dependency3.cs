using System;

namespace SimpleIoC.Test
{
    public class Dependency3 : IDependency3
    {
        public Dependency3(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; set; }
    }
}