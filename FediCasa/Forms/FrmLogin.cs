using System;
using System.Drawing;
using System.Windows.Forms;
using FediCasa.Services;

namespace FediCasa.Forms
{
    public class FrmLogin : Form
    {
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Label lblUsuario;
        private Label lblContraseña;

        private TextBox txtUsuario;
        private TextBox txtContraseña;

        private Button btnIngresar;
        private Button btnSalir;

        public FrmLogin()
        {
            ConfigurarFormulario();
            CrearControles();

            InicializarSistema();
        }

        private void ConfigurarFormulario()
        {
            Text = "FediCasa - Inicio de sesión";

            StartPosition = FormStartPosition.CenterScreen;

            Size = new Size(500, 400);

            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;

            BackColor = Color.White;

            Font = new Font(
                "Segoe UI",
                10F);
        }

        private void CrearControles()
        {
            lblTitulo = new Label
            {
                Text = "FEDICASA",
                Font = new Font(
                    "Segoe UI",
                    28F,
                    FontStyle.Bold),
                AutoSize = true,
                Location = new Point(150, 35)
            };

            lblSubtitulo = new Label
            {
                Text = "Sistema de Gestión de Ferretería",
                Font = new Font(
                    "Segoe UI",
                    11F),
                AutoSize = true,
                Location = new Point(115, 82)
            };

            lblUsuario = new Label
            {
                Text = "Usuario:",
                AutoSize = true,
                Location = new Point(80, 145)
            };

            txtUsuario = new TextBox
            {
                Location = new Point(180, 140),
                Size = new Size(240, 30),
                MaxLength = 30
            };

            lblContraseña = new Label
            {
                Text = "Contraseña:",
                AutoSize = true,
                Location = new Point(80, 195)
            };

            txtContraseña = new TextBox
            {
                Location = new Point(180, 190),
                Size = new Size(240, 30),
                MaxLength = 30,
                UseSystemPasswordChar = true
            };

            btnIngresar = new Button
            {
                Text = "Ingresar",
                Location = new Point(80, 250),
                Size = new Size(160, 45)
            };

            btnSalir = new Button
            {
                Text = "Salir",
                Location = new Point(260, 250),
                Size = new Size(160, 45)
            };

            Controls.Add(lblTitulo);
            Controls.Add(lblSubtitulo);

            Controls.Add(lblUsuario);
            Controls.Add(txtUsuario);

            Controls.Add(lblContraseña);
            Controls.Add(txtContraseña);

            Controls.Add(btnIngresar);
            Controls.Add(btnSalir);

            btnIngresar.Click += BtnIngresar_Click;
            btnSalir.Click += BtnSalir_Click;

            txtUsuario.KeyDown += TxtUsuario_KeyDown;
            txtContraseña.KeyDown += TxtContraseña_KeyDown;
        }

        // =========================================================
        // INICIALIZACIÓN DEL SISTEMA
        // =========================================================

        private void InicializarSistema()
        {
            try
            {
                SesionService.Inicializar();

                ProductoService.Inicializar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "No se pudo inicializar FediCasa.\n\n" +
                    "Detalle: " + ex.Message,
                    "Error de inicialización",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // =========================================================
        // LOGIN
        // =========================================================

        private void BtnIngresar_Click(
            object sender,
            EventArgs e)
        {
            IniciarSesion();
        }

        private void IniciarSesion()
        {
            string usuario = txtUsuario.Text.Trim();
            string contraseña = txtContraseña.Text;

            if (string.IsNullOrWhiteSpace(usuario))
            {
                MessageBox.Show(
                    "Ingresa tu usuario.",
                    "Campo obligatorio",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtUsuario.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(contraseña))
            {
                MessageBox.Show(
                    "Ingresa tu contraseña.",
                    "Campo obligatorio",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtContraseña.Focus();
                return;
            }

            bool accesoPermitido =
                SesionService.IniciarSesion(
                    usuario,
                    contraseña);

            if (!accesoPermitido)
            {
                MessageBox.Show(
                    "Usuario o contraseña incorrectos.",
                    "Acceso denegado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtContraseña.Clear();
                txtContraseña.Focus();

                return;
            }

            MostrarBienvenida();
        }

        // =========================================================
        // BIENVENIDA
        // =========================================================

        private void MostrarBienvenida()
        {
            string nombre =
                SesionService.ObtenerNombreEmpleadoActual();

            string usuario =
                SesionService.ObtenerUsuarioActual();

            string perfil;

            if (SesionService.EsUsuarioMaestro())
            {
                perfil = "USUARIO MAESTRO - ACCESO TOTAL";
            }
            else if (SesionService.EsAdministrador())
            {
                perfil = "ADMINISTRADOR";
            }
            else if (SesionService.EsInventario())
            {
                perfil = "INVENTARIO";
            }
            else if (SesionService.EsVentas())
            {
                perfil = "VENTAS";
            }
            else
            {
                perfil = "USUARIO";
            }

            // La contraseña nunca se muestra.
            MessageBox.Show(
                $"Bienvenido(a), {nombre}.\n\n" +
                $"Usuario: {usuario}\n" +
                $"Perfil: {perfil}",
                "Bienvenido a FediCasa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            AbrirMenuPrincipal();
        }

        // =========================================================
        // MENÚ PRINCIPAL
        // =========================================================

        private void AbrirMenuPrincipal()
        {
            Hide();

            using (FrmMenuPrincipal menu =
                   new FrmMenuPrincipal())
            {
                menu.ShowDialog();
            }

            if (SesionService.SesionActiva)
            {
                Show();

                txtContraseña.Clear();
                txtUsuario.Focus();
            }
            else
            {
                txtUsuario.Clear();
                txtContraseña.Clear();

                Show();
                txtUsuario.Focus();
            }
        }

        // =========================================================
        // SALIR
        // =========================================================

        private void BtnSalir_Click(
            object sender,
            EventArgs e)
        {
            DialogResult respuesta =
                MessageBox.Show(
                    "¿Deseas salir de FediCasa?",
                    "Confirmar salida",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (respuesta == DialogResult.Yes)
            {
                SesionService.CerrarSesion();

                Application.Exit();
            }
        }

        // =========================================================
        // TECLADO
        // =========================================================

        private void TxtUsuario_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                txtContraseña.Focus();
            }
        }

        private void TxtContraseña_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;

                IniciarSesion();
            }
        }

        // =========================================================
        // CIERRE DEL FORMULARIO
        // =========================================================

        protected override void OnFormClosing(
            FormClosingEventArgs e)
        {
            if (e.CloseReason ==
                CloseReason.UserClosing)
            {
                DialogResult respuesta =
                    MessageBox.Show(
                        "¿Deseas salir de FediCasa?",
                        "Confirmar salida",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                if (respuesta != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                SesionService.CerrarSesion();
            }

            base.OnFormClosing(e);
        }
    }
}
