using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Maximatron.ViewModels;

namespace Maximatron.Controls;

public class UserObject : ContentControl
{
    public TextBox? partTextField;
    public EventHandler init;
    
    public static readonly StyledProperty<ContextMenu>? ContextMenuTestProperty = AvaloniaProperty.Register<UserInteractable, ContextMenu>(
        nameof(ContextMenuTest));
    
    

    public ContextMenu? ContextMenuTest
    {
        get => GetValue(ContextMenuTestProperty);
        set => SetValue(ContextMenuTestProperty, value);
    }
    public UserObject()
    {
        DataContext = new UserObjectModel();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        partTextField = e.NameScope.Find<TextBox>("PART_TextField");
        
        // The textBox is set, we can call init to notify the pageView.cs 
        init.Invoke(null, EventArgs.Empty);
    }
}