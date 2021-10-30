using System;
using System.Collections.Generic;
using System.Text;

namespace MetaLinq.Generator {
    public class CodeBuilder {
        class NewLineState {
            public int? LastTabLevel;
        }

        public readonly CodeBuilder? Return;
        readonly StringBuilder builder;
        readonly int tabs;
        readonly NewLineState newLineState;
        int? LastTabLevel { get => newLineState.LastTabLevel; set => newLineState.LastTabLevel = value; }
        CodeBuilder? tab;

        public CodeBuilder Tab => tab ??= new CodeBuilder(builder, tabs + 1, this, newLineState);

        public CodeBuilder(StringBuilder builder) : this(builder, 0, null, new NewLineState()) { }
        CodeBuilder(StringBuilder builder, int tabs, CodeBuilder? @return, NewLineState newLineState) {
            this.builder = builder;
            this.tabs = tabs;
            Return = @return;
            this.newLineState = newLineState;
        }

        public CodeBuilder Append(string str) {
            BeforeAppend();
            builder.Append(str);
            return this;
        }
        public CodeBuilder Append(char character) {
            BeforeAppend();
            builder.Append(character);
            return this;
        }
        public CodeBuilder Append(string str, int statIndex, int count) {
            BeforeAppend();
            builder.Append(str, statIndex, count);
            return this;
        }
        public CodeBuilder AppendLine() {
            LastTabLevel = null;
            builder.Append(Environment.NewLine);
            return this;
        }
        void BeforeAppend() {
            if(LastTabLevel != null) {
                if(LastTabLevel != tabs)
                    throw new InvalidOperationException();
                return;
            }
            LastTabLevel = tabs;
            for(int i = 0; i < tabs; i++) {
                builder.Append("    ");
            }
        }
    }

    public static class CodeBuilderExtensions {
        public static CodeBuilder AppendIf(this CodeBuilder builder, bool condition, string str) => condition ? builder.Append(str) : builder;

        public static CodeBuilder AppendLine(this CodeBuilder builder, string str) => builder.Append(str).AppendLine();

        public static void AppendMultipleLines(this CodeBuilder builder, string lines, bool trimLeadingWhiteSpace = false) {
            foreach((int start, int length) in new LineEnumerator(lines, trimLeadingWhiteSpace)) {
                builder.Append(lines, start, length).AppendLine();
            }
        }

        public struct LineEnumerator {
            readonly string lines;
            readonly bool trimLeadingWhiteSpace;
            int startIndex;
            public (int start, int length) Current { get; private set; }

            public LineEnumerator(string source, bool trimLeadingWhiteSpace) {
                lines = source;
                this.trimLeadingWhiteSpace = trimLeadingWhiteSpace;
                Current = default;
                startIndex = 0;
            }
            public LineEnumerator GetEnumerator() {
                return this;
            }
            public bool MoveNext() {
                if(startIndex == lines.Length)
                    return false;
                int index = lines.IndexOf(Environment.NewLine, startIndex);
                if(index != -1) {
                    SetCurrent(startIndex, index);
                    startIndex = index + Environment.NewLine.Length;
                } else {
                    SetCurrent(startIndex, lines.Length);
                    startIndex = lines.Length;
                }
                return true;
            }
            void SetCurrent(int startIndex, int endIndex) {
                if(trimLeadingWhiteSpace) {
                    while(char.IsWhiteSpace(lines[startIndex])) {
                        startIndex++;
                    }
                }
                Current = (startIndex, endIndex - startIndex);
            }
        }

        public static CodeBuilder AppendFirstToUpperCase(this CodeBuilder builder, string str) {
            return builder.AppendChangeFirstCore(str, char.ToUpper(str[0]));
        }
        public static CodeBuilder AppendFirstToLowerCase(this CodeBuilder builder, string str) {
            return builder.AppendChangeFirstCore(str, char.ToLower(str[0]));
        }
        static CodeBuilder AppendChangeFirstCore(this CodeBuilder builder, string str, char firstChar) {
            return builder.Append(firstChar).Append(str, 1, str.Length - 1);
        }
        public static void AppendMultipleLinesWithSeparator(this CodeBuilder builder, IEnumerable<string> lines, string separator) {
            bool appendSeparator = false;
            foreach(string line in lines) {
                if(appendSeparator)
                    builder.Append(separator);
                builder.Append(line);
                appendSeparator = true;
            }
        }
    }
}
