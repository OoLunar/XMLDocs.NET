using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Xml;
using OoLunar.XmlDocsNET.Members;
using OoLunar.XmlDocsNET.Tags;

namespace OoLunar.XmlDocsNET
{
    public static class CompilerTags
    {
        public static readonly FrozenDictionary<string, Func<XmlReader, IDocumentationTag?>> TagParsers = new Dictionary<string, Func<XmlReader, IDocumentationTag?>>()
        {
            ["summary"] = reader => new SummaryTag(reader),
            ["remarks"] = reader => new RemarksTag(reader),
            ["returns"] = reader => new ReturnsTag(reader),
            ["see"] = reader => new SeeTag(reader),
            ["param"] = reader => new ParamMember(reader)
        }.ToFrozenDictionary();
    }
}
