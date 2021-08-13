using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crossfire
{
    public static class Tokenizer
    {
        public static string GetString(byte[] buffer, ref int offset)
        {
            System.Diagnostics.Debug.Assert(buffer != null);
            System.Diagnostics.Debug.Assert(buffer.Length > 0);
            System.Diagnostics.Debug.Assert(offset >= 0);
            System.Diagnostics.Debug.Assert(offset < buffer.Length);

            var start = offset;

            //read up to space or end of buffer
            for (; offset < buffer.Length && buffer[offset] != ' '; offset++) { }

            var str = Encoding.ASCII.GetString(buffer, start, offset - start);
            if (string.IsNullOrEmpty(str))
                throw new Exception();

            //skip over space if we can
            if (offset < buffer.Length && buffer[offset] == ' ')
                offset++;

            return str;
        }

        public static int GetStringInt(byte[] buffer, ref int offset)
        {
            var str = GetString(buffer, ref offset);

            if (!int.TryParse(str, out int val))
                throw new Exception();

            return val;
        }
    }
}
