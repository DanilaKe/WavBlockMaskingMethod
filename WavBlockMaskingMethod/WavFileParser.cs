using System;
using System.Collections.Generic;
using System.IO;

namespace WavBlockMaskingMethod
{
    class WavFileParser
    {
        private WavHeader Header;

        // Returns left and right double arrays. 'right' will be null if sound is mono.
        public void OpenWav(string filename, out List<short> left, out List<short> right)
        {
            Header = new WavHeader();

            left = new List<short>();
            right = new List<short>();

            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            using (var br = new BinaryReader(fs))
            {
                try
                {
                    Header.riffID = br.ReadBytes(4);
                    Header.size = br.ReadUInt32();
                    Header.wavID = br.ReadBytes(4);
                    Header.fmtID = br.ReadBytes(4);
                    Header.fmtSize = br.ReadUInt32();
                    Header.format = br.ReadUInt16();
                    Header.channels = br.ReadUInt16();
                    Header.sampleRate = br.ReadUInt32();
                    Header.bytePerSec = br.ReadUInt32();
                    Header.blockSize = br.ReadUInt16();
                    Header.bit = br.ReadUInt16();
                    Header.dataID = br.ReadBytes(4);
                    Header.dataSize = br.ReadUInt32();

                    for (var i = 0; i < Header.dataSize / Header.blockSize; i++)
                    {
                        left.Add((short)br.ReadUInt16());
                        right.Add((short)br.ReadUInt16());
                    }
                }
                finally
                {
                    br.Close();
                    fs.Close();
                }
            }
        }

        public void UpdateWav(string filename, List<short> left, List<short> right)
        {
            Header.dataSize = (uint)Math.Max(left.Count, right.Count) * 4;

            using (var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))

            using (var bw = new BinaryWriter(fs))
            {
                try
                {
                    bw.Write(Header.riffID);
                    bw.Write(Header.size);
                    bw.Write(Header.wavID);
                    bw.Write(Header.fmtID);
                    bw.Write(Header.fmtSize);
                    bw.Write(Header.format);
                    bw.Write(Header.channels);
                    bw.Write(Header.sampleRate);
                    bw.Write(Header.bytePerSec);
                    bw.Write(Header.blockSize);
                    bw.Write(Header.bit);
                    bw.Write(Header.dataID);
                    bw.Write(Header.dataSize);

                    for (var i = 0; i < Header.dataSize / Header.blockSize; i++)
                    {
                        if (i < left.Count)
                        {
                            bw.Write((ushort)left[i]);
                        }
                        else
                        {
                            bw.Write(0);
                        }

                        if (i < right.Count)
                        {
                            bw.Write((ushort)right[i]);
                        }
                        else
                        {
                            bw.Write(0);
                        }
                    }
                }
                finally
                {
                    bw.Close();
                    fs.Close();
                }
            }
        }
    }
}
