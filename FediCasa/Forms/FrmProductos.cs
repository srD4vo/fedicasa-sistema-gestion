using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FediCasa.Models;
using FediCasa.Services;

namespace FediCasa.Forms
{
    public class FrmProductos : Form
    {
        // =========================================================
        // CONTROLES
        // =========================================================

        private TextBox txtCodigo;
        private TextBox txtNombre;
        private TextBox txtPrecio;
        private TextBox txtStock;
        private TextBox txtStockMinimo;
        private ComboBox cboUnidadMedida;
        private CheckBox chkActivo;
        private TextBox txtBuscar;

        private Button btnRegistrar;
        private Button btnModificar;
        private Button btnDesactivar;
        private Button btnReactivar;
        private Button btnLimpiar;
        private Button btnCerrar;

        private DataGridView dgvProductos;

        private string codigoSeleccionado = string.Empty;

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public FrmProductos()
        {
            Text = "FediCasa - Gestión de productos";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1180, 720);
            MinimumSize = new Size(1000, 650);
            BackColor = Color.White;

            ConstruirInterfaz();
            ConfigurarPermisos();
            CargarProductos();
            LimpiarFormulario();
        }

        // =========================================================
        // INTERFAZ
        // =========================================================

        private void ConstruirInterfaz()
        {
            Label lblTitulo = new Label
            {
                Text = "GESTIÓN DE PRODUCTOS",
                Font = new Font(
                    "Segoe UI",
                    18,
                    FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 20)
            };

            Label lblSubtitulo = new Label
            {
                Text =
                    "Registro, actualización y control de productos de FediCasa",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(28, 53)
            };

            // =====================================================
            // DATOS DEL PRODUCTO
            // =====================================================

            GroupBox grpDatos = new GroupBox
            {
                Text = "Datos del producto",
                Location = new Point(20, 85),
                Size = new Size(440, 430)
            };

            Label lblCodigo = CrearEtiqueta(
                "Código:",
                new Point(20, 35));

            txtCodigo = CrearTextBox(
                new Point(145, 31),
                245);

            Label lblNombre = CrearEtiqueta(
                "Nombre:",
                new Point(20, 80));

            txtNombre = CrearTextBox(
                new Point(145, 76),
                245);

            Label lblPrecio = CrearEtiqueta(
                "Precio (S/):",
                new Point(20, 125));

            txtPrecio = CrearTextBox(
                new Point(145, 121),
                245);

            Label lblUnidad = CrearEtiqueta(
                "Unidad:",
                new Point(20, 170));

            cboUnidadMedida = new ComboBox
            {
                Location = new Point(145, 166),
                Size = new Size(245, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };

            cboUnidadMedida.Items.AddRange(
                new object[]
                {
                    "Unidad",
                    "Caja",
                    "Metro",
                    "Kilogramo",
                    "Litro",
                    "Galón",
                    "Juego",
                    "Rollo",
                    "Par",
                    "Bolsa"
                });

            Label lblStock = CrearEtiqueta(
                "Stock actual:",
                new Point(20, 215));

            txtStock = CrearTextBox(
                new Point(145, 211),
                245);

            Label lblStockMinimo = CrearEtiqueta(
                "Stock mínimo:",
                new Point(20, 260));

            txtStockMinimo = CrearTextBox(
                new Point(145, 256),
                245);

            chkActivo = new CheckBox
            {
                Text = "Producto activo",
                Location = new Point(145, 300),
                AutoSize = true,
                Checked = true
            };

            btnRegistrar = CrearBoton(
                "Registrar",
                new Point(20, 345),
                120);

            btnModificar = CrearBoton(
                "Modificar",
                new Point(155, 345),
                120);

            btnDesactivar = CrearBoton(
                "Desactivar",
                new Point(290, 345),
                120);

            btnReactivar = CrearBoton(
                "Reactivar",
                new Point(20, 385),
                120);

            btnLimpiar = CrearBoton(
                "Limpiar",
                new Point(155, 385),
                120);

            btnCerrar = CrearBoton(
                "Cerrar",
                new Point(290, 385),
                120);

            btnRegistrar.Click += BtnRegistrar_Click;
            btnModificar.Click += BtnModificar_Click;
            btnDesactivar.Click += BtnDesactivar_Click;
            btnReactivar.Click += BtnReactivar_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
            btnCerrar.Click += BtnCerrar_Click;

            grpDatos.Controls.AddRange(
                new Control[]
                {
                    lblCodigo,
                    txtCodigo,
                    lblNombre,
                    txtNombre,
                    lblPrecio,
                    txtPrecio,
                    lblUnidad,
                    cboUnidadMedida,
                    lblStock,
                    txtStock,
                    lblStockMinimo,
                    txtStockMinimo,
                    chkActivo,
                    btnRegistrar,
                    btnModificar,
                    btnDesactivar,
                    btnReactivar,
                    btnLimpiar,
                    btnCerrar
                });

            // =====================================================
            // LISTADO DE PRODUCTOS
            // =====================================================

            GroupBox grpListado = new GroupBox
            {
                Text = "Listado de productos",
                Location = new Point(480, 85),
                Size = new Size(675, 550)
            };

            Label lblBuscar = new Label
            {
                Text = "Buscar:",
                AutoSize = true,
                Location = new Point(20, 32)
            };

            txtBuscar = new TextBox
            {
                Location = new Point(80, 28),
                Size = new Size(570, 28)
            };

            txtBuscar.TextChanged += TxtBuscar_TextChanged;


            dgvProductos = new DataGridView
            {
                Location = new Point(20, 70),
                Size = new Size(630, 450),

                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,

                SelectionMode =
    DataGridViewSelectionMode.FullRowSelect,

                MultiSelect = false,

                AutoGenerateColumns = true,

                AutoSizeColumnsMode =
    DataGridViewAutoSizeColumnsMode.None,

                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,

                RowHeadersVisible = false,

                AllowUserToResizeRows = false
            };

            dgvProductos.SelectionChanged +=
                DgvProductos_SelectionChanged;

            dgvProductos.CellClick +=
                DgvProductos_CellClick;

            dgvProductos.CellFormatting +=
                DgvProductos_CellFormatting;



            grpListado.Controls.AddRange(
                new Control[]
                {
                    lblBuscar,
                    txtBuscar,
                    dgvProductos
                });

            // =====================================================
            // ESTADO
            // =====================================================

            Label lblEstado = new Label
            {
                Name = "lblEstado",
                Text = "Productos registrados",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Location = new Point(20, 650)
            };

            Controls.AddRange(
                new Control[]
                {
                    lblTitulo,
                    lblSubtitulo,
                    grpDatos,
                    grpListado,
                    lblEstado
                });
        }

        // =========================================================
        // PERMISOS
        // =========================================================

        private void ConfigurarPermisos()
        {
            bool puedeGestionar =
                SesionService.PuedeGestionarProductos();

            btnRegistrar.Enabled = puedeGestionar;
            btnModificar.Enabled = puedeGestionar;
            btnDesactivar.Enabled = puedeGestionar;
            btnReactivar.Enabled = puedeGestionar;

            txtCodigo.ReadOnly = !puedeGestionar;
            txtNombre.ReadOnly = !puedeGestionar;
            txtPrecio.ReadOnly = !puedeGestionar;
            txtStock.ReadOnly = !puedeGestionar;
            txtStockMinimo.ReadOnly = !puedeGestionar;

            cboUnidadMedida.Enabled = puedeGestionar;
            chkActivo.Enabled = puedeGestionar;

            if (!puedeGestionar)
            {
                Text =
                    "FediCasa - Consulta de productos";

                btnRegistrar.Visible = false;
                btnModificar.Visible = false;
                btnDesactivar.Visible = false;
                btnReactivar.Visible = false;

                btnLimpiar.Location =
                    new Point(20, 345);

                btnCerrar.Location =
                    new Point(155, 345);
            }
        }

        // =========================================================
        // CARGAR PRODUCTOS
        // =========================================================

        private void CargarProductos()
        {
            if (dgvProductos == null)
                return;

            string textoBusqueda =
                txtBuscar?.Text?.Trim()
                ?? string.Empty;

            var productos =
                string.IsNullOrWhiteSpace(textoBusqueda)
                    ? ProductoService.BuscarTodos(string.Empty)
                    : ProductoService.Buscar(textoBusqueda);

            var datos = productos
                .Select(p => new
                {
                    Codigo = p.Codigo,
                    Producto = p.Nombre,
                    Precio = $"S/ {p.Precio:0.00}",
                    Unidad = p.UnidadMedida,
                    Stock = p.Stock,
                    Minimo = p.StockMinimo,
                    EstadoStock = p.ObtenerEstadoStock(),
                    Estado = p.Activo
                        ? "Activo"
                        : "Inactivo"
                })
                .ToList();

            dgvProductos.DataSource = null;
            dgvProductos.DataSource = datos;

            ConfigurarColumnas();

            ActualizarEstado();
        }

        // =========================================================
        // CONFIGURAR COLUMNAS
        // =========================================================

        private void ConfigurarColumnas()
        {
            if (dgvProductos == null)
                return;

            if (dgvProductos.Columns.Count == 0)
                return;

            // Nombres visibles
            ConfigurarColumna(
                "Codigo",
                "Código",
                75);

            ConfigurarColumna(
                "Producto",
                "Producto",
                180);

            ConfigurarColumna(
                "Precio",
                "Precio",
                80);

            ConfigurarColumna(
                "Unidad",
                "Unidad",
                80);

            ConfigurarColumna(
                "Stock",
                "Stock",
                55);

            ConfigurarColumna(
                "Minimo",
                "Mínimo",
                55);

            ConfigurarColumna(
                "EstadoStock",
                "Estado de stock",
                110);

            ConfigurarColumna(
                "Estado",
                "Estado",
                75);

            // Alineaciones
            AlinearColumna(
                "Codigo",
                DataGridViewContentAlignment.MiddleCenter);

            AlinearColumna(
                "Precio",
                DataGridViewContentAlignment.MiddleRight);

            AlinearColumna(
                "Stock",
                DataGridViewContentAlignment.MiddleCenter);

            AlinearColumna(
                "Minimo",
                DataGridViewContentAlignment.MiddleCenter);

            AlinearColumna(
                "Estado",
                DataGridViewContentAlignment.MiddleCenter);

            // Evitar que las columnas se deformen
            foreach (DataGridViewColumn columna
                in dgvProductos.Columns)
            {
                columna.SortMode =
                    DataGridViewColumnSortMode.NotSortable;
            }
        }

        private void ConfigurarColumna(
            string nombreInterno,
            string titulo,
            int ancho)
        {
            if (dgvProductos == null)
                return;

            if (!dgvProductos.Columns.Contains(
                nombreInterno))
            {
                return;
            }

            DataGridViewColumn columna =
                dgvProductos.Columns[nombreInterno];

            if (columna == null)
                return;

            columna.HeaderText = titulo;
            columna.Width = ancho;
            columna.MinimumWidth = ancho;
        }

        private void AlinearColumna(
            string nombreInterno,
            DataGridViewContentAlignment alineacion)
        {
            if (dgvProductos == null)
                return;

            if (!dgvProductos.Columns.Contains(
                nombreInterno))
            {
                return;
            }

            DataGridViewColumn columna =
                dgvProductos.Columns[nombreInterno];

            if (columna == null)
                return;

            columna.DefaultCellStyle.Alignment =
                alineacion;
        }

        // =========================================================
        // ACTUALIZAR ESTADO
        // =========================================================

        private void ActualizarEstado()
        {
            Label lblEstado =
                Controls.Find(
                    "lblEstado",
                    true)
                .FirstOrDefault()
                as Label;

            if (lblEstado == null)
                return;

            int total =
                ProductoService
                    .BuscarTodos(string.Empty)
                    .Count;

            int activos =
                ProductoService
                    .ObtenerCantidadProductosActivos();

            int bajo =
                ProductoService
                    .ObtenerProductosStockBajo()
                    .Count;

            int agotados =
                ProductoService
                    .ObtenerProductosAgotados()
                    .Count;

            lblEstado.Text =
                $"Total: {total} | " +
                $"Activos: {activos} | " +
                $"Stock bajo: {bajo} | " +
                $"Agotados: {agotados}";
        }

        // =========================================================
        // REGISTRAR
        // =========================================================

        private void BtnRegistrar_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService
                .PuedeGestionarProductos())
            {
                MostrarSinPermiso();
                return;
            }

            if (!ValidarFormulario(
                out Producto producto))
            {
                return;
            }

            if (ProductoService.ExisteCodigo(
                producto.Codigo))
            {
                MessageBox.Show(
                    "Ya existe un producto con ese código.",
                    "Producto existente",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCodigo.Focus();
                return;
            }

            if (ProductoService.ExisteNombre(
                producto.Nombre))
            {
                MessageBox.Show(
                    "Ya existe un producto con ese nombre.",
                    "Producto existente",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtNombre.Focus();
                return;
            }

            if (!ProductoService.RegistrarProducto(
                producto,
                out string mensaje))
            {
                MessageBox.Show(
                    mensaje,
                    "No se pudo registrar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            MessageBox.Show(
                "Producto registrado correctamente.",
                "FediCasa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            CargarProductos();
            LimpiarFormulario();
        }

        // =========================================================
        // MODIFICAR
        // =========================================================

        private void BtnModificar_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService
                .PuedeGestionarProductos())
            {
                MostrarSinPermiso();
                return;
            }

            if (string.IsNullOrWhiteSpace(
                codigoSeleccionado))
            {
                MessageBox.Show(
                    "Seleccione un producto de la lista.",
                    "Modificar producto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (!ValidarFormulario(
                out Producto producto))
            {
                return;
            }

            if (!ProductoService.ModificarProducto(
                codigoSeleccionado,
                producto,
                out string mensaje))
            {
                MessageBox.Show(
                    mensaje,
                    "No se pudo modificar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            MessageBox.Show(
                "Producto modificado correctamente.",
                "FediCasa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            CargarProductos();
            LimpiarFormulario();
        }

        // =========================================================
        // DESACTIVAR
        // =========================================================

        private void BtnDesactivar_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService
                .PuedeGestionarProductos())
            {
                MostrarSinPermiso();
                return;
            }

            if (string.IsNullOrWhiteSpace(
                codigoSeleccionado))
            {
                MessageBox.Show(
                    "Seleccione un producto.",
                    "Desactivar producto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            Producto producto =
                ProductoService.BuscarPorCodigo(
                    codigoSeleccionado);

            if (producto == null)
            {
                MessageBox.Show(
                    "No se encontró el producto.",
                    "FediCasa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            DialogResult respuesta =
                MessageBox.Show(
                    $"¿Desea desactivar el producto?\n\n" +
                    $"Código: {producto.Codigo}\n" +
                    $"Producto: {producto.Nombre}",
                    "Confirmar desactivación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
                return;

            if (!ProductoService.DesactivarProducto(
                codigoSeleccionado,
                out string mensaje))
            {
                MessageBox.Show(
                    mensaje,
                    "FediCasa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            MessageBox.Show(
                "Producto desactivado correctamente.",
                "FediCasa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            CargarProductos();
            LimpiarFormulario();
        }

        // =========================================================
        // REACTIVAR
        // =========================================================

        private void BtnReactivar_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService
                .PuedeGestionarProductos())
            {
                MostrarSinPermiso();
                return;
            }

            if (string.IsNullOrWhiteSpace(
                codigoSeleccionado))
            {
                MessageBox.Show(
                    "Seleccione un producto.",
                    "Reactivar producto",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (!ProductoService.ReactivarProducto(
                codigoSeleccionado,
                out string mensaje))
            {
                MessageBox.Show(
                    mensaje,
                    "FediCasa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            MessageBox.Show(
                "Producto reactivado correctamente.",
                "FediCasa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            CargarProductos();
            LimpiarFormulario();
        }

        // =========================================================
        // SELECCIÓN DE PRODUCTO
        // =========================================================

        private void DgvProductos_CellClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            CargarProductoSeleccionado(e.RowIndex);
        }

        private void DgvProductos_SelectionChanged(
            object sender,
            EventArgs e)
        {
            if (dgvProductos.CurrentRow == null)
                return;

            CargarProductoSeleccionado(
                dgvProductos.CurrentRow.Index);
        }

        private void CargarProductoSeleccionado(
            int indiceFila)
        {
            if (indiceFila < 0)
                return;

            if (dgvProductos == null)
                return;

            if (indiceFila >= dgvProductos.Rows.Count)
                return;

            if (!dgvProductos.Columns.Contains(
                "Codigo"))
            {
                return;
            }

            DataGridViewRow fila =
                dgvProductos.Rows[indiceFila];

            if (fila == null)
                return;

            object valorCodigo =
                fila.Cells["Codigo"].Value;

            string codigo =
                valorCodigo?.ToString()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(codigo))
                return;

            Producto producto =
                ProductoService.BuscarPorCodigo(
                    codigo);

            if (producto == null)
                return;

            codigoSeleccionado =
                producto.Codigo;

            txtCodigo.Text =
                producto.Codigo;

            txtNombre.Text =
                producto.Nombre;

            txtPrecio.Text =
                producto.Precio.ToString("0.00");

            txtStock.Text =
                producto.Stock.ToString();

            txtStockMinimo.Text =
                producto.StockMinimo.ToString();

            cboUnidadMedida.SelectedItem =
                producto.UnidadMedida;

            if (cboUnidadMedida.SelectedIndex < 0)
            {
                cboUnidadMedida.Text =
                    producto.UnidadMedida;
            }

            chkActivo.Checked =
                producto.Activo;

            // El código original no se modifica
            txtCodigo.ReadOnly = true;
        }

        // =========================================================
        // BÚSQUEDA
        // =========================================================

        private void TxtBuscar_TextChanged(
            object sender,
            EventArgs e)
        {
            CargarProductos();
        }

        // =========================================================
        // LIMPIAR
        // =========================================================

        private void BtnLimpiar_Click(
            object sender,
            EventArgs e)
        {
            LimpiarFormulario();
        }

        private void LimpiarFormulario()
        {
            codigoSeleccionado =
                string.Empty;

            txtCodigo.Clear();
            txtNombre.Clear();
            txtPrecio.Clear();
            txtStock.Clear();
            txtStockMinimo.Clear();

            if (cboUnidadMedida.Items.Count > 0)
            {
                cboUnidadMedida.SelectedIndex = 0;
            }

            chkActivo.Checked = true;

            bool puedeGestionar =
                SesionService
                    .PuedeGestionarProductos();

            txtCodigo.ReadOnly =
                !puedeGestionar;

            txtNombre.ReadOnly =
                !puedeGestionar;

            txtPrecio.ReadOnly =
                !puedeGestionar;

            txtStock.ReadOnly =
                !puedeGestionar;

            txtStockMinimo.ReadOnly =
                !puedeGestionar;

            txtCodigo.Focus();
        }

        // =========================================================
        // VALIDACIÓN
        // =========================================================

        private bool ValidarFormulario(
            out Producto producto)
        {
            producto = null;

            string codigo =
                txtCodigo.Text.Trim();

            string nombre =
                txtNombre.Text.Trim();

            string precioTexto =
                txtPrecio.Text.Trim();

            string stockTexto =
                txtStock.Text.Trim();

            string minimoTexto =
                txtStockMinimo.Text.Trim();

            string unidad =
                cboUnidadMedida.Text.Trim();

            // -----------------------------------------------------
            // CÓDIGO
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(codigo))
            {
                MessageBox.Show(
                    "Ingrese el código del producto.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtCodigo.Focus();
                return false;
            }

            // -----------------------------------------------------
            // NOMBRE
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(nombre))
            {
                MessageBox.Show(
                    "Ingrese el nombre del producto.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtNombre.Focus();
                return false;
            }

            // -----------------------------------------------------
            // PRECIO
            // -----------------------------------------------------

            if (!double.TryParse(
                precioTexto,
                out double precio) ||
                precio <= 0)
            {
                MessageBox.Show(
                    "Ingrese un precio válido mayor que cero.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtPrecio.Focus();
                return false;
            }

            // -----------------------------------------------------
            // STOCK
            // -----------------------------------------------------

            if (!int.TryParse(
                stockTexto,
                out int stock) ||
                stock < 0)
            {
                MessageBox.Show(
                    "El stock debe ser un número entero " +
                    "mayor o igual a cero.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtStock.Focus();
                return false;
            }

            // -----------------------------------------------------
            // STOCK MÍNIMO
            // -----------------------------------------------------

            if (!int.TryParse(
                minimoTexto,
                out int stockMinimo) ||
                stockMinimo < 0)
            {
                MessageBox.Show(
                    "El stock mínimo debe ser un número entero " +
                    "mayor o igual a cero.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtStockMinimo.Focus();
                return false;
            }

            // -----------------------------------------------------
            // UNIDAD
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(unidad))
            {
                MessageBox.Show(
                    "Seleccione una unidad de medida.",
                    "Validación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                cboUnidadMedida.Focus();
                return false;
            }

            // -----------------------------------------------------
            // CREAR PRODUCTO
            // -----------------------------------------------------

            producto =
                new Producto(
                    codigo,
                    nombre,
                    precio,
                    stock,
                    stockMinimo,
                    unidad,
                    chkActivo.Checked);

            return true;
        }

        // =========================================================
        // FORMATO VISUAL
        // =========================================================

        private void DgvProductos_CellFormatting(
            object sender,
            DataGridViewCellFormattingEventArgs e)
        {
            if (dgvProductos == null)
                return;

            if (e.RowIndex < 0)
                return;

            if (e.ColumnIndex < 0 ||
                e.ColumnIndex >=
                dgvProductos.Columns.Count)
            {
                return;
            }

            DataGridViewColumn columna =
                dgvProductos.Columns[e.ColumnIndex];

            if (columna == null)
                return;

            if (columna.Name == "EstadoStock")
            {
                string estado =
                    e.Value?.ToString()
                    ?? string.Empty;

                if (estado.Contains("Agotado") ||
                    estado.Contains("Bajo"))
                {
                    e.CellStyle.Font =
                        new Font(
                            dgvProductos.Font,
                            FontStyle.Bold);
                }
            }

            if (columna.Name == "Estado")
            {
                string estado =
                    e.Value?.ToString()
                    ?? string.Empty;

                if (estado == "Inactivo")
                {
                    e.CellStyle.Font =
                        new Font(
                            dgvProductos.Font,
                            FontStyle.Italic);
                }
            }
        }

        // =========================================================
        // CERRAR
        // =========================================================

        private void BtnCerrar_Click(
            object sender,
            EventArgs e)
        {
            Close();
        }

        // =========================================================
        // UTILIDADES
        // =========================================================

        private static Label CrearEtiqueta(
            string texto,
            Point ubicacion)
        {
            return new Label
            {
                Text = texto,
                AutoSize = true,
                Location = ubicacion
            };
        }

        private static TextBox CrearTextBox(
            Point ubicacion,
            int ancho)
        {
            return new TextBox
            {
                Location = ubicacion,
                Size = new Size(ancho, 28)
            };
        }

        private static Button CrearBoton(
            string texto,
            Point ubicacion,
            int ancho)
        {
            return new Button
            {
                Text = texto,
                Location = ubicacion,
                Size = new Size(ancho, 32)
            };
        }

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
