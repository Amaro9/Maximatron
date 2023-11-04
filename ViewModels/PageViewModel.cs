using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maximatron.Services;

namespace Maximatron.ViewModels;

public partial class PageViewModel : ViewModelBase
{
    [ObservableProperty] private bool isNewBlocPopupVisible;
    [ObservableProperty] private string newBlocBtnTitle = "+";
    
    [ObservableProperty] public string lastSavePath = string.Empty;
    [ObservableProperty] public string docName = string.Empty;
    

    [RelayCommand]
    private void NewBlocButtonPressed()
    {
        IsNewBlocPopupVisible ^= true;
        NewBlocBtnTitle = IsNewBlocPopupVisible ? "X" : "+";
    }

    public void GetDocName()
    {
        if (LastSavePath == string.Empty || LastSavePath == "")
            return;
        
        string[] parts = LastSavePath.Split("/");
        DocName = parts.Last();
    }
}