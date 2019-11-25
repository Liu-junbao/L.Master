using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MasterDemo.ViewModels
{
    public class Window1ViewModel : BindableBase
    {
        private Dictionary<int,Model> _models;
        public Window1ViewModel()
        {

            var models = new List<Model>();
            for (int i = 0; i < 50; i++)
            {
                models.Add(new Model(i) { Name = $"Name{i + 1}", Group = $"{i % 5 + 1}" });
            }
            _models = models.ToDictionary(i=>i.Id);
            Items = new EditableCollection<Model>(OnSave);
            LoadItems();
        }
        private Model _model;
        public Model Model
        {
            get { return _model; }
            set { SetProperty(ref _model, value); }
        }
        public EditableCollection<Model> Items { get; }


        private async void ChangeModel(int index)
        {
            index++;
            Model = new Model(index) { Name = $"Name{index}" };
            await Task.Delay(1000);
            ChangeModel(index);
        }

        private void LoadItems()
        {
            Items.Change(_models);
        }
        private async Task OnSave(Model model, List<Tuple<string, object>> changedValues)
        {
            foreach (var item in changedValues)
            {
                switch (item.Item1)
                {
                    case nameof(Model.Name):
                        model.Name = item.Item2?.ToString();
                        break;
                    case nameof(Model.Group):
                        model.Group = item.Item2?.ToString();
                        break;
                    default:
                        break;
                }
            }
            var newModel = new Model(model.Id) { Name = model.Name, Group = model.Group };
            _models[model.Id] = newModel;
            LoadItems();
        }

        private string _text1;
        public string Text1
        {
            get { return _text1; }
            set { SetProperty(ref _text1, value); }
        }
        private string _text2;
        public string Text2
        {
            get { return _text2; }
            set { SetProperty(ref _text2, value); }
        }
        private string _text3;
        public string Text3
        {
            get { return _text3; }
            set { SetProperty(ref _text3, value); }
        }
        public Command Click { get; }

        private void OnClick()
        {
            
        }
    }
}
