using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Maximatron.Controls;

namespace Maximatron;

public partial class PageView : Window
{
    public PageView()
    {
        InitializeComponent();
    }

    private void AddUserObject(object? sender, RoutedEventArgs e)
    {
        // Check si le sender est valid
        if (sender is not UserInteractable)
            throw new Exception($"[ERROR] : {sender} is not a UserInteractable !");

        UserInteractable control = (UserInteractable)sender;
        // On Test le TextContent pour savoir quel USerControl il faudra spawn
        switch (control.TextContent)
        {
            case "FIELD":
                UserViewStackPanel.Children.Add(CreateTextField());
                break;
            case "LIST":
                UserViewStackPanel.Children.Add(CreateList());
                break;
            default:
                // On a pas trouver de type valid
                throw new Exception($"[ERROR] : {control.TextContent} is not implemented in AddUserControl switch !");
        }
        
    }


    private UserObject CreateTextField()
    {
        UserObject userObject = new UserObject()
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
        
        
        return userObject;

    }
    
    private UserObject CreateList()
    {
        UserObject userObject = new UserObject()
        {
            Classes = { "BasicList" }
        };

        // Creation du context menu (le truc quand tu fais click droit)
        ContextMenu contextMenu = new ContextMenu();
        contextMenu.Background = Brushes.Brown;
        
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
        
        return userObject;
    }
    
    
    
    
    
    private void PrintTemplateControl(object? sender, PointerPressedEventArgs e)
    {
        Console.WriteLine($"[INFO] : TemplateControl is {GetUserObject(sender).Classes[0]}");
    }


    private void AddControlInList(object? list)
    {
        
    }
    private void RemoveControl(object? sender, PointerPressedEventArgs e)
    {
        // Check si le sender est bien un control
        if (sender is not Control) return;
        
        UserObject? userObject = GetUserObject(sender);
        
        if (userObject != null) 
            UserViewStackPanel.Children.Remove(userObject);
    }
    

    private void PastText(object? sender, PointerPressedEventArgs e)
    {
        // Check si le sender est bien un control
        if (sender is not Control) return;
        
        var textBox = GetTextbox(sender);
        textBox?.Paste();
    }

    private void CopyText(object? sender, PointerPressedEventArgs e)
    {
        // Check si le sender est bien un control
        if (sender is not Control) return;
        
        var textBox = GetTextbox(sender);
        textBox?.Copy();
    }
    
    private void CutText(object? sender, PointerPressedEventArgs e)
    {
        // Check si le sender est bien un control
        if (sender is not Control) return;
        
        var textBox = GetTextbox(sender);
        textBox?.Cut();
    }

    private TextBox? GetTextbox(object? sender)
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

    private UserObject? GetUserObject(object? sender)
    {
        if (sender is Control control)
        {
            if (control.Parent is UserObject userObject)
                return userObject;

            return GetUserObject(control.Parent);
                
            
        }
        Console.WriteLine($"[ERROR] : {sender} is not a control");
        return null;
    }
}