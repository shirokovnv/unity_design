using System;
using UnityEngine;

namespace Architecture
{
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class ProvideAttribute : PropertyAttribute
    {
    }
}