using System;

namespace MatrixScreen
{
    public struct ColumnNode : IEquatable<ColumnNode>
    {
        public char character;
        public int position;
        public bool highLight;

        public bool Equals(ColumnNode other)
        {
            return this == other;
        }

        public static bool operator ==(ColumnNode a, ColumnNode b)
        {
            return a.character == b.character && a.highLight == b.highLight;
        }
        public static bool operator !=(ColumnNode a, ColumnNode b)
        {
            return !(a == b);
        }

        public override bool Equals(object obj)
        {
            return obj is ColumnNode && Equals((ColumnNode)obj);
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
