using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using FediCasa.Models;
using FediCasa.Services;

namespace FediCasa.Helpers
{
    public static class BoletaHelper
    {
        // =========================================================
        // DATOS DEL NEGOCIO
        // =========================================================

        public const string NombreNegocio =
            "FediCasa";

        public const string RucNegocio =
            "RUC: 10458963271";

        public const string DireccionNegocio =
            "Av. Las Palmeras 5201, Los Olivos 15304";

        public const string TelefonoNegocio =
            "Cel.: 998 186 179";

        public const double FactorIGV =
            1.18;

        // =========================================================
        // RUTAS
        // =========================================================

        private static string ObtenerCarpetaBoletas()
        {
            string carpeta =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Data",
                    "Boletas");

            Directory.CreateDirectory(
                carpeta);

            return carpeta;
        }

        public static string ObtenerRutaHistorial()
        {
            return Path.Combine(
                ObtenerCarpetaBoletas(),
                "boletas_historial.txt");
        }

        // =========================================================
        // CÁLCULO DE IGV
        // =========================================================

        public static void CalcularDesgloseIGV(
            double total,
            out double valorVenta,
            out double igv)
        {
            total =
                Math.Round(total, 2);

            valorVenta =
                Math.Round(
                    total / FactorIGV,
                    2);

            igv =
                Math.Round(
                    total - valorVenta,
                    2);
        }

        // =========================================================
        // NÚMERO DE BOLETA
        // =========================================================

        public static string GenerarNumeroBoleta(
            int idVentaBase)
        {
            return
                $"B001-{idVentaBase:00000000}";
        }

        // =========================================================
        // GENERAR BOLETA COMPLETA
        // =========================================================

        public static string GenerarContenidoBoleta(
            List<Venta> ventas,
            double totalVenta,
            string numeroBoleta = "")
        {
            if (ventas == null ||
                ventas.Count == 0)
            {
                return string.Empty;
            }

            Venta primeraVenta =
                ventas.First();

            totalVenta =
                Math.Round(
                    totalVenta,
                    2);

            CalcularDesgloseIGV(
                totalVenta,
                out double valorVenta,
                out double igv);

            if (string.IsNullOrWhiteSpace(
                numeroBoleta))
            {
                numeroBoleta =
                    GenerarNumeroBoleta(
                        primeraVenta.IdVenta);
            }

            StringBuilder sb =
                new StringBuilder();

            const int ancho = 64;

            sb.AppendLine(
                Centrar(
                    NombreNegocio,
                    ancho));

            sb.AppendLine(
                Centrar(
                    RucNegocio,
                    ancho));

            sb.AppendLine(
                Centrar(
                    DireccionNegocio,
                    ancho));

            sb.AppendLine(
                Centrar(
                    TelefonoNegocio,
                    ancho));

            sb.AppendLine(
                new string(
                    '=',
                    ancho));

            sb.AppendLine(
                Centrar(
                    "BOLETA DE VENTA",
                    ancho));

            sb.AppendLine(
                new string(
                    '=',
                    ancho));

            sb.AppendLine(
                $"N.° COMPROBANTE : {numeroBoleta}");

            sb.AppendLine(
                $"FECHA           : {primeraVenta.Fecha:dd/MM/yyyy HH:mm:ss}");

            sb.AppendLine(
                $"VENDEDOR        : {ObtenerTextoSeguro(primeraVenta.NombreVendedor)}");

            sb.AppendLine(
                $"USUARIO         : {ObtenerTextoSeguro(primeraVenta.UsuarioVendedor)}");

            sb.AppendLine();

            sb.AppendLine(
                "CLIENTE");

            sb.AppendLine(
                $"Nombre          : {ObtenerTextoSeguro(primeraVenta.NombreCliente)}");

            if (!string.IsNullOrWhiteSpace(
                primeraVenta.DocumentoCliente))
            {
                sb.AppendLine(
                    $"Documento       : {primeraVenta.DocumentoCliente}");
            }
            else
            {
                sb.AppendLine(
                    "Documento       : No registrado");
            }

            sb.AppendLine(
                $"Forma de pago   : {ObtenerTextoSeguro(primeraVenta.FormaPago)}");

            sb.AppendLine();

            sb.AppendLine(
                new string(
                    '-',
                    ancho));

            sb.AppendLine(
                "DETALLE DE LA VENTA");

            sb.AppendLine(
                new string(
                    '-',
                    ancho));

            sb.AppendLine(
                FormatearLinea(
                    "COD.",
                    "PRODUCTO",
                    "CANT.",
                    "P.UNIT.",
                    "IMPORTE"));

            sb.AppendLine(
                new string(
                    '-',
                    ancho));

            foreach (Venta venta in ventas)
            {
                string codigo =
                    LimitarTexto(
                        venta.CodigoProducto,
                        8);

                string nombre =
                    LimitarTexto(
                        venta.NombreProducto,
                        24);

                string cantidad =
                    venta.Cantidad.ToString();

                string precio =
                    venta.Precio.ToString(
                        "0.00",
                        CultureInfo.InvariantCulture);

                string importe =
                    venta.Subtotal.ToString(
                        "0.00",
                        CultureInfo.InvariantCulture);

                sb.AppendLine(
                    FormatearLinea(
                        codigo,
                        nombre,
                        cantidad,
                        precio,
                        importe));
            }

            sb.AppendLine(
                new string(
                    '-',
                    ancho));

            sb.AppendLine();

            sb.AppendLine(
                $"{"OP. GRAVADA",-22}: " +
                $"S/ {valorVenta,12:0.00}");

            sb.AppendLine(
                $"{"IGV (18%)",-22}: " +
                $"S/ {igv,12:0.00}");

            sb.AppendLine(
                $"{"TOTAL A PAGAR",-22}: " +
                $"S/ {totalVenta,12:0.00}");

            sb.AppendLine();

            sb.AppendLine(
                new string(
                    '=',
                    ancho));

            sb.AppendLine(
                Centrar(
                    "Gracias por su compra",
                    ancho));

            sb.AppendLine(
                Centrar(
                    "FediCasa - Ferretería",
                    ancho));

            sb.AppendLine(
                new string(
                    '=',
                    ancho));

            return sb.ToString();
        }

        // =========================================================
        // SOBRECARGA PARA UNA SOLA VENTA
        // =========================================================

        public static string GenerarContenidoBoleta(
            Venta venta,
            double totalVenta)
        {
            if (venta == null)
                return string.Empty;

            List<Venta> ventas =
                new List<Venta>
                {
                    venta
                };

            return GenerarContenidoBoleta(
                ventas,
                totalVenta);
        }

        // =========================================================
        // GUARDAR BOLETA
        // =========================================================

        public static string GuardarBoleta(
            List<Venta> ventas,
            double totalVenta)
        {
            if (ventas == null ||
                ventas.Count == 0)
            {
                return string.Empty;
            }

            int idVenta =
                ventas.First().IdVenta;

            string numeroBoleta =
                GenerarNumeroBoleta(
                    idVenta);

            string contenido =
                GenerarContenidoBoleta(
                    ventas,
                    totalVenta,
                    numeroBoleta);

            string nombreArchivo =
                $"Boleta_{idVenta:00000000}.txt";

            string ruta =
                Path.Combine(
                    ObtenerCarpetaBoletas(),
                    nombreArchivo);

            File.WriteAllText(
                ruta,
                contenido,
                Encoding.UTF8);

            GuardarEnHistorial(
                ventas,
                totalVenta,
                numeroBoleta);

            return ruta;
        }

        // =========================================================
        // HISTORIAL DE BOLETAS
        // =========================================================

        public static void GuardarEnHistorial(
            List<Venta> ventas,
            double totalVenta,
            string numeroBoleta)
        {
            if (ventas == null ||
                ventas.Count == 0)
            {
                return;
            }

            Venta primeraVenta =
                ventas.First();

            string ruta =
                ObtenerRutaHistorial();

            StringBuilder sb =
                new StringBuilder();

            if (!File.Exists(ruta))
            {
                sb.AppendLine(
                    "HISTORIAL DE BOLETAS - FEDICASA");

                sb.AppendLine(
                    $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");

                sb.AppendLine(
                    new string(
                        '=',
                        100));
            }

            sb.AppendLine(
                $"Boleta: {numeroBoleta}");

            sb.AppendLine(
                $"Venta: {primeraVenta.IdVenta}");

            sb.AppendLine(
                $"Fecha: {primeraVenta.Fecha:dd/MM/yyyy HH:mm:ss}");

            sb.AppendLine(
                $"Cliente: {ObtenerTextoSeguro(primeraVenta.NombreCliente)}");

            sb.AppendLine(
                $"Documento: " +
                $"{(string.IsNullOrWhiteSpace(primeraVenta.DocumentoCliente) ? "No registrado" : primeraVenta.DocumentoCliente)}");

            sb.AppendLine(
                $"Vendedor: {ObtenerTextoSeguro(primeraVenta.NombreVendedor)}");

            sb.AppendLine(
                $"Forma de pago: {ObtenerTextoSeguro(primeraVenta.FormaPago)}");

            sb.AppendLine(
                $"Cantidad de productos: {ventas.Count}");

            sb.AppendLine(
                $"Total: S/ {totalVenta:0.00}");

            sb.AppendLine(
                new string(
                    '-',
                    100));

            foreach (Venta venta in ventas)
            {
                sb.AppendLine(
                    $"{venta.CodigoProducto} | " +
                    $"{venta.NombreProducto} | " +
                    $"Cantidad: {venta.Cantidad} | " +
                    $"Precio: S/ {venta.Precio:0.00} | " +
                    $"Subtotal: S/ {venta.Subtotal:0.00}");
            }

            sb.AppendLine(
                new string(
                    '=',
                    100));

            File.AppendAllText(
                ruta,
                sb.ToString(),
                Encoding.UTF8);
        }

        // =========================================================
        // COMPATIBILIDAD
        // =========================================================

        public static void GuardarEnHistorial(
            Venta venta,
            double totalVenta)
        {
            if (venta == null)
                return;

            GuardarEnHistorial(
                new List<Venta>
                {
                    venta
                },
                totalVenta,
                GenerarNumeroBoleta(
                    venta.IdVenta));
        }

        // =========================================================
        // LECTURA DEL HISTORIAL
        // =========================================================

        public static string LeerHistorial()
        {
            string ruta =
                ObtenerRutaHistorial();

            if (!File.Exists(ruta))
            {
                return
                    "Todavía no existen boletas registradas.";
            }

            return File.ReadAllText(
                ruta,
                Encoding.UTF8);
        }

        // =========================================================
        // UTILIDADES DE FORMATO
        // =========================================================

        private static string Centrar(
            string texto,
            int ancho)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            if (texto.Length >= ancho)
                return texto;

            int espacios =
                (ancho - texto.Length) / 2;

            return
                new string(
                    ' ',
                    espacios) +
                texto;
        }

        private static string LimitarTexto(
            string texto,
            int maximo)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            texto = texto.Trim();

            if (texto.Length <= maximo)
                return texto;

            if (maximo <= 3)
                return texto.Substring(
                    0,
                    maximo);

            return
                texto.Substring(
                    0,
                    maximo - 3) +
                "...";
        }

        private static string FormatearLinea(
            string codigo,
            string producto,
            string cantidad,
            string precio,
            string importe)
        {
            return
                $"{codigo,-8} " +
                $"{producto,-24} " +
                $"{cantidad,5} " +
                $"{precio,10} " +
                $"{importe,10}";
        }

        private static string ObtenerTextoSeguro(
            string texto)
        {
            return string.IsNullOrWhiteSpace(texto)
                ? "-"
                : texto.Trim();
        }
    }
}
