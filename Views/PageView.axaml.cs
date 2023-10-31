using System;
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


    private void AddNewTextField(object? sender, RoutedEventArgs e)
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

        // On assigne le context menu a la textbox
        userObject.ContextMenuTest = contextMenu;

        // Ajout de la text box final a un panel
        UserViewStackPanel.Children.Add(userObject);
    }
    
    private void AddNewList(object? sender, RoutedEventArgs e)
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

        // On assigne le context menu a la textbox
        userObject.ContextMenuTest = contextMenu;

        // Ajout de la text box final a un panel
        UserViewStackPanel.Children.Add(userObject);
    }

    private void RemoveControl(object? sender, PointerPressedEventArgs e)
    {
        // Check si le sender est bien un control
        if (sender is not Control) return;
        
        var textBox = GetTemplateParent(sender);
        
        if (textBox != null) 
            UserViewStackPanel.Children.Remove(textBox);
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

    private Control? GetTemplateParent(object sender)
    {
        if (sender is Control control)
        {
            // on get la textBox
            // PS : on prend 3 parents au dessus pour avoir le control 
            var parent = (Control?)(control.Parent?.Parent?.Parent.Parent);
            Console.WriteLine(control.TemplatedParent); 
            return parent;

        }

        return null;
    }
}