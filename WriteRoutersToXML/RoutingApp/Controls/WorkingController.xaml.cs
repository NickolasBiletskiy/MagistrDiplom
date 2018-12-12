using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace RoutingApp.Controls
{
    /// <summary>
    /// Interaction logic for WorkingController.xaml
    /// </summary>
    public partial class WorkingController : Button
    {
        #region Fields

        protected bool isDragging;
        private Point clickPosition;
        public Action OnControllerMove;
        public double PositionX;
        public double PositionY;

        public bool IsMovingEnabled = true;

        #endregion

        public WorkingController()
        {
            InitializeComponent();
        }

        private void Control_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMovingEnabled)
            {
                isDragging = true;
                var draggableControl = sender as WorkingController;
                clickPosition = e.GetPosition(this);
                draggableControl.CaptureMouse();
            }
        }

        private void Control_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMovingEnabled)
            {
                isDragging = false;
                var draggable = sender as WorkingController;
                draggable.ReleaseMouseCapture();                
            }
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMovingEnabled)
            {
                var draggableControl = sender as WorkingController;

                if (isDragging && draggableControl != null)
                {
                    Point currentPosition = e.GetPosition(this.Parent as UIElement);

                    PositionX = currentPosition.X - clickPosition.X;
                    PositionY = currentPosition.Y - clickPosition.Y;

                    Canvas.SetLeft(this, PositionX);
                    Canvas.SetTop(this, PositionY);

                    OnControllerMove?.Invoke();
                }
            }
        }       
    }
}
