using System;

namespace SimpleIoC.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WindowWidth = 200;
            Console.WindowHeight = 50;

            var container = new SimpleContainer();

            container.RegisterTransient<IDependency1, Dependency1>();
            container.RegisterTransient<IDependency2, Dependency2>();
            container.RegisterTransient<IDependency3, Dependency3>(() => new Dependency3(Guid.NewGuid()));

            // Could catch circular dependencies. 
            container.Build();

            for (int i = 0; i < 5; i++)
            {
                container.Resolve<IDependency1>().CheckDependencyTree();
            }

            Console.WriteLine("Hello World!");
        }
    }
}