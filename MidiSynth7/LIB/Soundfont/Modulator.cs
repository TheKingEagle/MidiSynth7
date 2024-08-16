using System;
/*
 Copyright 2020 Mark Heath

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 */
namespace NAudio.SoundFont
{
    /// <summary>
    /// Transform Types
    /// </summary>
    public enum TransformEnum
    {
        /// <summary>
        /// Linear
        /// </summary>
        Linear = 0
    }

    /// <summary>
    /// Modulator
    /// </summary>
    public class Modulator
    {

        /// <summary>
        /// Source Modulation data type
        /// </summary>
        public ModulatorType SourceModulationData { get; set; }

        /// <summary>
        /// Destination generator type
        /// </summary>
        public GeneratorEnum DestinationGenerator { get; set; }

        /// <summary>
        /// Amount
        /// </summary>
        public short Amount { get; set; }

        /// <summary>
        /// Source Modulation Amount Type
        /// </summary>
        public ModulatorType SourceModulationAmount { get; set; }

        /// <summary>
        /// Source Transform Type
        /// </summary>
        public TransformEnum SourceTransform { get; set; }

        /// <summary>
        /// <see cref="Object.ToString"/>
        /// </summary>
        public override string ToString()
        {
            return String.Format("Modulator {0} {1} {2} {3} {4}",
                SourceModulationData, DestinationGenerator,
                Amount, SourceModulationAmount, SourceTransform);
        }

    }
}