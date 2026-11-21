using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FediCasa.Models;
using FediCasa.Services;

namespace FediCasa.Forms
{
    /// <summary>
    /// Solicitudes e incidencias.
    /// Inventario y Ventas las crean; el Administrador las revisa;
    /// el usuario maestro atiende las incidencias derivadas.
    /// </summary>
    public class FrmSolicitudes : Form
    {
        private DataGridView dgv;
        private ComboBox cboFiltro;

        private ComboBox cboTipo;
        private TextBox txtCodigo;
        private TextBox txtCantidad;
        private TextBox txtMotivo;
        private Button btnEnviar;

        private TextBox txtComentario;
        private Button btnAprobar;
        private Button btnRechazar;
        private Button btnDerivar;
        private Button btnResolver;

        public FrmSolicitudes()
        {
            Text = "FediCasa - Solicitudes e incidencias";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(1150, 700);
            BackColor = Color.White;
            Font = new Font("Segoe UI", 10F);

            Construir();
            Cargar();
        }

        private void Construir()
        {
            Controls.Add(new Label
            {
                Text = "Solicitudes e incidencias",
                Font = new Font("Segoe UI", 20F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(25, 15)
            });

            Controls.Add(new Label
            {
                Text = "Estado:",
                AutoSize = true,
                Location = new Point(28, 72)
            });

            cboFiltro = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(95, 68),
                Size = new Size(200, 28)
            };
            cboFiltro.Items.AddRange(new object[]
            {
                "Todos",
                Solicitud.Pendiente,
                Solicitud.Aprobada,
                Solicitud.Rechazada,
                Solicitud.Derivada,
                Solicitud.Resuelta
            });
            cboFiltro.SelectedIndex = 0;
            cboFiltro.SelectedIndexChanged += (s, e) => Cargar();
            Controls.Add(cboFiltro);

            Button btnCerrar = new Button
            {
                Text = "Cerrar",
                Location = new Point(985, 62),
                Size = new Size(140, 36)
            };
            btnCerrar.Click += (s, e) => Close();
            Controls.Add(btnCerrar);

            dgv = new DataGridView
            {
                Location = new Point(25, 108),
                Size = new Size(1100, 330),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };
            dgv.Columns.Add("Id", "N.°");
            dgv.Columns.Add("Fecha", "Fecha");
            dgv.Columns.Add("Tipo", "Tipo");
            dgv.Columns.Add("Producto", "Producto");
            dgv.Columns.Add("Cantidad", "Cant.");
            dgv.Columns.Add("Estado", "Estado");
            dgv.Columns.Add("Solicitante", "Solicitante");
            dgv.Columns.Add("Revisor", "Revisor");
            dgv.Columns.Add("Motivo", "Motivo");
            dgv.Columns.Add("Comentario", "Comentario");
            dgv.Columns["Id"].FillWeight = 30;
            dgv.Columns["Cantidad"].FillWeight = 35;
            dgv.Columns["Motivo"].FillWeight = 140;
            dgv.Columns["Comentario"].FillWeight = 120;
            dgv.RowTemplate.Height = 28;
            dgv.ColumnHeadersDefaultCellStyle.Font =
                new Font("Segoe UI", 10F, FontStyle.Bold);
            dgv.CellDoubleClick += Dgv_CellDoubleClick;
            Controls.Add(dgv);

            Controls.Add(new Label
            {
                Text = "Doble clic en una fila para ver el detalle completo.",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(320, 74)
            });

            if (SesionService.PuedeCrearSolicitud())
                ConstruirNuevaSolicitud();

            if (SesionService.PuedeAutorizarSolicitudes())
                ConstruirRevision();
        }

        // ---------------------------------------------------------
        // NUEVA SOLICITUD (Inventario / Ventas)
        // ---------------------------------------------------------

        private void ConstruirNuevaSolicitud()
        {
            GroupBox grp = new GroupBox
            {
                Text = "Nueva solicitud / incidencia",
                Location = new Point(25, 450),
                Size = new Size(700, 230)
            };

            grp.Controls.Add(new Label
            {
                Text = "Tipo:",
                AutoSize = true,
                Location = new Point(15, 35)
            });

            cboTipo = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(110, 31),
                Size = new Size(220, 28)
            };

            if (SesionService.EsInventario())
            {
                cboTipo.Items.AddRange(new object[]
                {
                    Solicitud.Ingreso,
                    Solicitud.Salida,
                    Solicitud.Diferencia,
                    Solicitud.CambioProducto,
                    Solicitud.Incidencia
                });
            }
            else
            {
                cboTipo.Items.Add(Solicitud.Incidencia);
            }

            cboTipo.SelectedIndex = 0;
            grp.Controls.Add(cboTipo);

            grp.Controls.Add(new Label
            {
                Text = "Código:",
                AutoSize = true,
                Location = new Point(350, 35)
            });

            txtCodigo = new TextBox
            {
                Location = new Point(420, 31),
                Size = new Size(110, 28)
            };
            grp.Controls.Add(txtCodigo);

            grp.Controls.Add(new Label
            {
                Text = "Cant.:",
                AutoSize = true,
                Location = new Point(545, 35)
            });

            txtCantidad = new TextBox
            {
                Location = new Point(600, 31),
                Size = new Size(80, 28),
                Text = "0"
            };
            grp.Controls.Add(txtCantidad);

            grp.Controls.Add(new Label
            {
                Text = "Motivo / descripción:",
                AutoSize = true,
                Location = new Point(15, 80)
            });

            txtMotivo = new TextBox
            {
                Location = new Point(15, 105),
                Size = new Size(665, 60),
                Multiline = true,
                MaxLength = 200
            };
            grp.Controls.Add(txtMotivo);

            grp.Controls.Add(new Label
            {
                Text = "En 'Diferencia' la cantidad es el stock REAL contado.",
                AutoSize = true,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 9F),
                Location = new Point(15, 185)
            });

            btnEnviar = new Button
            {
                Text = "Enviar solicitud",
                Location = new Point(520, 178),
                Size = new Size(160, 38)
            };
            btnEnviar.Click += BtnEnviar_Click;
            grp.Controls.Add(btnEnviar);

            Controls.Add(grp);
        }

        private void BtnEnviar_Click(object sender, EventArgs e)
        {
            int.TryParse(txtCantidad.Text.Trim(), out int cantidad);

            if (!SolicitudService.Crear(
                    cboTipo.SelectedItem.ToString(),
                    txtCodigo.Text,
                    cantidad,
                    txtMotivo.Text,
                    out string mensaje))
            {
                MessageBox.Show(mensaje, "Solicitudes",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            txtCodigo.Clear();
            txtCantidad.Text = "0";
            txtMotivo.Clear();
            Cargar();

            MessageBox.Show(mensaje, "Solicitudes",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ---------------------------------------------------------
        // REVISIÓN (Administrador / maestro)
        // ---------------------------------------------------------

        private void ConstruirRevision()
        {
            GroupBox grp = new GroupBox
            {
                Text = "Revisión",
                Location = new Point(740, 450),
                Size = new Size(385, 230)
            };

            // Si el usuario no crea solicitudes, la revisión ocupa más ancho.
            if (!SesionService.PuedeCrearSolicitud())
            {
                grp.Location = new Point(25, 450);
                grp.Size = new Size(1100, 230);
            }

            grp.Controls.Add(new Label
            {
                Text = "Comentario (obligatorio al rechazar, derivar o resolver):",
                AutoSize = true,
                Location = new Point(15, 30)
            });

            txtComentario = new TextBox
            {
                Location = new Point(15, 55),
                Size = new Size(grp.Width - 30, 50),
                Multiline = true,
                MaxLength = 200
            };
            grp.Controls.Add(txtComentario);

            btnAprobar = CrearBoton("Aprobar", 15, 120, grp);
            btnRechazar = CrearBoton("Rechazar", 195, 120, grp);
            btnResolver = CrearBoton("Marcar resuelta", 15, 170, grp);
            btnDerivar = CrearBoton("Derivar a admin", 195, 170, grp);

            // El usuario maestro no deriva: él es el destino.
            btnDerivar.Enabled = SesionService.EsAdministrador();

            btnAprobar.Click += (s, e) => Revisar("aprobar");
            btnRechazar.Click += (s, e) => Revisar("rechazar");
            btnResolver.Click += (s, e) => Revisar("resolver");
            btnDerivar.Click += (s, e) => Revisar("derivar");

            Controls.Add(grp);
        }

        private Button CrearBoton(string texto, int x, int y, GroupBox grp)
        {
            Button b = new Button
            {
                Text = texto,
                Location = new Point(x, y),
                Size = new Size(165, 36)
            };
            grp.Controls.Add(b);
            return b;
        }

        private void Revisar(string accion)
        {
            if (dgv.SelectedRows.Count == 0)
            {
                MessageBox.Show("Seleccione una solicitud.", "Solicitudes",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int id = Convert.ToInt32(
                dgv.SelectedRows[0].Cells["Id"].Value);

            string comentario = txtComentario.Text;
            string mensaje;
            bool ok;

            switch (accion)
            {
                case "aprobar":
                    ok = SolicitudService.Aprobar(id, comentario, out mensaje);
                    break;
                case "rechazar":
                    ok = SolicitudService.Rechazar(id, comentario, out mensaje);
                    break;
                case "derivar":
                    ok = SolicitudService.Derivar(id, comentario, out mensaje);
                    break;
                default:
                    ok = SolicitudService.Resolver(id, comentario, out mensaje);
                    break;
            }

            MessageBox.Show(mensaje, "Solicitudes",
                MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

            if (ok)
            {
                txtComentario.Clear();
                Cargar();
            }
        }

        // ---------------------------------------------------------
        // LISTA
        // ---------------------------------------------------------

        private void Cargar()
        {
            dgv.Rows.Clear();

            string filtro = cboFiltro.SelectedItem?.ToString() ?? "Todos";

            foreach (Solicitud s in SolicitudService.Obtener()
                .Where(x => filtro == "Todos" || x.Estado == filtro))
            {
                int fila = dgv.Rows.Add(
                    s.Id,
                    s.Fecha.ToString("dd/MM/yyyy HH:mm"),
                    s.Tipo,
                    s.CodigoProducto,
                    s.Cantidad,
                    s.Estado,
                    s.Solicitante,
                    s.Revisor,
                    s.Motivo,
                    s.Comentario);

                DataGridViewRow row = dgv.Rows[fila];
                row.Cells["Estado"].Style.ForeColor = ColorEstado(s.Estado);
                row.Cells["Estado"].Style.Font =
                    new Font("Segoe UI", 10F, FontStyle.Bold);
                row.Cells["Motivo"].ToolTipText = s.Motivo;
                row.Cells["Comentario"].ToolTipText = s.Comentario;
            }
        }

        private Color ColorEstado(string estado)
        {
            switch (estado)
            {
                case Solicitud.Aprobada:
                case Solicitud.Resuelta:
                    return Color.FromArgb(30, 120, 60);
                case Solicitud.Rechazada:
                    return Color.Firebrick;
                case Solicitud.Derivada:
                    return Color.FromArgb(110, 60, 160);
                default:
                    return Color.DarkOrange;
            }
        }

        // ---------------------------------------------------------
        // DETALLE (doble clic en una fila)
        // ---------------------------------------------------------

        private void Dgv_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            int id = Convert.ToInt32(dgv.Rows[e.RowIndex].Cells["Id"].Value);

            Solicitud solicitud = SolicitudService.Obtener()
                .FirstOrDefault(x => x.Id == id);

            if (solicitud != null)
                MostrarDetalle(solicitud);
        }

        private void MostrarDetalle(Solicitud s)
        {
            Producto producto = string.IsNullOrWhiteSpace(s.CodigoProducto)
                ? null
                : ProductoService.BuscarPorCodigo(s.CodigoProducto);

            string textoProducto =
                producto != null
                    ? producto.Nombre + " (" + producto.Codigo + ")"
                    : (s.CodigoProducto == string.Empty
                        ? "No aplica"
                        : s.CodigoProducto);

            using (Form f = new Form())
            {
                f.Text = "Detalle de solicitud N.° " + s.Id;
                f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog;
                f.MaximizeBox = false;
                f.MinimizeBox = false;
                f.ClientSize = new Size(640, 640);
                f.BackColor = Color.White;
                f.Font = new Font("Segoe UI", 10F);

                f.Controls.Add(new Label
                {
                    Text = s.Tipo + " N.° " + s.Id,
                    Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(20, 15)
                });

                f.Controls.Add(new Label
                {
                    Text = s.Estado,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = ColorEstado(s.Estado),
                    AutoSize = true,
                    Location = new Point(22, 50)
                });

                // Datos en dos columnas
                AgregarDato(f, "Fecha", s.Fecha.ToString("dd/MM/yyyy HH:mm"), 20, 90);
                AgregarDato(f, "Cantidad", s.Cantidad.ToString(), 330, 90);
                AgregarDato(f, "Producto", textoProducto, 20, 140);
                AgregarDato(f, "Solicitante", s.Solicitante, 20, 190);
                AgregarDato(f, "Revisor",
                    s.Revisor == string.Empty ? "Sin revisar" : s.Revisor,
                    330, 190);

                AgregarTextoLargo(f, "Motivo / descripción", s.Motivo, 245);
                AgregarTextoLargo(f, "Comentario de revisión",
                    s.Comentario == string.Empty
                        ? "Sin comentario todavía."
                        : s.Comentario,
                    425);

                Button cerrar = new Button
                {
                    Text = "Cerrar",
                    DialogResult = DialogResult.OK,
                    Location = new Point(500, 592),
                    Size = new Size(120, 36)
                };
                f.Controls.Add(cerrar);
                f.AcceptButton = cerrar;
                f.CancelButton = cerrar;

                f.ShowDialog(this);
            }
        }

        private void AgregarDato(Form f, string titulo, string valor, int x, int y)
        {
            f.Controls.Add(new Label
            {
                Text = titulo,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 9F),
                AutoSize = true,
                Location = new Point(x, y)
            });

            f.Controls.Add(new Label
            {
                Text = valor,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                AutoSize = false,
                Size = new Size(x == 20 && y == 140 ? 600 : 290, 26),
                Location = new Point(x, y + 20)
            });
        }

        // Campo amplio, multilínea, con ajuste de línea y scroll vertical.
        private void AgregarTextoLargo(Form f, string titulo, string texto, int y)
        {
            f.Controls.Add(new Label
            {
                Text = titulo,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(20, y)
            });

            f.Controls.Add(new TextBox
            {
                Text = texto,
                Multiline = true,
                ReadOnly = true,
                WordWrap = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(248, 248, 248),
                Location = new Point(20, y + 25),
                Size = new Size(600, 140),
                TabStop = false
            });
        }
    }
}
