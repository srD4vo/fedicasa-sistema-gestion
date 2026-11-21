using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FediCasa.Helpers;
using FediCasa.Models;
using FediCasa.Services;

namespace FediCasa.Forms
{
    public class FrmVentas : Form
    {
        // =========================================================
        // CONTROLES
        // =========================================================

        private TextBox txtCodigoProducto;
        private TextBox txtCantidad;
        private TextBox txtDocumentoCliente;
        private TextBox txtNombreCliente;

        private ComboBox cboFormaPago;

        private Button btnAgregar;
        private Button btnModificar;
        private Button btnEliminar;
        private Button btnVaciar;
        private Button btnRegistrar;
        private Button btnConfirmar;
        private Button btnBoleta;
        private Button btnAnular;
        private Button btnCerrar;

        private DataGridView dgvCarrito;

        private Label lblSubtotal;
        private Label lblIGV;
        private Label lblTotal;

        private Label lblVendedor;
        private Label lblEstadoVenta;

        private int idVentaActual = 0;

        private List<Venta> ventaActual =
            new List<Venta>();

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public FrmVentas()
        {
            Text = "FediCasa - Gestión de ventas";
            StartPosition =
                FormStartPosition.CenterScreen;

            Size =
                new Size(1180, 730);

            MinimumSize =
                new Size(1050, 650);

            BackColor =
                Color.White;

            ConstruirInterfaz();
            ConfigurarPermisos();
            InicializarFormulario();
        }

        // =========================================================
        // INTERFAZ
        // =========================================================

        private void ConstruirInterfaz()
        {
            Label lblTitulo =
                new Label
                {
                    Text =
                        "GESTIÓN DE VENTAS",

                    Font =
                        new Font(
                            "Segoe UI",
                            18,
                            FontStyle.Bold),

                    AutoSize = true,

                    Location =
                        new Point(25, 20)
                };

            lblVendedor =
                new Label
                {
                    Text =
                        "Vendedor:",

                    AutoSize = true,

                    ForeColor =
                        Color.DimGray,

                    Location =
                        new Point(28, 55)
                };

            // =====================================================
            // AGREGAR PRODUCTO
            // =====================================================

            GroupBox grpProducto =
                new GroupBox
                {
                    Text =
                        "Agregar producto",

                    Location =
                        new Point(20, 85),

                    Size =
                        new Size(420, 180)
                };

            Label lblCodigo =
                new Label
                {
                    Text =
                        "Código:",

                    AutoSize = true,

                    Location =
                        new Point(20, 35)
                };

            txtCodigoProducto =
                new TextBox
                {
                    Location =
                        new Point(115, 31),

                    Size =
                        new Size(260, 28)
                };

            txtCodigoProducto.KeyDown +=
                TxtCodigoProducto_KeyDown;

            Label lblCantidad =
                new Label
                {
                    Text =
                        "Cantidad:",

                    AutoSize = true,

                    Location =
                        new Point(20, 75)
                };

            txtCantidad =
                new TextBox
                {
                    Location =
                        new Point(115, 71),

                    Size =
                        new Size(100, 28),

                    Text = "1"
                };

            btnAgregar =
                new Button
                {
                    Text =
                        "Agregar",

                    Location =
                        new Point(230, 70),

                    Size =
                        new Size(145, 32)
                };

            btnAgregar.Click +=
                BtnAgregar_Click;

            Label lblInfo =
                new Label
                {
                    Text =
                        "Ingrese el código del producto y la cantidad.\n" +
                        "Solo se pueden vender productos activos.",

                    AutoSize = true,

                    ForeColor =
                        Color.DimGray,

                    Location =
                        new Point(20, 115)
                };

            grpProducto.Controls.AddRange(
                new Control[]
                {
                    lblCodigo,
                    txtCodigoProducto,
                    lblCantidad,
                    txtCantidad,
                    btnAgregar,
                    lblInfo
                });

            // =====================================================
            // DATOS DE LA VENTA
            // =====================================================

            GroupBox grpCliente =
                new GroupBox
                {
                    Text =
                        "Datos de la venta",

                    Location =
                        new Point(460, 85),

                    Size =
                        new Size(695, 180)
                };

            Label lblDocumento =
                new Label
                {
                    Text =
                        "Documento:",

                    AutoSize = true,

                    Location =
                        new Point(20, 35)
                };

            txtDocumentoCliente =
                new TextBox
                {
                    Location =
                        new Point(115, 31),

                    Size =
                        new Size(220, 28),

                    MaxLength = 20
                };

            Label lblNombre =
                new Label
                {
                    Text =
                        "Cliente:",

                    AutoSize = true,

                    Location =
                        new Point(360, 35)
                };

            txtNombreCliente =
                new TextBox
                {
                    Location =
                        new Point(420, 31),

                    Size =
                        new Size(245, 28),

                    MaxLength = 100
                };

            Label lblPago =
                new Label
                {
                    Text =
                        "Forma de pago:",

                    AutoSize = true,

                    Location =
                        new Point(20, 80)
                };

            cboFormaPago =
                new ComboBox
                {
                    Location =
                        new Point(115, 76),

                    Size =
                        new Size(220, 28),

                    DropDownStyle =
                        ComboBoxStyle.DropDownList
                };

            cboFormaPago.Items.AddRange(
                new object[]
                {
                    "Efectivo",
                    "Yape",
                    "Plin",
                    "Tarjeta",
                    "Transferencia"
                });

            if (cboFormaPago.Items.Count > 0)
                cboFormaPago.SelectedIndex = 0;

            lblEstadoVenta =
                new Label
                {
                    Text =
                        "Estado: Nueva venta",

                    AutoSize = true,

                    ForeColor =
                        Color.DimGray,

                    Location =
                        new Point(360, 80)
                };

            Label lblNota =
                new Label
                {
                    Text =
                        "El comprobante se genera después de confirmar la venta.",

                    AutoSize = true,

                    ForeColor =
                        Color.DimGray,

                    Location =
                        new Point(20, 125)
                };

            grpCliente.Controls.AddRange(
                new Control[]
                {
                    lblDocumento,
                    txtDocumentoCliente,
                    lblNombre,
                    txtNombreCliente,
                    lblPago,
                    cboFormaPago,
                    lblEstadoVenta,
                    lblNota
                });

            // =====================================================
            // CARRITO
            // =====================================================

            GroupBox grpCarrito =
                new GroupBox
                {
                    Text =
                        "Detalle de venta",

                    Location =
                        new Point(20, 285),

                    Size =
                        new Size(1135, 280)
                };

            dgvCarrito =
                new DataGridView
                {
                    Location =
                        new Point(20, 30),

                    Size =
                        new Size(1095, 180),

                    ReadOnly = true,

                    AllowUserToAddRows = false,

                    AllowUserToDeleteRows = false,

                    SelectionMode =
                        DataGridViewSelectionMode.FullRowSelect,

                    MultiSelect = false,

                    AutoSizeColumnsMode =
                        DataGridViewAutoSizeColumnsMode.Fill,

                    BackgroundColor =
                        Color.White
                };

            dgvCarrito.CellDoubleClick +=
                DgvCarrito_CellDoubleClick;

            btnModificar =
                new Button
                {
                    Text =
                        "Modificar cantidad",

                    Location =
                        new Point(20, 225),

                    Size =
                        new Size(150, 32)
                };

            btnEliminar =
                new Button
                {
                    Text =
                        "Eliminar",

                    Location =
                        new Point(180, 225),

                    Size =
                        new Size(120, 32)
                };

            btnVaciar =
                new Button
                {
                    Text =
                        "Vaciar carrito",

                    Location =
                        new Point(310, 225),

                    Size =
                        new Size(120, 32)
                };

            btnModificar.Click +=
                BtnModificar_Click;

            btnEliminar.Click +=
                BtnEliminar_Click;

            btnVaciar.Click +=
                BtnVaciar_Click;

            grpCarrito.Controls.AddRange(
                new Control[]
                {
                    dgvCarrito,
                    btnModificar,
                    btnEliminar,
                    btnVaciar
                });

            // =====================================================
            // TOTALES
            // =====================================================

            GroupBox grpTotales =
                new GroupBox
                {
                    Text =
                        "Totales",

                    Location =
                        new Point(20, 580),

                    Size =
                        new Size(500, 95)
                };

            lblSubtotal =
                CrearTotalLabel(
                    "Subtotal:",
                    new Point(20, 25));

            lblIGV =
                CrearTotalLabel(
                    "IGV (18%):",
                    new Point(170, 25));

            lblTotal =
                CrearTotalLabel(
                    "Total:",
                    new Point(325, 25));

            grpTotales.Controls.AddRange(
                new Control[]
                {
                    lblSubtotal,
                    lblIGV,
                    lblTotal
                });

            // =====================================================
            // BOTONES PRINCIPALES
            // =====================================================

            btnRegistrar =
                new Button
                {
                    Text =
                        "Registrar venta",

                    Location =
                        new Point(540, 590),

                    Size =
                        new Size(140, 38)
                };

            btnConfirmar =
                new Button
                {
                    Text =
                        "Confirmar venta",

                    Location =
                        new Point(690, 590),

                    Size =
                        new Size(140, 38)
                };

            btnBoleta =
                new Button
                {
                    Text =
                        "Generar boleta",

                    Location =
                        new Point(840, 590),

                    Size =
                        new Size(140, 38)
                };

            btnAnular =
                new Button
                {
                    Text =
                        "Anular venta",
                    Location =
                        new Point(390, 590),
                    Size =
                        new Size(140, 38)
                };

            btnCerrar =
                new Button
                {
                    Text =
                        "Cerrar",

                    Location =
                        new Point(990, 590),

                    Size =
                        new Size(140, 38)
                };

            btnRegistrar.Click +=
                BtnRegistrar_Click;

            btnConfirmar.Click +=
                BtnConfirmar_Click;

            btnBoleta.Click +=
                BtnBoleta_Click;

            btnAnular.Click +=
                BtnAnular_Click;

            btnCerrar.Click +=
                BtnCerrar_Click;

            Controls.AddRange(
                new Control[]
                {
                    lblTitulo,
                    lblVendedor,
                    grpProducto,
                    grpCliente,
                    grpCarrito,
                    grpTotales,
                    btnRegistrar,
                    btnConfirmar,
                    btnBoleta,
                    btnAnular,
                    btnCerrar
                });
        }

        // =========================================================
        // PERMISOS
        // =========================================================

        private void ConfigurarPermisos()
        {
            bool puedeVender =
                SesionService.PuedeGestionarVentas();

            btnAgregar.Enabled =
                puedeVender;

            btnModificar.Enabled =
                puedeVender;

            btnEliminar.Enabled =
                puedeVender;

            btnVaciar.Enabled =
                puedeVender;

            btnRegistrar.Enabled =
                puedeVender;

            btnConfirmar.Enabled = false; // se habilita al registrar

            btnBoleta.Enabled = false; // se habilita al confirmar

            btnAnular.Enabled =
                SesionService.PuedeAnularVenta();

            txtCodigoProducto.ReadOnly =
                !puedeVender;

            txtCantidad.ReadOnly =
                !puedeVender;

            txtDocumentoCliente.ReadOnly =
                !puedeVender;

            txtNombreCliente.ReadOnly =
                !puedeVender;

            cboFormaPago.Enabled =
                puedeVender;

            if (!puedeVender)
            {
                Text =
                    "FediCasa - Consulta de ventas";
            }
        }

        // =========================================================
        // INICIALIZACIÓN
        // =========================================================

        private void InicializarFormulario()
        {
            lblVendedor.Text =
                $"Vendedor: " +
                $"{SesionService.ObtenerNombreEmpleadoActual()} " +
                $"({SesionService.ObtenerUsuarioActual()})";

            ActualizarCarrito();
            ActualizarTotales();
            ActualizarEstado();
        }

        // =========================================================
        // AGREGAR PRODUCTO
        // =========================================================

        private void BtnAgregar_Click(
            object sender,
            EventArgs e)
        {
            AgregarProductoAlCarrito();
        }

        private void TxtCodigoProducto_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                AgregarProductoAlCarrito();
            }
        }

        private void AgregarProductoAlCarrito()
        {
            if (!SesionService.PuedeGestionarVentas())
            {
                MostrarSinPermiso();
                return;
            }

            string codigo =
                txtCodigoProducto.Text.Trim();

            if (string.IsNullOrWhiteSpace(codigo))
            {
                MessageBox.Show(
                    "Ingrese el código del producto.",
                    "Venta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCodigoProducto.Focus();
                return;
            }

            if (!int.TryParse(
                txtCantidad.Text.Trim(),
                out int cantidad) ||
                cantidad <= 0)
            {
                MessageBox.Show(
                    "Ingrese una cantidad válida mayor que cero.",
                    "Venta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCantidad.Focus();
                return;
            }

            // -----------------------------------------------------
            // BUSCAR PRODUCTO
            // -----------------------------------------------------

            Producto producto =
                ProductoService.BuscarPorCodigo(
                    codigo);

            if (producto == null)
            {
                MessageBox.Show(
                    "No existe un producto con ese código.",
                    "Producto no encontrado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCodigoProducto.Focus();
                return;
            }

            // -----------------------------------------------------
            // VALIDAR PRODUCTO ACTIVO
            // -----------------------------------------------------

            if (!producto.Activo)
            {
                MessageBox.Show(
                    "El producto está inactivo y no puede venderse.",
                    "Producto inactivo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // -----------------------------------------------------
            // VALIDAR CANTIDAD ACUMULADA
            // -----------------------------------------------------

            ItemCarrito existente =
                ObtenerItemCarrito(codigo);

            int cantidadFinal =
                cantidad +
                (existente?.Cantidad ?? 0);

            if (!ProductoService.HayStockSuficiente(
                codigo,
                cantidadFinal))
            {
                MessageBox.Show(
                    $"Stock insuficiente.\n\n" +
                    $"Producto: {producto.Nombre}\n" +
                    $"Stock disponible: {producto.Stock}\n" +
                    $"Cantidad solicitada: {cantidadFinal}",
                    "Stock insuficiente",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            // -----------------------------------------------------
            // AGREGAR AL CARRITO
            // IMPORTANTE: SE ENVÍA EL OBJETO PRODUCTO
            // -----------------------------------------------------

            if (!VentaService.AgregarAlCarrito(
                producto,
                cantidad,
                out string mensaje))
            {
                MessageBox.Show(
                    mensaje,
                    "No se pudo agregar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            txtCodigoProducto.Clear();
            txtCantidad.Text = "1";

            txtCodigoProducto.Focus();

            ActualizarCarrito();
            ActualizarTotales();
        }

        // =========================================================
        // MODIFICAR CANTIDAD
        // =========================================================

        private void BtnModificar_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService.PuedeGestionarVentas())
            {
                MostrarSinPermiso();
                return;
            }

            if (dgvCarrito.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione un producto del carrito.",
                    "Venta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string codigo =
                dgvCarrito.CurrentRow
                    .Cells["Código"]
                    .Value?
                    .ToString()
                    ?? string.Empty;

            ItemCarrito item =
                ObtenerItemCarrito(codigo);

            if (item == null)
                return;

            using Form dialogo =
                CrearDialogoCantidad(
                    item.Cantidad);

            if (dialogo.ShowDialog(this)
                != DialogResult.OK)
            {
                return;
            }

            NumericUpDown cantidadControl =
                dialogo.Controls
                    .OfType<NumericUpDown>()
                    .FirstOrDefault();

            if (cantidadControl == null)
                return;

            int nuevaCantidad =
                (int)cantidadControl.Value;

            if (!ProductoService.HayStockSuficiente(
                codigo,
                nuevaCantidad))
            {
                MessageBox.Show(
                    "La cantidad supera el stock disponible.",
                    "Stock insuficiente",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (!VentaService.ModificarCantidadCarrito(
                codigo,
                nuevaCantidad,
                out string mensaje))
            {
                MessageBox.Show(
                    mensaje,
                    "Venta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            ActualizarCarrito();
            ActualizarTotales();
        }

        // =========================================================
        // ELIMINAR PRODUCTO
        // =========================================================

        private void BtnEliminar_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService.PuedeGestionarVentas())
            {
                MostrarSinPermiso();
                return;
            }

            if (dgvCarrito.CurrentRow == null)
            {
                MessageBox.Show(
                    "Seleccione un producto del carrito.",
                    "Venta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string codigo =
                dgvCarrito.CurrentRow
                    .Cells["Código"]
                    .Value?
                    .ToString()
                    ?? string.Empty;

            if (!VentaService.EliminarDelCarrito(
                codigo,
                out string mensaje))
            {
                MessageBox.Show(
                    mensaje,
                    "Venta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            ActualizarCarrito();
            ActualizarTotales();
        }

        // =========================================================
        // VACIAR CARRITO
        // =========================================================

        private void BtnVaciar_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService.PuedeGestionarVentas())
            {
                MostrarSinPermiso();
                return;
            }

            if (dgvCarrito.Rows.Count == 0)
            {
                return;
            }

            DialogResult respuesta =
                MessageBox.Show(
                    "¿Desea vaciar todos los productos del carrito?",
                    "Confirmar",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
                return;

            VentaService.VaciarCarrito();

            ActualizarCarrito();
            ActualizarTotales();
        }

        // =========================================================
        // REGISTRAR VENTA
        // =========================================================

        private void BtnRegistrar_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService.PuedeGestionarVentas())
            {
                MostrarSinPermiso();
                return;
            }

            if (dgvCarrito.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Agregue al menos un producto.",
                    "Venta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (!ValidarDatosVenta())
                return;

            DialogResult respuesta =
                MessageBox.Show(
                    $"¿Desea registrar la venta?\n\n" +
                    $"Total: S/ {ObtenerTotalCarrito():0.00}",
                    "Confirmar registro",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
                return;

            bool resultado =
                VentaService.RegistrarVenta(
                    txtDocumentoCliente.Text.Trim(),
                    txtNombreCliente.Text.Trim(),
                    cboFormaPago.Text,
                    out int idVenta,
                    out string mensaje);

            if (!resultado)
            {
                MessageBox.Show(
                    mensaje,
                    "No se pudo registrar la venta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            idVentaActual =
                idVenta;

            ventaActual =
                VentaService.BuscarVentas(
                    idVentaActual);

            lblEstadoVenta.Text =
                $"Estado: Registrada - Venta N.° {idVentaActual}";

            MessageBox.Show(
                $"Venta registrada correctamente.\n\n" +
                $"Número de venta: {idVentaActual}\n" +
                $"Total: S/ {ObtenerTotalVentaActual():0.00}\n\n" +
                "Ahora debe confirmar la venta.",
                "FediCasa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            btnRegistrar.Enabled = false;
            btnConfirmar.Enabled = true;
            btnBoleta.Enabled = false;

            txtCodigoProducto.Enabled = false;
            txtCantidad.Enabled = false;
            btnAgregar.Enabled = false;
            btnModificar.Enabled = false;
            btnEliminar.Enabled = false;
            btnVaciar.Enabled = false;
        }

        // =========================================================
        // CONFIRMAR VENTA
        // =========================================================

        private void BtnConfirmar_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService.PuedeConfirmarVenta())
            {
                MostrarSinPermiso();
                return;
            }

            if (idVentaActual <= 0)
            {
                MessageBox.Show(
                    "Primero debe registrar una venta.",
                    "Confirmar venta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            DialogResult respuesta =
                MessageBox.Show(
                    "¿Confirma definitivamente esta venta?\n\n" +
                    "La confirmación representa la operación " +
                    "de salida de los productos.",
                    "Confirmar venta",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
                return;

            if (!VentaService.ConfirmarVenta(
                idVentaActual,
                out string mensaje))
            {
                MessageBox.Show(
                    mensaje,
                    "No se pudo confirmar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            ventaActual =
                VentaService.BuscarVentas(
                    idVentaActual);

            lblEstadoVenta.Text =
                $"Estado: CONFIRMADA - Venta N.° {idVentaActual}";

            btnConfirmar.Enabled = false;
            btnBoleta.Enabled = true;

            MessageBox.Show(
                "Venta confirmada correctamente.\n\n" +
                "La operación quedó lista para generar el comprobante.",
                "FediCasa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        // =========================================================
        // GENERAR BOLETA
        // =========================================================

        private void BtnBoleta_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService.PuedeGenerarComprobante())
            {
                MostrarSinPermiso();
                return;
            }

            if (idVentaActual <= 0)
            {
                MessageBox.Show(
                    "No existe una venta seleccionada.",
                    "Boleta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            List<Venta> ventas =
                VentaService.BuscarVentas(
                    idVentaActual);

            if (ventas.Count == 0)
            {
                MessageBox.Show(
                    "No se encontró el detalle de la venta.",
                    "Boleta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (!ventas.All(
                v => v.EstaConfirmada()))
            {
                MessageBox.Show(
                    "La venta debe estar confirmada " +
                    "antes de generar la boleta.",
                    "Boleta",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            double total =
                ventas.Sum(
                    v => v.Total);

            string ruta =
                BoletaHelper.GuardarBoleta(
                    ventas,
                    total);

            string contenido =
                BoletaHelper.GenerarContenidoBoleta(
                    ventas,
                    total);

            MostrarBoleta(
                contenido,
                ruta);

            PrepararNuevaVenta();
        }

        // =========================================================
        // ANULAR VENTA
        // =========================================================

        private void BtnAnular_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService.PuedeAnularVenta())
            {
                MostrarSinPermiso();
                return;
            }

            int idAnular = idVentaActual;

            // Si no hay una venta en pantalla, se pide el número.
            if (idAnular <= 0)
            {
                idAnular = PedirNumeroVenta();

                if (idAnular <= 0)
                    return;
            }

            DialogResult respuesta =
                MessageBox.Show(
                    $"¿Desea anular la venta N.° {idAnular}?\n\n" +
                    "Si estaba confirmada, el stock será devuelto.",
                    "Anular venta",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
                return;

            if (!VentaService.AnularVenta(
                idAnular,
                out string mensaje))
            {
                MessageBox.Show(
                    mensaje,
                    "No se pudo anular",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            MessageBox.Show(
                mensaje,
                "FediCasa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            if (idAnular == idVentaActual)
            {
                PrepararNuevaVenta();
            }
        }

        private int PedirNumeroVenta()
        {
            using (Form dialogo = new Form())
            {
                dialogo.Text = "Anular venta";
                dialogo.StartPosition = FormStartPosition.CenterParent;
                dialogo.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialogo.MaximizeBox = false;
                dialogo.MinimizeBox = false;
                dialogo.ClientSize = new Size(300, 120);

                Label lbl = new Label
                {
                    Text = "Número de venta:",
                    AutoSize = true,
                    Location = new Point(15, 20)
                };

                TextBox txt = new TextBox
                {
                    Location = new Point(15, 45),
                    Size = new Size(270, 25)
                };

                Button ok = new Button
                {
                    Text = "Aceptar",
                    DialogResult = DialogResult.OK,
                    Location = new Point(120, 80),
                    Size = new Size(80, 28)
                };

                Button cancelar = new Button
                {
                    Text = "Cancelar",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(205, 80),
                    Size = new Size(80, 28)
                };

                dialogo.Controls.AddRange(
                    new Control[] { lbl, txt, ok, cancelar });

                dialogo.AcceptButton = ok;
                dialogo.CancelButton = cancelar;

                if (dialogo.ShowDialog(this) != DialogResult.OK)
                    return 0;

                if (!int.TryParse(txt.Text.Trim(), out int id) ||
                    id <= 0)
                {
                    MessageBox.Show(
                        "Ingrese un número de venta válido.",
                        "Anular venta",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return 0;
                }

                return id;
            }
        }

        // =========================================================
        // VALIDAR DATOS DE VENTA
        // =========================================================

        private bool ValidarDatosVenta()
        {
            if (string.IsNullOrWhiteSpace(
                cboFormaPago.Text))
            {
                MessageBox.Show(
                    "Seleccione una forma de pago.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return false;
            }

            if (!string.IsNullOrWhiteSpace(
                txtDocumentoCliente.Text))
            {
                if (txtDocumentoCliente.Text.Length < 8)
                {
                    MessageBox.Show(
                        "El documento del cliente no parece válido.",
                        "Validación",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    txtDocumentoCliente.Focus();

                    return false;
                }
            }

            return true;
        }

        // =========================================================
        // ACTUALIZAR CARRITO
        // =========================================================

        private void ActualizarCarrito()
        {
            List<ItemCarrito> carrito =
                VentaService.ObtenerCarrito();

            dgvCarrito.DataSource = null;

            dgvCarrito.DataSource =
                carrito
                    .Select(
                        item => new
                        {
                            Código =
                                item.CodigoProducto,

                            Producto =
                                item.NombreProducto,

                            Precio =
                                $"S/ {item.Precio:0.00}",

                            Cantidad =
                                item.Cantidad,

                            Subtotal =
                                $"S/ {item.Subtotal:0.00}"
                        })
                    .ToList();
        }

        // =========================================================
        // ACTUALIZAR TOTALES
        // =========================================================

        private void ActualizarTotales()
        {
            double valorVenta =
                VentaService.CalcularValorVenta();

            double igv =
                VentaService.CalcularIGV();

            double total =
                VentaService.CalcularTotal();

            lblSubtotal.Text =
                $"Subtotal\nS/ {valorVenta:0.00}";

            lblIGV.Text =
                $"IGV\nS/ {igv:0.00}";

            lblTotal.Text =
                $"TOTAL\nS/ {total:0.00}";
        }

        // =========================================================
        // ACTUALIZAR ESTADO
        // =========================================================

        private void ActualizarEstado()
        {
            if (idVentaActual <= 0)
            {
                lblEstadoVenta.Text =
                    "Estado: Nueva venta";
            }
        }

        // =========================================================
        // PREPARAR NUEVA VENTA
        // =========================================================

        private void PrepararNuevaVenta()
        {
            idVentaActual = 0;

            ventaActual.Clear();

            VentaService.VaciarCarrito();

            txtCodigoProducto.Clear();

            txtCantidad.Text = "1";

            txtDocumentoCliente.Clear();

            txtNombreCliente.Clear();

            if (cboFormaPago.Items.Count > 0)
            {
                cboFormaPago.SelectedIndex = 0;
            }

            btnRegistrar.Enabled =
                SesionService.PuedeGestionarVentas();

            btnConfirmar.Enabled = false;

            btnBoleta.Enabled = false;

            txtCodigoProducto.Enabled = true;
            txtCantidad.Enabled = true;

            btnAgregar.Enabled =
                SesionService.PuedeGestionarVentas();

            btnModificar.Enabled =
                SesionService.PuedeGestionarVentas();

            btnEliminar.Enabled =
                SesionService.PuedeGestionarVentas();

            btnVaciar.Enabled =
                SesionService.PuedeGestionarVentas();

            lblEstadoVenta.Text =
                "Estado: Nueva venta";

            ActualizarCarrito();
            ActualizarTotales();

            txtCodigoProducto.Focus();
        }

        // =========================================================
        // MOSTRAR BOLETA
        // =========================================================

        private void MostrarBoleta(
            string contenido,
            string ruta)
        {
            Form ventana =
                new Form
                {
                    Text =
                        "FediCasa - Boleta de venta",

                    StartPosition =
                        FormStartPosition.CenterParent,

                    Size =
                        new Size(760, 700),

                    MinimizeBox = false,

                    MaximizeBox = true
                };

            RichTextBox visor =
                new RichTextBox
                {
                    Dock =
                        DockStyle.Fill,

                    ReadOnly = true,

                    Font =
                        new Font(
                            "Consolas",
                            10),

                    Text =
                        contenido,

                    BackColor =
                        Color.White
                };

            Button btnCerrar =
                new Button
                {
                    Text =
                        "Cerrar",

                    Dock =
                        DockStyle.Bottom,

                    Height = 40
                };

            btnCerrar.Click +=
                (s, e) =>
                {
                    ventana.Close();
                };

            ventana.Controls.Add(
                visor);

            ventana.Controls.Add(
                btnCerrar);

            ventana.ShowDialog(this);

            if (!string.IsNullOrWhiteSpace(ruta))
            {
                MessageBox.Show(
                    $"La boleta fue guardada correctamente.\n\n" +
                    $"Archivo:\n{ruta}",
                    "Boleta generada",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        // =========================================================
        // OBTENER ITEM DEL CARRITO
        // =========================================================

        private ItemCarrito ObtenerItemCarrito(
            string codigo)
        {
            return VentaService
                .ObtenerCarrito()
                .FirstOrDefault(
                    x =>
                        x.CodigoProducto.Equals(
                            codigo,
                            StringComparison.OrdinalIgnoreCase));
        }

        // =========================================================
        // OBTENER TOTAL DEL CARRITO
        // =========================================================

        private double ObtenerTotalCarrito()
        {
            return VentaService.CalcularTotal();
        }

        // =========================================================
        // OBTENER TOTAL DE VENTA ACTUAL
        // =========================================================

        private double ObtenerTotalVentaActual()
        {
            return ventaActual.Sum(
                x => x.Total);
        }

        // =========================================================
        // DIÁLOGO PARA MODIFICAR CANTIDAD
        // =========================================================

        private Form CrearDialogoCantidad(
            int cantidadActual)
        {
            Form dialogo =
                new Form
                {
                    Text =
                        "Modificar cantidad",

                    StartPosition =
                        FormStartPosition.CenterParent,

                    Size =
                        new Size(330, 180),

                    FormBorderStyle =
                        FormBorderStyle.FixedDialog,

                    MaximizeBox = false,

                    MinimizeBox = false
                };

            Label lbl =
                new Label
                {
                    Text =
                        "Nueva cantidad:",

                    AutoSize = true,

                    Location =
                        new Point(25, 25)
                };

            NumericUpDown num =
                new NumericUpDown
                {
                    Location =
                        new Point(140, 20),

                    Size =
                        new Size(120, 28),

                    Minimum = 1,

                    Maximum = 100000,

                    Value =
                        cantidadActual
                };

            Button aceptar =
                new Button
                {
                    Text =
                        "Aceptar",

                    DialogResult =
                        DialogResult.OK,

                    Location =
                        new Point(70, 80),

                    Size =
                        new Size(90, 32)
                };

            Button cancelar =
                new Button
                {
                    Text =
                        "Cancelar",

                    DialogResult =
                        DialogResult.Cancel,

                    Location =
                        new Point(170, 80),

                    Size =
                        new Size(90, 32)
                };

            dialogo.Controls.AddRange(
                new Control[]
                {
                    lbl,
                    num,
                    aceptar,
                    cancelar
                });

            dialogo.AcceptButton =
                aceptar;

            dialogo.CancelButton =
                cancelar;

            return dialogo;
        }

        // =========================================================
        // LABEL DE TOTALES
        // =========================================================

        private static Label CrearTotalLabel(
            string titulo,
            Point ubicacion)
        {
            return new Label
            {
                Text =
                    $"{titulo}\nS/ 0.00",

                AutoSize = true,

                Location =
                    ubicacion,

                Font =
                    new Font(
                        "Segoe UI",
                        10,
                        FontStyle.Bold)
            };
        }

        // =========================================================
        // DOBLE CLIC EN CARRITO
        // =========================================================

        private void DgvCarrito_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            BtnModificar_Click(
                sender,
                EventArgs.Empty);
        }

        // =========================================================
        // CERRAR
        // =========================================================

        private void BtnCerrar_Click(
            object sender,
            EventArgs e)
        {
            if (dgvCarrito.Rows.Count > 0 &&
                idVentaActual == 0)
            {
                DialogResult respuesta =
                    MessageBox.Show(
                        "Tiene productos en el carrito.\n\n" +
                        "¿Desea salir y perder la venta actual?",
                        "Cerrar ventas",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                if (respuesta != DialogResult.Yes)
                    return;
            }

            Close();
        }

        // =========================================================
        // SIN PERMISO
        // =========================================================

        private static void MostrarSinPermiso()
        {
            MessageBox.Show(
                "No tiene permisos para realizar esta operación.",
                "Acceso restringido",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
