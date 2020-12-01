using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Organizer
{
    public static class BytesExtensions
    {
        public static byte[] ToBytes(this int num) => BitConverter.GetBytes(num);

        public static int ToInt(this byte[] bytes) => BitConverter.ToInt32(bytes);
        
        public static byte[] ToBytes(this State state)
        {
            return state.UserId.ToBytes()
                .Concat(((int)state.GlobalState).ToBytes())
                .Concat(state.SubStateId.ToBytes())
                .ToArray();
        }
        
        public static State ToState(this byte[] bytes)
        {
            var result = new List<int>();
            for (var i = 0; i < bytes.Length; i += 4)
            {
                result.Add(new byte[] {bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3]}.ToInt());
            }
            return new State(result[0], (GlobalStates)result[1], result[2]);
        }
        
        public static byte[] ToBytes(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
 
        public static string To_String(this byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }
 
        public static byte[] ToBytes(this DateTime dateTime)
        {
            return BitConverter.GetBytes(dateTime.Ticks);
        }
 
        public static DateTime ToDateTime(this byte[] bytes)
        {
            long ticks = BitConverter.ToInt64(bytes);
            return new DateTime(ticks);
        }

        public static DateTime DateTimeFromFirstBytes(this byte[] bytes)
        {
            return bytes.Take(8).ToArray().ToDateTime();
        }

        public static string ToStringFrom(this byte[] bytes, int count)
        {
            return bytes.Skip(8).ToArray().To_String();
        }
        
        public static byte[] ToBytes(this Answer ans)
        {
            return ((int) ans.Format).ToBytes()
                .Concat(ans.NumberRange.Item1.ToBytes())
                .Concat(ans.NumberRange.Item2.ToBytes())
                .ToArray();
        }
        
        public static Answer ToAnswer(this byte[] bytes)
        {
            var result = new int[3];
            for (var j = 0; j < result.Length; j++)
            {
                var i = j * 4;
                result[i] = new byte[] {bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3]}.ToInt();
            }

            return new Answer()
            {
                Format = (ExpectingRequestFormat) result[0], 
                NumberRange = new Tuple<int, int>(result[1], result[2]),
                Headline = ""
            };
        }
        
        public static byte[] ToBytes(this AlarmItem alarmItem)
        {
            var firstPart = alarmItem.DateAndTime.ToBytes().ToList();
            var secondPart = alarmItem.Name.ToBytes().ToList();

            return firstPart.Concat(secondPart).ToArray();
        }

        public static AlarmItem ToAlarmItem(this byte[] bytes)
        {
            var dateAndTime = bytes.Take(8).ToArray().ToDateTime();
            var name = bytes.Skip(8).ToArray().To_String();

            return new AlarmItem(name, dateAndTime);
        }
    }
}