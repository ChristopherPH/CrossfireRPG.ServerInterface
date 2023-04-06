using System;
using System.Linq;
using System.Text;

namespace CrossfireCore.ServerInterface
{
    public static class BufferTokenizer
    {
        public static byte[] SpaceSeperator = new byte[] { 0x20 };
        public static byte[] NewlineSeperator = new byte[] { 0x0A };
        public static byte[] SpaceNewlineSeperator = new byte[] { 0x20, 0x0A };

        /// <summary>
        /// Retrieves a string from the buffer, seperated by the buffer end or a space
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset">Current buffer position, will be updated when data retrieved</param>
        /// <param name="end">End of buffer position</param>
        /// <returns>String</returns>
        public static string GetString(byte[] buffer, ref int offset, int end)
        {
            return GetString(buffer, ref offset, end, SpaceSeperator);
        }

        /// <summary>
        /// Retrieves a string from the buffer, seperated by the buffer end or by custom seperators
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset">Current buffer position, will be updated when data retrieved</param>
        /// <param name="end">End of buffer position</param>
        /// <param name="separators">Seperators to use to split the string</param>
        /// <returns>String</returns>
        /// <exception cref="BufferTokenizerException"></exception>
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
                throw new BufferTokenizerException("String cannot start on a separator");

            var str = Encoding.ASCII.GetString(buffer, start, offset - start);
            if (string.IsNullOrEmpty(str))
                throw new BufferTokenizerException("String is empty");

            //if we are not at the end of the buffer, it means we've found a separator, so skip over it
            if (offset < end)
                offset++;

            return str;
        }

        /// <summary>
        /// Retrieves an int from the buffer, seperated by the buffer end or by a space
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset">Current buffer position, will be updated when data retrieved</param>
        /// <param name="end">End of buffer position</param>
        /// <returns>int</returns>
        /// <exception cref="BufferTokenizerException"></exception>
        public static int GetStringAsInt(byte[] buffer, ref int offset, int end)
        {
            var str = GetString(buffer, ref offset, end);

            try
            {
                var val = int.Parse(str);
                return val;
            }
            catch (FormatException ex)
            {
                throw new BufferTokenizerException("String is not an int: " + str, ex);
            }
            catch (OverflowException ex)
            {
                throw new BufferTokenizerException("String int is too big: " + str, ex);
            }
        }

        /// <summary>
        /// Retrieves a byte from the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset">Current buffer position, will be updated when data retrieved</param>
        /// <returns>a byte</returns>
        /// <exception cref="BufferTokenizerException"></exception>
        public static byte GetByte(byte[] buffer, ref int offset)
        {
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(buffer.Length > 0);
            System.Diagnostics.Debug.Assert(offset >= 0);

            try
            {
                var val = buffer[offset++];
                return val;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new BufferTokenizerException("Buffer not long enough for byte", ex);
            }
        }

        /// <summary>
        /// Retrieves a UInt16 from the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset">Current buffer position, will be updated when data retrieved</param>
        /// <returns>a UInt16</returns>
        /// <exception cref="BufferTokenizerException"></exception>
        public static UInt16 GetUInt16(byte[] buffer, ref int offset)
        {
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(buffer.Length > 0);
            System.Diagnostics.Debug.Assert(offset >= 0);

            try
            {
                var val = (UInt16)((buffer[offset] << 8) + buffer[offset + 1]);

                //var val = BitConverter.ToUInt16(new byte[] { so.buffer[1], so.buffer[0] }, 0);

                offset += 2;

                return val;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new BufferTokenizerException("Buffer not long enough for UInt16", ex);
            }
        }

        /// <summary>
        /// Retrieves a Int16 from the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset">Current buffer position, will be updated when data retrieved</param>
        /// <returns>a Int16</returns>
        /// <exception cref="BufferTokenizerException"></exception>
        //TODO: verify is correct
        public static Int16 GetInt16(byte[] buffer, ref int offset)
        {
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(buffer.Length > 0);
            System.Diagnostics.Debug.Assert(offset >= 0);

            try
            {
                var val = (Int16)((buffer[offset] << 8) + buffer[offset + 1]);

                //var val2 = BitConverter.ToInt16(new byte[] { buffer[offset + 1], buffer[offset] }, 0);

                offset += 2;

                return val;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new BufferTokenizerException("Buffer not long enough for Int16", ex);
            }
        }

        /// <summary>
        /// Retrieves a UInt32 from the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset">Current buffer position, will be updated when data retrieved</param>
        /// <returns>a UInt32</returns>
        /// <exception cref="BufferTokenizerException"></exception>
        public static UInt32 GetUInt32(byte[] buffer, ref int offset)
        {
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(buffer.Length > 0);
            System.Diagnostics.Debug.Assert(offset >= 0);

            try
            {
                var val = (UInt32)((buffer[offset] << 24) +
                    (buffer[offset + 1] << 16) +
                    (buffer[offset + 2] << 8) +
                    (buffer[offset + 3]));

                offset += 4;

                return val;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new BufferTokenizerException("Buffer not long enough for UInt32", ex);
            }
        }

        /// <summary>
        /// Retrieves a Int32 from the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset">Current buffer position, will be updated when data retrieved</param>
        /// <returns>a Int32</returns>
        /// <exception cref="BufferTokenizerException"></exception>
        //TODO: verify is correct
        public static Int32 GetInt32(byte[] buffer, ref int offset)
        {
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(buffer.Length > 0);
            System.Diagnostics.Debug.Assert(offset >= 0);

            try
            {
                var val = (Int32)((buffer[offset] << 24) +
                    (buffer[offset + 1] << 16) +
                    (buffer[offset + 2] << 8) +
                    (buffer[offset + 3]));

                offset += 4;

                return val;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new BufferTokenizerException("Buffer not long enough for Int32", ex);
            }
        }

        /// <summary>
        /// Retrieves a UInt64 from the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset">Current buffer position, will be updated when data retrieved</param>
        /// <returns>a UInt64</returns>
        /// <exception cref="BufferTokenizerException"></exception>
        public static UInt64 GetUInt64(byte[] buffer, ref int offset)
        {
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(buffer.Length > 0);
            System.Diagnostics.Debug.Assert(offset >= 0);

            try
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
            catch (IndexOutOfRangeException ex)
            {
                throw new BufferTokenizerException("Buffer not long enough for UInt64", ex);
            }
        }

        /// <summary>
        /// Retrieves a byte array of a specific length from the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset">Current buffer position, will be updated when data retrieved</param>
        /// <param name="length">Length of data to retrieve</param>
        /// <returns>Byte array of requested size</returns>
        public static byte[] GetBytes(byte[] buffer, ref int offset, int length)
        {
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(buffer.Length > 0);
            System.Diagnostics.Debug.Assert(offset >= 0);
            System.Diagnostics.Debug.Assert(length >= 0);
            System.Diagnostics.Debug.Assert(offset + length <= buffer.Length);

            //A length of zero should in theory be invalid, but the server does send a zero byte
            //string length in some cases. The parser should catch this when possible.
            if (length == 0)
                return new byte[0];

            try
            {
                var b = new byte[length];
                Array.Copy(buffer, offset, b, 0, length);
                offset += length;

                return b;
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new BufferTokenizerException("Buffer not long enough for bytes: " + length.ToString(), ex);
            }
        }

        /// <summary>
        /// Retrieves a string of a specific length from the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset">Current buffer position, will be updated when data retrieved</param>
        /// <param name="length">Length of data to retrieve</param>
        /// <returns>String of requested size</returns>
        public static string GetBytesAsString(byte[] buffer, ref int offset, int length)
        {
            return Encoding.ASCII.GetString(GetBytes(buffer, ref offset, length));
        }

        /// <summary>
        /// Retrieves a byte array containing all remaining bytes from the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset">Current buffer position, will be updated when data retrieved</param>
        /// <param name="end">End of buffer position</param>
        /// <returns>Byte array</returns>
        public static byte[] GetRemainingBytes(byte[] buffer, ref int offset, int end)
        {
            return GetBytes(buffer, ref offset, end - offset);
        }

        /// <summary>
        /// Retrieves a string containing all remaining bytes from the buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset">Current buffer position, will be updated when data retrieved</param>
        /// <param name="end">End of buffer position</param>
        /// <returns>String</returns>
        public static string GetRemainingBytesAsString(byte[] buffer, ref int offset, int end)
        {
            return Encoding.ASCII.GetString(GetBytes(buffer, ref offset, end - offset));
        }
    }


    [Serializable]
    public class BufferTokenizerException : Exception
    {
        public BufferTokenizerException() { }
        public BufferTokenizerException(string message) : base(message) { }
        public BufferTokenizerException(string message, Exception inner) : base(message, inner) { }
        protected BufferTokenizerException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
