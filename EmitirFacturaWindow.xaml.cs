using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Microsoft.Win32;

namespace facturas
{
    public partial class EmitirFacturaWindow : Window
    {
        public ObservableCollection<Concepto> Conceptos { get; set; } = new ObservableCollection<Concepto>();

        public EmitirFacturaWindow()
        {
            InitializeComponent();
            DgConceptos.ItemsSource = Conceptos;
            DpFecha.SelectedDate = DateTime.Now;

            // add a realistic sample row
            Conceptos.Add(new Concepto { Codigo = "SRV-001", Descripcion = "Servicios de consultoria tecnica - 10h", Cantidad = 10, PrecioUnitario = 60.00m });
            RecalcularTotales();

            Conceptos.CollectionChanged += (s, e) => RecalcularTotales();
        }

        private void RecalcularTotales()
        {
            var baseImp = Conceptos.Sum(c => c.Cantidad * c.PrecioUnitario);
            var iva = Math.Round(baseImp * 0.21m, 2);
            var total = Math.Round(baseImp + iva, 2);

            TxtBase.Text = baseImp.ToString("F2", CultureInfo.InvariantCulture);
            TxtIva.Text = iva.ToString("F2", CultureInfo.InvariantCulture);
            TxtTotal.Text = total.ToString("F2", CultureInfo.InvariantCulture);
        }

        private void BtnGenerarCodigo_Click(object sender, RoutedEventArgs e)
        {
            var code = "CTI-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            MessageBox.Show($"Código de factura generado: {code}", "Código", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DgConceptos_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            // commit diferido fuera del contexto del evento
            DgConceptos.CommitEdit(DataGridEditingUnit.Row, true);
            RecalcularTotales();
        }), System.Windows.Threading.DispatcherPriority.Background);
    }
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var request = new TraversalRequest(FocusNavigationDirection.Next);
                var elementWithFocus = Keyboard.FocusedElement as System.Windows.UIElement;
                if (elementWithFocus != null)
                {
                    elementWithFocus.MoveFocus(request);
                    e.Handled = true;
                }
            }
        }

        private void BtnDescargarXml_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var code = "CTI-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                var doc = new XDocument(
                    new XElement("Factura",
                        new XElement("Codigo", code),
                        new XElement("Cabecera",
                            new XElement("Proveedor", TxtEmisor.Text ?? string.Empty),
                            new XElement("NIF", TxtNif.Text ?? string.Empty),
                            new XElement("Direccion", TxtDireccion.Text ?? string.Empty),
                            new XElement("Telefono", TxtTelefono.Text ?? string.Empty),
                            new XElement("Fecha", DpFecha.SelectedDate?.ToString("yyyy-MM-dd") ?? string.Empty),
                            new XElement("Numero", code)
                        ),
                        new XElement("Conceptos",
                            from c in Conceptos
                            select new XElement("Concepto",
                                new XElement("Codigo", c.Codigo),
                                new XElement("Descripcion", c.Descripcion),
                                new XElement("Cantidad", c.Cantidad),
                                new XElement("PrecioUnitario", c.PrecioUnitario.ToString("F2", CultureInfo.InvariantCulture)),
                                new XElement("Importe", (c.Cantidad * c.PrecioUnitario).ToString("F2", CultureInfo.InvariantCulture))
                            )
                        ),
                        new XElement("Pie",
                            new XElement("BaseImponible", TxtBase.Text),
                            new XElement("Iva", TxtIva.Text),
                            new XElement("Total", TxtTotal.Text),
                            new XElement("Observaciones", TxtObservaciones.Text ?? string.Empty)
                        )
                    )
                );

                var dlg = new SaveFileDialog();
                dlg.Filter = "XML Files|*.xml|All files|*.*";
                dlg.FileName = code + ".xml";
                if (dlg.ShowDialog() == true)
                {
                    doc.Save(dlg.FileName);
                    MessageBox.Show($"Factura guardada en: {dlg.FileName}", "Guardado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar XML: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEnviar_Click(object sender, RoutedEventArgs e)
        {
            // Simulate sending the invoice (e.g., via email or API)
            var code = "CTI-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            MessageBox.Show($"Factura {code} enviada (simulación).", "Enviar", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnImprimirPdf_Click(object sender, RoutedEventArgs e)
        {
            // For now generate a simple PDF-like window (real PDF generation would need a library)
            MessageBox.Show("Se generará un PDF con la factura (simulación).", "Imprimir PDF", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class Concepto : INotifyPropertyChanged
    {
        private string _codigo = string.Empty;
        public string Codigo { get => _codigo; set { _codigo = value; OnPropertyChanged(nameof(Codigo)); } }

        private string _descripcion = string.Empty;
        public string Descripcion { get => _descripcion; set { _descripcion = value; OnPropertyChanged(nameof(Descripcion)); } }

        private int _cantidad;
        public int Cantidad { get => _cantidad; set { _cantidad = value; OnPropertyChanged(nameof(Cantidad)); OnPropertyChanged(nameof(Importe)); } }

        private decimal _precioUnitario;
        public decimal PrecioUnitario { get => _precioUnitario; set { _precioUnitario = value; OnPropertyChanged(nameof(PrecioUnitario)); OnPropertyChanged(nameof(Importe)); } }
        public decimal Importe => Math.Round(Cantidad * PrecioUnitario, 2);

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
