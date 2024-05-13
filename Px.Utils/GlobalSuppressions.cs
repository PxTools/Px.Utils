// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Major Bug",
    "S2583:Conditionally executed code should be reachable",
    Justification = "False positive",
    Scope = "member",
    Target = "~M:PxUtils.Models.Metadata.ExtensionMethods.PropertyExtensions.ParseStringList(System.String,System.Char,System.Char)~System.Collections.Generic.List{System.String}"
    )]
