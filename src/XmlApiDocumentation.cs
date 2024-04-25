using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using OoLunar.XmlDocsNET.Members;
using OoLunar.XmlDocsNET.Tags;

namespace OoLunar.XmlDocsNET
{
    internal delegate IDocumentationTag? TryParseTagDelegate(XmlReader reader);

    public sealed class XmlApiDocumentation
    {
        public static IReadOnlyDictionary<string, IReadOnlyList<IDocumentationTag>> Parse(XmlReader reader)
        {
            Dictionary<string, List<IDocumentationTag>> allTags = [];
            if (!reader.Name.Equals("members", StringComparison.Ordinal) && !reader.ReadToFollowing("members"))
            {
                // No members found
                return new Dictionary<string, IReadOnlyList<IDocumentationTag>>();
            }

            // Iterate through all elements in the members tag
            while (reader.Read())
            {
                // Skip all non-element nodes, such as whitespace or comments
                if (reader.NodeType != XmlNodeType.Element
                    || !reader.Name.Equals("member", StringComparison.Ordinal)
                    || reader.GetAttribute("name") is not string id
                    || string.IsNullOrWhiteSpace(id))
                {
                    continue;
                }

                // Read all elements of the `member` tag
                XmlReader subReader = reader.ReadSubtree();
                subReader.Read();

                // Parse all tags in the `member` tag
                List<IDocumentationTag> tags = [];
                while (subReader.Read())
                {
                    if (subReader.NodeType != XmlNodeType.Element)
                    {
                        continue;
                    }
                    else if (CompilerTags.TagParsers.TryGetValue(subReader.Name, out Func<XmlReader, IDocumentationTag?>? parser) && parser(subReader.ReadSubtree()) is IDocumentationTag tag)
                    {
                        tags.Add(tag);
                    }
                    else
                    {
                        Dictionary<string, string> attributes = [];
                        if (subReader.HasAttributes)
                        {
                            while (subReader.MoveToNextAttribute())
                            {
                                attributes[subReader.Name] = subReader.Value;
                            }

                            subReader.MoveToElement();
                        }

                        // Custom tag parsing
                        tags.Add(new CustomTag(subReader.ReadSubtree())
                        {
                            Name = subReader.Name
                        });
                    }
                }

                allTags[id] = tags;
            }

            return allTags.ToDictionary(x => x.Key, x => (IReadOnlyList<IDocumentationTag>)x.Value);
        }

        public static IReadOnlyDictionary<string, IDocumentationMember> Resolve(Assembly assembly, string? xmlDocumentationPath = null)
        {
            xmlDocumentationPath ??= Path.ChangeExtension(assembly.Location, ".xml");

            IReadOnlyDictionary<string, IReadOnlyList<IDocumentationTag>> tags = Parse(XmlReader.Create(xmlDocumentationPath));
            Dictionary<string, IDocumentationMember> resolvedMemberInfoTags = [];
            Dictionary<string, IDocumentationMember> unresolvedMemberInfoTags = [];

            CSharpCompilation compilation = CSharpCompilation.Create(null);
            compilation = compilation.AddReferences(MetadataReference.CreateFromFile(assembly.Location, new MetadataReferenceProperties(), XmlDocumentationProvider.CreateFromFile(Path.ChangeExtension(assembly.Location, ".xml"))));

            // Recursively resolve all types, interfaces, and nested types
            void ResolveTypeInfo(Type type)
            {
                if (string.IsNullOrWhiteSpace(type.FullName))
                {
                    return;
                }

                INamedTypeSymbol? key = compilation.GetTypeByMetadataName(type.FullName);
                if (key is null)
                {
                    return;
                }

                string? id = key.GetDocumentationCommentId();
                if (string.IsNullOrWhiteSpace(id))
                {
                    return;
                }

                TypeMember member = !tags.TryGetValue(id, out IReadOnlyList<IDocumentationTag>? memberTags)
                    ? new() { DocumentationId = id }
                    : new()
                    {
                        DocumentationId = id,
                        Summary = memberTags.OfType<SummaryTag>().FirstOrDefault(),
                        Remarks = memberTags.OfType<RemarksTag>().FirstOrDefault()
                    };

                member.TrySetMember(type);
                resolvedMemberInfoTags.TryAdd(id, member);

                // Resolve all nested types
                foreach (Type nested in type.GetNestedTypes())
                {
                    ResolveTypeInfo(nested);
                }

                // Resolve properties
                foreach (PropertyInfo property in type.GetProperties())
                {
                    ResolvePropertyInfo(property);
                }

                // Resolve fields
                foreach (FieldInfo field in type.GetFields())
                {
                    ResolveFieldInfo(field);
                }

                // Resolve methods
                foreach (MethodInfo method in type.GetMethods())
                {
                    ResolveMethodInfo(method);
                }

                // Resolve events
                foreach (EventInfo @event in type.GetEvents())
                {
                    ResolveEventInfo(@event);
                }
            }

            void ResolvePropertyInfo(PropertyInfo property)
            {
                if (property.DeclaringType is null || string.IsNullOrWhiteSpace(property.DeclaringType.FullName))
                {
                    return;
                }

                INamedTypeSymbol? key = compilation.GetTypeByMetadataName(property.DeclaringType.FullName);
                if (key is null)
                {
                    return;
                }

                string? id = key.GetMembers(property.Name).FirstOrDefault()?.GetDocumentationCommentId();
                if (string.IsNullOrWhiteSpace(id))
                {
                    return;
                }

                PropertyMember member = !tags.TryGetValue(id, out IReadOnlyList<IDocumentationTag>? memberTags)
                    ? new() { DocumentationId = id }
                    : new()
                    {
                        DocumentationId = id,
                        Summary = memberTags.OfType<SummaryTag>().FirstOrDefault(),
                        Remarks = memberTags.OfType<RemarksTag>().FirstOrDefault(),
                        //Returns = memberTags.OfType<ReturnsTag>().FirstOrDefault(),
                        //Value = memberTags.OfType<ValueTag>().FirstOrDefault(),
                        //Exceptions = memberTags.OfType<ExceptionTag>().ToDictionary(x => x.ExceptionType, x => x)
                    };

                member.TrySetMember(property);
                resolvedMemberInfoTags.TryAdd(id, member);
            }

            void ResolveFieldInfo(FieldInfo field)
            {
                if (field.DeclaringType is null || string.IsNullOrWhiteSpace(field.DeclaringType.FullName))
                {
                    return;
                }

                INamedTypeSymbol? key = compilation.GetTypeByMetadataName(field.DeclaringType.FullName);
                if (key is null)
                {
                    return;
                }

                string? id = key.GetMembers(field.Name).FirstOrDefault()?.GetDocumentationCommentId();
                if (string.IsNullOrWhiteSpace(id))
                {
                    return;
                }

                FieldMember member = !tags.TryGetValue(id, out IReadOnlyList<IDocumentationTag>? memberTags)
                    ? new() { DocumentationId = id }
                    : new()
                    {
                        DocumentationId = id,
                        Summary = memberTags.OfType<SummaryTag>().FirstOrDefault(),
                        Remarks = memberTags.OfType<RemarksTag>().FirstOrDefault()
                    };

                member.TrySetMember(field);
                resolvedMemberInfoTags.TryAdd(id, member);
            }

            void ResolveMethodInfo(MethodInfo method)
            {
                if (method.DeclaringType is null || string.IsNullOrWhiteSpace(method.DeclaringType.FullName))
                {
                    return;
                }

                INamedTypeSymbol? key = compilation.GetTypeByMetadataName(method.DeclaringType.FullName);
                if (key is null)
                {
                    return;
                }

                if (key.GetMembers(method.Name).FirstOrDefault() is not IMethodSymbol methodSymbol || methodSymbol?.GetDocumentationCommentId() is not string id)
                {
                    return;
                }

                MethodMember member;
                if (tags.TryGetValue(id, out IReadOnlyList<IDocumentationTag>? memberTags))
                {
                    Dictionary<string, ParamMember> parameters = [];
                    foreach (ParameterInfo parameter in method.GetParameters())
                    {
                        foreach (IParameterSymbol symbol in methodSymbol.Parameters)
                        {
                            if (symbol.Name != parameter.Name)
                            {
                                continue;
                            }

                            ParamMember? param = memberTags.OfType<ParamMember>().FirstOrDefault(x => x.DocumentationId == symbol.Name);
                            if (param is not null && param.TrySetMember(parameter))
                            {
                                parameters[symbol.Name] = param;
                                break;
                            }
                        }
                    }

                    member = new()
                    {
                        DocumentationId = id,
                        Summary = memberTags.OfType<SummaryTag>().FirstOrDefault(),
                        Remarks = memberTags.OfType<RemarksTag>().FirstOrDefault(),
                        Parameters = memberTags.OfType<ParamMember>().ToDictionary(x => x.DocumentationId, x => x),
                        //Returns = memberTags.OfType<ReturnsTag>().FirstOrDefault(),
                        //Value = memberTags.OfType<ValueTag>().FirstOrDefault(),
                        //Exceptions = memberTags.OfType<ExceptionTag>().ToDictionary(x => x.ExceptionType, x => x)
                    };
                }
                else
                {
                    member = new() { DocumentationId = id };
                }

                member.TrySetMember(method);
                resolvedMemberInfoTags.TryAdd(id, member);
            }

            void ResolveEventInfo(EventInfo @event)
            {
                if (@event.DeclaringType is null || string.IsNullOrWhiteSpace(@event.DeclaringType.FullName))
                {
                    return;
                }

                INamedTypeSymbol? key = compilation.GetTypeByMetadataName(@event.DeclaringType.FullName);
                if (key is null)
                {
                    return;
                }

                string? id = key.GetMembers(@event.Name).FirstOrDefault()?.GetDocumentationCommentId();
                if (string.IsNullOrWhiteSpace(id))
                {
                    return;
                }

                tags.TryGetValue(id, out IReadOnlyList<IDocumentationTag>? memberTags);
                EventMember member = new()
                {
                    DocumentationId = id,
                    Summary = memberTags?.OfType<SummaryTag>().FirstOrDefault(),
                    Remarks = memberTags?.OfType<RemarksTag>().FirstOrDefault()
                };

                member.TrySetMember(@event);
                resolvedMemberInfoTags.TryAdd(id, member);
            }

            // Resolve all types in the assembly
            foreach (Type type in assembly.GetTypes())
            {
                ResolveTypeInfo(type);
            }

            return resolvedMemberInfoTags;
        }
    }
}
