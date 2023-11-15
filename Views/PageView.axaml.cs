using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Maximatron.Controls;
using Maximatron.Services;
using Maximatron.ViewModels;

namespace Maximatron;

public partial class PageView : Window
{
    public PageViewModel model;
    public bool init;
    

    public FullScreenPopup savePopup;
    
    public PageView()
    {
        InitializeComponent();
        DataContext = new PageViewModel();
        model = (PageViewModel)DataContext!;
        
        model.Init(this, UserViewStackPanel, NotificationPanel, hierarchyControl.panel);
        StartLoad();
        
        // Create Popup
        savePopup = new FullScreenPopup()
        {
            Name = "ScreenPopup",
            Title = "SAVING ?",
            DescriptionContent = "Would you like to save this document ?",
            ZIndex = 99,
            VerticalAlignment = VerticalAlignment.Top,
            [Grid.RowProperty] = 1,
        };
        View.Children.Add(savePopup);
        
        FinishLoad();
    }
    public void FinishLoad()
    {
        // We are doing a timer of 1sec to let everything init properly
        // The async methode are messing everything so the timer make sure that we are after everything
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        timer.Tick += (sender, e) =>
        {
            init = true;
            timer.Stop();
        };
        timer.Start();

    }
    private async Task StartLoad()
    {
        model.LastSavePath = SavingService.ReadStringFromFile(PageViewModel.PATH);
        await SavingService.Load(this,true, model.LastSavePath);
        model.GetDocName();
        await model.UpdateCurrDir(true);
        
        if (model.LastSavePath != string.Empty)
        {
            model.AddNewNotification("file load successfully", "(" + model.DocName + ")", new NotificationBrushColor().Success);
            model.SetSaveState(true);
        }
        else
        {
            model.AddNewNotification("Loading Failed", "Cannot open last edited file.", new NotificationBrushColor().Error);
        }
    }
    private async void Button_Save(object? sender, RoutedEventArgs e)
    {
        model.LastSavePath = await SavingService.Save(this, false, model.LastSavePath);
        SavingService.SaveStringToFile(PageViewModel.PATH, model.LastSavePath);
        model.GetDocName();
        await model.UpdateCurrDir(true);

        if (!model.IsSave) 
            return;
        
        model.AddNewNotification("file saved successfully", "(" + model.LastSavePath + ")", new NotificationBrushColor().Success);
        model.SetSaveState(true);
    }
    public async void Button_QuickSave(object? sender, RoutedEventArgs e)
    {
        model.LastSavePath = await SavingService.Save(this, true, model.LastSavePath);
        SavingService.SaveStringToFile(PageViewModel.PATH, model.LastSavePath);
        model.GetDocName();
        
        await model.UpdateCurrDir(true);

        if (model.LastSavePath != string.Empty)
        {
            model.AddNewNotification("file saved successfully", "(" + model.LastSavePath + ")", new NotificationBrushColor().Success);
            model.SetSaveState(true);
        }
        else
        {
            model.AddNewNotification("Save Cancel", string.Empty, new NotificationBrushColor().Warning);
        }
    }
    private async void Button_Load(object? sender, RoutedEventArgs e)
    {
        if (!model.IsSave)
        {
            savePopup.Opacity = 1;
            
            savePopup.yesEvent = async (sender, args) =>
            {
                Button_QuickSave(savePopup, null);
                await DoLoad();
            } ;
            
            savePopup.NoEvent = async (sender, args) =>
            {
                await DoLoad();
            } ;
        }
        else
        {
            await DoLoad();
        }


    }

    async Task DoLoad()
    {
        string path = await SavingService.Load(this);

        if (path != String.Empty)
        {
            model.LastSavePath = path;
            SavingService.SaveStringToFile(PageViewModel.PATH, model.LastSavePath);
            model.GetDocName();
            
            model.AddNewNotification("file load successfully", "(" + model.DocName + ")", new NotificationBrushColor().Success);
            model.SetSaveState(true);
        }
        else
        {
            model.AddNewNotification("loading Cancel", string.Empty, new NotificationBrushColor().Warning);
        }
    }
    
    private async void Button_LoadFolder(object? sender, RoutedEventArgs e)
    {
        await model.UpdateCurrDir();
    }
    

    private void AddUserObject(object? sender, RoutedEventArgs e)
    {
        // Check si le sender est valid
        if (sender is not UserInteractable)
            throw new Exception($"[ERROR] : {sender} is not a UserInteractable !");

        model.SetSaveState(false);
        UserInteractable control = (UserInteractable)sender;
        UserViewStackPanel.Children.Add(GetSpawnObject(control));
    }

    private static Control GetSpawnObject(Control control)
    {
        switch (control.Tag)
        {
            case "FIELD":
                return CreateTextField();
            case "LIST":
                // si le call vient du bouton c'est que c'est pas une sous liste
                return control.Name == "ListBtn" ? CreateList() : CreateList(false);
            case "CHECKBOX":
                return CreateCheckBox();
            default:
                // On a pas trouver de type valid
                throw new Exception($"[ERROR] : {control.Tag} is not implemented in GetSpawnObject switch !");
        }
    }


    public static UserObject CreateTextField()
    {
        UserObject userObject = new UserObject
        {
            Classes = { "BasicField" }
        };

        // Creation du context menu (le truc quand tu fais click droit)
        ContextMenu contextMenu = new ContextMenu();

        // Event Paste
        MenuItem pasteMenuItem = new MenuItem { Header = "Paste (ctrl + V)" };
        pasteMenuItem.PointerPressed += PastText;
        contextMenu.Items.Add(pasteMenuItem);

        // Event Copy
        MenuItem copyMenuItem = new MenuItem { Header = "Copy (ctrl + C)" };
        copyMenuItem.PointerPressed += CopyText;
        contextMenu.Items.Add(copyMenuItem);
        
        // Event Cut
        MenuItem cutMenuItem = new MenuItem { Header = "Cut (ctrl + X)" };
        cutMenuItem.PointerPressed += CutText;
        contextMenu.Items.Add(cutMenuItem);
        
        // Event Remove
        MenuItem removeMenuItem = new MenuItem { Header = "Remove" };
        removeMenuItem.PointerPressed += RemoveControl;
        removeMenuItem.Background = Brushes.Brown;
        contextMenu.Items.Add(removeMenuItem);

        // On assigne le context menu a la textbox
        userObject.ContextMenuTest = contextMenu;
        
        // Set event for when text is modify
        userObject.init += (sender, args) =>
        {
            if (userObject.partTextField == null)
                return;
            
            userObject.partTextField.TextChanged += (o, eventArgs) =>
            {
                UnsavePage(userObject);
            };
        };
        
        // On return le userObject fini
        return userObject;

    }
    
    public static UserObject CreateList(bool canAddNewList=true)
    {
        UserObject userObject = new UserObject
        {
            Classes = { "BasicList" }
        };

        StackPanel stackPanel = new StackPanel();
        stackPanel.Name = "ListPanel";
        userObject.Content = stackPanel;

        // Creation du context menu (le truc quand tu fais click droit)
        ContextMenu contextMenu = new ContextMenu();
        

        // On ajoute la propriter d'ajouter une sous liste seulement si c'est pas 
        // Deja une sous liste
        if (canAddNewList)
        {
            // Event Add List to List
            MenuItem addList = new MenuItem { Header = "Add List" };
            addList.Tag = "LIST";
            addList.PointerPressed += AddControlInList;
            contextMenu.Items.Add(addList);
        }

        // Event Add Field to list
        MenuItem addField = new MenuItem { Header = "Add Text Field" };
        addField.Tag = "FIELD";
        addField.PointerPressed += AddControlInList;
        contextMenu.Items.Add(addField);
        
        // Event Add Field to list
        MenuItem addCheckBox = new MenuItem { Header = "Add CheckBox" };
        addCheckBox.Tag = "CHECKBOX";
        addCheckBox.PointerPressed += AddControlInList;
        contextMenu.Items.Add(addCheckBox);
        
        // Event Remove
        MenuItem removeMenuItem = new MenuItem { Header = "Remove" };
        removeMenuItem.PointerPressed += RemoveControl;
        removeMenuItem.Background = Brushes.Brown;
        contextMenu.Items.Add(removeMenuItem);

        // On assigne le context menu a la textbox
        userObject.ContextMenuTest = contextMenu;
        
        // Set event for when text is modify
        userObject.init += (sender, args) =>
        {
            if (userObject.partTextField == null)
                return;
            
            userObject.partTextField.TextChanged += (o, eventArgs) =>
            {
                UnsavePage(userObject);
            };
        };
        
        // On return le userObject fini
        return userObject;
    }
    public static UserObject CreateCheckBox()
    {
        UserObject userObject = new UserObject
        {
            Classes = { "BasicCheckBox" },
            Name = "ListPanel"
        };
        
        // Creation du context menu (le truc quand tu fais click droit)
        ContextMenu contextMenu = new ContextMenu();
        
        // Event Paste
        MenuItem pasteMenuItem = new MenuItem { Header = "Paste (ctrl + V)" };
        pasteMenuItem.PointerPressed += PastText;
        contextMenu.Items.Add(pasteMenuItem);

        // Event Copy
        MenuItem copyMenuItem = new MenuItem { Header = "Copy (ctrl + C)" };
        copyMenuItem.PointerPressed += CopyText;
        contextMenu.Items.Add(copyMenuItem);
        
        // Event Cut
        MenuItem cutMenuItem = new MenuItem { Header = "Cut (ctrl + X)" };
        cutMenuItem.PointerPressed += CutText;
        contextMenu.Items.Add(cutMenuItem);
        
        // Event Remove
        MenuItem removeMenuItem = new MenuItem { Header = "Remove" };
        removeMenuItem.PointerPressed += RemoveControl;
        removeMenuItem.Background = Brushes.Brown;
        contextMenu.Items.Add(removeMenuItem);
        
        // On assigne le context menu a la textbox
        userObject.ContextMenuTest = contextMenu;
        
        // Set event for when text is modify
        userObject.init += (sender, args) =>
        {
            if (userObject.partTextField == null)
                return;
            
            userObject.partTextField.TextChanged += (o, eventArgs) =>
            {
                UnsavePage(userObject);
            };
            
            if (userObject.PartCheckBox == null)
                return;
            
            userObject.PartCheckBox.IsCheckedChanged += (o, eventArgs) =>
            {
                UnsavePage(userObject);
            };
        };
        
        // On return le userObject fini
        return userObject;
    }
    
    private static void PrintTemplateControl(object? sender, PointerPressedEventArgs e)
    {
        Console.WriteLine($"[INFO] : TemplateControl is {GetUserObject(sender)?.Classes[0]}");
    }
    
    private static void AddControlInList(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control control) return;
        
        Panel? panel = (Panel)GetUserObject(sender).Content;
        if (panel == null)
        {
            Console.WriteLine($"[ERROR] : {GetUserObject(sender)} n'est pas un panel");
            return;
        }
        // tout est bon, on peut add le control dans la liste
        panel.Children.Add(GetSpawnObject(control));
        UnsavePage(panel);
    }
    private static void RemoveControl(object? sender, PointerPressedEventArgs e)
    {
        // Check si le sender est bien un control
        if (sender is not Control) return;
        
        // On recup le truc qu'on veut suppr
        // Ps : c'est important de recup le userObject, sinon 
        //      on vas pas delete l'object en entier
        UserObject? userObject = GetUserObject(sender);
        if (userObject == null) 
            return;
        
        // On recup et test le parent pour voir si c'est un panel
        Panel? panel = (Panel)userObject.Parent!;
        panel.Children.Remove(userObject);
        
        UnsavePage(panel);
    }
    

    private static void PastText(object? sender, PointerPressedEventArgs e)
    {
        // Check si le sender est bien un control
        if (sender is not Control) return;
        
        // On recup la textBox
        TextBox? textBox = GetTextbox(sender);
        
        // On test si on a bien trouvé une textbox
        if (textBox == null)
            return;
        
        textBox.Paste();
    }

    private static void CopyText(object? sender, PointerPressedEventArgs e)
    {
        // Check si le sender est bien un control
        if (sender is not Control) return;
        
        // On recup la textBox
        TextBox? textBox = GetTextbox(sender);
        
        // On test si on a bien trouvé une textbox
        if (textBox == null)
            return;
        
        textBox.Copy();
    }
    
    private static void CutText(object? sender, PointerPressedEventArgs e)
    {
        // Check si le sender est bien un control
        if (sender is not Control) return;
        
        // On recup la textBox
        TextBox? textBox = GetTextbox(sender);
        
        // On test si on a bien trouvé une textbox
        if (textBox == null)
            return;
        
        // On recup cut le contenu 
        textBox.Cut();
    }

    private static TextBox? GetTextbox(object? sender)
    {
        if (GetUserObject(sender) == null)
            return null;
        
        TextBox? textBox = GetUserObject(sender)!.partTextField;
        return textBox;
    }

    private static UserObject? GetUserObject(object? sender) // On remonte les parents de l'obj jusqu'a trouver un UserObject
    {
        if (sender is Control control)
        {
            // On test si le control actuel est un UserObject
            if (control.Parent is UserObject userObject)
                return userObject;
            
            // Si le truc de base ne fais pas partie d'un UserObject alors on ne trouvera 
            // jamais de UserObject dans ses parents et quand il n'y aura plus de parent a 
            // test, alors on peut être sûr qu'il faut return
            if (control.Parent == null)
            {
                Console.WriteLine($"[ERROR] : no UserObject is detected on : {control}");
                return null;
            }

            // on a rien trouver donc au recommence avec le parent d'au dessus
            return GetUserObject(control.Parent);
            
        }
        // Le sender est invalid, on arrete tout.
        Console.WriteLine($"[ERROR] : {sender} is not a control");
        return null;
    }

    private static void UnsavePage(Control control)
    {
        PageView view = (PageView)control.GetVisualRoot()!;
        if (!view.init)
        {
            return;
        }

        PageViewModel model = (PageViewModel)view.DataContext!; 
        model.SetSaveState(false);
    }



}