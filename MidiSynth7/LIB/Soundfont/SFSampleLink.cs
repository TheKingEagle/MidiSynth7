/*
 Copyright 2020 Mark Heath

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

 */
namespace NAudio.SoundFont 
{
	/// <summary>
	/// Sample Link Type
	/// </summary>
	public enum SFSampleLink : ushort 
	{
		/// <summary>
		/// Mono Sample
		/// </summary>
		MonoSample = 1,
		/// <summary>
		/// Right Sample
		/// </summary>
		RightSample = 2,
		/// <summary>
		/// Left Sample
		/// </summary>
		LeftSample = 4,
		/// <summary>
		/// Linked Sample
		/// </summary>
		LinkedSample = 8,
		/// <summary>
		/// ROM Mono Sample
		/// </summary>
		RomMonoSample = 0x8001,
		/// <summary>
		/// ROM Right Sample
		/// </summary>
		RomRightSample = 0x8002,
		/// <summary>
		/// ROM Left Sample
		/// </summary>
		RomLeftSample = 0x8004,
		/// <summary>
		/// ROM Linked Sample
		/// </summary>
		RomLinkedSample = 0x8008
	}
}