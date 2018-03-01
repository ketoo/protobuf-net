﻿using Google.Protobuf.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProtoBuf.Reflection
{
    /// <summary>
    /// A coded generator that writes C#
    /// </summary>
    public class CSharpCodeGenerator : CommonCodeGenerator
    {
        /// <summary>
        /// Reusable code-generator instance
        /// </summary>
        public static CSharpCodeGenerator Default { get; } = new CSharpCodeGenerator();
        /// <summary>
        /// Create a new CSharpCodeGenerator instance
        /// </summary>
        protected CSharpCodeGenerator() { }
        /// <summary>
        /// Returns the language name
        /// </summary>
        public override string Name => "C#";
        /// <summary>
        /// Returns the default file extension
        /// </summary>
        protected override string DefaultFileExtension => "cs";
        /// <summary>
        /// Escapes language keywords
        /// </summary>
        protected override string Escape(string identifier)
        {
            switch (identifier)
            {
                case "abstract":
                case "event":
                case "new":
                case "struct":
                case "as":
                case "explicit":
                case "null":
                case "switch":
                case "base":
                case "extern":
                case "object":
                case "this":
                case "bool":
                case "false":
                case "operator":
                case "throw":
                case "break":
                case "finally":
                case "out":
                case "true":
                case "byte":
                case "fixed":
                case "override":
                case "try":
                case "case":
                case "float":
                case "params":
                case "typeof":
                case "catch":
                case "for":
                case "private":
                case "uint":
                case "char":
                case "foreach":
                case "protected":
                case "ulong":
                case "checked":
                case "goto":
                case "public":
                case "unchecked":
                case "class":
                case "if":
                case "readonly":
                case "unsafe":
                case "const":
                case "implicit":
                case "ref":
                case "ushort":
                case "continue":
                case "in":
                case "return":
                case "using":
                case "decimal":
                case "int":
                case "sbyte":
                case "virtual":
                case "default":
                case "interface":
                case "sealed":
                case "volatile":
                case "delegate":
                case "internal":
                case "short":
                case "void":
                case "do":
                case "is":
                case "sizeof":
                case "while":
                case "double":
                case "lock":
                case "stackalloc":
                case "else":
                case "long":
                case "static":
                case "enum":
                case "namespace":
                case "string":
                    return "@" + identifier;
                default:
                    return identifier;
            }
        }

        /// <summary>
        /// Get the language version for this language from a schema
        /// </summary>
        protected override string GetLanguageVersion(FileDescriptorProto obj)
            => obj?.Options?.GetOptions()?.CSharpLanguageVersion;

        /// <summary>
        /// Start a file
        /// </summary>
        protected override void WriteFileHeader(GeneratorContext ctx, FileDescriptorProto file, ref object state)
        {
            var prefix = ctx.Supports(CSharp6) ? "CS" : "";
            ctx.WriteLine("// This file was generated by a tool; you should avoid making direct changes.")
               .WriteLine("// Consider using 'partial classes' to extend these types")
               .WriteLine($"// Input: {Path.GetFileName(ctx.File.Name)}").WriteLine()
               .WriteLine($"#pragma warning disable {prefix}1591, {prefix}0612, {prefix}3021").WriteLine();


            var @namespace = ctx.NameNormalizer.GetName(file);

            if (!string.IsNullOrWhiteSpace(@namespace))
            {
                state = @namespace;
                ctx.WriteLine($"namespace {@namespace}");
                ctx.WriteLine("{").Indent().WriteLine();
            }

        }
        /// <summary>
        /// End a file
        /// </summary>
        protected override void WriteFileFooter(GeneratorContext ctx, FileDescriptorProto file, ref object state)
        {
            var @namespace = (string)state;
            var prefix = ctx.Supports(CSharp6) ? "CS" : "";
            if (!string.IsNullOrWhiteSpace(@namespace))
            {
                ctx.Outdent().WriteLine("}").WriteLine();
            }
            ctx.WriteLine($"#pragma warning restore {prefix}1591, {prefix}0612, {prefix}3021");
        }
        /// <summary>
        /// Start an enum
        /// </summary>
        protected override void WriteEnumHeader(GeneratorContext ctx, EnumDescriptorProto obj, ref object state)
        {
            var name = ctx.NameNormalizer.GetName(obj);
            var tw = ctx.Write($@"[global::ProtoBuf.ProtoContract(");
            if (name != obj.Name) tw.Write($@"Name = @""{obj.Name}""");
            tw.WriteLine(")]");
            WriteOptions(ctx, obj.Options);
            ctx.WriteLine($"{GetAccess(GetAccess(obj))} enum {Escape(name)}").WriteLine("{").Indent();
        }
        /// <summary>
        /// End an enum
        /// </summary>

        protected override void WriteEnumFooter(GeneratorContext ctx, EnumDescriptorProto obj, ref object state)
        {
            ctx.Outdent().WriteLine("}").WriteLine();
        }
        /// <summary>
        /// Write an enum value
        /// </summary>
        protected override void WriteEnumValue(GeneratorContext ctx, EnumValueDescriptorProto obj, ref object state)
        {
            var name = ctx.NameNormalizer.GetName(obj);
            if (name != obj.Name)
            {
                var tw = ctx.Write($@"[global::ProtoBuf.ProtoEnum(");
                tw.Write($@"Name = @""{obj.Name}""");
                tw.WriteLine(")]");
            }
            
            WriteOptions(ctx, obj.Options);
            ctx.WriteLine($"{Escape(name)} = {obj.Number},");
        }

        /// <summary>
        /// End a message
        /// </summary>
        protected override void WriteMessageFooter(GeneratorContext ctx, DescriptorProto obj, ref object state)
        {
            ctx.Outdent().WriteLine("}").WriteLine();
        }
        /// <summary>
        /// Start a message
        /// </summary>
        protected override void WriteMessageHeader(GeneratorContext ctx, DescriptorProto obj, ref object state)
        {
            var name = ctx.NameNormalizer.GetName(obj);
            var tw = ctx.Write($@"[global::ProtoBuf.ProtoContract(");
            if (name != obj.Name) tw.Write($@"Name = @""{obj.Name}""");
            tw.WriteLine(")]");
            WriteOptions(ctx, obj.Options);
            tw = ctx.Write($"{GetAccess(GetAccess(obj))} partial class {Escape(name)}");
            tw.Write(" : global::ProtoBuf.IExtensible");
            tw.WriteLine();
            ctx.WriteLine("{").Indent();
            if (obj.Options?.MessageSetWireFormat == true)
            {
                ctx.WriteLine("#error message_set_wire_format is not currently implemented").WriteLine();
            }
            
            ctx.WriteLine($"private global::ProtoBuf.IExtension {FieldPrefix}extensionData;")
                .WriteLine($"global::ProtoBuf.IExtension global::ProtoBuf.IExtensible.GetExtensionObject(bool createIfMissing)");

            if (ctx.Supports(CSharp6))
            {
                ctx.Indent().WriteLine($"=> global::ProtoBuf.Extensible.GetExtensionObject(ref {FieldPrefix}extensionData, createIfMissing);").Outdent().WriteLine();
            }
            else
            {
                ctx.WriteLine("{").Indent().WriteLine($"return global::ProtoBuf.Extensible.GetExtensionObject(ref {FieldPrefix}extensionData, createIfMissing);").Outdent().WriteLine("}");
            }
        }

        private static void WriteOptions<T>(GeneratorContext ctx, T obj) where T : class, ISchemaOptions
        {
            if (obj == null) return;
            if (obj.Deprecated)
            {
                ctx.WriteLine($"[global::System.Obsolete]");
            }
        }

        const string FieldPrefix = "__pbn__";

        /// <summary>
        /// Get the language specific keyword representing an access level
        /// </summary>
        public override string GetAccess(Access access)
        {
            switch (access)
            {
                case Access.Internal: return "internal";
                case Access.Public: return "public";
                case Access.Private: return "private";
                default: return base.GetAccess(access);
            }
        }


        /// <summary>
        /// Emit code beginning a constructor, if one is required
        /// </summary>
        /// <returns>true if a constructor is required</returns>
        protected override bool WriteContructorHeader(GeneratorContext ctx, DescriptorProto obj, ref object state)
        {
            if (ctx.Supports(CSharp6)) return false;

            var name = ctx.NameNormalizer.GetName(obj);
            ctx.WriteLine($"public {Escape(name)}()") // note: the .ctor is still public even if the type is internal; it is protected by the scope
                .WriteLine("{").Indent();
            return true;
        }

        /// <summary>
        /// Emit code terminating a constructor, if one is required
        /// </summary>
        protected override void WriteConstructorFooter(GeneratorContext ctx, DescriptorProto obj, ref object state)
        {
            ctx.WriteLine("OnConstructor();")
                .Outdent().WriteLine("}").WriteLine()
                .WriteLine("partial void OnConstructor();")
                .WriteLine();
        }

        /// <summary>
        /// Emit code initializing field values inside a constructor, if one is required
        /// </summary>
        protected override void WriteInitField(GeneratorContext ctx, FieldDescriptorProto obj, ref object state, OneOfStub[] oneOfs)
        {
            var name = ctx.NameNormalizer.GetName(obj);
            bool isOptional = obj.label == FieldDescriptorProto.Label.LabelOptional;
            bool isRepeated = obj.label == FieldDescriptorProto.Label.LabelRepeated;
            var typeName = GetTypeName(ctx, obj, out var dataFormat, out var isMap);
            OneOfStub oneOf = obj.ShouldSerializeOneofIndex() ? oneOfs?[obj.OneofIndex] : null;
            bool explicitValues = isOptional && oneOf == null && ctx.Syntax == FileDescriptorProto.SyntaxProto2
                    && obj.type != FieldDescriptorProto.Type.TypeMessage
                    && obj.type != FieldDescriptorProto.Type.TypeGroup;

            string defaultValue = GetDefaultValue(ctx, obj, typeName);

            if (isRepeated)
            {
                var mapMsgType = isMap ? ctx.TryFind<DescriptorProto>(obj.TypeName) : null;
                if (mapMsgType != null)
                {
                    var keyTypeName = GetTypeName(ctx, mapMsgType.Fields.Single(x => x.Number == 1),
                        out var keyDataFormat, out var _);
                    var valueTypeName = GetTypeName(ctx, mapMsgType.Fields.Single(x => x.Number == 2),
                        out var valueDataFormat, out var _);
                    ctx.WriteLine($"{Escape(name)} = new global::System.Collections.Generic.Dictionary<{keyTypeName}, {valueTypeName}>();");
                }
                else if (UseArray(obj))
                { } // nothing needed
                else
                {
                    ctx.WriteLine($"{Escape(name)} = new global::System.Collections.Generic.List<{typeName}>();");
                }
            }
            else if (oneOf != null)
            { } // nothing to do
            else if(explicitValues)
            { } // nothing to do
            else
            {
                if (!string.IsNullOrWhiteSpace(defaultValue))
                {
                    ctx.WriteLine($"{Escape(name)} = {defaultValue};");
                }
            }
            
        }

        private string GetDefaultValue(GeneratorContext ctx, FieldDescriptorProto obj, string typeName)
        {
            string defaultValue = null;
            bool isOptional = obj.label == FieldDescriptorProto.Label.LabelOptional;

            if (isOptional || ctx.EmitRequiredDefaults || obj.type == FieldDescriptorProto.Type.TypeEnum)
            {
                defaultValue = obj.DefaultValue;

                if (obj.type == FieldDescriptorProto.Type.TypeString)
                {
                    defaultValue = string.IsNullOrEmpty(defaultValue) ? "\"\""
                        : ("@\"" + (defaultValue ?? "").Replace("\"", "\"\"") + "\"");
                }
                else if (obj.type == FieldDescriptorProto.Type.TypeDouble)
                {
                    switch (defaultValue)
                    {
                        case "inf": defaultValue = "double.PositiveInfinity"; break;
                        case "-inf": defaultValue = "double.NegativeInfinity"; break;
                        case "nan": defaultValue = "double.NaN"; break;
                    }
                }
                else if (obj.type == FieldDescriptorProto.Type.TypeFloat)
                {
                    switch (defaultValue)
                    {
                        case "inf": defaultValue = "float.PositiveInfinity"; break;
                        case "-inf": defaultValue = "float.NegativeInfinity"; break;
                        case "nan": defaultValue = "float.NaN"; break;
                    }
                }
                else if (obj.type == FieldDescriptorProto.Type.TypeEnum)
                {
                    var enumType = ctx.TryFind<EnumDescriptorProto>(obj.TypeName);
                    if (enumType != null)
                    {
                        EnumValueDescriptorProto found = null;
                        if (!string.IsNullOrEmpty(defaultValue))
                        {
                            found = enumType.Values.FirstOrDefault(x => x.Name == defaultValue);
                        }
                        else if (ctx.Syntax == FileDescriptorProto.SyntaxProto2)
                        {
                            // find the first one; if that is a zero, we don't need it after all
                            found = enumType.Values.FirstOrDefault();
                            if (found != null && found.Number == 0)
                            {
                                if (!isOptional) found = null; // we don't need it after all
                            }
                        }
                        // for proto3 the default is 0, so no need to do anything - GetValueOrDefault() will do it all

                        if (found != null)
                        {
                            defaultValue = ctx.NameNormalizer.GetName(found);
                        }
                        if (!string.IsNullOrWhiteSpace(defaultValue))
                        {
                            defaultValue = typeName + "." + defaultValue;
                        }
                    }
                }
            }

            return defaultValue;
        }
        /// <summary>
        /// Write a field
        /// </summary>
        protected override void WriteField(GeneratorContext ctx, FieldDescriptorProto obj, ref object state, OneOfStub[] oneOfs)
        {
            var name = ctx.NameNormalizer.GetName(obj);
            var tw = ctx.Write($@"[global::ProtoBuf.ProtoMember({obj.Number}");
            if (name != obj.Name)
            {
                tw.Write($@", Name = @""{obj.Name}""");
            }
            var options = obj.Options?.GetOptions();
            if (options?.AsReference == true)
            {
                tw.Write($@", AsReference = true");
            }
            if (options?.DynamicType == true)
            {
                tw.Write($@", DynamicType = true");
            }

            bool isOptional = obj.label == FieldDescriptorProto.Label.LabelOptional;
            bool isRepeated = obj.label == FieldDescriptorProto.Label.LabelRepeated;

            OneOfStub oneOf = obj.ShouldSerializeOneofIndex() ? oneOfs?[obj.OneofIndex] : null;
            if (oneOf != null && oneOf.CountTotal == 1)
            {
                oneOf = null; // not really a one-of, then!
            }
            bool explicitValues = isOptional && oneOf == null && ctx.Syntax == FileDescriptorProto.SyntaxProto2
                && obj.type != FieldDescriptorProto.Type.TypeMessage
                && obj.type != FieldDescriptorProto.Type.TypeGroup;
            
            bool suppressDefaultAttribute = !isOptional;
            var typeName = GetTypeName(ctx, obj, out var dataFormat, out var isMap);
            string defaultValue = GetDefaultValue(ctx, obj, typeName);


            if (!string.IsNullOrWhiteSpace(dataFormat))
            {
                tw.Write($", DataFormat = global::ProtoBuf.DataFormat.{dataFormat}");
            }
            if (obj.IsPacked(ctx.Syntax))
            {
                tw.Write($", IsPacked = true");
            }
            if (obj.label == FieldDescriptorProto.Label.LabelRequired)
            {
                tw.Write($", IsRequired = true");
            }
            tw.WriteLine(")]");
            if (!isRepeated && !string.IsNullOrWhiteSpace(defaultValue) && !suppressDefaultAttribute)
            {
                ctx.WriteLine($"[global::System.ComponentModel.DefaultValue({defaultValue})]");
            }
            WriteOptions(ctx, obj.Options);
            if (isRepeated)
            {
                var mapMsgType = isMap ? ctx.TryFind<DescriptorProto>(obj.TypeName) : null;
                if (mapMsgType != null)
                {
                    var keyTypeName = GetTypeName(ctx, mapMsgType.Fields.Single(x => x.Number == 1),
                        out var keyDataFormat, out var _);
                    var valueTypeName = GetTypeName(ctx, mapMsgType.Fields.Single(x => x.Number == 2),
                        out var valueDataFormat, out var _);

                    bool first = true;
                    tw = ctx.Write($"[global::ProtoBuf.ProtoMap");
                    if (!string.IsNullOrWhiteSpace(keyDataFormat))
                    {
                        tw.Write($"{(first ? "(" : ", ")}KeyFormat = global::ProtoBuf.DataFormat.{keyDataFormat}");
                        first = false;
                    }
                    if (!string.IsNullOrWhiteSpace(valueDataFormat))
                    {
                        tw.Write($"{(first ? "(" : ", ")}ValueFormat = global::ProtoBuf.DataFormat.{valueDataFormat}");
                        first = false;
                    }
                    tw.WriteLine(first ? "]" : ")]");
                    if (ctx.Supports(CSharp6))
                    {
                        ctx.WriteLine($"{GetAccess(GetAccess(obj))} global::System.Collections.Generic.Dictionary<{keyTypeName}, {valueTypeName}> {Escape(name)} {{ get; }} = new global::System.Collections.Generic.Dictionary<{keyTypeName}, {valueTypeName}>();");
                    }
                    else
                    {
                        ctx.WriteLine($"{GetAccess(GetAccess(obj))} global::System.Collections.Generic.Dictionary<{keyTypeName}, {valueTypeName}> {Escape(name)} {{ get; private set; }}");
                    }
                }
                else if (UseArray(obj))
                {
                    ctx.WriteLine($"{GetAccess(GetAccess(obj))} {typeName}[] {Escape(name)} {{ get; set; }}");
                }
                else if(ctx.Supports(CSharp6))
                {
                    ctx.WriteLine($"{GetAccess(GetAccess(obj))} global::System.Collections.Generic.List<{typeName}> {Escape(name)} {{ get; }} = new global::System.Collections.Generic.List<{typeName}>();");
                }
                else
                {
                    ctx.WriteLine($"{GetAccess(GetAccess(obj))} global::System.Collections.Generic.List<{typeName}> {Escape(name)} {{ get; private set; }}");
                }
            }
            else if (oneOf != null)
            {
                var defValue = string.IsNullOrWhiteSpace(defaultValue) ? $"default({typeName})" : defaultValue;
                var fieldName = FieldPrefix + oneOf.OneOf.Name;
                var storage = oneOf.GetStorage(obj.type, obj.TypeName);
                ctx.WriteLine($"{GetAccess(GetAccess(obj))} {typeName} {Escape(name)}").WriteLine("{").Indent();

                switch (obj.type)
                {
                    case FieldDescriptorProto.Type.TypeMessage:
                    case FieldDescriptorProto.Type.TypeGroup:
                    case FieldDescriptorProto.Type.TypeEnum:
                    case FieldDescriptorProto.Type.TypeBytes:
                    case FieldDescriptorProto.Type.TypeString:
                        ctx.WriteLine($"get {{ return {fieldName}.Is({obj.Number}) ? (({typeName}){fieldName}.{storage}) : {defValue}; }}");
                        break;
                    default:
                        ctx.WriteLine($"get {{ return {fieldName}.Is({obj.Number}) ? {fieldName}.{storage} : {defValue}; }}");
                        break;
                }
                var unionType = oneOf.GetUnionType();
                ctx.WriteLine($"set {{ {fieldName} = new global::ProtoBuf.{unionType}({obj.Number}, value); }}")
                    .Outdent().WriteLine("}");

                if (ctx.Supports(CSharp6))
                {
                    ctx.WriteLine($"{GetAccess(GetAccess(obj))} bool ShouldSerialize{name}() => {fieldName}.Is({obj.Number});")
                    .WriteLine($"{GetAccess(GetAccess(obj))} void Reset{name}() => global::ProtoBuf.{unionType}.Reset(ref {fieldName}, {obj.Number});");
                }
                else
                {
                    ctx.WriteLine($"{GetAccess(GetAccess(obj))} bool ShouldSerialize{name}()").WriteLine("{").Indent()
                        .WriteLine($"return {fieldName}.Is({obj.Number});").Outdent().WriteLine("}")
                        .WriteLine($"{GetAccess(GetAccess(obj))} void Reset{name}()").WriteLine("{").Indent()
                        .WriteLine($"global::ProtoBuf.{unionType}.Reset(ref {fieldName}, {obj.Number});").Outdent().WriteLine("}");
                }

                if (oneOf.IsFirst())
                {
                    ctx.WriteLine().WriteLine($"private global::ProtoBuf.{unionType} {fieldName};");
                }
            }
            else if (explicitValues)
            {
                string fieldName = FieldPrefix + name, fieldType;
                bool isRef = false;
                switch (obj.type)
                {
                    case FieldDescriptorProto.Type.TypeString:
                    case FieldDescriptorProto.Type.TypeBytes:
                        fieldType = typeName;
                        isRef = true;
                        break;
                    default:
                        fieldType = typeName + "?";
                        break;
                }
                ctx.WriteLine($"{GetAccess(GetAccess(obj))} {typeName} {Escape(name)}").WriteLine("{").Indent();
                tw = ctx.Write($"get {{ return {fieldName}");
                if (!string.IsNullOrWhiteSpace(defaultValue))
                {
                    tw.Write(" ?? ");
                    tw.Write(defaultValue);
                }
                else if (!isRef)
                {
                    tw.Write(".GetValueOrDefault()");
                }
                tw.WriteLine("; }");
                ctx.WriteLine($"set {{ {fieldName} = value; }}")
                    .Outdent().WriteLine("}");
                if (ctx.Supports(CSharp6))
                {
                    ctx.WriteLine($"{GetAccess(GetAccess(obj))} bool ShouldSerialize{name}() => {fieldName} != null;")
                    .WriteLine($"{GetAccess(GetAccess(obj))} void Reset{name}() => {fieldName} = null;");
                }
                else
                {
                    ctx.WriteLine($"{GetAccess(GetAccess(obj))} bool ShouldSerialize{name}()").WriteLine("{").Indent()
                        .WriteLine($"return {fieldName} != null;").Outdent().WriteLine("}")
                        .WriteLine($"{GetAccess(GetAccess(obj))} void Reset{name}()").WriteLine("{").Indent()
                        .WriteLine($"{fieldName} = null;").Outdent().WriteLine("}");
                }
                ctx.WriteLine($"private {fieldType} {fieldName};");
            }
            else
            {
                tw = ctx.Write($"{GetAccess(GetAccess(obj))} {typeName} {Escape(name)} {{ get; set; }}");
                if (!string.IsNullOrWhiteSpace(defaultValue) && ctx.Supports(CSharp6)) tw.Write($" = {defaultValue};");
                tw.WriteLine();
            }
            ctx.WriteLine();
        }

        static readonly Version CSharp6 = new Version(6, 0);

        /// <summary>
        /// Starts an extgensions block
        /// </summary>
        protected override void WriteExtensionsHeader(GeneratorContext ctx, FileDescriptorProto obj, ref object state)
        {
            var name = obj?.Options?.GetOptions()?.ExtensionTypeName;
            if (string.IsNullOrWhiteSpace(name)) name = "Extensions";
            ctx.WriteLine($"{GetAccess(GetAccess(obj))} static class {Escape(name)}").WriteLine("{").Indent();
        }
        /// <summary>
        /// Ends an extgensions block
        /// </summary>
        protected override void WriteExtensionsFooter(GeneratorContext ctx, FileDescriptorProto obj, ref object state)
        {
            ctx.Outdent().WriteLine("}");
        }
        /// <summary>
        /// Starts an extensions block
        /// </summary>
        protected override void WriteExtensionsHeader(GeneratorContext ctx, DescriptorProto obj, ref object state)
        {
            var name = obj?.Options?.GetOptions()?.ExtensionTypeName;
            if (string.IsNullOrWhiteSpace(name)) name = "Extensions";
            ctx.WriteLine($"{GetAccess(GetAccess(obj))} static class {Escape(name)}").WriteLine("{").Indent();
        }
        /// <summary>
        /// Ends an extensions block
        /// </summary>
        protected override void WriteExtensionsFooter(GeneratorContext ctx, DescriptorProto obj, ref object state)
        {
            ctx.Outdent().WriteLine("}");
        }
        /// <summary>
        /// Write an extension
        /// </summary>
        protected override void WriteExtension(GeneratorContext ctx, FieldDescriptorProto field)
        {
            var type = GetTypeName(ctx, field, out string dataFormat, out bool isMap);

            if (isMap)
            {
                ctx.WriteLine("#error map extensions not yet implemented");
            }
            else if (field.label == FieldDescriptorProto.Label.LabelRepeated)
            {
                ctx.WriteLine("#error repeated extensions not yet implemented");
            }
            else
            {
                var msg = ctx.TryFind<DescriptorProto>(field.Extendee);
                var extendee = MakeRelativeName(field, msg, ctx.NameNormalizer);

                var @this = field.Parent is FileDescriptorProto ? "this " : "";
                string name = ctx.NameNormalizer.GetName(field);
                ctx.WriteLine($"{GetAccess(GetAccess(field))} static {type} Get{name}({@this}{extendee} obj)");

                TextWriter tw;
                if (ctx.Supports(CSharp6))
                {
                    tw = ctx.Indent().Write($"=> ");
                }
                else
                {
                    ctx.WriteLine("{").Indent();
                    tw = ctx.Write("return ");
                }
                tw.Write($"obj == null ? default({type}) : global::ProtoBuf.Extensible.GetValue<{type}>(obj, {field.Number}");
                if (!string.IsNullOrEmpty(dataFormat))
                {
                    tw.Write($", global::ProtoBuf.DataFormat.{dataFormat}");
                }
                tw.WriteLine(");");
                if(ctx.Supports(CSharp6))
                {
                    ctx.Outdent().WriteLine();
                }
                else
                {
                    ctx.Outdent().WriteLine("}").WriteLine();   
                }

                //  GetValue<TValue>(IExtensible instance, int tag, DataFormat format)
            }
        }

        private static bool UseArray(FieldDescriptorProto field)
        {
            switch (field.type)
            {
                case FieldDescriptorProto.Type.TypeBool:
                case FieldDescriptorProto.Type.TypeDouble:
                case FieldDescriptorProto.Type.TypeFixed32:
                case FieldDescriptorProto.Type.TypeFixed64:
                case FieldDescriptorProto.Type.TypeFloat:
                case FieldDescriptorProto.Type.TypeInt32:
                case FieldDescriptorProto.Type.TypeInt64:
                case FieldDescriptorProto.Type.TypeSfixed32:
                case FieldDescriptorProto.Type.TypeSfixed64:
                case FieldDescriptorProto.Type.TypeSint32:
                case FieldDescriptorProto.Type.TypeSint64:
                case FieldDescriptorProto.Type.TypeUint32:
                case FieldDescriptorProto.Type.TypeUint64:
                    return true;
                default:
                    return false;
            }
        }

        private string GetTypeName(GeneratorContext ctx, FieldDescriptorProto field, out string dataFormat, out bool isMap)
        {
            dataFormat = "";
            isMap = false;
            switch (field.type)
            {
                case FieldDescriptorProto.Type.TypeDouble:
                    return "double";
                case FieldDescriptorProto.Type.TypeFloat:
                    return "float";
                case FieldDescriptorProto.Type.TypeBool:
                    return "bool";
                case FieldDescriptorProto.Type.TypeString:
                    return "string";
                case FieldDescriptorProto.Type.TypeSint32:
                    dataFormat = nameof(DataFormat.ZigZag);
                    return "int";
                case FieldDescriptorProto.Type.TypeInt32:
                    return "int";
                case FieldDescriptorProto.Type.TypeSfixed32:
                    dataFormat = nameof(DataFormat.FixedSize);
                    return "int";
                case FieldDescriptorProto.Type.TypeSint64:
                    dataFormat = nameof(DataFormat.ZigZag);
                    return "long";
                case FieldDescriptorProto.Type.TypeInt64:
                    return "long";
                case FieldDescriptorProto.Type.TypeSfixed64:
                    dataFormat = nameof(DataFormat.FixedSize);
                    return "long";
                case FieldDescriptorProto.Type.TypeFixed32:
                    dataFormat = nameof(DataFormat.FixedSize);
                    return "uint";
                case FieldDescriptorProto.Type.TypeUint32:
                    return "uint";
                case FieldDescriptorProto.Type.TypeFixed64:
                    dataFormat = nameof(DataFormat.FixedSize);
                    return "ulong";
                case FieldDescriptorProto.Type.TypeUint64:
                    return "ulong";
                case FieldDescriptorProto.Type.TypeBytes:
                    return "byte[]";
                case FieldDescriptorProto.Type.TypeEnum:
                    switch (field.TypeName)
                    {
                        case ".bcl.DateTime.DateTimeKind":
                            return "global::System.DateTimeKind";
                    }
                    var enumType = ctx.TryFind<EnumDescriptorProto>(field.TypeName);
                    return MakeRelativeName(field, enumType, ctx.NameNormalizer);
                case FieldDescriptorProto.Type.TypeGroup:
                case FieldDescriptorProto.Type.TypeMessage:
                    switch (field.TypeName)
                    {
                        case WellKnownTypeTimestamp:
                            dataFormat = "WellKnown";
                            return "global::System.DateTime?";
                        case WellKnownTypeDuration:
                            dataFormat = "WellKnown";
                            return "global::System.TimeSpan?";
                        case ".bcl.NetObjectProxy":
                            return "object";
                        case ".bcl.DateTime":
                            return "global::System.DateTime?";
                        case ".bcl.TimeSpan":
                            return "global::System.TimeSpan?";
                        case ".bcl.Decimal":
                            return "decimal?";
                        case ".bcl.Guid":
                            return "global::System.Guid?";
                    }
                    var msgType = ctx.TryFind<DescriptorProto>(field.TypeName);
                    if (field.type == FieldDescriptorProto.Type.TypeGroup)
                    {
                        dataFormat = nameof(DataFormat.Group);
                    }
                    isMap = msgType?.Options?.MapEntry ?? false;
                    return MakeRelativeName(field, msgType, ctx.NameNormalizer);
                default:
                    return field.TypeName;
            }
        }

        private string MakeRelativeName(FieldDescriptorProto field, IType target, NameNormalizer normalizer)
        {
            if (target == null) return Escape(field.TypeName); // the only thing we know

            var declaringType = field.Parent;

            if (declaringType is IType)
            {
                var name = FindNameFromCommonAncestor((IType)declaringType, target, normalizer);
                if (!string.IsNullOrWhiteSpace(name)) return name;
            }
            return Escape(field.TypeName); // give up!
        }

        // k, what we do is; we have two types; each knows the parent, but nothing else, so:
        // for each, use a stack to build the ancestry tree - the "top" of the stack will be the
        // package, the bottom of the stack will be the type itself. They will often be stacks
        // of different heights.
        //
        // Find how many is in the smallest stack; now take that many items, in turn, until we
        // get something that is different (at which point, put that one back on the stack), or 
        // we run out of items in one of the stacks.
        //
        // There are now two options:
        // - we ran out of things in the "target" stack - in which case, they are common enough to not
        //   need any resolution - just give back the fixed name
        // - we have things left in the "target" stack - in which case we have found a common ancestor,
        //   or the target is a descendent; either way, just concat what is left (including the package
        //   if the package itself was different)

        private string FindNameFromCommonAncestor(IType declaring, IType target, NameNormalizer normalizer)
        {
            // trivial case; asking for self, or asking for immediate child
            if (ReferenceEquals(declaring, target) || ReferenceEquals(declaring, target.Parent))
            {
                if (target is DescriptorProto) return Escape(normalizer.GetName((DescriptorProto)target));
                if (target is EnumDescriptorProto) return Escape(normalizer.GetName((EnumDescriptorProto)target));
                return null;
            }

            var origTarget = target;
            var xStack = new Stack<IType>();

            while (declaring != null)
            {
                xStack.Push(declaring);
                declaring = declaring.Parent;
            }
            var yStack = new Stack<IType>();

            while (target != null)
            {
                yStack.Push(target);
                target = target.Parent;
            }
            int lim = Math.Min(xStack.Count, yStack.Count);
            for (int i = 0; i < lim; i++)
            {
                declaring = xStack.Peek();
                target = yStack.Pop();
                if (!ReferenceEquals(target, declaring))
                {
                    // special-case: if both are the package (file), and they have the same namespace: we're OK
                    if (target is FileDescriptorProto && declaring is FileDescriptorProto &&
                        normalizer.GetName((FileDescriptorProto)declaring) == normalizer.GetName((FileDescriptorProto)target))
                    {
                        // that's fine, keep going
                    }
                    else
                    {
                        // put it back
                        yStack.Push(target);
                        break;
                    }
                }
            }
            // if we used everything, then the target is an ancestor-or-self
            if (yStack.Count == 0)
            {
                target = origTarget;
                if (target is DescriptorProto) return Escape(normalizer.GetName((DescriptorProto)target));
                if (target is EnumDescriptorProto) return Escape(normalizer.GetName((EnumDescriptorProto)target));
                return null;
            }

            var sb = new StringBuilder();
            while (yStack.Count != 0)
            {
                target = yStack.Pop();

                string nextName;
                if (target is FileDescriptorProto) nextName = normalizer.GetName((FileDescriptorProto)target);
                else if (target is DescriptorProto) nextName = normalizer.GetName((DescriptorProto)target);
                else if (target is EnumDescriptorProto) nextName = normalizer.GetName((EnumDescriptorProto)target);
                else return null;

                if (!string.IsNullOrWhiteSpace(nextName))
                {
                    if (sb.Length == 0 && target is FileDescriptorProto) sb.Append("global::");
                    else if (sb.Length != 0) sb.Append('.');
                    sb.Append(Escape(nextName));
                }
            }
            return sb.ToString();
        }

        static bool IsAncestorOrSelf(IType parent, IType child)
        {
            while (parent != null)
            {
                if (ReferenceEquals(parent, child)) return true;
                parent = parent.Parent;
            }
            return false;
        }
        const string WellKnownTypeTimestamp = ".google.protobuf.Timestamp",
                     WellKnownTypeDuration = ".google.protobuf.Duration";
    }
}
