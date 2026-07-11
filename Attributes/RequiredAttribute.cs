using System;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class RequiredAttribute : Attribute
{
}