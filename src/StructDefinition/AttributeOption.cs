namespace StructDefinition
{
    internal class AttributeOption
    {
        internal const int HexPaddingPropertyDefaultValue = -1;
        internal const bool IsLittleEndianPropertyDefaultValue = true;
        internal const bool OverrideToStringDefaultValue = true;

        public AttributeOption(string name, string ns)
        {
            Name = name;
            Namespace = ns;
        }

        public string Name { get; }

        public string Namespace { get; }

        public string BaseType { get; set; } = string.Empty;

        public int HexPadding { get; set; } = HexPaddingPropertyDefaultValue;

        public bool IsLittleEndian { get; set; } = IsLittleEndianPropertyDefaultValue;

        public bool OverrideToString { get; set; } = true;
    }
}