using System;
using System.Drawing;
using System.Windows.Forms;
using FediCasa.Services;

namespace FediCasa.Forms
{
    public class FrmMenuPrincipal : Form
    {
        private Label lblTitulo;
        private Label lblBienvenida;
        private Label lblPerfil;
        private Label lblEstado;

        private Button btnProductos;
        private Button btnInventario;
        private Button btnVentas;
        private Button btnEmpleados;
        private Button btnReportes;
        private Button btnHistorial;
        private Button btnCerrarSesion;
        private Button btnSalir;

        public FrmMenuPrincipal()
        {
            ConfigurarFormulario();
            CrearControles();
            ActualizarInterfazSegunPermisos();
        }

        // =========================================================
        // CONFIGURACIÓN DEL FORMULARIO
        // =========================================================

        private void ConfigurarFormulario()
        {
            Text = "FediCasa - Menú principal";

            StartPosition =
                FormStartPosition.CenterScreen;

            Size = new Size(1050, 650);

            MinimumSize =
                new Size(1000, 600);

            BackColor = Color.White;

            Font = new Font(
                "Segoe UI",
                10F);

            FormBorderStyle =
                FormBorderStyle.FixedSingle;

            MaximizeBox = false;

            FormClosing +=
                FrmMenuPrincipal_FormClosing;
        }

        // =========================================================
        // CREACIÓN DE CONTROLES
        // =========================================================

        private void CrearControles()
        {
            lblTitulo = new Label
            {
                Text = "FediCasa",
                Font = new Font(
                    "Segoe UI",
                    28F,
                    FontStyle.Bold),
                AutoSize = true,
                Location = new Point(45, 35)
            };

            lblBienvenida = new Label
            {
                Text = "Bienvenido",
                Font = new Font(
                    "Segoe UI",
                    13F,
                    FontStyle.Regular),
                AutoSize = true,
                Location = new Point(48, 85)
            };

            lblPerfil = new Label
            {
                Text = "Perfil",
                Font = new Font(
                    "Segoe UI",
                    10F,
                    FontStyle.Bold),
                AutoSize = true,
                Location = new Point(48, 115)
            };

            lblEstado = new Label
            {
                Text = "Sesión activa",
                Font = new Font(
                    "Segoe UI",
                    9F),
                AutoSize = true,
                Location = new Point(48, 145),
                ForeColor = Color.DimGray
            };

            // -----------------------------------------------------
            // BOTONES PRINCIPALES
            // -----------------------------------------------------

            btnProductos = CrearBoton(
                "Productos",
                45,
                205);

            btnInventario = CrearBoton(
                "Inventario",
                280,
                205);

            btnVentas = CrearBoton(
                "Ventas",
                515,
                205);

            btnEmpleados = CrearBoton(
                "Usuarios",
                750,
                205);

            btnReportes = CrearBoton(
                "Reportes",
                45,
                320);

            btnHistorial = CrearBoton(
                "Historial",
                280,
                320);

            btnCerrarSesion = CrearBoton(
                "Cerrar sesión",
                280,
                440);

            btnSalir = CrearBoton(
                "Salir",
                515,
                440);

            Controls.Add(lblTitulo);
            Controls.Add(lblBienvenida);
            Controls.Add(lblPerfil);
            Controls.Add(lblEstado);

            Controls.Add(btnProductos);
            Controls.Add(btnInventario);
            Controls.Add(btnVentas);
            Controls.Add(btnEmpleados);
            Controls.Add(btnReportes);
            Controls.Add(btnHistorial);
            Controls.Add(btnCerrarSesion);
            Controls.Add(btnSalir);

            btnProductos.Click +=
                BtnProductos_Click;

            btnInventario.Click +=
                BtnInventario_Click;

            btnVentas.Click +=
                BtnVentas_Click;

            btnEmpleados.Click +=
                BtnEmpleados_Click;

            btnReportes.Click +=
                BtnReportes_Click;

            btnHistorial.Click +=
                BtnHistorial_Click;

            btnCerrarSesion.Click +=
                BtnCerrarSesion_Click;

            btnSalir.Click +=
                BtnSalir_Click;
        }

        private Button CrearBoton(
            string texto,
            int x,
            int y)
        {
            return new Button
            {
                Text = texto,
                Location = new Point(x, y),
                Size = new Size(200, 80),

                Font = new Font(
                    "Segoe UI",
                    11F,
                    FontStyle.Bold),

                FlatStyle =
                    FlatStyle.Standard,

                TabStop = true
            };
        }

        // =========================================================
        // INFORMACIÓN DEL USUARIO
        // =========================================================

        private void ActualizarInterfazSegunPermisos()
        {
            if (!SesionService.SesionActiva)
            {
                Close();
                return;
            }

            string nombre =
                SesionService.ObtenerNombreEmpleadoActual();

            string usuario =
                SesionService.ObtenerUsuarioActual();

            string cargo =
                SesionService.ObtenerCargoEmpleadoActual();

            lblBienvenida.Text =
                $"Bienvenido, {nombre}";

            if (SesionService.EsUsuarioMaestro())
            {
                lblPerfil.Text =
                    "USUARIO MAESTRO · ACCESO TOTAL";
            }
            else
            {
                lblPerfil.Text =
                    $"ROL: {cargo.ToUpperInvariant()}";
            }

            lblEstado.Text =
                $"Usuario: {usuario}";

            // -----------------------------------------------------
            // PRODUCTOS
            // CU-03 Gestionar productos
            // -----------------------------------------------------

            btnProductos.Visible =
                SesionService.PuedeGestionarProductos();

            // -----------------------------------------------------
            // INVENTARIO
            //
            // Este botón representa el módulo de inventario.
            // Inventario puede trabajar con sus operaciones.
            // Ventas puede consultar inventario.
            // Administrador puede acceder si posteriormente
            // se habilitan consultas administrativas.
            // -----------------------------------------------------

            btnInventario.Visible =
                SesionService.PuedeConsultarInventario() ||
                SesionService.PuedeRegistrarIngreso() ||
                SesionService.PuedeGestionarDiferencias();

            // -----------------------------------------------------
            // VENTAS
            // -----------------------------------------------------

            btnVentas.Visible =
                SesionService.PuedeGestionarVentas() ||
                SesionService.PuedeGestionarClientes() ||
                SesionService.PuedeGestionarProformas();

            // -----------------------------------------------------
            // USUARIOS
            // CU-02 Gestionar usuarios
            // -----------------------------------------------------

            btnEmpleados.Visible =
                SesionService.PuedeGestionarUsuarios();

            // -----------------------------------------------------
            // REPORTES
            // CU-17 Generar reportes
            // -----------------------------------------------------

            btnReportes.Visible =
                SesionService.PuedeGenerarReportes();

            // -----------------------------------------------------
            // HISTORIAL
            // CU-18 Consultar historial de operaciones
            // -----------------------------------------------------

            btnHistorial.Visible =
                SesionService.PuedeConsultarHistorial();

            // -----------------------------------------------------
            // ADMINISTRADOR
            // -----------------------------------------------------

            if (SesionService.EsAdministrador())
            {
                btnProductos.Visible = true;
                btnEmpleados.Visible = true;
                btnReportes.Visible = true;
            }

            // -----------------------------------------------------
            // MASTER
            // -----------------------------------------------------

            if (SesionService.TieneAccesoTotal())
            {
                btnProductos.Visible = true;
                btnInventario.Visible = true;
                btnVentas.Visible = true;
                btnEmpleados.Visible = true;
                btnReportes.Visible = true;
                btnHistorial.Visible = true;
            }
        }

        // =========================================================
        // PRODUCTOS
        // =========================================================

        private void BtnProductos_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService.PuedeGestionarProductos())
            {
                MostrarAccesoDenegado();
                return;
            }

            AbrirFormulario(
                new FrmProductos());
        }

        // =========================================================
        // INVENTARIO
        // =========================================================

        private void BtnInventario_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService.PuedeConsultarInventario() &&
                !SesionService.PuedeRegistrarIngreso() &&
                !SesionService.PuedeGestionarDiferencias())
            {
                MostrarAccesoDenegado();
                return;
            }

            AbrirFormulario(
                new FrmInventario());
        }

        // =========================================================
        // HISTORIAL
        // =========================================================

        private void BtnHistorial_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService.PuedeConsultarHistorial())
            {
                MostrarAccesoDenegado();
                return;
            }

            AbrirFormulario(
                new FrmHistorial());
        }

        // =========================================================
        // VENTAS
        // =========================================================

        private void BtnVentas_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService.PuedeGestionarVentas() &&
                !SesionService.PuedeGestionarClientes() &&
                !SesionService.PuedeGestionarProformas())
            {
                MostrarAccesoDenegado();
                return;
            }

            AbrirFormulario(
                new FrmVentas());
        }

        // =========================================================
        // USUARIOS
        // =========================================================

        private void BtnEmpleados_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService.PuedeGestionarUsuarios())
            {
                MostrarAccesoDenegado();
                return;
            }

            AbrirFormulario(
                new FrmEmpleados());
        }

        // =========================================================
        // REPORTES
        // =========================================================

        private void BtnReportes_Click(
            object sender,
            EventArgs e)
        {
            if (!SesionService.PuedeGenerarReportes())
            {
                MostrarAccesoDenegado();
                return;
            }

            AbrirFormulario(
                new FrmReportes());
        }

        // =========================================================
        // ABRIR FORMULARIOS
        // =========================================================

        private void AbrirFormulario(
            Form formulario)
        {
            Hide();

            try
            {
                formulario.StartPosition =
                    FormStartPosition.CenterScreen;

                formulario.ShowDialog();
            }
            finally
            {
                formulario.Dispose();
            }

            if (SesionService.SesionActiva)
            {
                ActualizarInterfazSegunPermisos();

                Show();
                Activate();
            }
            else
            {
                Close();
            }
        }

        // =========================================================
        // CERRAR SESIÓN
        // =========================================================

        private void BtnCerrarSesion_Click(
            object sender,
            EventArgs e)
        {
            DialogResult respuesta =
                MessageBox.Show(
                    "¿Deseas cerrar la sesión actual?",
                    "Cerrar sesión",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
                return;

            SesionService.CerrarSesion();

            Close();
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
                    "Salir del sistema",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

            if (respuesta != DialogResult.Yes)
                return;

            SesionService.CerrarSesion();

            Application.Exit();
        }

        // =========================================================
        // CIERRE DEL FORMULARIO
        // =========================================================

        private void FrmMenuPrincipal_FormClosing(
            object sender,
            FormClosingEventArgs e)
        {
            if (!SesionService.SesionActiva)
                return;

            if (e.CloseReason ==
                CloseReason.UserClosing)
            {
                DialogResult respuesta =
                    MessageBox.Show(
                        "¿Deseas salir de FediCasa?",
                        "Cerrar aplicación",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                if (respuesta != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }

                SesionService.CerrarSesion();
            }
        }

        // =========================================================
        // MENSAJE DE SEGURIDAD
        // =========================================================

        private void MostrarAccesoDenegado()
        {
            MessageBox.Show(
                "Tu perfil no tiene permisos para realizar esta operación.",
                "Acceso denegado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
        }
    }
}
