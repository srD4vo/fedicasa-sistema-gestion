using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FediCasa.Models;
using FediCasa.Services;

namespace FediCasa.Forms
{
    /// <summary>
    /// Historial de operaciones.
    /// Una fila por venta (agrupada por número de venta) y una fila por
    /// operación de inventario. Doble clic abre el detalle completo.
    /// </summary>
    public class FrmHistorial : Form
    {
        // Fila de la tabla principal.
        private class Fila
        {
            public DateTime Fecha;
            public string Tipo;
            public string Referencia;
            public string Usuario;
            public string Resumen;
            public List<Venta> LineasVenta;   // si es una venta
            public Movimiento Movimiento;     // si es inventario
        }

        private ComboBox cboTipo;
        private DataGridView dgvHistorial;

        public FrmHistorial()
        {
            if (!SesionService.PuedeConsultarHistorial() &&
                !SesionService.PuedeConsultarMovimientos())
            {
                MessageBox.Show(
                    "Tu perfil no tiene permisos para esta operación.",
                    "Acceso denegado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                Load += (s, e) => Close();
                return;
            }

            Text = "FediCasa - Historial de operaciones";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(1100, 600);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);

            Construir();
            Cargar();
        }

        // Inventario (sin permiso de historial general) solo ve inventario.
        private bool VeVentas()
        {
            return SesionService.PuedeConsultarHistorial();
        }

        private void Construir()
        {
            Controls.Add(new Label
            {
                Text = "Historial de operaciones",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 15)
            });

            Controls.Add(new Label
            {
                Text = "Tipo:",
                AutoSize = true,
                Location = new Point(28, 72)
            });

            cboTipo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(80, 68),
                Size = new Size(220, 28)
            };
            cboTipo.Items.Add("Todos");
            cboTipo.Items.Add("Ingreso");
            cboTipo.Items.Add("Salida");
            cboTipo.Items.Add("Diferencia");

            if (VeVentas())
            {
                cboTipo.Items.Add("Venta registrada");
                cboTipo.Items.Add("Venta confirmada");
                cboTipo.Items.Add("Venta anulada");
            }

            cboTipo.SelectedIndex = 0;
            cboTipo.SelectedIndexChanged += (s, e) => Cargar();
            Controls.Add(cboTipo);

            Controls.Add(new Label
            {
                Text = "Doble clic en una fila para ver el detalle completo.",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(330, 74)
            });

            dgvHistorial = new DataGridView
            {
                Location = new Point(25, 110),
                Size = new Size(1050, 430),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };
            dgvHistorial.Columns.Add("Fecha", "Fecha y hora");
            dgvHistorial.Columns.Add("Tipo", "Operación");
            dgvHistorial.Columns.Add("Referencia", "N.° / Código");
            dgvHistorial.Columns.Add("Usuario", "Usuario");
            dgvHistorial.Columns.Add("Resumen", "Resumen");
            dgvHistorial.Columns["Resumen"].FillWeight = 220;
            dgvHistorial.RowTemplate.Height = 28;
            dgvHistorial.CellDoubleClick += Dgv_CellDoubleClick;
            Controls.Add(dgvHistorial);

            Button btnCerrar = new Button
            {
                Text = "Cerrar",
                Location = new Point(930, 550),
                Size = new Size(145, 38)
            };
            btnCerrar.Click += (s, e) => Close();
            Controls.Add(btnCerrar);
        }

        // ---------------------------------------------------------
        // ARMAR FILAS
        // ---------------------------------------------------------

        private List<Fila> ArmarFilas()
        {
            List<Fila> filas = new List<Fila>();

            // Inventario: una fila por movimiento.
            // Los movimientos de ventas se omiten: las ventas se
            // muestran agrupadas desde el registro de ventas.
            foreach (Movimiento m in MovimientoService.Obtener()
                .Where(x => !x.Tipo.StartsWith("Venta")))
            {
                string signo = m.Cantidad.ToString("+0;-0;0");

                filas.Add(new Fila
                {
                    Fecha = m.Fecha,
                    Tipo = m.Tipo,
                    Referencia = m.CodigoProducto,
                    Usuario = m.Usuario,
                    Resumen = m.NombreProducto + ": " + signo +
                              " (stock " + m.StockResultante + ")",
                    Movimiento = m
                });
            }

            if (!VeVentas())
                return filas;

            // Ventas: una fila por IdVenta.
            foreach (var g in VentaService.ObtenerVentas()
                .GroupBy(v => v.IdVenta))
            {
                List<Venta> lineas = g.ToList();
                Venta primera = lineas[0];

                DateTime fecha =
                    primera.Estado == "Confirmada" &&
                    primera.FechaConfirmacion.Year > 2000
                        ? primera.FechaConfirmacion
                        : primera.Fecha;

                filas.Add(new Fila
                {
                    Fecha = fecha,
                    Tipo = "Venta " + primera.Estado.ToLower(),
                    Referencia = "Venta N.° " + g.Key,
                    Usuario = primera.UsuarioVendedor,
                    Resumen = lineas.Count + " producto(s), " +
                              lineas.Sum(v => v.Cantidad) + " unid. | " +
                              primera.NombreCliente + " | Total S/ " +
                              lineas.Sum(v => v.Total).ToString("0.00"),
                    LineasVenta = lineas
                });
            }

            return filas;
        }

        private void Cargar()
        {
            dgvHistorial.Rows.Clear();

            string tipo = cboTipo.SelectedItem?.ToString() ?? "Todos";

            foreach (Fila f in ArmarFilas()
                .Where(x => tipo == "Todos" ||
                            x.Tipo.Equals(
                                tipo,
                                StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.Fecha))
            {
                int i = dgvHistorial.Rows.Add(
                    f.Fecha.ToString("dd/MM/yyyy HH:mm:ss"),
                    f.Tipo,
                    f.Referencia,
                    f.Usuario,
                    f.Resumen);

                dgvHistorial.Rows[i].Tag = f;
            }
        }

        // ---------------------------------------------------------
        // DETALLE (doble clic)
        // ---------------------------------------------------------

        private void Dgv_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            Fila fila = dgvHistorial.Rows[e.RowIndex].Tag as Fila;

            if (fila == null)
                return;

            if (fila.LineasVenta != null)
                MostrarDetalleVenta(fila);
            else
                MostrarDetalleMovimiento(fila.Movimiento);
        }

        private Form CrearDialogo(string titulo, int alto)
        {
            return new Form
            {
                Text = titulo,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                ClientSize = new Size(760, alto),
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10F)
            };
        }

        private void MostrarDetalleVenta(Fila fila)
        {
            List<Venta> lineas = fila.LineasVenta;
            Venta v = lineas[0];

            using (Form f = CrearDialogo(fila.Referencia, 560))
            {
                f.Controls.Add(new Label
                {
                    Text = fila.Referencia + "  -  " + v.Estado,
                    Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(20, 15)
                });

                string comprobante =
                    string.IsNullOrWhiteSpace(v.NumeroComprobante)
                        ? "Sin comprobante"
                        : v.NumeroComprobante;

                f.Controls.Add(new Label
                {
                    Text =
                        "Fecha de registro: " +
                        v.Fecha.ToString("dd/MM/yyyy HH:mm:ss") + "\n" +
                        "Cliente: " + v.NombreCliente +
                        (string.IsNullOrWhiteSpace(v.DocumentoCliente)
                            ? ""
                            : "   |   Documento: " + v.DocumentoCliente) + "\n" +
                        "Vendedor: " + v.NombreVendedor +
                        " (" + v.UsuarioVendedor + ")\n" +
                        "Forma de pago: " + v.FormaPago +
                        "   |   Comprobante: " + comprobante,
                    AutoSize = false,
                    Size = new Size(720, 95),
                    Location = new Point(20, 55)
                });

                DataGridView grid = new DataGridView
                {
                    Location = new Point(20, 160),
                    Size = new Size(720, 280),
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AllowUserToDeleteRows = false,
                    RowHeadersVisible = false,
                    SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                    BackgroundColor = Color.White
                };
                grid.Columns.Add("Codigo", "Código");
                grid.Columns.Add("Producto", "Producto");
                grid.Columns.Add("Cantidad", "Cant.");
                grid.Columns.Add("Precio", "Precio");
                grid.Columns.Add("Importe", "Importe");
                grid.Columns["Producto"].FillWeight = 200;
                grid.Columns["Cantidad"].FillWeight = 50;

                foreach (Venta l in lineas)
                {
                    grid.Rows.Add(
                        l.CodigoProducto,
                        l.NombreProducto,
                        l.Cantidad,
                        "S/ " + l.Precio.ToString("0.00"),
                        "S/ " + l.Subtotal.ToString("0.00"));
                }
                f.Controls.Add(grid);

                f.Controls.Add(new Label
                {
                    Text =
                        "TOTAL: S/ " + lineas.Sum(l => l.Total).ToString("0.00") +
                        "   (incluye IGV S/ " +
                        lineas.Sum(l => l.IGV).ToString("0.00") + ")",
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleRight,
                    Size = new Size(720, 30),
                    Location = new Point(20, 450)
                });

                AgregarBotonCerrar(f, 505);
                f.ShowDialog(this);
            }
        }

        private void MostrarDetalleMovimiento(Movimiento m)
        {
            using (Form f = CrearDialogo(
                "Detalle de " + m.Tipo.ToLower(), 430))
            {
                f.Controls.Add(new Label
                {
                    Text = m.Tipo,
                    Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(20, 15)
                });

                f.Controls.Add(new Label
                {
                    Text =
                        "Fecha: " + m.Fecha.ToString("dd/MM/yyyy HH:mm:ss") + "\n" +
                        "Usuario que registró: " + m.Usuario + "\n" +
                        "Producto: " + m.NombreProducto +
                        " (" + m.CodigoProducto + ")\n" +
                        "Cantidad: " + m.Cantidad.ToString("+0;-0;0") +
                        "   |   Stock resultante: " + m.StockResultante,
                    AutoSize = false,
                    Size = new Size(720, 100),
                    Location = new Point(20, 55)
                });

                f.Controls.Add(new Label
                {
                    Text = "Detalle",
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(20, 165)
                });

                f.Controls.Add(new TextBox
                {
                    Text = m.Detalle,
                    Multiline = true,
                    ReadOnly = true,
                    WordWrap = true,
                    ScrollBars = ScrollBars.Vertical,
                    BackColor = Color.FromArgb(248, 248, 248),
                    Location = new Point(20, 190),
                    Size = new Size(720, 140),
                    TabStop = false
                });

                AgregarBotonCerrar(f, 355);
                f.ShowDialog(this);
            }
        }

        private void AgregarBotonCerrar(Form f, int y)
        {
            Button cerrar = new Button
            {
                Text = "Cerrar",
                DialogResult = DialogResult.OK,
                Location = new Point(620, y),
                Size = new Size(120, 36)
            };
            f.Controls.Add(cerrar);
            f.AcceptButton = cerrar;
            f.CancelButton = cerrar;
        }
    }
}
