using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MidiSynth7.entities.controls.extensions
{
    public static class ComboBoxExtensions
    {
        public static object GetItemOrFirst(this ComboBox comboBox, int index)
        {
            if (comboBox.Items.Count == 0)
                return null;

            return (index >= 0 && index < comboBox.Items.Count)
                ? comboBox.Items[index]
                : comboBox.Items[0];
        }
    }

}
