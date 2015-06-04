﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.CSharp;
using System;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Reflection;
using Test2d;

namespace TestEDITOR
{
    /// <summary>
    /// 
    /// </summary>
    public class RoslynScriptEngine : IScriptEngine
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="context"></param>
        /// <param name="execute"></param>
        public void Eval(string code, object context, Action<Action> execute)
        {
            var path = System.IO.Path.GetDirectoryName(typeof(object).Assembly.Location);

            ScriptOptions options = ScriptOptions.Default
                .AddReferences(MetadataReference.CreateFromFile(System.IO.Path.Combine(path, "mscorlib.dll")))
                .AddReferences(MetadataReference.CreateFromFile(System.IO.Path.Combine(path, "System.dll")))
                .AddReferences(MetadataReference.CreateFromFile(System.IO.Path.Combine(path, "System.Core.dll")))
                .AddReferences(MetadataReference.CreateFromFile(System.IO.Path.Combine(path, "System.Runtime.dll")))
                .AddNamespaces("System")
                .AddNamespaces("System.Collections.Generic")
                .AddNamespaces("System.Text")
                .AddNamespaces("System.Threading")
                .AddNamespaces("System.Threading.Tasks")
                .AddReferences(Assembly.GetAssembly(typeof(ObservableCollection<>)))
                .AddNamespaces("System.Collections.ObjectModel")
                .AddReferences(Assembly.GetAssembly(typeof(ImmutableArray<>)))
                .AddNamespaces("System.Collections.Immutable")
                .AddReferences(Assembly.GetAssembly(typeof(System.Linq.Enumerable)))
                .AddNamespaces("System.Linq")
                .AddReferences(Assembly.GetAssembly(typeof(ObservableObject)))
                .AddNamespaces("Test2d")
                .AddReferences(Assembly.GetAssembly(typeof(EditorContext)))
                .AddNamespaces("TestEDITOR")
                .AddNamespaces("TestSIM")
                .AddNamespaces("Dxf");

            CSharpScript.Eval(
                code,
                options,
                new RolsynScriptGlobals()
                {
                    Context = context as EditorContext,
                    Execute = execute
                });
        }
    }

}
