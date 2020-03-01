using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleIoC
{
    public class ContainerScope : IContainerScope
    {
        private static int scopeId = 0;

        public ContainerScope() => this.id = ++scopeId;

        public int id { get; private set; }
        private ContainerScope outerScope = null;
        private Action<ContainerScope> onDisposed;
        private Dictionary<string, object> scopedObjects = new Dictionary<string, object>();

        internal object GetResolvedScopedObject(string name)
        {
            if (scopedObjects.ContainsKey(name))
                return scopedObjects[name];
            return null;
        }

        internal void SetResovedScopedObject(string name, object obj) => scopedObjects.Add(name, obj);

        public static ContainerScope BeginScope(ContainerScope outerScope, Action<ContainerScope> onDisposed)
        {
            return new ContainerScope()
            {
                outerScope = outerScope,
                onDisposed = onDisposed,
            };
        }

        public void Dispose()
        {
            scopedObjects = null;
            scopeId--;
            onDisposed(outerScope);
            outerScope = null;
        }
    }
}
