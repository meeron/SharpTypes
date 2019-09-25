using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpTypes
{
    public class TypeWriter
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

        public void Write(Type type, TextWriter textWriter)
        {
            var nestedTypes = new List<Type>();

            var classType = GetClassType(type);
            if (classType == null)
            {
                return;
            }

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

            textWriter.Write("}");

            foreach (var nestedType in nestedTypes)
            {
                textWriter.Write("\n");
                Write(nestedType, textWriter);
            }
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

            if (type.GetInterfaces().Any(i => i == typeof(IEnumerable)) && type.GenericTypeArguments.Length == 1)
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
