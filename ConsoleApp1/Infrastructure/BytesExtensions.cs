using System;
using System.Collections.Generic;
using System.Linq;

namespace Organizer
{
    public static class BytesExtensions
    {
        private static IEnumerable<byte> ToBytes(this int num) => BitConverter.GetBytes(num);

        private static int ToInt(this byte[] bytes) => BitConverter.ToInt32(bytes);
        
        public static byte[] ToBytes(this State state)
        {
            return state.UserId.ToBytes()
                .Concat(state.StateId.ToBytes())
                .Concat(state.SubStateId.ToBytes())
                .ToArray();
        }
        
        public static State ToState(this byte[] bytes)
        {
            var result = new List<int>();
            for (var i = 0; i < bytes.Length; i++)
            {
                var j = i * 4;
                result.Add(new byte[] {bytes[j], bytes[j + 1], bytes[j + 2], bytes[j + 3]}.ToInt());
            }
            return new State(result[0], result[1], result[2]);
        }
    }
}