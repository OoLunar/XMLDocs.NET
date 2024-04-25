using System.Xml;

namespace OoLunar.XmlDocsNET.Tags
{
    public class CustomTag : DocumentationString, IDocumentationTag
    {
        public required string Name { get; init; }
        public CustomTag(XmlReader reader) : base(reader) { }
    }
}
