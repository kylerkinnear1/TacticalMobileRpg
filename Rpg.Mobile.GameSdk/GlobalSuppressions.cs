// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

#pragma warning disable IDE0076
[assembly: SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "No refactor available currently. Not sure its good yet. I wanted something that was basically a mutable record, instead of DI focused.", Target = "~M:Rpg.Mobile.GameSdk.EmbeddedResourceImageLoader.#ctor(Rpg.Mobile.GameSdk.EmbeddedResourceImageLoader.Options)")]
[assembly: SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "<Pending>", Scope = "member", Target = "~M:Rpg.Mobile.GameSdk.StateManagement.Subscription.Dispose")]
#pragma warning restore IDE0076
