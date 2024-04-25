using System.Xml;

namespace OoLunar.XmlDocsNET.Tags
{
    public class SummaryTag : DocumentationString, IDocumentationTag
    {
        public SummaryTag(XmlReader reader) : base(reader) { }
    }
}
