using System;
using System.IO;

namespace StudioSB.Tools
{
    public class BinaryWriterExt : BinaryWriter
    {
        public bool BigEndian { get; set; } = false;

        public BinaryWriterExt(Stream input) : base(input)
        {
        }

        public override void Write(short v)
        {
            Write(Reverse(BitConverter.GetBytes(v)));
        }

        public override void Write(ushort v)
        {
            Write(Reverse(BitConverter.GetBytes(v)));
        }

        public override void Write(int v)
        {
            Write(Reverse(BitConverter.GetBytes(v)));
        }

        public override void Write(uint v)
        {
            Write(Reverse(BitConverter.GetBytes(v)));
        }

        public override void Write(float v)
        {
            Write(Reverse(BitConverter.GetBytes(v)));
        }

        public byte[] Reverse(byte[] b)
        {
            if (BitConverter.IsLittleEndian && BigEndian)
                Array.Reverse(b);
            return b;
        }

        public void WriteAt(int pos, int val)
        {
            var temp = Position;
            Position = (uint)pos;
            Write(val);
            Position = temp;
        }

        public void PrintPosition()
        {
            Console.WriteLine("Stream at 0x{0}", BaseStream.Position.ToString("X"));
        }

        public override void Write(string v)
        {
            Write(v.ToCharArray());
            Write((byte)0);
        }
        
        public uint Position
        {
            get { return (uint)BaseStream.Position; }
            set
            {
                BaseStream.Position = value;
            }
        }
    }
}
