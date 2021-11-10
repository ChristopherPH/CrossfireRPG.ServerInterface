using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossfireCore.ServerInterface
{
    public static class BufferTokenizer
    {
        public static byte[] SpaceSeperator = new byte[] { 0x20 };
        public static byte[] NewlineSeperator = new byte[] { 0x0A };
        public static byte[] SpaceNewlineSeperator = new byte[] { 0x20, 0x0A };

        public static string GetString(byte[] buffer, ref int offset, int end)
        {
            return GetString(buffer, ref offset, end, SpaceSeperator);
        }

        public static string GetString(byte[] buffer, ref int offset, int end, byte[] separators)
        {
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(buffer.Length > 0);
            System.Diagnostics.Debug.Assert(end >= 0);
            System.Diagnostics.Debug.Assert(end <= buffer.Length);
            System.Diagnostics.Debug.Assert(offset >= 0);
            System.Diagnostics.Debug.Assert(offset <= end);
            System.Diagnostics.Debug.Assert(separators != null);
            System.Diagnostics.Debug.Assert(separators.Length > 0);

            var start = offset;

            //read up to end of buffer, looking for a separator
            for (; offset < end; offset++) 
            {
                if (separators.Contains(buffer[offset]))
                    break;
            }

            if (start == offset)
                throw new Exception("String cannot start on a separator");

            var str = Encoding.ASCII.GetString(buffer, start, offset - start);
            if (string.IsNullOrEmpty(str))
                throw new Exception();

            //if we are not at the end of the buffer, it means we've found a separator, so skip over it
            if (offset < end)
                offset++;

            return str;
        }

        public static int GetStringAsInt(byte[] buffer, ref int offset, int end)
        {
            var str = GetString(buffer, ref offset, end);

            if (!int.TryParse(str, out int val))
                throw new Exception();

            return val;
        }

        public static byte GetByte(byte[] buffer, ref int offset)
        {
            var val = buffer[offset++];
            return val;
        }

        public static UInt16 GetUInt16(byte[] buffer, ref int offset)
        {
            var val = (UInt16)((buffer[offset] << 8) + 
                (buffer[offset + 1]));

            //var val = BitConverter.ToUInt16(new byte[] { so.buffer[1], so.buffer[0] }, 0);

            offset += 2;

            return val;
        }

        //TODO: verify is correct
        public static Int16 GetInt16(byte[] buffer, ref int offset)
        {
            var val = (Int16)((buffer[offset] << 8) +
                (buffer[offset + 1]));

            //var val2 = BitConverter.ToInt16(new byte[] { buffer[offset + 1], buffer[offset] }, 0);

            offset += 2;

            return val;
        }

        public static UInt32 GetUInt32(byte[] buffer, ref int offset)
        {
            var val = (UInt32)((buffer[offset] << 24) +
                (buffer[offset + 1] << 16) +
                (buffer[offset + 2] << 8) +
                (buffer[offset + 3]));

            offset += 4;

            return val;
        }

        //TODO: verify is correct
        public static Int32 GetInt32(byte[] buffer, ref int offset)
        {
            var val = (Int32)((buffer[offset] << 24) +
                (buffer[offset + 1] << 16) +
                (buffer[offset + 2] << 8) +
                (buffer[offset + 3]));

            offset += 4;

            return val;
        }

        public static UInt64 GetUInt64(byte[] buffer, ref int offset)
        {
            var val = (UInt64)
                ((buffer[offset] << 56) +
                (buffer[offset + 1] << 48) +
                (buffer[offset + 2] << 40) +
                (buffer[offset + 3] << 32) +
                (buffer[offset + 4] << 24) +
                (buffer[offset + 5] << 16) +
                (buffer[offset + 6] << 8) +
                (buffer[offset + 7]));

            offset += 8;

            return val;
        }

        /// <summary>
        /// Get length bytes from buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static byte[] GetBytes(byte[] buffer, ref int offset, int length)
        {
            var b = new byte[length];
            Array.Copy(buffer, offset, b, 0, length);
            offset += length;

            return b;
        }

        public static string GetBytesAsString(byte[] buffer, ref int offset, int length)
        {
            return Encoding.ASCII.GetString(GetBytes(buffer, ref offset, length));
        }

        public static byte[] GetRemainingBytes(byte[] buffer, ref int offset, int end)
        {
            return GetBytes(buffer, ref offset, end - offset);
        }

        public static string GetRemainingBytesAsString(byte[] buffer, ref int offset, int end)
        {
            return Encoding.ASCII.GetString(GetBytes(buffer, ref offset, end - offset));
        }
    }
}
