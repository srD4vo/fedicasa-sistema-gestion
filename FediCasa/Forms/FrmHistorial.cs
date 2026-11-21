using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FediCasa.Models;
using FediCasa.Services;

namespace FediCasa.Forms
{
    /// <summary>
    /// CU-18 Consultar historial de operaciones (Administrador y maestro).
    /// </summary>
    public class FrmHistorial : Form
    {
        private ComboBox cboTipo;
        private DataGridView dgvHistorial;

        public FrmHistorial()
        {
            if (!SesionService.PuedeConsultarHistorial())
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
            cboTipo.Items.AddRange(new object[]
            {
                "Todos",
                "Ingreso",
                "Salida",
                "Incidencia",
                "Venta confirmada",
                "Venta anulada",
                "Venta registrada anulada"
            });
            cboTipo.SelectedIndex = 0;
            cboTipo.SelectedIndexChanged += (s, e) => Cargar();
            Controls.Add(cboTipo);

            dgvHistorial = new DataGridView
            {
                Location = new Point(25, 110),
                Size = new Size(1050, 430),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };
            dgvHistorial.Columns.Add("Fecha", "Fecha");
            dgvHistorial.Columns.Add("Tipo", "Tipo");
            dgvHistorial.Columns.Add("Codigo", "Código");
            dgvHistorial.Columns.Add("Producto", "Producto");
            dgvHistorial.Columns.Add("Cantidad", "Cantidad");
            dgvHistorial.Columns.Add("Stock", "Stock resultante");
            dgvHistorial.Columns.Add("Usuario", "Usuario");
            dgvHistorial.Columns.Add("Detalle", "Detalle");
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

        private void Cargar()
        {
            dgvHistorial.Rows.Clear();

            string tipo = cboTipo.SelectedItem?.ToString() ?? "Todos";

            var lista = MovimientoService.Obtener()
                .Where(m => tipo == "Todos" ||
                            m.Tipo.Equals(tipo, StringComparison.OrdinalIgnoreCase));

            foreach (Movimiento m in lista)
            {
                dgvHistorial.Rows.Add(
                    m.Fecha.ToString("dd/MM/yyyy HH:mm:ss"),
                    m.Tipo,
                    m.CodigoProducto,
                    m.NombreProducto,
                    m.Cantidad,
                    m.StockResultante,
                    m.Usuario,
                    m.Detalle);
            }
        }
    }
}
