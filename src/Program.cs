using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleIoC
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new SimpleContainer();

            container.RegisterTransient<RndGuidWriter, RandomGuidWriter1>();
            container.RegisterSingleton<RndGuidWriter, RandomGuidWriter2>();
            container.RegisterScoped<RndGuidWriter, RandomGuidWriter3>();

            container.RegisterTransient<RandomGuid>();


            container.ResolveAll<RndGuidWriter>().PrintAll();

            using (var scope = container.BeginScope())
            {
                container.ResolveAll<RndGuidWriter>().PrintAll();
            }

            container.ResolveAll<RndGuidWriter>().PrintAll();



            //TODO: Fix circular dependency.
            //container.RegisterTransient<A>();
            //container.RegisterTransient<B>();
            //container.RegisterTransient<C>();
            //
            //container.Resolve<C>();

            Console.WriteLine("Done");
        }
    }

    public static class IoCExtensions
    {
        public static void PrintAll(this IEnumerable<RndGuidWriter> writers)
        {
            int i = 1;
            foreach (var item in writers)
            {
                Console.Write($"Item {i++}:");
                item.PrintGuid();
            }
        }
    }

    public interface RndGuidWriter
    {
        void PrintGuid();
    }

    class RandomGuidWriter1 : RndGuidWriter
    {
        private RandomGuid rndGuid;

        public RandomGuidWriter1(RandomGuid rndGuid)
        {
            this.rndGuid = rndGuid;
        }

        public void PrintGuid() => Console.WriteLine(rndGuid.Id);
    }

    class RandomGuidWriter2 : RndGuidWriter
    {
        private RandomGuid rndGuid;

        public RandomGuidWriter2(RandomGuid rndGuid)
        {
            this.rndGuid = rndGuid;
        }

        public void PrintGuid() => Console.WriteLine(rndGuid.Id);
    }

    class RandomGuidWriter3 : RndGuidWriter
    {
        private RandomGuid rndGuid;

        public RandomGuidWriter3(RandomGuid rndGuid)
        {
            this.rndGuid = rndGuid;
        }

        public void PrintGuid() => Console.WriteLine(rndGuid.Id);
    }

    class RandomGuid
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    class A
    {
        private B b;

        public A(B b)
        {
            this.b = b;
        }
    }

    class B
    {

        private A a;

        public B(A a)
        {
            this.a = a;
        }
    }

    class C
    {
        private A a;
        private B b;

        public C(A a, B b)
        {
            this.a = a;
            this.b = b;
        }
    }

}
