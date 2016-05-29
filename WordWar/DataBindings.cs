using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordWar
{
    class DataBindings : INotifyPropertyChanged
    {
        private string _bString = string.Empty;
        public string bString
        {
            get { return _bString; }
            set { _bString = value; NotifyPropertyChanged("bString"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(info));
        }
    }
}

