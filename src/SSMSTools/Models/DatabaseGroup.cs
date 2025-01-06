using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SSMSTools.Models
{
    public class DatabaseGroup : INotifyPropertyChanged
    {
        private string _title;

        public Guid? Id { get; set; }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged(nameof(Title));
                }
            }
        }

        private List<string> _databases;
        public List<string> Databases
        {
            get => _databases;
            set
            {
                if (_databases != value)
                {
                    _databases = value;
                    OnPropertyChanged(nameof(Databases));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
