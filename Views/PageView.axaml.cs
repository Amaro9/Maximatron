using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Maximatron.Controls;
using Maximatron.Services;
using Maximatron.ViewModels;

// ReSharper disable All

namespace Maximatron;

public partial class PageView : Window
{
    public PageViewModel model;
    public PageView()
    {
        InitializeComponent();
        DataContext = new PageViewModel();
        model = (PageViewModel)DataContext!;
        
        Load();
    }

    public async void Load()
    {
        model.LastSavePath = SavingService.ReadStringFromFile("lastSavePath.txt");
        await SavingService.Load(this,true, model.LastSavePath);
        model.GetDocName();

    }
    
    private async void Button_Save(object? sender, RoutedEventArgs e)
    {
        string path = "lastSavePath.txt";
        model.LastSavePath = await SavingService.Save(this, false, model.LastSavePath);
        SavingService.SaveStringToFile(path, model.LastSavePath);
        
        model.GetDocName();

    }
    
    private async void Button_QuickSave(object? sender, RoutedEventArgs e)
    {
        await SavingService.Save(this, true, model.LastSavePath);
    }
    private async void Button_Load(object? sender, RoutedEventArgs e)
    {
        string path = "lastSavePath.txt";
        model.LastSavePath = await SavingService.Load(this);
        SavingService.SaveStringToFile(path, model.LastSavePath);
        model.GetDocName();

    }

    private void AddUserObject(object? sender, RoutedEventArgs e)
    {
        // Check si le sender est valid
        if (sender is not UserInteractable)
            throw new Exception($"[ERROR] : {sender} is not a UserInteractable !");

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
        
        // Event Test
        MenuItem test = new MenuItem { Header = "[In Editor] Get Template Control" };
        test.PointerPressed += PrintTemplateControl;
        contextMenu.Items.Add(test);

        // On assigne le context menu a la textbox
        userObject.ContextMenuTest = contextMenu;
        
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
        
        // Event Test
        MenuItem test = new MenuItem { Header = "[In Editor] Get Template Control" };
        test.PointerPressed += PrintTemplateControl;
        contextMenu.Items.Add(test);


        // On assigne le context menu a la textbox
        userObject.ContextMenuTest = contextMenu;
        
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
        
        // Event Test
        MenuItem test = new MenuItem { Header = "[In Editor] Get Template Control" };
        test.PointerPressed += PrintTemplateControl;
        contextMenu.Items.Add(test);


        // On assigne le context menu a la textbox
        userObject.ContextMenuTest = contextMenu;
        
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
        panel.Children.Add(GetSpawnObject(control));

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
        Panel? panel = (Panel)userObject.Parent;
        if (panel == null)
            return;
        
        panel.Children.Remove(userObject);
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
        
        textBox?.Paste();
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
        if (sender is Control control)
        {
            // on get la textBox
            // PS : on prend 3 parents au dessus pour avoir la textbox 
            var textBox = (TextBox?)(control.Parent?.Parent?.Parent);
            return textBox;
        }

        return null;
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
    
}