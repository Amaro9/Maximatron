using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Maximatron.ViewModels;

namespace Maximatron.Controls;

public class UserInteractable : Button
{

    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<UserInteractable, string>(
        nameof(TextContent), "default");

    public string TextContent
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

}