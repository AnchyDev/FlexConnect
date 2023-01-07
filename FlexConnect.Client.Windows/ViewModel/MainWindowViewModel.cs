using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexConnect.Client.Windows.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _title;
        public string Title
        {
            get => _title;
            set => SetAndNotify(ref _title, value);
        }

        public MainWindowViewModel()
        {
            Title = "Flex Connect!";
        }
    }
}
