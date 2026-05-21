using System.Collections.Generic;
using Xinchuan.U8Bridge.Models;

namespace Xinchuan.U8Bridge.U8
{
    public sealed class U8BrokerCall
    {
        public int? VoucherType { get; set; }

        public string ReturnIdName { get; set; }

        public bool BooleanReturn { get; set; }

        public IDictionary<string, object> NormalValues { get; } = new Dictionary<string, object>();

        public IList<U8BoObject> BusinessObjects { get; } = new List<U8BoObject>();

        public IList<U8ExtBoObject> ExtensionObjects { get; } = new List<U8ExtBoObject>();

        public void ApplyTo(object broker, U8Reflection reflection)
        {
            foreach (KeyValuePair<string, object> item in NormalValues)
            {
                reflection.AssignNormalValue(broker, item.Key, ResolveValue(item.Value, reflection));
            }

            foreach (U8BoObject item in BusinessObjects)
            {
                ApplyBusinessObject(broker, reflection, item);
            }

            foreach (U8ExtBoObject item in ExtensionObjects)
            {
                object entity = reflection.GetExtBoEntity(broker, item.Name);
                ApplyExtensionObject(entity, reflection, item);
            }
        }

        private static object ResolveValue(object value, U8Reflection reflection)
        {
            var special = value as U8BrokerSpecialValue;
            if (special == null)
            {
                return value;
            }

            return special.Create(reflection);
        }

        private static void ApplyBusinessObject(object broker, U8Reflection reflection, U8BoObject item)
        {
            object bo = reflection.GetBoParam(broker, item.Name);
            bo.GetType().InvokeMember(
                "RowCount",
                System.Reflection.BindingFlags.SetProperty,
                null,
                bo,
                new object[] { item.Rows.Count });
            for (int row = 0; row < item.Rows.Count; row++)
            {
                foreach (KeyValuePair<string, object> field in item.Rows[row])
                {
                    reflection.SetBoValue(bo, row, field.Key, field.Value);
                }
            }
        }

        private static void ApplyExtensionObject(object entity, U8Reflection reflection, U8ExtBoObject item)
        {
            reflection.SetExtItemCount(entity, item.Rows.Count);
            for (int row = 0; row < item.Rows.Count; row++)
            {
                object extItem = reflection.GetExtItem(entity, row);
                foreach (KeyValuePair<string, object> field in item.Rows[row].Fields)
                {
                    reflection.SetExtValue(extItem, field.Key, field.Value);
                }

                foreach (KeyValuePair<string, U8ExtBoObject> child in item.Rows[row].Children)
                {
                    object subEntity = reflection.GetSubEntity(extItem, child.Key);
                    ApplyExtensionObject(subEntity, reflection, child.Value);
                }
            }
        }
    }

    public sealed class U8BoObject
    {
        public U8BoObject(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public IList<IDictionary<string, object>> Rows { get; } = new List<IDictionary<string, object>>();
    }

    public sealed class U8ExtBoObject
    {
        public U8ExtBoObject(string name)
        {
            Name = name;
        }

        public string Name { get; }

        public IList<U8ExtBoRow> Rows { get; } = new List<U8ExtBoRow>();
    }

    public sealed class U8ExtBoRow
    {
        public IDictionary<string, object> Fields { get; } = new Dictionary<string, object>();

        public IDictionary<string, U8ExtBoObject> Children { get; } = new Dictionary<string, U8ExtBoObject>();
    }

    public sealed class U8BrokerSpecialValue
    {
        private readonly string kind;
        private readonly string progId;

        private U8BrokerSpecialValue(string kind, string progId = null)
        {
            this.kind = kind;
            this.progId = progId;
        }

        public static U8BrokerSpecialValue DomDocument()
        {
            return new U8BrokerSpecialValue("dom");
        }

        public static U8BrokerSpecialValue Com(string progId)
        {
            return new U8BrokerSpecialValue("com", progId);
        }

        public static U8BrokerSpecialValue EmptyObject()
        {
            return new U8BrokerSpecialValue("object");
        }

        public object Create(U8Reflection reflection)
        {
            if (kind == "dom")
            {
                return reflection.CreateDomDocument();
            }

            if (kind == "com")
            {
                return reflection.CreateComObject(progId);
            }

            return new object();
        }
    }
}
