using System;
using System.Linq;

namespace MetaLinq.Generator {
    public enum TypeModifiers { 
        Class,
        StaticClass,
        Struct,
        ReadonlyStruct
    }
    public static class CodeBuilderExtensionsEx {
        public readonly struct CloseBracketDiposable : IDisposable {
            readonly CodeBuilder builder;
            public CloseBracketDiposable(CodeBuilder builder) {
                this.builder = builder;
            }
            public void Dispose() {
                builder.AppendLine("}");
            }
        }
        public static CloseBracketDiposable BuildNamespace(this CodeBuilder builder, out CodeBuilder namespaceBuilder, string namespaceName) {
            builder.Append("namespace ").Append(namespaceName).AppendLine(" {");
            namespaceBuilder = builder.Tab;
            return new CloseBracketDiposable(builder);
        }

        public static CloseBracketDiposable BuildType(
            this CodeBuilder builder,
            out CodeBuilder typeBuilder,
            TypeModifiers modifiers,
            string name,
            bool partial = false,
            bool isPublic = false,
            string? generics = null,
            string? baseType = null
        ) {
            bool hasGenerics = generics != null;
            bool hasbaseType = baseType != null;
            builder
                .AppendIf(isPublic, "public ")
                .Append(GetReadonlyOrStaticModifier(modifiers))
                .AppendIf(partial, "partial ")
                .Append(GetClassOrStructModifiers(modifiers))
                .Append(name)

                .AppendIf(hasGenerics, "<")
                .AppendIf(hasGenerics, generics!)
                .AppendIf(hasGenerics, ">")

                .AppendIf(hasbaseType, " : ")
                .AppendIf(hasbaseType, baseType!)

                .AppendLine(" {");
            typeBuilder = builder.Tab;
            return new CloseBracketDiposable(builder);
        }

        static string GetClassOrStructModifiers(TypeModifiers modifiers) {
            return modifiers switch { 
                TypeModifiers.Class or TypeModifiers.StaticClass => "class ", 
                TypeModifiers.Struct or TypeModifiers.ReadonlyStruct => "struct ", 
                _ => throw new NotImplementedException()
            };
        }
        static string GetReadonlyOrStaticModifier(TypeModifiers modifiers) {
            return modifiers switch {
                TypeModifiers.StaticClass => "static ",
                TypeModifiers.ReadonlyStruct => "readonly ",
                _ => string.Empty
            };
        }
    }
}
