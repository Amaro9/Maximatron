using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Maximatron.Controls;

public class FullScreenPopup : TemplatedControl
{
    public EventHandler yesEvent;
    public EventHandler NoEvent;
    private Button? yesBtn;
    private Button? noBtn;
    
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<FullScreenPopup, string>(
        nameof(Title), "Title");

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> DescriptionContentProperty = AvaloniaProperty.Register<FullScreenPopup, string>(
        nameof(DescriptionContent), "No description.");

    public string DescriptionContent
    {
        get => GetValue(DescriptionContentProperty);
        set => SetValue(DescriptionContentProperty, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> YesClickEvent =
        RoutedEvent.Register<FullScreenPopup, RoutedEventArgs>(nameof(ClickYes), RoutingStrategies.Bubble);



    public event EventHandler<RoutedEventArgs> ClickYes
    {
        add => AddHandler(YesClickEvent, value);
        remove => RemoveHandler(YesClickEvent, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Opacity = 0;

        yesBtn = e.NameScope.Find<Button>("YesBtn");
        if (yesBtn != null)
        {
            yesBtn.Click += YesClick;
        }
        noBtn = e.NameScope.Find<Button>("NoBtn");
        if (noBtn != null)
        {
            noBtn.Click += NoClick;
        }
    }

    private void YesClick(object? sender, RoutedEventArgs routedEventArgs)
    {
        Opacity = 0;
        yesEvent.Invoke(this, EventArgs.Empty);
    }
    private void NoClick(object? sender, RoutedEventArgs routedEventArgs)
    {
        Opacity = 0;
        NoEvent.Invoke(this, EventArgs.Empty);
    }
}