using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Maximatron;

public partial class NoteControl : UserControl
{
    public NoteControl()
    {
        InitializeComponent();
    }

    private Point positionInBlock;
    private TranslateTransform transform = null!;


    public void StartMove(PointerPressedEventArgs e)
    {
        positionInBlock = e.GetPosition(this);

        if (transform != null!)
            positionInBlock = new Point(
                positionInBlock.X - transform.X,
                positionInBlock.Y - transform.Y);

    }

    public TranslateTransform MoveBlock(PointerEventArgs e, Point currentPosition)
    {

        var offsetX = currentPosition.X - positionInBlock.X;
        var offsetY = currentPosition.Y - positionInBlock.Y;

        return new TranslateTransform(offsetX, offsetY);

    }
}