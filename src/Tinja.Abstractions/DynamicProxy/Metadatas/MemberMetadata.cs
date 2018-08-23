using System.Reflection;

namespace Tinja.Abstractions.DynamicProxy.Metadatas
{
    public class MemberMetadata
    {
        public static readonly MemberMetadata[] EmptyMembers = new MemberMetadata[0];

        public MemberInfo Member { get; set; }

        public MemberMetadata[] InterfaceInherits { get; set; }

        public bool IsEvent =>
            Member.MemberType == MemberTypes.Event;

        public bool IsMethod =>
            Member.MemberType == MemberTypes.Method;

        public bool IsProperty =>
            Member.MemberType == MemberTypes.Property;

        public MemberMetadata(MemberInfo memberInfo)
        {
            Member = memberInfo;
            InterfaceInherits = EmptyMembers;
        }
    }
}
