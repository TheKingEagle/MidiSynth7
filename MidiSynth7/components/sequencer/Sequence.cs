using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiSynth7.components.sequencer
{
    public class Sequence : INotifyPropertyChanged
    {
        private string _name;
        public string SequenceName
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(SequenceName));
            }
        }
        public int Tempo { get; set; }
        public int Divisions { get; set; }
        public int Measures { get; set; }
        public int NotesPerMeasure { get; set; }

        public List<SequencePattern> Patterns { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return SequenceName;
        }
    }
}
