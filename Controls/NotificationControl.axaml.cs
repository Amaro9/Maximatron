using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace Maximatron.Controls;

public struct NotificationBrushColor
{
    public SolidColorBrush Success = new (Color.FromRgb(0,250,0));
    public SolidColorBrush Warning = new (Color.FromRgb(230,250,0));
    public SolidColorBrush Error = new (Color.FromRgb(250,0,0));

    public NotificationBrushColor()
    {
    }
}

public class NotificationControl : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<NotificationControl, string>(
            nameof(Title), "Title");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> DescriptionProperty = AvaloniaProperty.Register<NotificationControl, string>(
            nameof(Description), "Description");

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly StyledProperty<IBrush> BorderColorProperty = AvaloniaProperty.Register<NotificationControl, IBrush>(
        nameof(BorderColor) );

    public IBrush? BorderColor
    {
        get => GetValue(BorderColorProperty);
        set => SetValue(BorderColorProperty, value);
    }
    

}