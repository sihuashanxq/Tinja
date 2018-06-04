﻿using System.Reflection;

namespace Tinja.Resolving
{
    public class TypeConstructor
    {
        public ConstructorInfo ConstructorInfo { get; }

        public ParameterInfo[] Paramters { get; }

        public TypeConstructor(ConstructorInfo constructor, ParameterInfo[] parameters)
        {
            ConstructorInfo = constructor;
            Paramters = parameters;
        }
    }
}