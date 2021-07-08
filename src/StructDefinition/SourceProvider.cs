﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace StructDefinition
{
    internal static class SourceProvider
    {
        internal const string AttributeFullName = "StructDefinitionAttribute";
        internal const string AttributeName = "StructDefinition";
        internal const string InterfaceName = "IByteArrayConvertible";
        internal const string Namespace = "StructDefinition";

        internal static (string hintName, SourceText sourceText) AttributeSource()
        {
            var source = $@"
#nullable enable
namespace {Namespace}
{{
    using System;

    [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class {AttributeFullName}: Attribute
    {{
        public {AttributeFullName}(Type baseType)
        {{
            {nameof(AttributeOption.BaseType)} = baseType;
        }}

        public Type {nameof(AttributeOption.BaseType)} {{ get; }}

        public bool {nameof(AttributeOption.IsLittleEndian)} {{ get; set; }} = true;

        public int {nameof(AttributeOption.HexPadding)} {{ get; set; }} = -1;

        public bool {nameof(AttributeOption.OverrideToString)} {{ get; set; }} = true;
    }}
}}
";

            return ($"{AttributeFullName}.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        internal static (string hintName, SourceText sourceText) InterfaceSource()
        {
            var source = $@"
#nullable enable
namespace {Namespace}
{{
    using System;
    
    public interface {InterfaceName}
    {{
        byte[] ToByteArray();

        void Write(Span<byte> buffer);

        int Size {{ get; }}
    }}
}}
";

            return ($"{InterfaceName}.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        internal static (string hintName, SourceText sourceText) Source(AttributeOption option)
        {
            var builder = new StringBuilder()
                .AppendStruct(option.Name, option.BaseType, option.Namespace)
                .AppendConstructor(option.Name, option.BaseType)
                .AppendCommonInterfaceImplementations(option.Name, option.BaseType)
                .AppendBufferImplicitOperator(option.Name, option.BaseType, option.IsLittleEndian, true)
                .AppendBufferImplicitOperator(option.Name, option.BaseType, option.IsLittleEndian, false)
                .AppendToByteArrayMethod(option.BaseType, option.IsLittleEndian);

            if (option.OverrideToString)
            {
                builder.AppendStringOverride(option.HexPadding);
            }

            builder.Finalize();

            return ($"{option.Name}.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
        }

        private static StringBuilder AppendBufferImplicitOperator(this StringBuilder builder, string name, string baseType, bool isLittleEndian, bool isReadOnlySpan)
        {
            builder.AppendFormat(CultureInfo.InvariantCulture, "        public static implicit operator {0}({1}<byte> buffer) => new {0}(", name,
                isReadOnlySpan ? "ReadOnlySpan" : "Span");

            if (string.Equals(baseType, "byte", StringComparison.OrdinalIgnoreCase))
            {
                builder.Append("buffer[0]");
            }
            else
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "BinaryPrimitives.Read{0}{1}(buffer)", ConvertBaseTypeToSystemName(baseType), GetEndianString(isLittleEndian));
            }

            builder.Append(");");

            return builder;
        }

        [SuppressMessage("Design", "MA0051:Method is too long", Justification = "It's not.")]
        private static StringBuilder AppendCommonInterfaceImplementations(this StringBuilder builder, string name, string baseType) =>
            builder.AppendFormat(CultureInfo.InvariantCulture,
                @"
        public static int BaseTypeSize => sizeof({0});

        public int Size => BaseTypeSize;

        public int CompareTo(object? obj) => _baseValue.CompareTo(obj);

        public int CompareTo({0} other) => _baseValue.CompareTo(other);

        public bool Equals({0} other) => _baseValue.Equals(other);

        public TypeCode GetTypeCode() => _baseValue.GetTypeCode();

        bool IConvertible.ToBoolean(IFormatProvider? provider) => Convert.ToBoolean(_baseValue, provider);

        byte IConvertible.ToByte(IFormatProvider? provider) => Convert.ToByte(_baseValue, provider);

        char IConvertible.ToChar(IFormatProvider? provider) => Convert.ToChar(_baseValue, provider);

        DateTime IConvertible.ToDateTime(IFormatProvider? provider) => throw new InvalidCastException(""Unable to cast to DateTime"");

        decimal IConvertible.ToDecimal(IFormatProvider? provider) => Convert.ToDecimal(_baseValue, provider);

        double IConvertible.ToDouble(IFormatProvider? provider) => Convert.ToDouble(_baseValue, provider);

        short IConvertible.ToInt16(IFormatProvider? provider) => Convert.ToInt16(_baseValue, provider);

        int IConvertible.ToInt32(IFormatProvider? provider) => Convert.ToInt32(_baseValue, provider);

        long IConvertible.ToInt64(IFormatProvider? provider) => Convert.ToInt64(_baseValue, provider);

        sbyte IConvertible.ToSByte(IFormatProvider? provider) => Convert.ToSByte(_baseValue, provider);

        float IConvertible.ToSingle(IFormatProvider? provider) => Convert.ToSingle(_baseValue, provider);

        object IConvertible.ToType(Type conversionType, IFormatProvider? provider) => Convert.ChangeType(this, conversionType, provider);

        ushort IConvertible.ToUInt16(IFormatProvider? provider) => Convert.ToUInt16(_baseValue, provider);

        uint IConvertible.ToUInt32(IFormatProvider? provider) => Convert.ToUInt32(_baseValue, provider);

        ulong IConvertible.ToUInt64(IFormatProvider? provider) => Convert.ToUInt64(_baseValue, provider);        

        public string ToString(IFormatProvider? provider) => _baseValue.ToString(provider);

        public string ToString(string? format) => _baseValue.ToString(format);

        public string ToString(string? format, IFormatProvider? provider) => _baseValue.ToString(format, provider);        

        public static implicit operator {1}({0} value) => new {1}(value);

        public static implicit operator {0}({1} value)  => value._baseValue;

        public override int GetHashCode() => _baseValue.GetHashCode();

        public override bool Equals(object? obj) => obj is {0} other && Equals(other);

        public static bool operator ==({1} left, {1} right) => left.Equals(right);

        public static bool operator !=({1} left, {1} right) => !(left == right);

        public static bool operator <({1} left, {1} right) => left.CompareTo(right) < 0;

        public static bool operator <=({1} left, {1} right) => left.CompareTo(right) <= 0;

        public static bool operator >({1} left, {1} right) => left.CompareTo(right) > 0;

        public static bool operator >=({1} left, {1} right) => left.CompareTo(right) >= 0;

",
                baseType,
                name);

        private static StringBuilder AppendConstructor(this StringBuilder builder, string name, string baseType, string modifier = "private") =>
            builder.AppendFormat(CultureInfo.InvariantCulture,
                @"
        private {1} _baseValue;

        private {2}({1} value)
        {{
            _baseValue = value;
        }}
",
                modifier,
                baseType,
                name);

        private static StringBuilder AppendStringOverride(this StringBuilder builder, int hexPadding)
        {
            builder.AppendLine();
            builder.AppendLine();
            builder.Append("        ");
            return hexPadding < 0
                ? builder.Append("public override string ToString() => ToString(System.Globalization.CultureInfo.InvariantCulture);")
                : builder.AppendFormat(
                    CultureInfo.InvariantCulture,
                    "public override string ToString() => string.Format(System.Globalization.CultureInfo.InvariantCulture, \"0x{{0:X{0}}}\", _baseValue);",
                    hexPadding);
        }

        private static StringBuilder AppendStruct(this StringBuilder builder, string name, string baseType, string nameSpace) =>
            builder.AppendFormat(CultureInfo.InvariantCulture,
                @"
#nullable enable
namespace {0} 
{{
    using System;
    using System.Diagnostics;
    using System.Buffers.Binary;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]    
    [DebuggerDisplay(""{1}: {{ToString()}}"")]
    public partial struct {1} : IComparable, IComparable<{2}>, IConvertible, IEquatable<{2}>, IFormattable, {3}.{4}
    {{    
",
                nameSpace,
                name,
                baseType,
                Namespace,
                InterfaceName);

        private static StringBuilder AppendToByteArrayMethod(this StringBuilder builder, string baseType, bool isLittleEndian)
        {
            builder.AppendLine();
            if (string.Equals(baseType, "byte", StringComparison.OrdinalIgnoreCase))
            {
                builder.AppendLine();
                builder.AppendLine("public byte[] ToByteArray() => new [] { _baseValue };");
                builder.AppendLine();
                builder.Append("public void Write(Span<byte> buffer) => buffer[0] = _baseValue;");
            }
            else
            {
                builder.Append(
                    @"
        public byte[] ToByteArray()
        {{
            var bytes = new byte[Size];
            Write(bytes);

            return bytes;
        }}");

                builder.AppendFormat(CultureInfo.InvariantCulture,
                    @"

        public void Write(Span<byte> buffer) => BinaryPrimitives.Write{0}{1}(buffer, _baseValue);",
                    ConvertBaseTypeToSystemName(baseType),
                    GetEndianString(isLittleEndian));
            }

            return builder;
        }

        private static string ConvertBaseTypeToSystemName(string baseType)
        {
            return baseType switch
            {
                "short" => nameof(Int16),
                "ushort" => nameof(UInt16),
                "int" => nameof(Int32),
                "uint" => nameof(UInt32),
                "long" => nameof(Int64),
                "ulong" => nameof(UInt64),
                _ => throw new NotSupportedException($"Specified type '{baseType}' is not supported.")
            };
        }

        private static StringBuilder Finalize(this StringBuilder builder) =>
            builder.Append(@"
    }
}");

        private static string GetEndianString(bool isLittleEndian) => isLittleEndian ? "LittleEndian" : "BigEndian";
    }
}