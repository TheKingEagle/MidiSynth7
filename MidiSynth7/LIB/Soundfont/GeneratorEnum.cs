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
	/// Generator types
	/// </summary>
	public enum GeneratorEnum 
	{
		/// <summary>Start address offset</summary>
		StartAddressOffset = 0,
		/// <summary>End address offset</summary>
		EndAddressOffset,
		/// <summary>Start loop address offset</summary>
		StartLoopAddressOffset,
		/// <summary>End loop address offset</summary>
		EndLoopAddressOffset,
		/// <summary>Start address coarse offset</summary>
		StartAddressCoarseOffset,
		/// <summary>Modulation LFO to pitch</summary>
		ModulationLFOToPitch,
		/// <summary>Vibrato LFO to pitch</summary>
		VibratoLFOToPitch,
		/// <summary>Modulation envelope to pitch</summary>
		ModulationEnvelopeToPitch,
		/// <summary>Initial filter cutoff frequency</summary>
		InitialFilterCutoffFrequency,
		/// <summary>Initial filter Q</summary>
		InitialFilterQ,
		/// <summary>Modulation LFO to filter Cutoff frequency</summary>
		ModulationLFOToFilterCutoffFrequency,
		/// <summary>Modulation envelope to filter cutoff frequency</summary>
		ModulationEnvelopeToFilterCutoffFrequency,
		/// <summary>End address coarse offset</summary>
		EndAddressCoarseOffset,
		/// <summary>Modulation LFO to volume</summary>
		ModulationLFOToVolume,
		/// <summary>Unused</summary>
		Unused1,
		/// <summary>Chorus effects send</summary>
		ChorusEffectsSend,
		/// <summary>Reverb effects send</summary>
		ReverbEffectsSend,
		/// <summary>Pan</summary>
		Pan,
		/// <summary>Unused</summary>
		Unused2,
		/// <summary>Unused</summary>
		Unused3,
		/// <summary>Unused</summary>
		Unused4,
		/// <summary>Delay modulation LFO</summary>
		DelayModulationLFO,
		/// <summary>Frequency modulation LFO</summary>
		FrequencyModulationLFO,
		/// <summary>Delay vibrato LFO</summary>
		DelayVibratoLFO,
		/// <summary>Frequency vibrato LFO</summary>
		FrequencyVibratoLFO,
		/// <summary>Delay modulation envelope</summary>
		DelayModulationEnvelope,
		/// <summary>Attack modulation envelope</summary>
		AttackModulationEnvelope,
		/// <summary>Hold modulation envelope</summary>
		HoldModulationEnvelope,
		/// <summary>Decay modulation envelope</summary>
		DecayModulationEnvelope,
		/// <summary>Sustain modulation envelop</summary>
		SustainModulationEnvelope,
		/// <summary>Release modulation envelope</summary>
		ReleaseModulationEnvelope,
		/// <summary>Key number to modulation envelope hold</summary>
		KeyNumberToModulationEnvelopeHold,
		/// <summary>Key number to modulation envelope decay</summary>
		KeyNumberToModulationEnvelopeDecay,
		/// <summary>Delay volume envelope</summary>
		DelayVolumeEnvelope,
		/// <summary>Attack volume envelope</summary>
		AttackVolumeEnvelope,
		/// <summary>Hold volume envelope</summary>
		HoldVolumeEnvelope,
		/// <summary>Decay volume envelope</summary>
		DecayVolumeEnvelope,
		/// <summary>Sustain volume envelope</summary>
		SustainVolumeEnvelope,
		/// <summary>Release volume envelope</summary>
		ReleaseVolumeEnvelope,
		/// <summary>Key number to volume envelope hold</summary>
		KeyNumberToVolumeEnvelopeHold,
		/// <summary>Key number to volume envelope decay</summary>
		KeyNumberToVolumeEnvelopeDecay,
		/// <summary>Instrument</summary>
		Instrument,
		/// <summary>Reserved</summary>
		Reserved1,
		/// <summary>Key range</summary>
		KeyRange,
		/// <summary>Velocity range</summary>
		VelocityRange,
		/// <summary>Start loop address coarse offset</summary>
		StartLoopAddressCoarseOffset,
		/// <summary>Key number</summary>
		KeyNumber,
		/// <summary>Velocity</summary>
		Velocity,
		/// <summary>Initial attenuation</summary>
		InitialAttenuation,
		/// <summary>Reserved</summary>
		Reserved2,
		/// <summary>End loop address coarse offset</summary>
		EndLoopAddressCoarseOffset,
		/// <summary>Coarse tune</summary>
		CoarseTune,
		/// <summary>Fine tune</summary>
		FineTune,
		/// <summary>Sample ID</summary>
		SampleID,
		/// <summary>Sample modes</summary>
		SampleModes,
		/// <summary>Reserved</summary>
		Reserved3,
		/// <summary>Scale tuning</summary>
		ScaleTuning,
		/// <summary>Exclusive class</summary>
		ExclusiveClass,
		/// <summary>Overriding root key</summary>
		OverridingRootKey,
		/// <summary>Unused</summary>
		Unused5,
		/// <summary>Unused</summary>
		UnusedEnd
	}
}
