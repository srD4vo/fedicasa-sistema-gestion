using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using FediCasa.Helpers;
using FediCasa.Models;
using FediCasa.Services;

namespace FediCasa.Forms
{
    public class FrmReportes : Form
    {
        private Label lblTitulo;
        private Label lblInformacion;
        private Label lblReportes;

        private Button btnGenerarReporte;
        private Button btnActualizar;
        private Button btnVerReporte;
        private Button btnAbrirCarpeta;
        private Button btnCerrar;

        private ListBox lstReportes;
        private RichTextBox rtbReporte;

        public FrmReportes()
        {
            InicializarFormulario();
            ConfigurarPorRol();
            CargarReportes();
        }

        private void InicializarFormulario()
        {
            Text = "FediCasa - Reportes";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(1100, 700);
            BackColor = Color.White;

            lblTitulo = new Label
            {
                Text = "Reportes del Sistema",
                Font = new Font(
                    "Segoe UI",
                    22,
                    FontStyle.Bold),
                AutoSize = true,
                Location = new Point(35, 25)
            };

            lblInformacion = new Label
            {
                Text =
                    "Consulta el historial de ventas, inventario y " +
                    "estadísticas de FediCasa.",
                Font = new Font(
                    "Segoe UI",
                    10),
                AutoSize = true,
                Location = new Point(38, 70)
            };

            btnGenerarReporte = CrearBoton(
                "Generar reporte",
                new Point(40, 110),
                160);

            btnActualizar = CrearBoton(
                "Actualizar lista",
                new Point(210, 110),
                160);

            btnVerReporte = CrearBoton(
                "Ver reporte",
                new Point(380, 110),
                150);

            btnAbrirCarpeta = CrearBoton(
                "Abrir carpeta",
                new Point(540, 110),
                150);

            btnCerrar = CrearBoton(
                "Cerrar",
                new Point(700, 110),
                150);

            lblReportes = new Label
            {
                Text = "Reportes generados:",
                Font = new Font(
                    "Segoe UI",
                    10,
                    FontStyle.Bold),
                AutoSize = true,
                Location = new Point(40, 175)
            };

            lstReportes = new ListBox
            {
                Name = "lstReportes",
                Font = new Font(
                    "Segoe UI",
                    10),
                Location = new Point(40, 205),
                Size = new Size(300, 430)
            };

            rtbReporte = new RichTextBox
            {
                Name = "rtbReporte",
                Font = new Font(
                    "Consolas",
                    9),
                Location = new Point(360, 205),
                Size = new Size(700, 430),
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnGenerarReporte.Click +=
                BtnGenerarReporte_Click;

            btnActualizar.Click +=
                BtnActualizar_Click;

            btnVerReporte.Click +=
                BtnVerReporte_Click;

            btnAbrirCarpeta.Click +=
                BtnAbrirCarpeta_Click;

            btnCerrar.Click +=
                BtnCerrar_Click;

            lstReportes.DoubleClick +=
                LstReportes_DoubleClick;

            Controls.Add(lblTitulo);
            Controls.Add(lblInformacion);

            Controls.Add(btnGenerarReporte);
            Controls.Add(btnActualizar);
            Controls.Add(btnVerReporte);
            Controls.Add(btnAbrirCarpeta);
            Controls.Add(btnCerrar);

            Controls.Add(lblReportes);
            Controls.Add(lstReportes);
            Controls.Add(rtbReporte);
        }

        // Ventas e Inventario solo ven "Generar reporte" (su turno).
        private bool EsSoloTurno()
        {
            return SesionService.PuedeGenerarReporteTurno();
        }

        private void ConfigurarPorRol()
        {
            if (!EsSoloTurno())
                return;

            btnActualizar.Visible = false;
            btnVerReporte.Visible = false;
            btnAbrirCarpeta.Visible = false;
            lblReportes.Visible = false;
            lstReportes.Visible = false;

            btnCerrar.Location = new Point(210, 110);

            rtbReporte.Location = new Point(40, 175);
            rtbReporte.Size = new Size(1020, 460);

            lblInformacion.Text =
                "Genera el resumen de tus operaciones de hoy (turno).";
        }

        private void GenerarReporteDeTurno()
        {
            Empleado empleado =
                SesionService.ObtenerEmpleadoActual();

            if (empleado == null)
            {
                MessageBox.Show(
                    "No existe una sesión activa.",
                    "FediCasa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            try
            {
                string ruta =
                    ReporteHelper.GenerarReporteTurno(
                        empleado,
                        VentaService.ObtenerVentas(),
                        SolicitudService.Obtener(),
                        MovimientoService.Obtener());

                rtbReporte.Text =
                    ReporteHelper.LeerReporte(
                        Path.GetFileName(ruta));

                MessageBox.Show(
                    "Reporte de turno generado y guardado.\n\n" +
                    "Archivo:\n" + Path.GetFileName(ruta),
                    "FediCasa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo generar el reporte.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private Button CrearBoton(
            string texto,
            Point ubicacion,
            int ancho)
        {
            return new Button
            {
                Text = texto,
                Font = new Font(
                    "Segoe UI",
                    9,
                    FontStyle.Bold),
                Location = ubicacion,
                Size = new Size(ancho, 40),
                Cursor = Cursors.Hand
            };
        }

        private void CargarReportes()
        {
            lstReportes.Items.Clear();
            rtbReporte.Clear();

            if (EsSoloTurno())
            {
                rtbReporte.Text =
                    "Presione \"Generar reporte\" para ver el resumen " +
                    "de sus operaciones de hoy.";
                return;
            }

            List<string> reportes =
                ReporteHelper.ObtenerReportesHistoricos();

            foreach (string reporte in reportes)
            {
                lstReportes.Items.Add(reporte);
            }

            if (reportes.Count == 0)
            {
                rtbReporte.Text =
                    "No existen reportes generados todavía.\n\n" +
                    "Presione \"Generar reporte\" para crear el " +
                    "primer reporte de FediCasa.";
            }
        }

        private void BtnGenerarReporte_Click(
            object sender,
            EventArgs e)
        {
            if (EsSoloTurno())
            {
                GenerarReporteDeTurno();
                return;
            }

            try
            {
                List<Venta> ventas =
                    VentaService.ObtenerVentasConfirmadas();

                List<Producto> productos =
                    ProductoService.ObtenerProductos();

                Empleado empleado =
                    SesionService.ObtenerEmpleadoActual();

                if (empleado == null)
                {
                    MessageBox.Show(
                        "No existe una sesión activa.",
                        "FediCasa",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }

                string ruta =
                    ReporteHelper.GenerarReporteCompleto(
                        ventas,
                        productos,
                        empleado);

                MessageBox.Show(
                    "Reporte generado correctamente.\n\n" +
                    "Archivo:\n" +
                    Path.GetFileName(ruta),
                    "FediCasa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                CargarReportes();

                SeleccionarReporteGenerado(
                    Path.GetFileName(ruta));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo generar el reporte.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SeleccionarReporteGenerado(
            string nombreArchivo)
        {
            for (int i = 0;
                 i < lstReportes.Items.Count;
                 i++)
            {
                if (lstReportes.Items[i].ToString()
                    .Equals(
                        nombreArchivo,
                        StringComparison.OrdinalIgnoreCase))
                {
                    lstReportes.SelectedIndex = i;
                    MostrarReporteSeleccionado();
                    break;
                }
            }
        }

        private void BtnActualizar_Click(
            object sender,
            EventArgs e)
        {
            CargarReportes();
        }

        private void BtnVerReporte_Click(
            object sender,
            EventArgs e)
        {
            MostrarReporteSeleccionado();
        }

        private void LstReportes_DoubleClick(
            object sender,
            EventArgs e)
        {
            MostrarReporteSeleccionado();
        }

        private void MostrarReporteSeleccionado()
        {
            if (lstReportes.SelectedItem == null)
            {
                MessageBox.Show(
                    "Seleccione un reporte.",
                    "FediCasa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            string nombreArchivo =
                lstReportes.SelectedItem.ToString();

            try
            {
                string contenido =
                    ReporteHelper.LeerReporte(
                        nombreArchivo);

                if (string.IsNullOrWhiteSpace(contenido))
                {
                    rtbReporte.Text =
                        "El reporte está vacío.";
                }
                else
                {
                    rtbReporte.Text = contenido;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo leer el reporte.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnAbrirCarpeta_Click(
            object sender,
            EventArgs e)
        {
            try
            {
                string carpeta =
                    ReporteHelper.ObtenerCarpetaReportes();

                Directory.CreateDirectory(carpeta);

                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = carpeta,
                        UseShellExecute = true
                    });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo abrir la carpeta de reportes.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BtnCerrar_Click(
            object sender,
            EventArgs e)
        {
            Close();
        }
    }
}
