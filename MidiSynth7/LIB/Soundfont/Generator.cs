/*
 Copyright 2020 Mark Heath

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 */
namespace NAudio.SoundFont
{
    /// <summary>
    /// Soundfont generator
    /// </summary>
    public class Generator
    {
        /// <summary>
        /// Gets the generator type
        /// </summary>
        public GeneratorEnum GeneratorType { get; set; }

        /// <summary>
        /// Generator amount as an unsigned short
        /// </summary>
        public ushort UInt16Amount { get; set; }

        /// <summary>
        /// Generator amount as a signed short
        /// </summary>
        public short Int16Amount
        {
            get => (short)UInt16Amount;
            set => UInt16Amount = (ushort)value;
        }

        /// <summary>
        /// Low byte amount
        /// </summary>
        public byte LowByteAmount
        {
            get
            {
                return (byte)(UInt16Amount & 0x00FF);
            }
            set
            {
                UInt16Amount &= 0xFF00;
                UInt16Amount += value;
            }
        }

        /// <summary>
        /// High byte amount
        /// </summary>
        public byte HighByteAmount
        {
            get
            {
                return (byte)((UInt16Amount & 0xFF00) >> 8);
            }
            set
            {
                UInt16Amount &= 0x00FF;
                UInt16Amount += (ushort)(value << 8);
            }
        }

        /// <summary>
        /// Instrument
        /// </summary>
        public Instrument Instrument { get; set; }

        /// <summary>
        /// Sample Header
        /// </summary>
        public SampleHeader SampleHeader { get; set; }

        /// <summary>
        /// <see cref="object.ToString"/>
        /// </summary>
        public override string ToString()
        {
            if (GeneratorType == GeneratorEnum.Instrument)
                return $"Generator Instrument {Instrument.Name}";
            else if (GeneratorType == GeneratorEnum.SampleID)
                return $"Generator SampleID {SampleHeader}";
            else
                return $"Generator {GeneratorType} {UInt16Amount}";
        }
    }
}