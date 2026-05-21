using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Xinchuan.U8Bridge.U8
{
    public sealed class U8Reflection
    {
        private static readonly string[] AssemblyNames =
        {
            "UFIDA.U8.U8APIFramework",
            "UFIDA.U8.U8MOMAPIFramework",
            "UFIDA.U8.MomServiceCommon"
        };

        private readonly IList<string> probeDirectories;

        public U8Reflection(IEnumerable<string> probeDirectories)
        {
            this.probeDirectories = probeDirectories
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }

        public Type ResolveType(string fullName)
        {
            Type type = Type.GetType(fullName);
            if (type != null)
            {
                return type;
            }

            foreach (Assembly assembly in LoadCandidateAssemblies())
            {
                type = assembly.GetType(fullName, false);
                if (type != null)
                {
                    return type;
                }
            }

            throw new InvalidOperationException("U8 API type was not found: " + fullName);
        }

        public object Call(object target, string method, params object[] args)
        {
            return target.GetType().InvokeMember(method, BindingFlags.InvokeMethod, null, target, args);
        }

        public object GetBoParam(object broker, string name)
        {
            return Call(broker, "GetBoParam", name);
        }

        public object GetExtBoEntity(object broker, string name)
        {
            return Call(broker, "GetExtBoEntity", name);
        }

        public void AssignNormalValue(object broker, string name, object value)
        {
            Call(broker, "AssignNormalValue", name, value);
        }

        public void SetBoValue(object bo, int rowIndex, string field, object value)
        {
            object row = bo.GetType().InvokeMember(
                "Item",
                BindingFlags.GetProperty,
                null,
                bo,
                new object[] { rowIndex });
            row.GetType().InvokeMember(
                "Item",
                BindingFlags.SetProperty,
                null,
                row,
                new[] { field, value ?? string.Empty });
        }

        public void SetExtItemCount(object entity, int count)
        {
            entity.GetType().InvokeMember(
                "ItemCount",
                BindingFlags.SetProperty,
                null,
                entity,
                new object[] { count });
        }

        public object GetExtItem(object entity, int rowIndex)
        {
            return entity.GetType().InvokeMember(
                "Item",
                BindingFlags.GetProperty,
                null,
                entity,
                new object[] { rowIndex });
        }

        public object GetSubEntity(object item, string name)
        {
            object subEntities = item.GetType().InvokeMember(
                "SubEntity",
                BindingFlags.GetProperty,
                null,
                item,
                null);
            return subEntities.GetType().InvokeMember(
                "Item",
                BindingFlags.GetProperty,
                null,
                subEntities,
                new object[] { name });
        }

        public void SetExtValue(object item, string field, object value)
        {
            item.GetType().InvokeMember(
                "Item",
                BindingFlags.SetProperty,
                null,
                item,
                new[] { field, value ?? string.Empty });
        }

        public object CreateDomDocument()
        {
            Type type = Type.GetTypeFromProgID("MSXML2.DOMDocument.6.0")
                ?? Type.GetTypeFromProgID("MSXML2.DOMDocument");
            return type == null ? null : Activator.CreateInstance(type);
        }

        public object CreateComObject(string progId)
        {
            Type type = Type.GetTypeFromProgID(progId);
            if (type == null)
            {
                return null;
            }

            try
            {
                return Activator.CreateInstance(type);
            }
            catch
            {
                return null;
            }
        }

        private IEnumerable<Assembly> LoadCandidateAssemblies()
        {
            foreach (string name in AssemblyNames)
            {
                Assembly loaded = TryLoadAssembly(name);
                if (loaded != null)
                {
                    yield return loaded;
                }
            }
        }

        private Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            string name = new AssemblyName(args.Name).Name + ".dll";
            return probeDirectories.Select(path => Path.Combine(path, name))
                .Where(File.Exists)
                .Select(TryLoadFile)
                .FirstOrDefault(assembly => assembly != null);
        }

        private Assembly TryLoadAssembly(string name)
        {
            try
            {
                return Assembly.Load(name);
            }
            catch
            {
                return ResolveAssembly(this, new ResolveEventArgs(name));
            }
        }

        private static Assembly TryLoadFile(string path)
        {
            try
            {
                return Assembly.LoadFrom(path);
            }
            catch
            {
                return null;
            }
        }
    }
}
