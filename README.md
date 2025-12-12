# facturas

Aplicación WPF simple para emitir, guardar (XML), listar e inspeccionar facturas.

Plataforma
- .NET: `NET 8`
- UI: WPF

Resumen
- Propósito: crear y gestionar facturas (generar XML, listar facturas recibidas, ver detalle).

Requisitos
- SDK .NET 8
- Visual Studio 2022/2024 o VS Code con extensiones C# y soporte WPF

Cómo compilar y ejecutar
1. Abrir la carpeta del proyecto en el IDE.
2. Restaurar paquetes si es necesario.
3. Compilar y ejecutar la aplicación (proyecto `facturas`).

Estructura principal
- `App.xaml`, `App.xaml.cs` — entrada de la aplicación.
- `MainWindow.xaml`, `MainWindow.xaml.cs` — ventana principal con menú.
- `EmitirFacturaWindow.xaml`, `EmitirFacturaWindow.xaml.cs` — formulario para crear/emitir facturas.
- `FacturasRecibidasWindow.xaml`, `FacturasRecibidasWindow.xaml.cs` — importar/listar facturas XML.
- `FacturaDetalleWindow.xaml`, `FacturaDetalleWindow.xaml.cs` — visor de detalle de una factura XML.
- `facturas.csproj` — configuración del proyecto.

Modelos de datos

- `Concepto` (clase)
  - Representa una línea de factura.
  - Propiedades:
    - `string Codigo`
    - `string Descripcion`
    - `int Cantidad`
    - `decimal PrecioUnitario`
    - `decimal Importe` — calculado: `Math.Round(Cantidad * PrecioUnitario, 2)`.
  - Implementa `INotifyPropertyChanged` para soporte de binding.

- `XmlFactura` (clase)
  - Propiedades:
    - `string Nombre`
    - `string Contenido` — texto XML completo.
    - `string Display` — texto para mostrar en listas.

Ventanas y comportamientos clave

- `MainWindow`
  - Menú con opciones:
    - `Facturas -> Emitir factura` — abre `EmitirFacturaWindow`.
    - `Facturas -> Facturas recibidas (XML)` — abre `FacturasRecibidasWindow`.

- `EmitirFacturaWindow`
  - Controles importantes: `TxtEmisor`, `TxtNif`, `TxtDireccion`, `TxtTelefono`, `TxtCliente`, `DpFecha`, `DgConceptos`, `TxtObservaciones`, `TxtBase`, `TxtIva`, `TxtTotal`.
  - `ObservableCollection<Concepto> Conceptos` como ItemsSource de `DgConceptos`.
  - `RecalcularTotales()` — suma la base imponible, calcula IVA al 21% y total (redondeo a 2 decimales).
  - `DgConceptos_CellEditEnding(...)` — confirma edición y fuerza recálculo mediante `Dispatcher.BeginInvoke` para asegurar que el valor editado se propague.
  - `BtnDescargarXml_Click(...)` — genera un `XDocument` con la factura y guarda con `SaveFileDialog`.
  - Otros botones: generar código, enviar (simulación), imprimir PDF (simulación), cerrar.

- `FacturasRecibidasWindow`
  - Lista `LbArchivos` con `ObservableCollection<XmlFactura>`.
  - `BtnCargarEjemplo_Click()` — genera facturas XML de ejemplo.
  - `BtnCargarDesdeArchivo_Click()` — abre `OpenFileDialog`, valida y añade XML a la lista.
  - `BtnAbrirFactura_Click()` — abre `FacturaDetalleWindow` para ver detalle.

- `FacturaDetalleWindow`
  - Constructor `FacturaDetalleWindow(string xmlContent)` — parsea el XML y rellena controles y la lista de `Concepto`es.
  - `ParseXml(string xml)` — parseo robusto con `TryParse` y `CultureInfo.InvariantCulture` para valores numéricos.

Formato XML esperado

Estructura general:

```
<Factura>
  <Codigo>...</Codigo>
  <Cabecera>
    <Proveedor>...</Proveedor>
    <NIF>...</NIF>
    <Direccion>...</Direccion>
    <Telefono>...</Telefono>
    <Fecha>yyyy-MM-dd</Fecha>
    <Numero>...</Numero>
  </Cabecera>
  <Conceptos>
    <Concepto>
      <Codigo>...</Codigo>
      <Descripcion>...</Descripcion>
      <Cantidad>0</Cantidad>
      <PrecioUnitario>0.00</PrecioUnitario>
      <Importe>0.00</Importe>
    </Concepto>
    <!-- ... -->
  </Conceptos>
  <Pie>
    <BaseImponible>0.00</BaseImponible>
    <Iva>0.00</Iva>
    <Total>0.00</Total>
    <Observaciones>...</Observaciones>
  </Pie>
</Factura>
```

Notas técnicas y recomendaciones
- Se usa `CultureInfo.InvariantCulture` para serializar/parsear valores monetarios; evita problemas con separador decimal según cultura.
- Extraer la tasa de IVA a una constante o configuración si se requiere flexibilidad.
- Añadir validaciones: NIF, que existan `Concepto`es antes de guardar/enviar, coherencia de totales.
- Para exportar a PDF real, integrar librería (por ejemplo `PdfSharp`, `QuestPDF`, `iText7` — verificar licencias).
- Mantener la llamada a `DgConceptos.CommitEdit(...)` en el manejador de `CellEditEnding` para asegurar actualización del binding.

Contribuir
- Abrir un PR en el repo remoto `origin`.
- Seguir convenciones del proyecto y añadir tests si se añade lógica compleja.

Licencia
- Sin licencia explícita; añadir `LICENSE` si se desea.

---

Si quieres, puedo:
- Añadir comentarios XML `///` en las clases y métodos para generar documentación con herramientas como DocFX.
- Generar una versión HTML o `docs/` con esta documentación.

