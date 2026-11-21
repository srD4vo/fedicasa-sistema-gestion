using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FediCasa.Models;
using FediCasa.Services;

namespace FediCasa.Forms
{
    /// <summary>
    /// Consulta de inventario (Ventas e Inventario) y operaciones de
    /// existencias: ingreso, salida, ajuste e incidencia de diferencia
    /// (Inventario y usuario maestro).
    /// </summary>
    public class FrmInventario : Form
    {
        private TextBox txtBuscar;
        private DataGridView dgvProductos;
        private TextBox txtCantidad;
        private TextBox txtMotivo;
        private Button btnIngreso;
        private Button btnSalida;
        private Button btnDiferencia;
        private Button btnCerrar;
        private Label lblResumen;

        public FrmInventario()
        {
            Text = "FediCasa - Inventario";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(980, 600);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);

            Construir();
            ConfigurarPermisos();
            CargarProductos();
        }

        private void Construir()
        {
            Controls.Add(new Label
            {
                Text = "Inventario",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 15)
            });

            Controls.Add(new Label
            {
                Text = "Buscar:",
                AutoSize = true,
                Location = new Point(28, 70)
            });

            txtBuscar = new TextBox
            {
                Location = new Point(95, 66),
                Size = new Size(300, 28)
            };
            txtBuscar.TextChanged += (s, e) => CargarProductos();
            Controls.Add(txtBuscar);

            lblResumen = new Label
            {
                AutoSize = true,
                ForeColor = Color.DimGray,
                Location = new Point(420, 70)
            };
            Controls.Add(lblResumen);

            dgvProductos = new DataGridView
            {
                Location = new Point(25, 105),
                Size = new Size(930, 330),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };
            dgvProductos.Columns.Add("Codigo", "Código");
            dgvProductos.Columns.Add("Nombre", "Producto");
            dgvProductos.Columns.Add("Stock", "Stock");
            dgvProductos.Columns.Add("Minimo", "Mínimo");
            dgvProductos.Columns.Add("Unidad", "Unidad");
            dgvProductos.Columns.Add("Estado", "Estado");
            Controls.Add(dgvProductos);

            Controls.Add(new Label
            {
                Text = "Cantidad:",
                AutoSize = true,
                Location = new Point(28, 458)
            });

            txtCantidad = new TextBox
            {
                Location = new Point(110, 454),
                Size = new Size(100, 28)
            };
            Controls.Add(txtCantidad);

            Controls.Add(new Label
            {
                Text = "Motivo:",
                AutoSize = true,
                Location = new Point(240, 458)
            });

            txtMotivo = new TextBox
            {
                Location = new Point(305, 454),
                Size = new Size(400, 28),
                MaxLength = 100
            };
            Controls.Add(txtMotivo);

            btnIngreso = CrearBoton("Registrar ingreso", 25, 505, 200);
            btnSalida = CrearBoton("Registrar salida", 240, 505, 200);
            btnDiferencia = CrearBoton("Registrar diferencia", 455, 505, 220);
            btnCerrar = CrearBoton("Cerrar", 810, 505, 145);

            Controls.Add(new Label
            {
                Text = "En 'diferencia' escriba el stock REAL contado.",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(455, 555)
            });

            btnIngreso.Click += BtnIngreso_Click;
            btnSalida.Click += BtnSalida_Click;
            btnDiferencia.Click += BtnDiferencia_Click;
            btnCerrar.Click += (s, e) => Close();
        }

        private Button CrearBoton(string texto, int x, int y, int ancho)
        {
            Button boton = new Button
            {
                Text = texto,
                Location = new Point(x, y),
                Size = new Size(ancho, 40)
            };
            Controls.Add(boton);
            return boton;
        }

        // Ventas solo consulta; Inventario y maestro operan.
        private void ConfigurarPermisos()
        {
            btnIngreso.Enabled = SesionService.PuedeRegistrarIngreso();
            btnSalida.Enabled = SesionService.PuedeRegistrarSalida();
            btnDiferencia.Enabled =
                SesionService.PuedeGestionarDiferencias() &&
                SesionService.PuedeActualizarExistencias();

            bool puedeOperar =
                btnIngreso.Enabled ||
                btnSalida.Enabled ||
                btnDiferencia.Enabled;

            txtCantidad.Enabled = puedeOperar;
            txtMotivo.Enabled = puedeOperar;

            if (!puedeOperar)
                Text = "FediCasa - Consulta de inventario";
        }

        private void CargarProductos()
        {
            string seleccionado = ObtenerCodigoSeleccionado();

            dgvProductos.Rows.Clear();

            var lista = ProductoService.BuscarTodos(txtBuscar.Text);

            foreach (Producto p in lista)
            {
                int fila = dgvProductos.Rows.Add(
                    p.Codigo,
                    p.Nombre,
                    p.Stock,
                    p.StockMinimo,
                    p.UnidadMedida,
                    p.ObtenerEstadoStock());

                if (p.Activo && p.Stock <= p.StockMinimo)
                    dgvProductos.Rows[fila].DefaultCellStyle.BackColor =
                        Color.MistyRose;

                if (p.Codigo == seleccionado)
                    dgvProductos.Rows[fila].Selected = true;
            }

            lblResumen.Text =
                $"Productos: {lista.Count} | " +
                $"Stock bajo: {ProductoService.ObtenerProductosStockBajo().Count}";
        }

        private string ObtenerCodigoSeleccionado()
        {
            if (dgvProductos.SelectedRows.Count == 0)
                return string.Empty;

            return dgvProductos.SelectedRows[0]
                .Cells["Codigo"].Value?.ToString() ?? string.Empty;
        }

        // Devuelve false si falta algún dato; el mensaje ya se mostró.
        private bool LeerDatos(
            bool permiteCero,
            out string codigo,
            out int cantidad,
            out string motivo)
        {
            codigo = ObtenerCodigoSeleccionado();
            cantidad = 0;
            motivo = txtMotivo.Text.Trim().Replace("|", "/");

            if (codigo == string.Empty)
            {
                MessageBox.Show("Seleccione un producto.", "Inventario",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (!int.TryParse(txtCantidad.Text.Trim(), out cantidad) ||
                cantidad < 0 ||
                (!permiteCero && cantidad == 0))
            {
                MessageBox.Show("Ingrese una cantidad válida.", "Inventario",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (motivo == string.Empty)
            {
                MessageBox.Show("Ingrese el motivo.", "Inventario",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private void Terminar(string tipo, string codigo, int cantidad, string motivo)
        {
            MovimientoService.Registrar(tipo, codigo, cantidad, motivo);
            txtCantidad.Clear();
            txtMotivo.Clear();
            CargarProductos();

            MessageBox.Show("Operación registrada correctamente.", "Inventario",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnIngreso_Click(object sender, EventArgs e)
        {
            if (!SesionService.PuedeRegistrarIngreso())
                return;

            if (!LeerDatos(false, out string codigo, out int cantidad, out string motivo))
                return;

            if (!ProductoService.AumentarStock(codigo, cantidad, out string mensaje))
            {
                MessageBox.Show(mensaje, "Inventario",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Terminar("Ingreso", codigo, cantidad, motivo);
        }

        private void BtnSalida_Click(object sender, EventArgs e)
        {
            if (!SesionService.PuedeRegistrarSalida())
                return;

            if (!LeerDatos(false, out string codigo, out int cantidad, out string motivo))
                return;

            if (!ProductoService.ReducirStock(codigo, cantidad, out string mensaje))
            {
                MessageBox.Show(mensaje, "Inventario",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Terminar("Salida", codigo, -cantidad, motivo);
        }

        // Incidencia: el stock contado no coincide con el del sistema.
        private void BtnDiferencia_Click(object sender, EventArgs e)
        {
            if (!SesionService.PuedeGestionarDiferencias() ||
                !SesionService.PuedeActualizarExistencias())
                return;

            if (!LeerDatos(true, out string codigo, out int stockReal, out string motivo))
                return;

            Producto producto = ProductoService.BuscarPorCodigo(codigo);

            if (producto == null)
                return;

            int diferencia = stockReal - producto.Stock;

            if (diferencia == 0)
            {
                MessageBox.Show("El stock contado coincide con el sistema.",
                    "Inventario", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            bool ok;
            string mensaje;

            if (diferencia > 0)
                ok = ProductoService.AumentarStock(codigo, diferencia, out mensaje);
            else
                ok = ProductoService.ReducirStock(codigo, -diferencia, out mensaje);

            if (!ok)
            {
                MessageBox.Show(mensaje, "Inventario",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Terminar(
                "Incidencia",
                codigo,
                diferencia,
                $"Diferencia de inventario (sistema: {producto.Stock}, real: {stockReal}). {motivo}");
        }
    }
}
