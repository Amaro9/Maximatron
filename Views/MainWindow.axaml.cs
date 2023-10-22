using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;
using System;
using System.Diagnostics;

namespace Maximatron.Views
{
    public partial class MainWindow : Window
    {
        private bool isPressed;


        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateNewNote(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Random ran = new Random();
            NoteControl noteControl = new NoteControl
            {
                Width = 500,
                Height = 50,
            };


            Debug.WriteLine(ConvertColumnStarToPixels(0));

            noteControl.PointerPressed += OnPointerPressed;
            noteControl.PointerMoved += OnPointerMoved;
            noteControl.PointerReleased += OnPointerReleased;

            noteControl.Background = new SolidColorBrush(Colors.LightBlue);

            BoardView.Children.Add(noteControl);
        }

        public void OnPointerPressed(object? sender, PointerPressedEventArgs e)
        {

            if (sender == null)
                return;

            isPressed = true;

            var tkt = (NoteControl)sender;
            tkt.StartMove(e);

        }

        private double ConvertStarToPixels(double starSize, double totalAvailableSpace)
        {
            // Calculate the size in pixels for a column with the given star size
            return starSize * (totalAvailableSpace / (1 + starSize));
        }

        private double ConvertColumnStarToPixels(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= MainGrid.ColumnDefinitions.Count)
            {
                throw new ArgumentOutOfRangeException("Invalid column index");
            }

            ColumnDefinition column = MainGrid.ColumnDefinitions[columnIndex];
            double starSize = column.Width.Value;

             

            double sizeInPixels = ConvertStarToPixels(starSize, Width);
            return sizeInPixels;

        }

        public void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            isPressed = false;
        }

        public void OnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (sender == null ||!isPressed)
                return;

            var movable = (NoteControl)sender;


            // Calc des position pour que le control reste centrer par rapport a la souris
            double CurrX = e.GetPosition(this).X - Width / 2 + movable.Width / 2; // position de la souris relative a la window - window with / 2 + la width du control / 2
            CurrX += ConvertColumnStarToPixels(0);
            double CurrY = e.GetPosition(this).Y - GetControlChildIndex(movable, BoardView) * movable.Height; // Vue que le movable est dans un stack faut retirer le nb de control * par la taille
            Point CurrentNotePos = new Point(CurrX, CurrY);
            
            // Movement du control
            movable.RenderTransform = movable.MoveBlock(e, CurrentNotePos);

        }

        int GetControlChildIndex(Control control, Panel parent)
        {
            // On loop dans tous les childs du parent.
            for (int i = 0; i < parent.Children.Count; i++)
            {
                // Check pour savoir si le child == le control.
                if (ReferenceEquals(parent.Children[i], control))
                {
                    return i;
                }
            }

            // On a rien trouver du coup on return -1.
            return -1;
        }
    }

}