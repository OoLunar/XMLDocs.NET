using System.Xml;

namespace OoLunar.XmlDocsNET.Tags
{
    public class SeeTag : DocumentationString, IDocumentationTag
    {
        public string? Cref => Attributes.TryGetValue("cref", out string? cref) ? cref : null;
        public string? Href => Attributes.TryGetValue("href", out string? href) ? href : null;

        public SeeTag(XmlReader reader) : base(reader) { }
    }
}
