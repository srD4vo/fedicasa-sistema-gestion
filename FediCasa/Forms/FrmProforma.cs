using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace FediCasa.Forms
{
    /// <summary>
    /// Muestra una proforma como una hoja blanca (tipo hoja bond)
    /// sobre fondo gris y permite imprimirla.
    /// </summary>
    public class FrmProforma : Form
    {
        private readonly string[] lineas;
        private int lineaActual;

        public FrmProforma(string contenido, string ruta)
        {
            lineas =
                (contenido ?? string.Empty)
                    .Replace("\r\n", "\n")
                    .Split('\n');

            Text = "FediCasa - Proforma / Cotización";
            StartPosition = FormStartPosition.CenterParent;
            Size = new Size(860, 780);
            MinimumSize = new Size(760, 600);
            BackColor = Color.FromArgb(215, 215, 215);

            // ----- Barra superior -----
            Panel barra =
                new Panel
                {
                    Dock = DockStyle.Top,
                    Height = 56,
                    BackColor = Color.White
                };

            Button btnImprimir =
                new Button
                {
                    Text = "Imprimir proforma",
                    Location = new Point(15, 10),
                    Size = new Size(160, 36)
                };

            Button btnCerrar =
                new Button
                {
                    Text = "Cerrar",
                    Location = new Point(185, 10),
                    Size = new Size(120, 36)
                };

            Label lblRuta =
                new Label
                {
                    Text = string.IsNullOrWhiteSpace(ruta)
                        ? string.Empty
                        : "Guardada en: " + ruta,
                    Location = new Point(320, 10),
                    Size = new Size(500, 36),
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = Color.DimGray,
                    Anchor = AnchorStyles.Top |
                             AnchorStyles.Left |
                             AnchorStyles.Right
                };

            btnImprimir.Click += (s, e) => Imprimir();
            btnCerrar.Click += (s, e) => Close();

            barra.Controls.Add(btnImprimir);
            barra.Controls.Add(btnCerrar);
            barra.Controls.Add(lblRuta);

            // ----- Fondo gris con la "hoja" -----
            Panel fondo =
                new Panel
                {
                    Dock = DockStyle.Fill,
                    AutoScroll = true,
                    AutoScrollMargin = new Size(0, 20),
                    BackColor = Color.FromArgb(215, 215, 215)
                };

            Panel hoja =
                new Panel
                {
                    Size = new Size(700, 980),
                    Location = new Point(20, 20),
                    BackColor = Color.White,
                    BorderStyle = BorderStyle.FixedSingle,
                    Padding = new Padding(40)
                };

            RichTextBox visor =
                new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    BorderStyle = BorderStyle.None,
                    BackColor = Color.White,
                    WordWrap = false,
                    Font = new Font("Courier New", 10F),
                    Text = contenido ?? string.Empty
                };

            hoja.Controls.Add(visor);
            fondo.Controls.Add(hoja);

            // La hoja se mantiene centrada al cambiar el tamaño.
            Action centrar =
                () =>
                {
                    hoja.Left =
                        Math.Max(
                            20,
                            (fondo.ClientSize.Width - hoja.Width) / 2);
                };

            fondo.Resize += (s, e) => centrar();
            Load += (s, e) => centrar();

            // Primero el panel que rellena, luego la barra superior.
            Controls.Add(fondo);
            Controls.Add(barra);
        }

        // =========================================================
        // IMPRESIÓN (vista previa con botón de imprimir)
        // =========================================================

        private void Imprimir()
        {
            try
            {
                PrintDocument documento =
                    new PrintDocument
                    {
                        DocumentName = "Proforma FediCasa"
                    };

                documento.BeginPrint += (s, e) => lineaActual = 0;
                documento.PrintPage += Documento_PrintPage;

                using (PrintPreviewDialog vista =
                    new PrintPreviewDialog
                    {
                        Document = documento,
                        Width = 900,
                        Height = 800
                    })
                {
                    vista.ShowDialog(this);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo abrir la vista de impresión.\n\n" +
                    "Verifique que exista una impresora instalada.\n\n" +
                    ex.Message,
                    "Imprimir proforma",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void Documento_PrintPage(
            object sender,
            PrintPageEventArgs e)
        {
            Graphics g = e.Graphics!;

            using (Font fuente = new Font("Courier New", 10F))
            {
                float alto = fuente.GetHeight(g);
                float y = e.MarginBounds.Top;

                while (lineaActual < lineas.Length &&
                       y + alto <= e.MarginBounds.Bottom)
                {
                    g.DrawString(
                        lineas[lineaActual],
                        fuente,
                        Brushes.Black,
                        e.MarginBounds.Left,
                        y);

                    y += alto;
                    lineaActual++;
                }

                e.HasMorePages = lineaActual < lineas.Length;
            }
        }
    }
}
