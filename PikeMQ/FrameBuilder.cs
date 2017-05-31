using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PikeMQ.Core
{
    public class FrameBuilder
    {
        MemoryStream strm = new MemoryStream();

        public byte[] GetData()
        {
            var buff = new byte[strm.Position];
            strm.Seek(0, SeekOrigin.Begin);
            strm.Read(buff, 0, buff.Length);
            strm.Seek(buff.Length, SeekOrigin.Begin);
            return buff;
        }

        public void WriteString(string str)
        {            
            var data = Encoding.UTF8.GetBytes(str);
            WriteMultiByte(data.Length);
            WriteArray(data);
        }

        public void WriteArray(byte[] data)
        {
            strm.Write(data, 0,data.Length);
        }

        public void WriteMultiByte(int val)
        {
            var bytes = ConvertToMultiByte(val);

            strm.Write(bytes, 0, bytes.Length);
        }

        public void WriteByte(byte b)
        {
            strm.WriteByte(b);
        }

        private byte[] ConvertToMultiByte(int val)
        {
            var tmp = val;
            List<byte> bytes = new List<byte>();
            while (tmp > 0)
            {
                byte current = (byte)(tmp & 0x7F);

                // We're still on the first value,
                // the lowest byte never gets the carry bit set.
                if (tmp == val)
                    bytes.Add(current);
                else
                    bytes.Add((byte)(current | 0x80));

                tmp = tmp >> 7;
            }

            // we started at the lowest byte, but actually we 
            // want it the other way round, so the decoding is easier.
            bytes.Reverse();
            return bytes.ToArray();
        }

        public byte[] Build(FrameType frameType)
        {
            var payload = GetData();
            List<byte> data = new List<byte>();
            data.Add(0x02);
            data.AddRange(ConvertToMultiByte(payload.Length + 1));  // + 1 for frametype!
            data.Add((byte)frameType);
            data.AddRange(GetData());
            data.Add(0x03);
            return data.ToArray();
        }
    }
}
