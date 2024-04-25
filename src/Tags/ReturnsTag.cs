using System.Xml;

namespace OoLunar.XmlDocsNET.Tags
{
    public class ReturnsTag : DocumentationString, IDocumentationTag
    {
        public ReturnsTag(XmlReader reader) : base(reader) { }
    }
}
