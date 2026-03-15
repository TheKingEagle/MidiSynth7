public void GenerateMIDIEngine(ISynthView view, int deviceId=0)
{
    try
    {
        if (MidiEngine != null)
        {
            MidiEngine.MidiEngine_Close();
        }
        MidiEngine = new MidiEngine(deviceId);

        MidiEngine.NotePlayed += MidiEngine_NotePlayed;
        MidiEngine.NoteStopped += MidiEngine_NoteStoped;
        MidiEngine.FileLoadComplete += MidiEngine_FileLoadComplete;
        MidiEngine.SequenceBuilder_Completed += MidiEngine_SequenceBuilder_Completed;

        //tell view we updated shit
        view.HandleEvent(this, new EventArgs(), "RefMainWin");
        view.HandleEvent(this, new EventArgs(), "MTaskWorker");

        // clean previous inDevice if any
        if (MidiEngine.inDevice != null)
        {
            try { MidiEngine.inDevice.StopRecording(); } catch { }
            try { MidiEngine.inDevice.Close(); } catch { }
            MidiEngine.inDevice = null;
        }

        // open first input device only if index is valid
        if (AppConfig.ActiveInputDeviceIndex > -1 && AppConfig.ActiveInputDeviceIndex < Sanford.Multimedia.Midi.InputDevice.DeviceCount)
        {
            try
            {
                MidiEngine.inDevice = new Sanford.Multimedia.Midi.InputDevice(AppConfig.ActiveInputDeviceIndex);

                // Use library defaults / safe behavior for driver callbacks
                MidiEngine.inDevice.PostDriverCallbackToDelegateQueue = true;
                MidiEngine.inDevice.PostEventsOnCreationContext = true;

                MidiEngine.inDevice.ChannelMessageReceived += InDevice_ChannelMessageReceived;
                MidiEngine.inDevice.StartRecording();
            }
            catch (Sanford.Multimedia.Midi.InputDeviceException idex)
            {
                // diagnostic info and safe cleanup
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Failed to StartRecording for input device index {AppConfig.ActiveInputDeviceIndex}: {idex.Message}");
                try
                {
                    for (int i = 0; i < Sanford.Multimedia.Midi.InputDevice.DeviceCount; i++)
                    {
                        sb.AppendLine($"Input[{i}] = {Sanford.Multimedia.Midi.InputDevice.GetDeviceCapabilities(i).name}");
                    }
                }
                catch { sb.AppendLine("Failed to enumerate input devices."); }

                System.Diagnostics.Trace.WriteLine(sb.ToString());
                try { MidiEngine.inDevice?.Close(); } catch { }
                MidiEngine.inDevice = null;
                AppConfig.ActiveInputDeviceIndex = -1; // avoid repeated immediate failures
            }
        }
        else
        {
            // invalid index: clear any existing reference to avoid StartRecording calls later
            MidiEngine.inDevice = null;
            AppConfig.ActiveInputDeviceIndex = -1;
        }

        // second device: mirror same guarded logic
        if (MidiEngine.inDevice2 != null)
        {
            try { MidiEngine.inDevice2.StopRecording(); } catch { }
            try { MidiEngine.inDevice2.Close(); } catch { }
            MidiEngine.inDevice2 = null;
        }

        if (AppConfig.ActiveInputDevice2Index > -1 && AppConfig.ActiveInputDevice2Index < Sanford.Multimedia.Midi.InputDevice.DeviceCount)
        {
            try
            {
                MidiEngine.inDevice2 = new Sanford.Multimedia.Midi.InputDevice(AppConfig.ActiveInputDevice2Index);
                MidiEngine.inDevice2.PostDriverCallbackToDelegateQueue = true;
                MidiEngine.inDevice2.PostEventsOnCreationContext = true;
                MidiEngine.inDevice2.ChannelMessageReceived += InDevice_ChannelMessageReceived;
                MidiEngine.inDevice2.StartRecording();
            }
            catch (Sanford.Multimedia.Midi.InputDeviceException idex)
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"Failed to StartRecording for input device2 index {AppConfig.ActiveInputDevice2Index}: {idex.Message}");
                try
                {
                    for (int i = 0; i < Sanford.Multimedia.Midi.InputDevice.DeviceCount; i++)
                    {
                        sb.AppendLine($"Input[{i}] = {Sanford.Multimedia.Midi.InputDevice.GetDeviceCapabilities(i).name}");
                    }
                }
                catch { sb.AppendLine("Failed to enumerate input devices."); }

                System.Diagnostics.Trace.WriteLine(sb.ToString());
                try { MidiEngine.inDevice2?.Close(); } catch { }
                MidiEngine.inDevice2 = null;
                AppConfig.ActiveInputDevice2Index = -1;
            }
        }
        else
        {
            MidiEngine.inDevice2 = null;
            AppConfig.ActiveInputDevice2Index = -1;
        }
    }
    catch (Exception ex)
    {
        Dialog.Message(this, GR_OverlayContent, "Failed to create a MIDI Engine:\r\n" + ex.Message, "MIDI Engine", Icons.Critical);
    }

}