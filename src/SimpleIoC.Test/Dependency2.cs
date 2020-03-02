using System;

namespace SimpleIoC.Test
{
    public class Dependency2 : IDependency2
    {
        public Dependency2(IDependency3 dependency3)
        {
            Dependency3 = dependency3;
        }

        public IDependency3 Dependency3 { get; set; }
        public Guid Id => Dependency3.Id;
    }
}