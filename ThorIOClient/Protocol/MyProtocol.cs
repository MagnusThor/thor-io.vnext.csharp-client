   using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;


namespace ThorIOClient.Protocol
{
 
   public static class Endian
    {
        public static byte[] ToBigEndianBytes<T>(this int source)
        {
            var bytes = new byte[] {};

            Type type = typeof (T);
            if (type == typeof (ushort))
                bytes = BitConverter.GetBytes((ushort) source);
            else if (type == typeof (ulong))
                bytes = BitConverter.GetBytes((ulong) source);
            else if (type == typeof (int))
                bytes = BitConverter.GetBytes(source);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        public static int ToLittleEndianInt(this byte[] source)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(source);

            if (source.Length == 2)
                return BitConverter.ToUInt16(source, 0);

            if (source.Length == 8)
                return (int) BitConverter.ToUInt64(source, 0);

            return 0;
        }
    }

 
    public enum FrameType
    {
        Continuation = 0,
        Text = 1,
        Binary = 2,
        Close = 8,
        Ping = 9,
        Pong = 10,
    }

      public class ReadState
    {
        public ReadState()
        {
            Data = new List<byte>();
        }

        public List<byte> Data { get; private set; }
        public FrameType? FrameType { get; set; }

        public void Clear()
        {
            Data.Clear();
            FrameType = null;
        }
    }
  
    public class DataFrame
    {

        public DataFrame(List<byte> payload){
            this.IsFinal = true;
            this.IsMasked = true;
            this.MaskKey = new Random().Next(0, 34298);
            this.Payload =  payload.ToArray();
            
            }
        private bool IsFinal { get; set; }

    
        private bool IsMasked { get; set; }

        private long PayloadLength
        {
            get { return Payload.Length; }
        }

        private int MaskKey { get; set; }

        public byte[] Payload { get; set; }

        public byte[] ToBytes()
        {
            var memoryStream = new MemoryStream();

            const bool rsv1 = false;
            const bool rsv2 = false;
            const bool rsv3 = false;

            var bt = (IsFinal ? 1 : 0) * 0x80;
            bt += (rsv1 ? 0x40 : 0x0);
            bt += (rsv2 ? 0x20 : 0x0);
            bt += (rsv3 ? 0x10 : 0x0);
            bt += (byte)2;

            memoryStream.WriteByte((byte)bt);

            byte[] payloadLengthBytes = GetLengthBytes();

            memoryStream.Write(payloadLengthBytes, 0, payloadLengthBytes.Length);

            byte[] payload = Payload;

            if (IsMasked)
            {
                byte[] keyBytes = BitConverter.GetBytes(MaskKey);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(keyBytes);
                memoryStream.Write(keyBytes, 0, keyBytes.Length);
                payload = TransformBytes(Payload, MaskKey);
            }

            memoryStream.Write(payload, 0, Payload.Length);

            return memoryStream.ToArray();
        }
        private byte[] GetLengthBytes()
        {
            var payloadLengthBytes = new List<byte>(9);

            if (PayloadLength > ushort.MaxValue)
            {
                payloadLengthBytes.Add(127);
                byte[] lengthBytes = BitConverter.GetBytes(PayloadLength);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(lengthBytes);
                payloadLengthBytes.AddRange(lengthBytes);
            }
            else if (PayloadLength > 125)
            {
                payloadLengthBytes.Add(126);
                byte[] lengthBytes = BitConverter.GetBytes((UInt16)PayloadLength);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(lengthBytes);
                payloadLengthBytes.AddRange(lengthBytes);
            }
            else
            {
                payloadLengthBytes.Add((byte)PayloadLength);
            }

            payloadLengthBytes[0] += (byte)(IsMasked ? 128 : 0);

            return payloadLengthBytes.ToArray();
        }

        public static byte[] TransformBytes(byte[] bytes, int mask)
        {
            var output = new byte[bytes.Length];
            byte[] maskBytes = BitConverter.GetBytes(mask);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(maskBytes);

            for (int i = 0; i < bytes.Length; i++)
            {
                output[i] = (byte)(bytes[i] ^ maskBytes[i % 4]);
            }

            return output;
        }
    }

    static class DataFrameReader{
         public static void Read(List<byte> data, ReadState readState, Action<FrameType, byte[]> completed)
        {
            while (data.Count >= 2)
            {
                bool isFinal = (data[0] & 128) != 0;
                var frameType = (FrameType)(data[0] & 15);
                int length = (data[1] & 127);
                var isMasked = (data[1] & 128) != 0;

                var reservedBits = (data[0] & 112);

                if (reservedBits != 0)
                {                    
                    return;
                }

                int index = 2;
                int payloadLength;
                if (length == 127)
                {
                    if (data.Count < index + 8)
                        return; //Not complete
                    payloadLength = data.Skip(index).Take(8).ToArray().ToLittleEndianInt();
                    index += 8;
                }
                else if (length == 126)
                {
                    if (data.Count < index + 2)
                        return; //Not complete
                   payloadLength = data.Skip(index).Take(2).ToArray().ToLittleEndianInt();
                    index += 2;
                }
                else
                {
                    payloadLength = length;
                }

         

                var maskBytes = data.Skip(index).Take(4).ToArray();

                index += 4;

                if (data.Count < index + payloadLength)
                    return; //Not complete
                    
                IEnumerable<byte> payload = data
                    .Skip(index)
                    .Take(payloadLength)
                   .Select((x, i) => (byte)(x ^ maskBytes[i % 4]));


                readState.Data.AddRange(payload);
                data.RemoveRange(0, index + payloadLength);
                if (frameType != FrameType.Continuation)
                    readState.FrameType = frameType;
                if (!isFinal || !readState.FrameType.HasValue) continue;
                byte[] stateData = readState.Data.ToArray();

                FrameType? stateFrameType = readState.FrameType;
                readState.Clear();
                completed(stateFrameType.Value, stateData);
            }

        }

    }




}