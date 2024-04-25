using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using OoLunar.XmlDocsNET.Tags;

namespace OoLunar.XmlDocsNET
{
    public class DocumentationString : IFormattable
    {
        public string Value { get; init; }
        public IReadOnlyDictionary<string, string> Attributes { get; init; }
        public IReadOnlyList<IDocumentationTag> EmbeddedTags { get; init; }

        public DocumentationString(XmlReader reader)
        {
            if (reader.ReadState == ReadState.Initial)
            {
                // Move to the first element.
                reader.Read();
            }

            StringBuilder value = new();
            Dictionary<string, string> attributes = [];
            if (reader.HasAttributes)
            {
                while (reader.MoveToNextAttribute())
                {
                    value.Append($"{{{attributes.Count}}}");
                    attributes[reader.Name] = reader.Value;
                }

                reader.MoveToElement();
            }

            List<IDocumentationTag> embeddedTags = [];
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Text)
                {
                    value.Append(reader.Value);
                }
                else if (reader.NodeType == XmlNodeType.Element
                    && CompilerTags.TagParsers.TryGetValue(reader.Name, out Func<XmlReader, IDocumentationTag?>? parser)
                    && parser(reader) is IDocumentationTag tag)
                {
                    // Replace the tag with its index `{0}`, `{1}`, etc.
                    value.Append($"{{{attributes.Count + embeddedTags.Count}}}");
                    embeddedTags.Add(tag);
                }
            }

            Value ??= value.ToString();
            Attributes = attributes;
            EmbeddedTags = embeddedTags;
        }

        public override string ToString() => ToString(null, CultureInfo.CurrentCulture);
        public virtual string ToString(string? format, IFormatProvider? formatProvider) => string.Format(formatProvider, format ?? Value, [.. Attributes.Values, .. EmbeddedTags.Select(tag => tag.ToString(format, formatProvider))]);
    }
}
