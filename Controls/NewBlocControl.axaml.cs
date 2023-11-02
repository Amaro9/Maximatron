using Avalonia;
using Avalonia.Controls;

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

    public static readonly StyledProperty<string> ImagePathProperty = AvaloniaProperty.Register<UserInteractable, string>(
        nameof(ImagePath), "../Assets/checkMark.svg");

    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<UserInteractable, bool>(
        "IsOpen");

    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public string ImagePath
    {
        get => GetValue(ImagePathProperty);
        set => SetValue(ImagePathProperty, value);
    }
    
   

}