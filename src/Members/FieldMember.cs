using System;
using System.Reflection;
using OoLunar.XmlDocsNET.Tags;

namespace OoLunar.XmlDocsNET.Members
{
    public class FieldMember : IDocumentationMember<FieldInfo>
    {
        public required string DocumentationId { get; init; }
        public SummaryTag? Summary { get; init; }
        public RemarksTag? Remarks { get; init; }

        public string? ReflectionFullName { get; protected set; }
        public FieldInfo? Member { get; protected set; }

        public bool TrySetMember(FieldInfo? member)
        {
            if (member is null)
            {
                return false;
            }

            Member = member;
            ReflectionFullName = member.DeclaringType is not null ? member.DeclaringType.FullName + "." + member.Name : member.Name;
            return true;
        }

        public string ToString(string? format, IFormatProvider? formatProvider) => Summary?.ToString(format, formatProvider) ?? "";
    }
}
