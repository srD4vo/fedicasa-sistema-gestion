using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using FediCasa.Helpers;
using FediCasa.Models;
using FediCasa.Services;

namespace FediCasa.Forms
{
    public class FrmEmpleados : Form
    {
        private Label lblTitulo;
        private Label lblSubtitulo;
        private Label lblUsuario;
        private Label lblContraseña;
        private Label lblNombre;
        private Label lblCargo;
        private Label lblBuscar;
        private Label lblAviso;

        private TextBox txtUsuario;
        private TextBox txtContraseña;
        private TextBox txtNombre;
        private ComboBox cmbCargo;
        private TextBox txtBuscar;

        private Button btnRegistrar;
        private Button btnModificar;
        private Button btnEliminar;
        private Button btnLimpiar;
        private Button btnCerrar;

        private DataGridView dgvEmpleados;

        private string usuarioSeleccionado = string.Empty;

        public FrmEmpleados()
        {
            ConfigurarFormulario();
            CrearControles();
            CargarEmpleados();

            Load += FrmEmpleados_Load;
        }

        // =========================================================
        // SEGURIDAD
        // =========================================================

        private void FrmEmpleados_Load(
            object sender,
            EventArgs e)
        {
            if (!SesionService.PuedeGestionarUsuarios())
            {
                MessageBox.Show(
                    "No tienes permisos para acceder a la gestión de usuarios.",
                    "Acceso denegado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                Close();
                return;
            }

            ActualizarEstadoControles();
        }

        // =========================================================
        // CONFIGURACIÓN
        // =========================================================

        private void ConfigurarFormulario()
        {
            Text = "FediCasa - Gestión de usuarios";

            StartPosition = FormStartPosition.CenterScreen;

            Size = new Size(1180, 700);
            MinimumSize = new Size(1100, 650);

            BackColor = Color.White;

            Font = new Font(
                "Segoe UI",
                10F);

            FormBorderStyle =
                FormBorderStyle.FixedSingle;

            MaximizeBox = false;
        }

        // =========================================================
        // CONTROLES
        // =========================================================

        private void CrearControles()
        {
            lblTitulo = new Label
            {
                Text = "GESTIÓN DE USUARIOS",
                Font = new Font(
                    "Segoe UI",
                    22F,
                    FontStyle.Bold),
                AutoSize = true,
                Location = new Point(35, 25)
            };

            lblSubtitulo = new Label
            {
                Text =
                    "Administra los usuarios y roles autorizados de FediCasa.",
                Font = new Font(
                    "Segoe UI",
                    10F),
                AutoSize = true,
                Location = new Point(38, 67),
                ForeColor = Color.DimGray
            };

            // -----------------------------------------------------
            // DATOS DEL USUARIO
            // -----------------------------------------------------

            lblUsuario = new Label
            {
                Text = "Usuario",
                AutoSize = true,
                Location = new Point(40, 120)
            };

            txtUsuario = new TextBox
            {
                Location = new Point(40, 145),
                Size = new Size(330, 30),
                MaxLength = 30
            };

            lblContraseña = new Label
            {
                Text = "Contraseña",
                AutoSize = true,
                Location = new Point(40, 190)
            };

            txtContraseña = new TextBox
            {
                Location = new Point(40, 215),
                Size = new Size(330, 30),
                MaxLength = 30,
                UseSystemPasswordChar = true
            };

            lblNombre = new Label
            {
                Text = "Nombre completo",
                AutoSize = true,
                Location = new Point(40, 260)
            };

            txtNombre = new TextBox
            {
                Location = new Point(40, 285),
                Size = new Size(330, 30),
                MaxLength = 80
            };

            lblCargo = new Label
            {
                Text = "Rol",
                AutoSize = true,
                Location = new Point(40, 330)
            };

            cmbCargo = new ComboBox
            {
                Location = new Point(40, 355),
                Size = new Size(330, 30),
                DropDownStyle =
                    ComboBoxStyle.DropDownList
            };

            cmbCargo.Items.Add("Administrador");
            cmbCargo.Items.Add("Inventario");
            cmbCargo.Items.Add("Ventas");

            cmbCargo.SelectedIndex = 2;

            // -----------------------------------------------------
            // BOTONES
            // -----------------------------------------------------

            btnRegistrar = new Button
            {
                Text = "Registrar usuario",
                Location = new Point(40, 410),
                Size = new Size(155, 42)
            };

            btnModificar = new Button
            {
                Text = "Modificar",
                Location = new Point(205, 410),
                Size = new Size(165, 42)
            };

            btnEliminar = new Button
            {
                Text = "Eliminar",
                Location = new Point(40, 462),
                Size = new Size(155, 42)
            };

            btnLimpiar = new Button
            {
                Text = "Limpiar",
                Location = new Point(205, 462),
                Size = new Size(165, 42)
            };

            btnCerrar = new Button
            {
                Text = "Cerrar",
                Location = new Point(40, 525),
                Size = new Size(330, 42)
            };

            // -----------------------------------------------------
            // INFORMACIÓN
            // -----------------------------------------------------

            lblAviso = new Label
            {
                Text =
                    "La cuenta maestra del sistema está protegida\r\n" +
                    "y no puede modificarse desde este módulo.",
                AutoSize = true,
                Location = new Point(40, 585),
                ForeColor = Color.DimGray,
                Font = new Font(
                    "Segoe UI",
                    9F)
            };

            // -----------------------------------------------------
            // BÚSQUEDA
            // -----------------------------------------------------

            lblBuscar = new Label
            {
                Text = "Buscar usuario",
                AutoSize = true,
                Location = new Point(420, 120)
            };

            txtBuscar = new TextBox
            {
                Location = new Point(420, 145),
                Size = new Size(700, 30),
                MaxLength = 80
            };

            // -----------------------------------------------------
            // TABLA
            // -----------------------------------------------------

            dgvEmpleados = new DataGridView
            {
                Location = new Point(420, 195),
                Size = new Size(700, 435),

                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,

                ReadOnly = true,

                MultiSelect = false,

                SelectionMode =
                    DataGridViewSelectionMode.FullRowSelect,

                AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.Fill,

                BackgroundColor = Color.White,

                BorderStyle =
                    BorderStyle.FixedSingle,

                RowHeadersVisible = false,

                AllowDrop = false
            };

            dgvEmpleados.Columns.Add(
                "Usuario",
                "Usuario");

            dgvEmpleados.Columns.Add(
                "Nombre",
                "Nombre completo");

            dgvEmpleados.Columns.Add(
                "Cargo",
                "Rol");

            Controls.Add(lblTitulo);
            Controls.Add(lblSubtitulo);

            Controls.Add(lblUsuario);
            Controls.Add(txtUsuario);

            Controls.Add(lblContraseña);
            Controls.Add(txtContraseña);

            Controls.Add(lblNombre);
            Controls.Add(txtNombre);

            Controls.Add(lblCargo);
            Controls.Add(cmbCargo);

            Controls.Add(btnRegistrar);
            Controls.Add(btnModificar);
            Controls.Add(btnEliminar);
            Controls.Add(btnLimpiar);
            Controls.Add(btnCerrar);

            Controls.Add(lblAviso);

            Controls.Add(lblBuscar);
            Controls.Add(txtBuscar);

            Controls.Add(dgvEmpleados);

            // -----------------------------------------------------
            // EVENTOS
            // -----------------------------------------------------

            btnRegistrar.Click += BtnRegistrar_Click;
            btnModificar.Click += BtnModificar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnLimpiar.Click += BtnLimpiar_Click;
            btnCerrar.Click += BtnCerrar_Click;

            txtBuscar.TextChanged +=
                TxtBuscar_TextChanged;

            dgvEmpleados.CellClick +=
                DgvEmpleados_CellClick;

            dgvEmpleados.CellDoubleClick +=
                DgvEmpleados_CellDoubleClick;

            txtUsuario.KeyDown +=
                TextBox_KeyDown;

            txtContraseña.KeyDown +=
                TextBox_KeyDown;

            txtNombre.KeyDown +=
                TextBox_KeyDown;
        }

        // =========================================================
        // CARGAR USUARIOS
        // =========================================================

        private void CargarEmpleados(
            string filtro = "")
        {
            dgvEmpleados.Rows.Clear();

            List<Empleado> empleados =
                ArchivoHelper.LeerEmpleados();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                filtro = filtro.Trim();

                empleados = empleados
                    .Where(e =>
                        e.Usuario.Contains(
                            filtro,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        e.NombreCompleto.Contains(
                            filtro,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        e.Cargo.Contains(
                            filtro,
                            StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            foreach (Empleado empleado in empleados)
            {
                int fila = dgvEmpleados.Rows.Add(
                    empleado.Usuario,
                    empleado.NombreCompleto,
                    empleado.EsUsuarioMaestro
                        ? "Usuario maestro"
                        : empleado.Cargo);

                dgvEmpleados.Rows[fila].Tag =
                    empleado;
            }

            dgvEmpleados.ClearSelection();

            ActualizarEstadoControles();
        }

        // =========================================================
        // REGISTRAR
        // =========================================================

        private void BtnRegistrar_Click(
            object sender,
            EventArgs e)
        {
            if (!ValidarDatos())
                return;

            List<Empleado> empleados =
                ArchivoHelper.LeerEmpleados();

            string usuario =
                txtUsuario.Text.Trim();

            bool existe = empleados.Any(e =>
                e.Usuario.Equals(
                    usuario,
                    StringComparison.OrdinalIgnoreCase));

            if (existe)
            {
                MessageBox.Show(
                    "El usuario ingresado ya existe.",
                    "Usuario duplicado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtUsuario.Focus();
                return;
            }

            // Nunca se puede crear otro usuario maestro.
            if (usuario.Equals(
                "admin",
                StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "La cuenta 'admin' está reservada para el usuario maestro del sistema.",
                    "Usuario reservado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtUsuario.Focus();
                return;
            }

            Empleado nuevoEmpleado =
                new Empleado
                {
                    Usuario = usuario,
                    Contraseña = txtContraseña.Text,
                    NombreCompleto =
                        txtNombre.Text.Trim(),
                    Cargo = cmbCargo.Text,
                    EsUsuarioMaestro = false
                };

            empleados.Add(nuevoEmpleado);

            ArchivoHelper.GuardarEmpleados(
                empleados);

            MessageBox.Show(
                "El usuario fue registrado correctamente.",
                "Registro exitoso",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            CargarEmpleados();

            LimpiarCampos();
        }

        // =========================================================
        // MODIFICAR
        // =========================================================

        private void BtnModificar_Click(
            object sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                usuarioSeleccionado))
            {
                MessageBox.Show(
                    "Selecciona un usuario de la tabla.",
                    "Usuario no seleccionado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (!ValidarDatos())
                return;

            if (usuarioSeleccionado.Equals(
                "admin",
                StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "La cuenta maestra 'admin' está protegida y no puede modificarse desde este módulo.",
                    "Operación no permitida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (usuarioSeleccionado.Equals(
                SesionService.ObtenerUsuarioActual(),
                StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "No puedes modificar el usuario con el que estás conectado.",
                    "Operación no permitida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            List<Empleado> empleados =
                ArchivoHelper.LeerEmpleados();

            Empleado empleado =
                empleados.FirstOrDefault(e =>
                    e.Usuario.Equals(
                        usuarioSeleccionado,
                        StringComparison.OrdinalIgnoreCase));

            if (empleado == null)
            {
                MessageBox.Show(
                    "No se encontró el usuario seleccionado.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                CargarEmpleados();

                LimpiarCampos();

                return;
            }

            string nuevoUsuario =
                txtUsuario.Text.Trim();

            bool usuarioDuplicado =
                empleados.Any(e =>
                    !e.Usuario.Equals(
                        usuarioSeleccionado,
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    e.Usuario.Equals(
                        nuevoUsuario,
                        StringComparison.OrdinalIgnoreCase));

            if (usuarioDuplicado)
            {
                MessageBox.Show(
                    "El nuevo nombre de usuario ya está siendo utilizado.",
                    "Usuario duplicado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtUsuario.Focus();

                return;
            }

            bool eraAdministrador =
                empleado.Cargo.Equals(
                    "Administrador",
                    StringComparison.OrdinalIgnoreCase);

            bool seraAdministrador =
                cmbCargo.Text.Equals(
                    "Administrador",
                    StringComparison.OrdinalIgnoreCase);

            if (eraAdministrador &&
                !seraAdministrador)
            {
                int cantidadAdministradores =
                    empleados.Count(e =>
                        !e.EsUsuarioMaestro &&
                        e.Cargo.Equals(
                            "Administrador",
                            StringComparison.OrdinalIgnoreCase));

                if (cantidadAdministradores <= 1)
                {
                    MessageBox.Show(
                        "No puedes quitar el último usuario con rol Administrador del sistema.",
                        "Operación no permitida",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    cmbCargo.SelectedItem =
                        "Administrador";

                    return;
                }
            }

            empleado.Usuario =
                nuevoUsuario;

            empleado.Contraseña =
                txtContraseña.Text;

            empleado.NombreCompleto =
                txtNombre.Text.Trim();

            empleado.Cargo =
                cmbCargo.Text;

            empleado.EsUsuarioMaestro =
                false;

            ArchivoHelper.GuardarEmpleados(
                empleados);

            MessageBox.Show(
                "El usuario fue modificado correctamente.",
                "Modificación exitosa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            CargarEmpleados();

            LimpiarCampos();
        }

        // =========================================================
        // ELIMINAR
        // =========================================================

        private void BtnEliminar_Click(
            object sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(
                usuarioSeleccionado))
            {
                MessageBox.Show(
                    "Selecciona un usuario de la tabla.",
                    "Usuario no seleccionado",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (usuarioSeleccionado.Equals(
                "admin",
                StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "La cuenta maestra 'admin' está protegida y no puede eliminarse.",
                    "Operación no permitida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (usuarioSeleccionado.Equals(
                SesionService.ObtenerUsuarioActual(),
                StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "No puedes eliminar el usuario con el que estás conectado.",
                    "Operación no permitida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            List<Empleado> empleados =
                ArchivoHelper.LeerEmpleados();

            Empleado empleado =
                empleados.FirstOrDefault(e =>
                    e.Usuario.Equals(
                        usuarioSeleccionado,
                        StringComparison.OrdinalIgnoreCase));

            if (empleado == null)
            {
                MessageBox.Show(
                    "No se encontró el usuario seleccionado.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                CargarEmpleados();

                LimpiarCampos();

                return;
            }

            if (empleado.Cargo.Equals(
                "Administrador",
                StringComparison.OrdinalIgnoreCase))
            {
                int cantidadAdministradores =
                    empleados.Count(e =>
                        !e.EsUsuarioMaestro &&
                        e.Cargo.Equals(
                            "Administrador",
                            StringComparison.OrdinalIgnoreCase));

                if (cantidadAdministradores <= 1)
                {
                    MessageBox.Show(
                        "No puedes eliminar al último usuario con rol Administrador.",
                        "Operación no permitida",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);

                    return;
                }
            }

            DialogResult respuesta =
                MessageBox.Show(
                    $"¿Deseas eliminar al usuario '{empleado.Usuario}'?\n\n" +
                    $"Nombre: {empleado.NombreCompleto}\n" +
                    $"Rol: {empleado.Cargo}",
                    "Confirmar eliminación",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

            if (respuesta != DialogResult.Yes)
                return;

            empleados.Remove(empleado);

            ArchivoHelper.GuardarEmpleados(
                empleados);

            MessageBox.Show(
                "El usuario fue eliminado correctamente.",
                "Eliminación exitosa",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            CargarEmpleados();

            LimpiarCampos();
        }

        // =========================================================
        // VALIDACIÓN
        // =========================================================

        private bool ValidarDatos()
        {
            string usuario =
                txtUsuario.Text.Trim();

            string contraseña =
                txtContraseña.Text;

            string nombre =
                txtNombre.Text.Trim();

            string cargo =
                cmbCargo.Text;

            if (!Validaciones.EsUsuarioValido(
                usuario))
            {
                MessageBox.Show(
                    "El usuario debe tener entre 3 y 30 caracteres y contener únicamente letras, números, punto, guion o guion bajo.",
                    "Usuario inválido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtUsuario.Focus();

                return false;
            }

            if (!Validaciones.EsContraseñaValida(
                contraseña))
            {
                MessageBox.Show(
                    "La contraseña debe tener entre 4 y 30 caracteres.",
                    "Contraseña inválida",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtContraseña.Focus();

                return false;
            }

            if (!Validaciones.EsNombreEmpleadoValido(
                nombre))
            {
                MessageBox.Show(
                    "Ingresa un nombre completo válido.",
                    "Nombre inválido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtNombre.Focus();

                return false;
            }

            if (!Validaciones.EsCargoValido(
                cargo))
            {
                MessageBox.Show(
                    "Selecciona un rol válido.",
                    "Rol inválido",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                cmbCargo.Focus();

                return false;
            }

            return true;
        }

        // =========================================================
        // SELECCIONAR USUARIO
        // =========================================================

        private void DgvEmpleados_CellClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            CargarEmpleadoSeleccionado(
                e.RowIndex);
        }

        private void DgvEmpleados_CellDoubleClick(
            object sender,
            DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            CargarEmpleadoSeleccionado(
                e.RowIndex);
        }

        private void CargarEmpleadoSeleccionado(
            int indice)
        {
            DataGridViewRow fila =
                dgvEmpleados.Rows[indice];

            if (fila.Tag is not Empleado empleado)
                return;

            usuarioSeleccionado =
                empleado.Usuario;

            txtUsuario.Text =
                empleado.Usuario;

            txtContraseña.Text =
                empleado.Contraseña;

            txtNombre.Text =
                empleado.NombreCompleto;

            if (empleado.EsUsuarioMaestro)
            {
                cmbCargo.SelectedIndex = -1;

                txtUsuario.ReadOnly = true;
                txtContraseña.ReadOnly = true;
                txtNombre.ReadOnly = true;
                cmbCargo.Enabled = false;

                btnModificar.Enabled = false;
                btnEliminar.Enabled = false;

                return;
            }

            txtUsuario.ReadOnly = false;
            txtContraseña.ReadOnly = false;
            txtNombre.ReadOnly = false;
            cmbCargo.Enabled = true;

            if (cmbCargo.Items.Contains(
                empleado.Cargo))
            {
                cmbCargo.SelectedItem =
                    empleado.Cargo;
            }
            else
            {
                cmbCargo.SelectedIndex = 2;
            }

            ActualizarEstadoControles();
        }

        // =========================================================
        // BÚSQUEDA
        // =========================================================

        private void TxtBuscar_TextChanged(
            object sender,
            EventArgs e)
        {
            CargarEmpleados(
                txtBuscar.Text.Trim());
        }

        // =========================================================
        // LIMPIAR
        // =========================================================

        private void BtnLimpiar_Click(
            object sender,
            EventArgs e)
        {
            LimpiarCampos();
        }

        private void LimpiarCampos()
        {
            usuarioSeleccionado =
                string.Empty;

            txtUsuario.ReadOnly = false;
            txtContraseña.ReadOnly = false;
            txtNombre.ReadOnly = false;

            cmbCargo.Enabled = true;

            txtUsuario.Clear();
            txtContraseña.Clear();
            txtNombre.Clear();

            cmbCargo.SelectedIndex = 2;

            dgvEmpleados.ClearSelection();

            ActualizarEstadoControles();

            txtUsuario.Focus();
        }

        // =========================================================
        // ESTADO DE BOTONES
        // =========================================================

        private void ActualizarEstadoControles()
        {
            bool puedeGestionar =
                SesionService.PuedeGestionarUsuarios();

            btnRegistrar.Enabled =
                puedeGestionar;

            btnModificar.Enabled =
                puedeGestionar &&
                !string.IsNullOrWhiteSpace(
                    usuarioSeleccionado) &&
                !usuarioSeleccionado.Equals(
                    "admin",
                    StringComparison.OrdinalIgnoreCase);

            btnEliminar.Enabled =
                puedeGestionar &&
                !string.IsNullOrWhiteSpace(
                    usuarioSeleccionado) &&
                !usuarioSeleccionado.Equals(
                    "admin",
                    StringComparison.OrdinalIgnoreCase);
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
        // TECLADO
        // =========================================================

        private void TextBox_KeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
            }
        }
    }
}
