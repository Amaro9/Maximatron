using System;
using System.Collections.Generic;
using System.IO;
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
    private bool init;
    public static string PATH = "lastSavePath.txt"; 
    public static string FolderSavePATH = "lastFolderSavePath.txt"; 
    
    [ObservableProperty] private bool isNewBlocPopupVisible;
    [ObservableProperty] private string newBlocBtnTitle = "+";
    
    [ObservableProperty] public string lastSavePath = string.Empty;
    [ObservableProperty] public string docName = string.Empty;
    
    [ObservableProperty] public bool isSave = false;

    [ObservableProperty] public string currDir = "C:";
    [ObservableProperty] public string folderName = "no folder open";

    private Panel? notificationPanel;
    private Panel? userViewPanel;
    private Panel? hierarchyPanel;
    
    public void Init(Panel? userView, Panel? notifPanel, Panel? hierarchyView)
    {
        // Check si on peut commencer a init
        if (notifPanel == null || userView == null || hierarchyView == null)
        {
            init = false;
            
            Console.WriteLine($"[ERROR] : Init in PageViewModel.cs failed");
            return;
        }

        // Set les vars qui faut
        notificationPanel = notifPanel;
        userViewPanel = userView;
        hierarchyPanel = hierarchyView;

        // Load the directory chosen by the user in the last session
        CurrDir = SavingService.ReadStringFromFile(FolderSavePATH);
        
        // On a reussi l'init
        init = true;
    }

    [RelayCommand]
    private void NewBlocButtonPressed()
    {
        IsNewBlocPopupVisible ^= true;
        NewBlocBtnTitle = IsNewBlocPopupVisible ? "X" : "+";
    }

    public string GetDocName()
    {
        if (LastSavePath == string.Empty)
        {
            DocName = "Untitled";
            SetSaveState(false);
            return DocName;
        }
        
        string[] parts = LastSavePath.Split("/");
        DocName = parts.Last();
        return DocName;
    }
    
    public void AddNewNotification(string title="", string desc="", SolidColorBrush? color=null)
    {
        if (!init || notificationPanel == null)
            return;
        
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
    
    [RelayCommand]
    private void CreateNewPage()
    {
        // TODO : check si la page est save avant de tout clear

        if (!init || userViewPanel == null)
            return;
        
        // On clear la page de tous les element qu'elle contenait
        userViewPanel.Children.Clear();
        
        // On reset la save
        LastSavePath = string.Empty;
        GetDocName();
        
        // Petit notif pour du feedback
        AddNewNotification("New Document Created", string.Empty, new NotificationBrushColor().Success);
    }

    public void SetSaveState(bool newSate)
    {
        IsSave = newSate;
        if (!DocName.Contains("*"))
            DocName += IsSave ? "" : "*"; // Si le doc est pas save alors on ajoute une * apres le nom du doc
    }
    public async Task UpdateCurrDir(bool quickUpdate=false)
    {
        if (hierarchyPanel == null || userViewPanel == null)
        {
            return;
        }

        if (!quickUpdate)
        {
            // Get the new directory
            CurrDir = await SavingService.LoadFolder(userViewPanel);
            CurrDir = CurrDir.Replace("file:///", "");
        }
        
        // Check if the user select something
        if (CurrDir == string.Empty)
            return;
        
        SavingService.SaveStringToFile(FolderSavePATH, CurrDir);

        // Updating current folder name
        string[] folders = currDir.Split("/");
        for (int i = 0; i < folders.Length; i++)
        {
            if (i == folders.Length - 1)
            {
                FolderName = "> " + folders[i] + " > ...";
            }
        }

        
        // Destroy everything in the panel
        hierarchyPanel.Children.Clear();
        
        string[] files = Directory.GetFiles(CurrDir);
        // Loop in all found files (including random things like .png)
        foreach (string file in files)
        {
            if (file.Contains(".txt")) // Check if file is in right extention
            {
              
                // Get the file name without the extension
                string name = file.Replace(currDir + "\\", "");
                
                Border border = new Border()
                {
                    Classes = { "Hierarchy" },
                    Background = Brushes.Transparent
                };

                // Loading the Page
                border.PointerPressed += async (sender, args) =>
                {
                    // Get the border, do the effect and disable the effect on all the others
                    foreach (Control control in hierarchyPanel.Children)
                    {
                        if (control is Border element)
                        {
                            if (element == border)
                            {
                                border.Opacity = .6f;
                            }
                            else
                            {
                                element.Opacity = 1;
                            }
                        }
                    }
                    
                    LastSavePath = await SavingService.Load(userViewPanel, true, file);
                    SavingService.SaveStringToFile(PATH, LastSavePath);
                    GetDocName();
                    
                    if (LastSavePath != string.Empty)
                    {
                        AddNewNotification("file load successfuly", "(" + DocName + ")", new NotificationBrushColor().Success);
                        SetSaveState(true);
                    }
                    else
                    {
                        AddNewNotification("loading Cancel", string.Empty, new NotificationBrushColor().Warning);
                    }
                };
                    
                

                // Creating the hierarchy control
                var textBlock = new TextBlock()
                {
                    TextWrapping = TextWrapping.Wrap,
                    Text = "> " + name,
                    Tag = file, // Storing the access path in the control tag
                };

                border.Child = textBlock;
                hierarchyPanel.Children.Add(border);
                
                // Debug
                Console.WriteLine($"[INFO]: File found : {name} at {file}");
            }
            
        }
    }
    
}