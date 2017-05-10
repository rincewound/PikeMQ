using System;
using System.Collections.Generic;
using System.Text;

namespace PikeMQ.Core
{
    public static class Util
    {
        /*
         * This method extracts a multi-byte encoded integer from a given array.
         * For each byte inspected, the upper bit determines, if another byte is 
         * part of the value, i.e.:
         * 0x01 => Value 1
         * 0x81 => A value greater 127, as the MSB is set. This indicates, that the next byte is part of the value.
         * (0x81, 0x01) => 129
         * => note that, if the MSB is set, the value for the byte is shifted by 7 bits, thus the value 128 is
         *    encoded as (0x81, 0x00)!
         * The method accepts at most 4 byte!
         */
        public static (bool success, UInt32 value, int numBytesUsed) ExtractMultiByte(byte[] source, int startIndex)
        {
            uint len = 0;
            int idx = -1;
            int dataIndex = 0;
            do
            {
                idx++;

                dataIndex = startIndex + idx;

                // If this happens, we saw a carry bit set but have
                // no data remaining -> this is a bad value. FAIL
                if (dataIndex >= source.Length)
                    return (false, 0,0);

                len = len << 7;
                len += (uint)source[dataIndex] & 0x7F;      // Upper bit denots "theres still another length byte to come"

                // Bad length, we allow 4 byte at max!
                if (idx >= 3)
                    return (false, 0,0);                                

            } while ((source[dataIndex] & 0x80) != 0);

            return (true, len, idx + 1);
        }

        public static (bool success, UInt32 value) ExtractMultiByte(this System.IO.MemoryStream strm)
        {
            uint len = 0;
            int numBytesRead = 0;
            do
            {
                len = len << 7;
                byte theByte = (byte) strm.ReadByte();
                len += (uint)theByte & 0x7F;      // Upper bit denots "theres still another length byte to come"
                numBytesRead++;

                // Bad length, we allow 4 byte at max!
                if (numBytesRead >= 3)
                    return (false, 0);

                if ((theByte & 0x80) == 0)
                    break;

            } while (true);

            return (true, len);
        }

        public static bool MatchSubscription(this string str, string template)
        {
            var hasWildcard = template.Contains("*");            
            if(hasWildcard)
            {
                // Get data ahead of wildcard:
                var usedTemplate = template.Substring(0, template.IndexOf('*'));
                return (str.StartsWith(usedTemplate));
            }
            return str.Equals(template);
        }

    }
}
