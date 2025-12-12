using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;
using Microsoft.Win32;
using facturas.Data;

namespace facturas
{
    public partial class FacturasRecibidasWindow : Window
    {
        public ObservableCollection<XmlFactura> Archivos { get; set; } = new ObservableCollection<XmlFactura>();
        private readonly SqlQueryService _sqlService = new SqlQueryService();

        public FacturasRecibidasWindow()
        {
            InitializeComponent();
            LbArchivos.ItemsSource = Archivos;
            LbArchivos.SelectionChanged += LbArchivos_SelectionChanged;
        }

        private void LbArchivos_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (LbArchivos.SelectedItem is XmlFactura xf)
            {
                TxtVista.Text = xf.Contenido;
            }
            else
            {
                TxtVista.Text = string.Empty;
            }
        }

        private void BtnCargarEjemplo_Click(object sender, RoutedEventArgs e)
        {
            // create two realistic example XML invoices
            var xml1 = GenerarXmlEjemplo("CTI-REC-" + DateTime.Now.ToString("yyyyMMddHHmmss"), "Proveedor Canarias S.L.", "B12345678", "Calle La Palma 12, 35000 Las Palmas", "(+34) 928 111 222", "2025-11-01", "INV-2025-001");
            var xml2 = GenerarXmlEjemplo("CTI-REC-" + DateTime.Now.AddMinutes(1).ToString("yyyyMMddHHmmss"), "Servicios Atlánticos SA", "B87654321", "Avenida Gran Canaria 45, 38002 Las Palmas", "(+34) 928 333 444", "2025-11-05", "INV-2025-002");

            Archivos.Clear();
            Archivos.Add(new XmlFactura { Nombre = "factura_2025_001.xml", Contenido = xml1, Display = "factura_2025_001.xml - Proveedor Canarias S.L. - INV-2025-001" });
            Archivos.Add(new XmlFactura { Nombre = "factura_2025_002.xml", Contenido = xml2, Display = "factura_2025_002.xml - Servicios Atlánticos SA - INV-2025-002" });
        }

        private void BtnCargarDesdeArchivo_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "XML Files|*.xml|All files|*.*";
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var text = File.ReadAllText(dlg.FileName);
                    // validate XML
                    var doc = XDocument.Parse(text);
                    Archivos.Add(new XmlFactura { Nombre = Path.GetFileName(dlg.FileName), Contenido = text, Display = Path.GetFileName(dlg.FileName) });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar XML: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnAbrirFactura_Click(object sender, RoutedEventArgs e)
        {
            if (LbArchivos.SelectedItem is XmlFactura xf)
            {
                try
                {
                    var view = new FacturaDetalleWindow(xf.Contenido);
                    view.Owner = this;
                    view.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al abrir factura: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Seleccione una factura.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string GenerarXmlEjemplo(string codigo, string proveedor, string nif, string direccion, string telefono, string fecha, string numero)
        {
            var doc = new XDocument(
                new XElement("Factura",
                    new XElement("Codigo", codigo),
                    new XElement("Cabecera",
                        new XElement("Proveedor", proveedor),
                        new XElement("NIF", nif),
                        new XElement("Direccion", direccion),
                        new XElement("Telefono", telefono),
                        new XElement("Fecha", fecha),
                        new XElement("Numero", numero)
                    ),
                    new XElement("Conceptos",
                        new XElement("Concepto",
                            new XElement("Codigo", "PRD-001"),
                            new XElement("Descripcion", "Suministro de hardware"),
                            new XElement("Cantidad", 2),
                            new XElement("PrecioUnitario", 250.00),
                            new XElement("Importe", 500.00)
                        ),
                        new XElement("Concepto",
                            new XElement("Codigo", "SRV-010"),
                            new XElement("Descripcion", "Instalación y soporte"),
                            new XElement("Cantidad", 5),
                            new XElement("PrecioUnitario", 40.00),
                            new XElement("Importe", 200.00)
                        )
                    ),
                    new XElement("Pie",
                        new XElement("BaseImponible", 700.00),
                        new XElement("Iva", 147.00),
                        new XElement("Total", 847.00),
                        new XElement("Observaciones", "Pago a 30 días")
                    )
                )
            );

            using (var sw = new StringWriter())
            {
                doc.Save(sw);
                return sw.ToString();
            }
        }

        private void BtnGenerarInsert_Click(object sender, RoutedEventArgs e)
        {
            if (LbArchivos.SelectedItem is XmlFactura xf)
            {
                try
                {
                    var rec = _sqlService.GenerateInsertFromXml(xf.Contenido);
                    MessageBox.Show("INSERT guardado en Data/queries.json.", "Generado", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al generar INSERT: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Seleccione una factura.", "Atención", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnImprimirPdf_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Se generará un PDF con la factura recibida (simulación).", "Imprimir PDF", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnUploadToDb_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de subida a BD pendiente de configuración.", "Subir a BD", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class XmlFactura
    {
        public string Nombre { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
    }
}