using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;

namespace Tinja.Interception
{
    public static class ProxyUtil
    {
        const string AssemblyName = "Tinja.Interception.DynamicProxy";

        const string ModuleName = "ProxyModules";

        internal static ModuleBuilder ModuleBuilder { get; }

        internal static AssemblyBuilder AssemblyBuilder { get; }

        internal static ConcurrentDictionary<Type, Type> ProxyTypeCaches { get; }

        static ProxyUtil()
        {
            AssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(AssemblyName), AssemblyBuilderAccess.Run);
            ModuleBuilder = AssemblyBuilder.DefineDynamicModule(ModuleName);
            ProxyTypeCaches = new ConcurrentDictionary<Type, Type>();
        }

        public static Type GenerateProxyType(Type implType, Type baseType)
        {
            var baseTypeDescriptor = GetTypeMetadata(implType, baseType);
            var typeBuilder = DefineType(implType, baseType);

            return typeBuilder.CreateType();
        }

        internal static TypeBuilder DefineType(Type implType, Type baseType)
        {
            if (implType.IsInterface || implType.IsAbstract)
            {
                throw new NotSupportedException($"implemetion type:{implType.FullName} must not be interface and abstract!");
            }

            return
                ModuleBuilder.DefineType(
                   GetTypeName(baseType),
                   TypeAttributes.Class | TypeAttributes.Public,
                   baseType.IsInterface ? null : baseType,
                   baseType.IsInterface ? new[] { baseType } : null
               );
        }

        //internal static ConstructorBuilder[] DefineConstructors(TypeBuilder typeBuilder, ProxyTypeMetadata typeMetadata)
        //{
        //    var ctors = new List<ConstructorBuilder>();
        //    var fields = DefineFields(typeBuilder, baseType.Intereceptors.Concat());
        //    var ctor = typeBuilder.DefineConstructor(
        //        MethodAttributes.Public,
        //        CallingConventions.HasThis,
        //        parameterTypes
        //    );

        //    var il = ctor.GetILGenerator();

        //    for (var i = 0; i < parameterTypes.Length; i++)
        //    {
        //        il.Emit(OpCodes.Ldarg_0);
        //        il.Emit(OpCodes.Ldarg, i + 1);
        //        il.Emit(OpCodes.Stfld, fields[parameterTypes[i]]);
        //    }

        //    il.Emit(OpCodes.Ret);

        //    return ctor;
        //}

        //internal static ConstructorBuilder[] DefineConstrctor(TypeBuilder typeBuilder, ConstructorInfo baseConstructor)
        //{

        //}

        internal static Dictionary<Type, FieldBuilder> DefineFields(TypeBuilder typeBuilder, Type[] fieldTypes)
        {
            var map = new Dictionary<Type, FieldBuilder>();

            if (fieldTypes == null)
            {
                return map;
            }

            for (var i = 0; i < fieldTypes.Length; i++)
            {
                var fieldType = fieldTypes[i];
                if (fieldType == null)
                {
                    continue;
                }

                map[fieldType] = typeBuilder.DefineField(
                    "__" + fieldType.Name + i,
                    fieldTypes[i],
                    FieldAttributes.Private
                );
            }

            return map;
        }

        internal static ProxyTypeMetadata GetTypeMetadata(Type implType, Type baseType)
        {
            var typeIntereceptors = implType.GetCustomAttributes<InterceptorAttribute>();
            var members = new List<MemberMetadata>();

            foreach (var item in implType.GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                if (item.MemberType != MemberTypes.Method &&
                    item.MemberType != MemberTypes.Property)
                {
                    continue;
                }

                members.Add(new MemberMetadata(
                    item,
                    item
                    .GetCustomAttributes<InterceptorAttribute>()
                    .Concat(typeIntereceptors)
                    .Select(i => i.InterceptorType).Distinct()
                    .ToArray()
                ));
            }

            return new ProxyTypeMetadata(implType, baseType, members.ToArray());
        }

        static string GetTypeName(Type serviceType)
        {
            return AssemblyName + serviceType.Namespace + "." + serviceType.Name + Guid.NewGuid().ToString().Replace("-", "");
        }
    }

    public class ProxyTypeMetadata
    {
        public Type ImplType { get; }

        public Type[] Intereceptors { get; }

        public MemberMetadata[] Members { get; }

        public Type BaseType { get; }

        public ConstructorInfo[] BaseConstructors { get; }

        public ProxyTypeMetadata(Type implType, Type baseType, MemberMetadata[] members)
        {
            Members = members;
            BaseType = baseType;
            ImplType = implType;
            Intereceptors = Members.SelectMany(i => i.Intereceptors).Distinct().ToArray();
            BaseConstructors = baseType.IsInterface ? new ConstructorInfo[0] : baseType.GetConstructors();
        }
    }

}
