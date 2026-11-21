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

        // Ventas y Administrador solo consultan.
        // Inventario envía solicitudes; el maestro opera directamente.
        private void ConfigurarPermisos()
        {
            bool maestro = SesionService.EsUsuarioMaestro();
            bool inventario = SesionService.EsInventario();

            bool puedeOperar = maestro || inventario;

            btnIngreso.Enabled = puedeOperar;
            btnSalida.Enabled = puedeOperar;
            btnDiferencia.Enabled = puedeOperar;
            txtCantidad.Enabled = puedeOperar;
            txtMotivo.Enabled = puedeOperar;

            if (inventario)
            {
                btnIngreso.Text = "Solicitar ingreso";
                btnSalida.Text = "Solicitar salida";
                btnDiferencia.Text = "Solicitar diferencia";
            }

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

        private void BtnIngreso_Click(object sender, EventArgs e)
        {
            Procesar(Solicitud.Ingreso);
        }

        private void BtnSalida_Click(object sender, EventArgs e)
        {
            Procesar(Solicitud.Salida);
        }

        // En "diferencia" la cantidad es el stock real contado.
        private void BtnDiferencia_Click(object sender, EventArgs e)
        {
            Procesar(Solicitud.Diferencia);
        }

        private void Procesar(string tipo)
        {
            if (!LeerDatos(
                    tipo == Solicitud.Diferencia,
                    out string codigo,
                    out int cantidad,
                    out string motivo))
                return;

            bool ok;
            string mensaje;

            if (SesionService.EsUsuarioMaestro())
            {
                // El maestro opera directamente.
                ok = SolicitudService.EjecutarOperacion(
                    tipo, codigo, cantidad, motivo,
                    SesionService.ObtenerUsuarioActual(),
                    out mensaje);

                if (ok)
                    mensaje = "Operación registrada correctamente.";
            }
            else
            {
                // Inventario envía la solicitud al Administrador.
                ok = SolicitudService.Crear(
                    tipo, codigo, cantidad, motivo, out mensaje);
            }

            MessageBox.Show(mensaje, "Inventario",
                MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (ok)
            {
                txtCantidad.Clear();
                txtMotivo.Clear();
                CargarProductos();
            }
        }
    }
}
