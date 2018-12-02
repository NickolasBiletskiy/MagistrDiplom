using System;
using System.Windows;

namespace RoutingApp
{
    /// <summary>
    /// Interaction logic for CreateTrafficPopup.xaml
    /// </summary>
    public partial class CreateTrafficPopup : Window
    {
        public Action<int, int, int, int, int, string> OkClicked { get; set; }

        public CreateTrafficPopup()
        {
            InitializeComponent();
        }

        private void buttonCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void buttonOk_Click(object sender, RoutedEventArgs e)
        {
            //validate here

            OkClicked?.Invoke(int.Parse(routerFrom.Text.Trim()), int.Parse(routerTo.Text.Trim()), int.Parse(numberOfPackets.Text.Trim()), int.Parse(sizeOfPackets.Text.Trim()), int.Parse(desiredSpeed.Text.Trim()), trafficName.Text);
            Close();
        }
    }
}
