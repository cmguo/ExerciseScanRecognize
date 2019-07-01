using Base.Mvvm;
using Exercise.Model;
using MyToolkit.Mvvm;
using Panuon.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exercise.ViewModel
{
    class HistoryViewModel : ViewModelBase
    {

        public RelayCommand LoadMoreCommand { get; private set; }

        private HistoryModel historyModel = HistoryModel.Instance;

        public HistoryViewModel()
        {
            LoadMoreCommand = new RelayCommand((o) => historyModel.LoadMore());
            new RelayCommand((o) => historyModel.Load()).Execute(null);
        }
    }
}
