using System;
using System.Globalization;
using System.Linq;

namespace FediCasa.Helpers
{
    public static class Validaciones
    {
        // =========================================================
        // TEXTOS GENERALES
        // =========================================================

        public static bool EsTextoValido(string texto)
        {
            return !string.IsNullOrWhiteSpace(texto);
        }

        public static bool TieneLongitudMinima(
            string texto,
            int longitudMinima)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return false;

            return texto.Trim().Length >= longitudMinima;
        }

        public static bool TieneLongitudMaxima(
            string texto,
            int longitudMaxima)
        {
            if (texto == null)
                return false;

            return texto.Trim().Length <= longitudMaxima;
        }

        // =========================================================
        // USUARIOS
        // =========================================================

        public static bool EsUsuarioValido(string usuario)
        {
            if (string.IsNullOrWhiteSpace(usuario))
                return false;

            usuario = usuario.Trim();

            if (usuario.Length < 3 || usuario.Length > 30)
                return false;

            // No se permite crear/modificar el usuario maestro
            // desde la gestión normal de empleados.
            if (usuario.Equals(
                    "admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            foreach (char caracter in usuario)
            {
                bool permitido =
                    char.IsLetterOrDigit(caracter) ||
                    caracter == '.' ||
                    caracter == '_' ||
                    caracter == '-';

                if (!permitido)
                    return false;
            }

            return true;
        }

        // =========================================================
        // CONTRASEÑAS
        // =========================================================

        public static bool EsContraseñaValida(string contraseña)
        {
            if (string.IsNullOrEmpty(contraseña))
                return false;

            return contraseña.Length >= 4 &&
                   contraseña.Length <= 30;
        }

        // =========================================================
        // NÚMEROS ENTEROS
        // =========================================================

        public static bool TryObtenerEntero(
            string texto,
            out int valor)
        {
            return int.TryParse(
                texto,
                out valor);
        }

        public static bool EsEnteroPositivo(int valor)
        {
            return valor > 0;
        }

        public static bool EsEnteroNoNegativo(int valor)
        {
            return valor >= 0;
        }

        // =========================================================
        // NÚMEROS DECIMALES
        // =========================================================

        public static bool TryObtenerDouble(
            string texto,
            out double valor)
        {
            return double.TryParse(
                texto,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out valor);
        }

        public static bool EsPrecioValido(double precio)
        {
            return precio > 0 &&
                   precio <= 999999.99;
        }

        public static bool EsPrecioValido(
            double precio,
            double minimo,
            double maximo)
        {
            return precio >= minimo &&
                   precio <= maximo;
        }

        // =========================================================
        // STOCK
        // =========================================================

        public static bool EsStockValido(int stock)
        {
            return stock >= 0 &&
                   stock <= 999999;
        }

        public static bool EsCantidadVentaValida(int cantidad)
        {
            return cantidad > 0 &&
                   cantidad <= 999999;
        }

        // =========================================================
        // PRODUCTOS
        // =========================================================

        public static bool EsCodigoProductoValido(string codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                return false;

            codigo = codigo.Trim();

            if (codigo.Length < 3 || codigo.Length > 20)
                return false;

            foreach (char caracter in codigo)
            {
                bool permitido =
                    char.IsLetterOrDigit(caracter) ||
                    caracter == '-' ||
                    caracter == '_';

                if (!permitido)
                    return false;
            }

            return true;
        }

        public static bool EsNombreProductoValido(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return false;

            nombre = nombre.Trim();

            return nombre.Length >= 3 &&
                   nombre.Length <= 100;
        }

        // =========================================================
        // EMPLEADOS
        // =========================================================

        public static bool EsNombreEmpleadoValido(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return false;

            nombre = nombre.Trim();

            if (nombre.Length < 3 || nombre.Length > 80)
                return false;

            foreach (char caracter in nombre)
            {
                if (char.IsLetter(caracter) ||
                    char.IsWhiteSpace(caracter) ||
                    caracter == '.' ||
                    caracter == '-' ||
                    caracter == '\'')
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida únicamente los cargos que representan actores
        /// reales del CUS de FediCasa.
        ///
        /// IMPORTANTE:
        /// "Admin" NO es un cargo. Es el usuario maestro.
        /// </summary>
        public static bool EsCargoValido(string cargo)
        {
            if (string.IsNullOrWhiteSpace(cargo))
                return false;

            string cargoLimpio = cargo.Trim();

            string[] cargosPermitidos =
            {
                "Administrador",
                "Inventario",
                "Ventas"
            };

            return cargosPermitidos.Any(c =>
                c.Equals(
                    cargoLimpio,
                    StringComparison.OrdinalIgnoreCase));
        }

        // =========================================================
        // ARCHIVOS
        // =========================================================

        public static bool EsTextoCompatibleConArchivo(
            string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return true;

            return !texto.Contains('|') &&
                   !texto.Contains('\r') &&
                   !texto.Contains('\n');
        }

        public static string LimpiarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            return texto.Trim();
        }

        // =========================================================
        // VENTAS
        // =========================================================

        public static bool EsTotalVentaValido(double total)
        {
            return total > 0 &&
                   total <= 999999999.99;
        }

        // =========================================================
        // MENSAJES DE VALIDACIÓN
        // =========================================================

        public static string MensajeCampoObligatorio(
            string nombreCampo)
        {
            return $"El campo '{nombreCampo}' es obligatorio.";
        }

        public static string MensajeValorInvalido(
            string nombreCampo)
        {
            return $"El valor ingresado para '{nombreCampo}' no es válido.";
        }
    }
}
