using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Maximatron.Controls;
using Maximatron.Services;


namespace Maximatron.ViewModels;


public partial class PageViewModel : ViewModelBase
{
    private bool init = false;
    
    [ObservableProperty] private bool isNewBlocPopupVisible;
    [ObservableProperty] private string newBlocBtnTitle = "+";
    
    [ObservableProperty] public string lastSavePath = string.Empty;
    [ObservableProperty] public string docName = string.Empty;

    private Panel? notificationPanel = null;
    private Panel? userViewPanel = null;
    
    public void Init(Panel? userView, Panel? notifPanel)
    {
        // Check si on peut commencer a init
        if (notifPanel == null || userView == null)
        {
            init = false;
            
            Console.WriteLine($"[ERROR] : Init in PageViewModel.cs failed");
            return;
        }

        // Set les vars qui faut
        notificationPanel = notifPanel;
        userViewPanel = userView;
        
        // On a reussi l'init
        init = true;
    }

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
    
    public void AddNewNotification(string title="", string desc="", SolidColorBrush? color=null)
    {
        if (!init || notificationPanel == null)
            return;
        
        /* TODO :
         * A voir mais ça pourrais être sympa de faire des
         * notif de differentes couleurs en fonction de si
         * c'est une erreur / success / warning
         * Apres ça pourrais pas etre util de ouf
         */
        
        var brushColor = Color.FromRgb(255, 0, 0); // Red color, change the RGB values as needed
        var solidColorBrush = new SolidColorBrush(brushColor);
        
        NotificationControl notification = new NotificationControl()
        {
            Title = title,
            Description = desc,
            BorderColor = color,
        };

        notification.PointerPressed += (sender, args) => notificationPanel.Children.Remove(notification);
        
        notificationPanel.Children.Add(notification);
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1.9f)
        };
        
        // Event quand le timer se fini
        timer.Tick += (sender, e) =>
        {
            // On degage la notif de son panel
            notificationPanel.Children.Remove(notification);
            // On arrete le timer
            timer.Stop();
        };
        
        // On commence le timer
        timer.Start();
    }
    
}