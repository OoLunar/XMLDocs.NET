using System;
using System.Reflection;
using System.Xml;
using OoLunar.XmlDocsNET.Tags;

namespace OoLunar.XmlDocsNET.Members
{
    public class ParamMember : DocumentationString, IDocumentationMember<ParameterInfo>
    {
        public string DocumentationId { get; init; }
        public SummaryTag? Summary { get; init; }
        public RemarksTag? Remarks { get; init; }

        public string? ReflectionFullName { get; protected set; }
        public ParameterInfo? Member { get; protected set; }

        public ParamMember(XmlReader reader) : base(reader) => DocumentationId = Attributes.TryGetValue("name", out string? name) ? name : throw new InvalidOperationException("ParamMember must have a name attribute.");

        public bool TrySetMember(ParameterInfo? member)
        {
            if (member is null || Member is not null)
            {
                return false;
            }

            Member = member;
            ReflectionFullName = $"{member.Member.DeclaringType!.FullName}.{member.Member.Name}.{member.Name}";
            return true;
        }
    }
}
