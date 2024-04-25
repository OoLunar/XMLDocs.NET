using System;
using System.Collections.Generic;
using System.Reflection;
using OoLunar.XmlDocsNET.Tags;

namespace OoLunar.XmlDocsNET.Members
{
    public class MethodMember : IDocumentationMember<MethodInfo>
    {
        public required string DocumentationId { get; init; }
        public SummaryTag? Summary { get; init; }
        public RemarksTag? Remarks { get; init; }
        public DocumentationString? Returns { get; init; }
        public IReadOnlyDictionary<string, ParamMember> Parameters { get; init; } = new Dictionary<string, ParamMember>();

        public string? ReflectionFullName { get; protected set; }
        public MethodInfo? Member { get; protected set; }

        public bool TrySetMember(MethodInfo? member)
        {
            if (member is null || Member is not null)
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
