﻿using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Maximatron.ViewModels;

public partial class PageViewModel : ViewModelBase
{
    [ObservableProperty] private bool isNewBlocPopupVisible;
    [ObservableProperty] private string newBlocBtnTitle = "=";
    

    [RelayCommand]
    private void NewBlocButtonPressed()
    {
        IsNewBlocPopupVisible ^= true;
        NewBlocBtnTitle = IsNewBlocPopupVisible ? "X" : "=";
    }
    
}