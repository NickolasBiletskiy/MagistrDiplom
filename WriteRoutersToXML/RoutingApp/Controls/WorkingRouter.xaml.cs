using RoutingApp.Core.Models.NetComponents;
using System;
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
        public Action<WorkingRouter> OnRouterMove;

        public Router Router;

        public bool IsMovingEnabled = true;

        #endregion

        public WorkingRouter(Router router) : this()
        {
            this.Router = router;
            var toolTip = this.FindName("RouterToolTip") as ToolTip;

            toolTip.Content = Router.Name;
        }

        public WorkingRouter()
        {
            InitializeComponent();
        }

        private void Control_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (IsMovingEnabled)
            {
                isDragging = true;
                var draggableControl = sender as WorkingRouter;
                clickPosition = e.GetPosition(this);
                draggableControl.CaptureMouse();
            }
        }

        private void Control_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (IsMovingEnabled)
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
        }

        private void Control_MouseMove(object sender, MouseEventArgs e)
        {
            if (IsMovingEnabled)
            {
                var draggableControl = sender as WorkingRouter;

                if (isDragging && draggableControl != null)
                {
                    Point currentPosition = e.GetPosition(this.Parent as UIElement);

                    Router.PositionX = currentPosition.X - clickPosition.X;
                    Router.PositionY = currentPosition.Y - clickPosition.Y;

                    Canvas.SetLeft(this, Router.PositionX);
                    Canvas.SetTop(this, Router.PositionY);

                    OnRouterMove?.Invoke(this);
                }
            }
        }
    }
}
