using Prism.Mvvm;
using System.Collections.Generic;

namespace MasterDemo.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public MainWindowViewModel()
        {
            List<string> headers = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                headers.Add($"项{i+1}");
            }
            Headers = headers;
        }
        private List<string> _headers;
        public List<string> Headers
        {
            get { return _headers; }
            set { SetProperty(ref _headers, value); }
        }
    }
}
