using System;
using OoLunar.XmlDocsNET.Tags;

namespace OoLunar.XmlDocsNET.Members
{
    public class TypeMember : IDocumentationMember<Type>
    {
        public required string DocumentationId { get; init; }
        public SummaryTag? Summary { get; init; }
        public RemarksTag? Remarks { get; init; }

        public string? ReflectionFullName { get; protected set; }
        public Type? Member { get; protected set; }

        public bool TrySetMember(Type? member)
        {
            if (member is null)
            {
                return false;
            }

            Member = member;
            ReflectionFullName = member.FullName;
            return true;
        }

        public string ToString(string? format, IFormatProvider? formatProvider) => Summary?.ToString(format, formatProvider) ?? "";
    }
}
