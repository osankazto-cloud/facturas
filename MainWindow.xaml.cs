using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace facturas
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Menu_EmitirFactura_Click(object sender, RoutedEventArgs e)
        {
            var w = new EmitirFacturaWindow();
            w.Owner = this;
            w.ShowDialog();
        }

        private void Menu_FacturasRecibidas_Click(object sender, RoutedEventArgs e)
        {
            var w = new FacturasRecibidasWindow();
            w.Owner = this;
            w.ShowDialog();
        }
    }
}