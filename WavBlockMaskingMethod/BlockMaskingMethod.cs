using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WavBlockMaskingMethod
{
    class BlockMaskingMethod
    {
        private List<byte> _bits;
        
        public void HideMessage(string filePath, string message)
        {
            var parser = new WavFileParser();
            parser.OpenWav(filePath, out var leftStream, out var rightStream);

            var bufferMessage = Encoding.UTF8.GetBytes(message);
            var bitMessage = new BitArray(bufferMessage);
            var bufferIndex = 0;
            var bufferLength = bitMessage.Length;
            var channelLength = leftStream.Count;
            if (bufferLength > channelLength)
                throw new Exception();

            leftStream[0] = (short)(bufferLength / short.MaxValue);
            rightStream[0] = (short)(bufferLength % short.MaxValue);
            var countBufferMessage = 0;

            var blockLength = channelLength * 2 / bufferLength;
            var bitIndex = 0;
            for (int i = 1; i < leftStream.Count && bitIndex + 2 < bufferLength; i += blockLength)
            {
                var leftBlock = leftStream.GetRange(i, blockLength);
                var rightBlock = rightStream.GetRange(i, blockLength);

                var lBit = leftBlock.Sum(x => Math.Abs((int)x)) % 2 == 1;
                var rBit = rightBlock.Sum(x => Math.Abs((int)x)) % 2 == 1;
                

                if (lBit != bitMessage[bitIndex])
                    leftStream[i + blockLength - 1] = (short)(leftStream[i + blockLength - 1] ^ 1);
                bitIndex++;

                if (rBit != bitMessage[bitIndex])
                    rightStream[i + blockLength - 1] = (short)(rightStream[i + blockLength - 1] ^ 1);
                bitIndex++;
            }

            parser.UpdateWav(filePath, leftStream, rightStream);
        }

        public string ExtractMessage(string filePath)
        {
            new WavFileParser().OpenWav(filePath, out var leftStream, out var rightStream);
            var bufferIndex = 0;
            var messageLengthQuotient = leftStream[0];
            var messageLengthRemainder = rightStream[0];
            var channelLength = leftStream.Count;

            var bufferLength = short.MaxValue * messageLengthQuotient + messageLengthRemainder;
            var blockLength = channelLength * 2 / bufferLength;
            
            var bufferMessage = new BitArray(bufferLength);
            var bitIndex = 0;
            for (var i = 1; i < leftStream.Count && bitIndex + 2 < bufferLength; i += blockLength)
            {
                var leftBlock = leftStream.GetRange(i, blockLength);
                var rightBlock = rightStream.GetRange(i, blockLength);

                var lBit = leftBlock.Sum(x => Math.Abs((int)x)) % 2 == 1;
                var rBit = rightBlock.Sum(x => Math.Abs((int)x)) % 2 == 1;
                bufferMessage[bitIndex++] = lBit;
                bufferMessage[bitIndex++] = rBit;
            }

            return Encoding.UTF8.GetString(ToByteArray(bufferMessage));
        }

        private static byte[] ToByteArray(BitArray bits)
        {
            var numBytes = bits.Count / 8;
            if (bits.Count % 8 != 0) numBytes++;

            var bytes = new byte[numBytes];
            int byteIndex = 0, bitIndex = 0;

            for (var i = 0; i < bits.Count; i++)
            {
                if (bits[i])
                    bytes[byteIndex] |= (byte)(1 << (bitIndex));

                bitIndex++;
                if (bitIndex == 8)
                {
                    bitIndex = 0;
                    byteIndex++;
                }
            }

            return bytes;
        }
    }
}
