using CommunityToolkit.Mvvm.ComponentModel;

namespace Maximatron.ViewModels;

public partial class UserObjectModel : ViewModelBase
{
    [ObservableProperty] public string title = "";

    [ObservableProperty] public string textContent = "";
    [ObservableProperty] public bool isCheck = false;
}