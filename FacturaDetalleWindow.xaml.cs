using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace facturas
{
    public partial class FacturaDetalleWindow : Window
    {
        public ObservableCollection<Concepto> Conceptos { get; set; } = new ObservableCollection<Concepto>();

        public FacturaDetalleWindow(string xmlContent)
        {
            InitializeComponent();
            DgConceptos.ItemsSource = Conceptos;
            ParseXml(xmlContent);
        }

        private void ParseXml(string xml)
        {
            var doc = XDocument.Parse(xml);
            var root = doc.Element("Factura");
            if (root == null) throw new Exception("XML no tiene elemento raiz 'Factura'.");

            var cab = root.Element("Cabecera");
            if (cab != null)
            {
                TxtProveedor.Text = (string?)cab.Element("Proveedor") ?? string.Empty;
                TxtNif.Text = (string?)cab.Element("NIF") ?? string.Empty;
                TxtDireccion.Text = (string?)cab.Element("Direccion") ?? string.Empty;
                TxtTelefono.Text = (string?)cab.Element("Telefono") ?? string.Empty;
                TxtFecha.Text = (string?)cab.Element("Fecha") ?? string.Empty;
                TxtNumero.Text = (string?)cab.Element("Numero") ?? string.Empty;
            }

            Conceptos.Clear();
            var conceptos = root.Element("Conceptos");
            if (conceptos != null)
            {
                foreach (var c in conceptos.Elements("Concepto"))
                {
                    var codigo = (string?)c.Element("Codigo") ?? string.Empty;
                    var descripcion = (string?)c.Element("Descripcion") ?? string.Empty;
                    int cantidad = 0;
                    decimal precio = 0m;
                    if (int.TryParse((string?)c.Element("Cantidad"), out var q)) cantidad = q;
                    if (decimal.TryParse((string?)c.Element("PrecioUnitario"), NumberStyles.Any, CultureInfo.InvariantCulture, out var p)) precio = p;

                    var cc = new Concepto
                    {
                        Codigo = codigo,
                        Descripcion = descripcion,
                        Cantidad = cantidad,
                        PrecioUnitario = precio
                    };
                    Conceptos.Add(cc);
                }
            }

            // Pie datos
            decimal baseImp = 0m;
            decimal iva = 0m;
            decimal total = 0m;
            var pie = root.Element("Pie");
            if (pie != null)
            {
                if (decimal.TryParse((string?)pie.Element("BaseImponible"), NumberStyles.Any, CultureInfo.InvariantCulture, out var b)) baseImp = b;
                if (decimal.TryParse((string?)pie.Element("Iva"), NumberStyles.Any, CultureInfo.InvariantCulture, out var i)) iva = i;
                if (decimal.TryParse((string?)pie.Element("Total"), NumberStyles.Any, CultureInfo.InvariantCulture, out var t)) total = t;
            }

            TxtBase.Text = baseImp.ToString("F2", CultureInfo.InvariantCulture);
            TxtIva.Text = iva.ToString("F2", CultureInfo.InvariantCulture);
            TxtTotal.Text = total.ToString("F2", CultureInfo.InvariantCulture);
        }

        private void BtnImprimirPdf_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Se generará un PDF con la factura (simulación).", "Imprimir PDF", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
