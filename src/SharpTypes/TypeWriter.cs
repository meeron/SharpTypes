using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SharpTypes
{
    public sealed class TypeWriter
    {
        private static readonly Dictionary<Type, string> Types = new Dictionary<Type, string>
        {
            { typeof(Guid), "string" },
            { typeof(string), "string" },
            { typeof(bool), "boolean" },
            { typeof(int), "number" },
            { typeof(uint), "number" },
            { typeof(long), "number" },
            { typeof(ulong), "number" },
            { typeof(float), "number" },
            { typeof(double), "number" },
            { typeof(short), "number" },
            { typeof(ushort), "number" },
            { typeof(byte), "number" },
            { typeof(sbyte), "number" },
            { typeof(decimal), "number" },
            { typeof(DateTime), "string" }
        };

        private readonly List<string> _duplicatesGuard;

        public TypeWriter()
        {
            _duplicatesGuard = new List<string>();
        }

        public void Write(IEnumerable<Type> types, TextWriter textWriter)
        {
            foreach (var type in types)
            {
                Write(type, textWriter);
            }
        }

        public void Write(Type type, TextWriter textWriter)
        {
            var nestedTypes = new List<Type>();

            var classType = GetClassType(type);
            if (classType == null)
            {
                return;
            }

            if (_duplicatesGuard.Contains(classType.Name))
            {
                return;
            }

            _duplicatesGuard.Add(classType.Name);

            var properties = classType.GetProperties();

            textWriter.Write($"export interface {classType.Name} {{ ");

            foreach (var property in properties)
            {
                var propName = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);

                textWriter.Write($"{propName}: {GetTypeSymbol(property.PropertyType)}; ");

                var nestedType = GetClassType(property.PropertyType);
                if (nestedType != null && !nestedTypes.Contains(nestedType))
                {
                    nestedTypes.Add(nestedType);
                }
            }

            textWriter.Write("}\n");

            Write(nestedTypes, textWriter);
        }

        private static Type GetClassType(Type type)
        {
            if (type == null)
            {
                return null;
            }

            if (type.IsArray)
            {
                return GetClassType(type.GetElementType());
            }

            if ((type.GetInterfaces().Any(i => i == typeof(IEnumerable)) && type.GenericTypeArguments.Length == 1)
                || type.BaseType == typeof(Task) && type.IsGenericType)
            {
                return GetClassType(type.GenericTypeArguments[0]);
            }

            if (IsUserClass(type))
            {
                return type;
            }

            return null;
        }

        private static string GetTypeSymbol(Type type)
        {
            if (Types.ContainsKey(type))
            {
                return Types[type];
            }

            if (type.IsArray)
            {
                return $"{GetTypeSymbol(type.GetElementType())}[]";
            }

            if (type.GetInterfaces().Any(i => i == typeof(IEnumerable)) && type.GenericTypeArguments.Length == 1)
            {
                return $"{GetTypeSymbol(type.GenericTypeArguments[0])}[]";
            }

            if (IsUserClass(type))
            {
                return type.Name;
            }

            return "any";
        }

        private static bool IsUserClass(Type type)
        {
            if (type == typeof(string) || type == typeof(object))
            {
                return false;
            }

            return !type.IsPrimitive && !type.IsGenericType && type.IsClass;
        }
    }
}
