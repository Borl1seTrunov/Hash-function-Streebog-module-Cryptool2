using CrypTool.PluginBase;
using System.ComponentModel;

namespace Streebog
{
    public class StreebogSettings : ISettings
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private int hashSize;

        public StreebogSettings()
        {
            this.HashSize = 1;
        }

        [TaskPane("Размер хэша", "Выберите размер хэша ГОСТ Р 34.11-2012.", null, 0, false, ControlType.ComboBox, new[] { "256 бит", "512 бит" })]
        public int HashSize
        {
            get { return hashSize; }
            set
            {
                if (hashSize != value)
                {
                    hashSize = value;
                    OnPropertyChanged("HashSize");
                }
            }
        }

        public void Initialize()
        {
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
