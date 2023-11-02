using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Maximatron.ViewModels;

namespace Maximatron.Controls;

public class UserObject : ContentControl
{
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
}