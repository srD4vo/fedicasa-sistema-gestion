using System;
using System.Linq;
using FediCasa.Helpers;
using FediCasa.Models;

namespace FediCasa.Services
{
    public static class SesionService
    {
        // =========================================================
        // SESIÓN ACTUAL
        // =========================================================

        public static Empleado EmpleadoActual { get; private set; }

        public static bool SesionActiva
        {
            get
            {
                return EmpleadoActual != null;
            }
        }

        // =========================================================
        // INICIALIZACIÓN
        // =========================================================

        public static void Inicializar()
        {
            ArchivoHelper.InicializarCarpetaData();
            ArchivoHelper.CrearEmpleadosIniciales();
        }

        // =========================================================
        // INICIAR SESIÓN
        // =========================================================

        public static bool IniciarSesion(
            string usuario,
            string contraseña)
        {
            Inicializar();

            if (string.IsNullOrWhiteSpace(usuario) ||
                string.IsNullOrWhiteSpace(contraseña))
            {
                return false;
            }

            var empleados = ArchivoHelper.LeerEmpleados();

            Empleado empleado = empleados.FirstOrDefault(e =>
                e.Usuario.Equals(
                    usuario.Trim(),
                    StringComparison.OrdinalIgnoreCase)
                &&
                e.Contraseña == contraseña);

            if (empleado == null)
            {
                EmpleadoActual = null;
                return false;
            }

            EmpleadoActual = empleado;

            return true;
        }

        // =========================================================
        // CERRAR SESIÓN
        // =========================================================

        public static void CerrarSesion()
        {
            EmpleadoActual = null;
        }

        // =========================================================
        // INFORMACIÓN DEL USUARIO ACTUAL
        // =========================================================

        public static Empleado ObtenerEmpleadoActual()
        {
            return EmpleadoActual;
        }

        public static string ObtenerNombreEmpleadoActual()
        {
            if (!SesionActiva)
                return string.Empty;

            return EmpleadoActual.NombreCompleto;
        }

        public static string ObtenerCargoEmpleadoActual()
        {
            if (!SesionActiva)
                return string.Empty;

            return EmpleadoActual.Cargo;
        }

        public static string ObtenerUsuarioActual()
        {
            if (!SesionActiva)
                return string.Empty;

            return EmpleadoActual.Usuario;
        }

        // =========================================================
        // IDENTIFICACIÓN DE PERFIL
        // =========================================================

        /// <summary>
        /// Determina si el usuario actual es el usuario maestro Admin.
        /// El usuario maestro tiene acceso total al sistema.
        /// </summary>
        public static bool EsUsuarioMaestro()
        {
            return SesionActiva &&
                   EmpleadoActual.EsUsuarioMaestro;
        }

        /// <summary>
        /// Determina si el usuario actual es el actor Administrador
        /// del CUS.
        ///
        /// IMPORTANTE:
        /// Un usuario maestro también tiene cargo Administrador,
        /// pero no debe confundirse con este actor.
        /// </summary>
        public static bool EsAdministrador()
        {
            return SesionActiva &&
                   !EmpleadoActual.EsUsuarioMaestro &&
                   EmpleadoActual.Cargo.Equals(
                       "Administrador",
                       StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determina si el usuario actual pertenece al área
        /// de Inventario.
        /// </summary>
        public static bool EsInventario()
        {
            return SesionActiva &&
                   !EmpleadoActual.EsUsuarioMaestro &&
                   EmpleadoActual.Cargo.Equals(
                       "Inventario",
                       StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determina si el usuario actual pertenece al área
        /// de Ventas.
        /// </summary>
        public static bool EsVentas()
        {
            return SesionActiva &&
                   !EmpleadoActual.EsUsuarioMaestro &&
                   EmpleadoActual.Cargo.Equals(
                       "Ventas",
                       StringComparison.OrdinalIgnoreCase);
        }

        // =========================================================
        // PERMISOS GENERALES
        // =========================================================

        /// <summary>
        /// El usuario maestro puede acceder a cualquier módulo.
        /// </summary>
        public static bool TieneAccesoTotal()
        {
            return EsUsuarioMaestro();
        }

        // =========================================================
        // PERMISOS DE CASOS DE USO
        // =========================================================

        // ---------------------------------------------------------
        // CU-02 Gestionar usuarios
        // ---------------------------------------------------------

        public static bool PuedeGestionarUsuarios()
        {
            return EsUsuarioMaestro() ||
                   EsAdministrador();
        }

        // ---------------------------------------------------------
        // CU-03 Gestionar productos
        // ---------------------------------------------------------

        public static bool PuedeGestionarProductos()
        {
            return EsUsuarioMaestro() ||
                   EsAdministrador();
        }

        // ---------------------------------------------------------
        // CU-04 Consultar inventario
        // ---------------------------------------------------------

        public static bool PuedeConsultarInventario()
        {
            return EsUsuarioMaestro() ||
                   EsInventario() ||
                   EsVentas();
        }

        // ---------------------------------------------------------
        // CU-05 Registrar ingreso de productos
        // ---------------------------------------------------------

        public static bool PuedeRegistrarIngreso()
        {
            return EsUsuarioMaestro() ||
                   EsInventario();
        }

        // ---------------------------------------------------------
        // CU-06 Registrar salida de productos
        // ---------------------------------------------------------

        public static bool PuedeRegistrarSalida()
        {
            return EsUsuarioMaestro() ||
                   EsInventario();
        }

        // ---------------------------------------------------------
        // CU-07 Actualizar existencias
        // ---------------------------------------------------------

        public static bool PuedeActualizarExistencias()
        {
            return EsUsuarioMaestro() ||
                   EsInventario();
        }

        // ---------------------------------------------------------
        // CU-08 Gestionar diferencias de inventario
        // ---------------------------------------------------------

        public static bool PuedeGestionarDiferencias()
        {
            return EsUsuarioMaestro() ||
                   EsInventario();
        }

        // ---------------------------------------------------------
        // CU-09 Consultar movimientos de inventario
        // ---------------------------------------------------------

        public static bool PuedeConsultarMovimientos()
        {
            return EsUsuarioMaestro() ||
                   EsInventario();
        }

        // ---------------------------------------------------------
        // CU-10 Gestionar clientes
        // ---------------------------------------------------------

        public static bool PuedeGestionarClientes()
        {
            return EsUsuarioMaestro() ||
                   EsVentas();
        }

        // ---------------------------------------------------------
        // CU-11 Gestionar proformas
        // ---------------------------------------------------------

        public static bool PuedeGestionarProformas()
        {
            return EsUsuarioMaestro() ||
                   EsVentas();
        }

        // ---------------------------------------------------------
        // CU-12 Convertir proforma en venta
        // ---------------------------------------------------------

        public static bool PuedeConvertirProforma()
        {
            return EsUsuarioMaestro() ||
                   EsVentas();
        }

        // ---------------------------------------------------------
        // CU-13 Gestionar ventas
        // ---------------------------------------------------------

        public static bool PuedeGestionarVentas()
        {
            return EsUsuarioMaestro() ||
                   EsVentas();
        }

        // ---------------------------------------------------------
        // CU-14 Confirmar venta
        // ---------------------------------------------------------

        public static bool PuedeConfirmarVenta()
        {
            return EsUsuarioMaestro() ||
                   EsVentas();
        }

        // Anular venta (registrada o confirmada)
        public static bool PuedeAnularVenta()
        {
            return EsUsuarioMaestro() ||
                   EsVentas();
        }

        // ---------------------------------------------------------
        // CU-15 Generar comprobante de venta
        // ---------------------------------------------------------

        public static bool PuedeGenerarComprobante()
        {
            return EsUsuarioMaestro() ||
                   EsVentas();
        }

        // ---------------------------------------------------------
        // CU-16 Gestionar entregas
        // ---------------------------------------------------------

        public static bool PuedeGestionarEntregas()
        {
            return EsUsuarioMaestro() ||
                   EsVentas();
        }

        // ---------------------------------------------------------
        // CU-17 Generar reportes
        // ---------------------------------------------------------

        public static bool PuedeGenerarReportes()
        {
            return EsUsuarioMaestro() ||
                   EsAdministrador();
        }

        // ---------------------------------------------------------
        // CU-18 Consultar historial de operaciones
        // ---------------------------------------------------------

        public static bool PuedeConsultarHistorial()
        {
            return EsUsuarioMaestro() ||
                   EsAdministrador();
        }
    }
}

