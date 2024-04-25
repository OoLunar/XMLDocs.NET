using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using OoLunar.XmlDocsNET.Tags;

namespace OoLunar.XmlDocsNET.Members
{
    public interface IDocumentationMember : IDocumentationTag
    {
        public string DocumentationId { get; init; }
        public SummaryTag? Summary { get; init; }
        public RemarksTag? Remarks { get; init; }

        public string? ReflectionFullName { get; }
        public ICustomAttributeProvider? MemberInfo { get; }

        [MemberNotNullWhen(true, nameof(MemberInfo), nameof(ReflectionFullName))]
        public bool TrySetMemberInfo(ICustomAttributeProvider? memberInfo);
    }

    public interface IDocumentationMember<T> : IDocumentationMember where T : ICustomAttributeProvider
    {
        public T? Member { get; }

        [MemberNotNullWhen(true, nameof(Member))]
        public bool TrySetMember(T? member);

        ICustomAttributeProvider? IDocumentationMember.MemberInfo => Member;
        bool IDocumentationMember.TrySetMemberInfo(ICustomAttributeProvider? memberInfo) => TrySetMember(memberInfo != default ? (T)memberInfo : default);
    }
}
