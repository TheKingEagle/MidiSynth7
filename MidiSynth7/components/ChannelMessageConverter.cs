using Newtonsoft.Json;
using Sanford.Multimedia.Midi;
using System.Collections.Generic;
using System;

public class ChannelMessageConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(ChannelMessage);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        var channelMessage = (ChannelMessage)value;
        var data = new
        {
            Command = (int)channelMessage.Command, // Ensure it's serialized as an integer
            MidiChannel = channelMessage.MidiChannel,
            Data1 = channelMessage.Data1,
            Data2 = channelMessage.Data2
        };
        serializer.Serialize(writer, data);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        var data = serializer.Deserialize<Dictionary<string, object>>(reader);

        // Safely convert each field to the required type
        var command = Convert.ToInt32(data["Command"]); // Convert to int
        var midiChannel = Convert.ToInt32(data["MidiChannel"]);
        var data1 = Convert.ToInt32(data["Data1"]);
        var data2 = Convert.ToInt32(data["Data2"]);

        return new ChannelMessage((ChannelCommand)command, midiChannel, data1, data2);
    }

}
