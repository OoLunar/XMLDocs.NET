using System.Xml;

namespace OoLunar.XmlDocsNET.Tags
{
    public class RemarksTag : DocumentationString, IDocumentationTag
    {
        public RemarksTag(XmlReader reader) : base(reader) { }
    }
}
