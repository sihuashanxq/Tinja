using System.Reflection;

namespace Tinja.Interception
{
    public class InterceptionMemberMetadata
    {
        public int Priority { get; set; }

        public MemberInfo Member { get; set; }

        public override int GetHashCode()
        {
            return Member.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj is MemberPriority m)
            {
                return m.Target == Member;
            }

            return false;
        }
    }
}
