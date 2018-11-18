using RoutingApp.Core.Models.NetComponents;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RoutingApp.Controls
{
    /// <summary>
    /// Interaction logic for WorkingRouter.xaml
    /// </summary>
    public partial class WorkingRouter : Button
    {
        #region Fields

        protected bool isDragging;
        private Point clickPosition;

        protected Router Router;

        #endregion

        public WorkingRouter(Router router) : this()
        {
            this.Router = router;
        }

        public WorkingRouter()
        {
            InitializeComponent();
        }

        private void Control_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            var draggableControl = sender as WorkingRouter;
            clickPosition = e.GetPosition(this);
            draggableControl.CaptureMouse();
        }

        private void Control_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            var draggable = sender as WorkingRouter;
            draggable.ReleaseMouseCapture();

            //save position state
            if (Router != null)
            {
                Router.PositionX = Canvas.GetLeft(this);
                Router.PositionY = Canvas.GetTop(this);
            }
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            var draggableControl = sender as WorkingRouter;

            if (isDragging && draggableControl != null)
            {
                Point currentPosition = e.GetPosition(this.Parent as UIElement);

                Canvas.SetLeft(this, currentPosition.X - clickPosition.X);
                Canvas.SetTop(this, currentPosition.Y - clickPosition.Y);
            }
        }
    }
}
