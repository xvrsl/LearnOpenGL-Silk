
using Silk.NET.Maths;

namespace Common
{

    public static class Matrix4X4Extensions
    {
        public static Span<T> ToSpan<T>(this Matrix4X4<T> matrix) where T : unmanaged, IFormattable, IEquatable<T>, IComparable<T>
        {
            return new T[]{
            matrix[0,0],matrix[0,1],matrix[0,2],matrix[0,3],
            matrix[1,0],matrix[1,1],matrix[1,2],matrix[1,3],
            matrix[2,0],matrix[2,1],matrix[2,2],matrix[2,3],
            matrix[3,0],matrix[3,1],matrix[3,2],matrix[3,3],
        };
        }
    }
}