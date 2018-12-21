using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using static TankLib.Util;

namespace TankLib.STU {
    /// <summary>Manages StructuredData objects. Singleton</summary>
    public class teStructuredDataMgr {
        private readonly HashSet<uint>                                                                  _missingInstances;
        public           Dictionary<uint, Type>                                                         Enums;
        public           Dictionary<Type, IStructuredDataPrimitiveFactory>                              Factories;
        public           Dictionary<uint, Dictionary<uint, KeyValuePair<FieldInfo, STUFieldAttribute>>> FieldAttributes;
        public           Dictionary<Type, IStructuredDataFieldReader>                                   FieldReaders;

        public Dictionary<uint, STUAttribute> InstanceAttributes;
        public Dictionary<uint, uint[]>       InstanceFields; // in the correct order

        public Dictionary<uint, Type> Instances;
        public Dictionary<Type, uint> InstancesInverted;

        public teStructuredDataMgr() {
            Factories    = new Dictionary<Type, IStructuredDataPrimitiveFactory>();
            FieldReaders = new Dictionary<Type, IStructuredDataFieldReader>();

            Instances         = new Dictionary<uint, Type>();
            Enums             = new Dictionary<uint, Type>();
            InstancesInverted = new Dictionary<Type, uint>();

            InstanceAttributes = new Dictionary<uint, STUAttribute>();
            FieldAttributes    = new Dictionary<uint, Dictionary<uint, KeyValuePair<FieldInfo, STUFieldAttribute>>>();
            InstanceFields     = new Dictionary<uint, uint[]>();

            var assembly = typeof(teStructuredDataMgr).Assembly;
            AddAssemblyInstances(assembly);
            AddAssemblyFieldReaders(assembly);
            AddAssemblyFactories(assembly);

            _missingInstances = new HashSet<uint>();
        }

        public void AddAssemblyFactories(Assembly assembly) {
            foreach (var type in GetAssemblyTypes<IStructuredDataPrimitiveFactory>(assembly)) {
                if (type.IsInterface) continue;
                AddFactory(type);
            }
        }

        public void AddFactory(Type type) {
            var reader = (IStructuredDataPrimitiveFactory) Activator.CreateInstance(type);
            Factories[reader.GetValueType()] = reader;
        }

        public void AddAssemblyFieldReaders(Assembly assembly) {
            foreach (var type in GetAssemblyTypes<IStructuredDataFieldReader>(assembly)) {
                if (type.IsInterface) continue;
                AddFieldReader(type);
            }
        }

        public void AddFieldReader(Type type) {
            var reader = (IStructuredDataFieldReader) Activator.CreateInstance(type);
            FieldReaders[type] = reader;
        }

        public void AddAssemblyInstances(Assembly assembly) {
            foreach (var type in GetAssemblyTypes<STUInstance>(assembly)) AddInstance(type);
        }

        public void AddInstance(Type type) {
            var attribute = type.GetCustomAttribute<STUAttribute>();
            if (attribute      == null) return;
            if (attribute.Hash == 0) return;

            InstanceAttributes[attribute.Hash] = attribute;
            Instances[attribute.Hash]          = type;
            InstancesInverted[type]            = attribute.Hash;
            FieldAttributes[attribute.Hash]    = new Dictionary<uint, KeyValuePair<FieldInfo, STUFieldAttribute>>();
            //List<uint> fieldOrderTemp = new List<uint>();

            foreach (var field in type.GetFields()) {
                var fieldAttribute = field.GetCustomAttribute<STUFieldAttribute>();
                if (fieldAttribute      == null) continue;
                if (fieldAttribute.Hash == 0) continue;
                //fieldOrderTemp.Add(fieldAttribute.Hash);

                FieldAttributes[attribute.Hash][fieldAttribute.Hash] = new KeyValuePair<FieldInfo, STUFieldAttribute>(field, fieldAttribute);
            }

            //InstanceFields[attribute.Hash] = fieldOrderTemp.ToArray();
            InstanceFields[attribute.Hash] = GetFieldOrder(type)
                .ToArray(); // shrug
        }

        public List<uint> GetFieldOrder(Type type) {
            var parentFields                                       = new List<uint>();
            if (type.BaseType != typeof(STUInstance)) parentFields = GetFieldOrder(type.BaseType);
            var fields                                             = new List<uint>();

            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)) {
                var fieldAttribute = field.GetCustomAttribute<STUFieldAttribute>();
                if (fieldAttribute      == null) continue;
                if (fieldAttribute.Hash == 0) continue;
                fields.Add(fieldAttribute.Hash);
            }

            var temp = parentFields.Concat(fields)
                                   .ToList();
            //var temp = fields.Concat(parentFields).ToList();
            return temp;
        }

        public STUInstance CreateInstance(uint hash) {
            if (Instances.TryGetValue(hash, out var instanceType)) return (STUInstance) Activator.CreateInstance(instanceType);

            if (_missingInstances.Add(hash)) Debugger.Log(0, "teStructuredDataMgr", $"Unhandled instance: {hash:X8}\r\n");
            return null;
        }

        public void WipeInstances() {
            Instances.Clear();
            InstancesInverted.Clear();
            InstanceAttributes.Clear();
            FieldAttributes.Clear();
            InstanceFields.Clear();
        }

        public void WipeEnums() { Enums.Clear(); }
    }
}
