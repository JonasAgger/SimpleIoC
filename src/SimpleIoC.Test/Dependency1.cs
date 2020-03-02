using System;

namespace SimpleIoC.Test
{
    public class Dependency1 : IDependency1
    {
        public Dependency1(IDependency2 dependency2, IDependency3 dependency3)
        {
            Dependency2 = dependency2;
            Dependency3 = dependency3;
        }

        public IDependency2 Dependency2 { get; set; }
        public IDependency3 Dependency3 { get; set; }

        public void CheckDependencyTree()
        {
            Console.WriteLine($"Dependency3: {Dependency3.Id}");
            Console.WriteLine($"Dependency2: {Dependency2.Id}");
        }
    }
}