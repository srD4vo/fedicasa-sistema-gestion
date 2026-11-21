using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using FediCasa.Models;
using FediCasa.Services;

namespace FediCasa.Helpers
{
    /// <summary>
    /// Genera y guarda proformas (cotizaciones) de FediCasa.
    ///
    /// Una proforma NO es una venta: no descuenta stock, no se
    /// registra en ventas.txt y no es un comprobante de pago.
    /// Se guarda como archivo de texto en Data/Proformas.
    /// </summary>
    public static class ProformaHelper
    {
        // Días que la cotización mantiene sus precios.
        public const int DiasValidez = 7;

        private const int Ancho = 68;

        // =========================================================
        // RUTAS
        // =========================================================

        public static string ObtenerCarpetaProformas()
        {
            string carpeta =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Data",
                    "Proformas");

            Directory.CreateDirectory(carpeta);

            return carpeta;
        }

        // =========================================================
        // NUMERACIÓN (según los archivos ya guardados)
        // =========================================================

        private static int ObtenerSiguienteNumero()
        {
            int maximo = 0;

            foreach (string archivo in Directory.GetFiles(
                ObtenerCarpetaProformas(),
                "Proforma_*.txt"))
            {
                string nombre =
                    Path.GetFileNameWithoutExtension(archivo);

                string parte =
                    nombre.Substring("Proforma_".Length);

                if (int.TryParse(parte, out int numero) &&
                    numero > maximo)
                {
                    maximo = numero;
                }
            }

            return maximo + 1;
        }

        public static string FormatearNumero(int numero)
        {
            return "P001-" + numero.ToString("00000000");
        }

        // =========================================================
        // GUARDAR PROFORMA
        // =========================================================

        /// <summary>
        /// Crea el documento a partir de los productos del carrito,
        /// lo guarda y devuelve la ruta del archivo.
        /// </summary>
        public static string GuardarProforma(
            List<ItemCarrito> items,
            string documentoCliente,
            string nombreCliente,
            out string contenido)
        {
            if (items == null || items.Count == 0)
            {
                throw new InvalidOperationException(
                    "No hay productos para la proforma.");
            }

            int numero = ObtenerSiguienteNumero();

            contenido =
                GenerarContenido(
                    items,
                    documentoCliente,
                    nombreCliente,
                    numero,
                    DateTime.Now);

            string ruta =
                Path.Combine(
                    ObtenerCarpetaProformas(),
                    "Proforma_" + numero.ToString("00000000") + ".txt");

            File.WriteAllText(ruta, contenido, Encoding.UTF8);

            return ruta;
        }

        public static string LeerProforma(string ruta)
        {
            return File.ReadAllText(ruta, Encoding.UTF8);
        }

        // =========================================================
        // CONTENIDO DEL DOCUMENTO
        // =========================================================

        public static string GenerarContenido(
            List<ItemCarrito> items,
            string documentoCliente,
            string nombreCliente,
            int numero,
            DateTime fecha)
        {
            StringBuilder sb = new StringBuilder();

            string doble = new string('=', Ancho);
            string simple = new string('-', Ancho);

            double total = 0;

            foreach (ItemCarrito item in items)
                total += item.Subtotal;

            total = Math.Round(total, 2);

            BoletaHelper.CalcularDesgloseIGV(
                total,
                out double valorVenta,
                out double igv);

            // ----- Encabezado del negocio -----
            sb.AppendLine(Centrar(BoletaHelper.NombreNegocio.ToUpper() +
                                  " - FERRETERÍA"));
            sb.AppendLine(Centrar(BoletaHelper.RucNegocio));
            sb.AppendLine(Centrar(BoletaHelper.DireccionNegocio));
            sb.AppendLine(Centrar(BoletaHelper.TelefonoNegocio));
            sb.AppendLine(doble);
            sb.AppendLine(Centrar("PROFORMA / COTIZACIÓN"));
            sb.AppendLine(Centrar("N.° " + FormatearNumero(numero)));
            sb.AppendLine(doble);
            sb.AppendLine();

            // ----- Datos generales -----
            sb.AppendLine("FECHA DE EMISIÓN : " +
                          fecha.ToString("dd/MM/yyyy HH:mm"));
            sb.AppendLine("VÁLIDA HASTA     : " +
                          fecha.AddDays(DiasValidez).ToString("dd/MM/yyyy") +
                          " (" + DiasValidez + " días)");
            sb.AppendLine("VENDEDOR         : " +
                          Seguro(SesionService.ObtenerNombreEmpleadoActual()));
            sb.AppendLine();

            sb.AppendLine("CLIENTE");
            sb.AppendLine("Nombre           : " +
                          (string.IsNullOrWhiteSpace(nombreCliente)
                              ? "Cliente general"
                              : nombreCliente.Trim()));
            sb.AppendLine("Documento        : " +
                          (string.IsNullOrWhiteSpace(documentoCliente)
                              ? "No registrado"
                              : documentoCliente.Trim()));
            sb.AppendLine();

            // ----- Detalle -----
            sb.AppendLine(simple);
            sb.AppendLine("DETALLE DE LA COTIZACIÓN");
            sb.AppendLine(simple);
            sb.AppendLine(Linea("ÍT.", "CÓDIGO", "DESCRIPCIÓN",
                                "CANT.", "P.UNIT.", "IMPORTE"));
            sb.AppendLine(simple);

            int n = 1;

            foreach (ItemCarrito item in items)
            {
                sb.AppendLine(Linea(
                    n.ToString(),
                    Limitar(item.CodigoProducto, 8),
                    Limitar(item.NombreProducto, 26),
                    item.Cantidad.ToString(),
                    item.Precio.ToString("0.00", CultureInfo.InvariantCulture),
                    item.Subtotal.ToString("0.00", CultureInfo.InvariantCulture)));

                n++;
            }

            sb.AppendLine(simple);
            sb.AppendLine();

            // ----- Totales -----
            sb.AppendLine("OP. GRAVADA".PadRight(22) + ": S/ " +
                          valorVenta.ToString("0.00").PadLeft(12));
            sb.AppendLine("IGV (18%)".PadRight(22) + ": S/ " +
                          igv.ToString("0.00").PadLeft(12));
            sb.AppendLine("TOTAL COTIZADO".PadRight(22) + ": S/ " +
                          total.ToString("0.00").PadLeft(12));
            sb.AppendLine();

            // ----- Condiciones -----
            sb.AppendLine(simple);
            sb.AppendLine("CONDICIONES");
            sb.AppendLine("- Los precios incluyen IGV.");
            sb.AppendLine("- Cotización válida por " + DiasValidez +
                          " días desde su emisión.");
            sb.AppendLine("- Sujeta a disponibilidad de stock.");
            sb.AppendLine("- Este documento no es un comprobante de pago.");
            sb.AppendLine(simple);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine(Centrar("______________________________"));
            sb.AppendLine(Centrar("Vendedor / Sello"));
            sb.AppendLine();
            sb.AppendLine(doble);
            sb.AppendLine(Centrar("Gracias por su preferencia"));
            sb.AppendLine(doble);

            return sb.ToString();
        }

        // =========================================================
        // UTILIDADES DE FORMATO
        // =========================================================

        private static string Linea(
            string item,
            string codigo,
            string descripcion,
            string cantidad,
            string precio,
            string importe)
        {
            return item.PadLeft(3) + " " +
                   codigo.PadRight(8) + " " +
                   descripcion.PadRight(26) + " " +
                   cantidad.PadLeft(5) + " " +
                   precio.PadLeft(10) + " " +
                   importe.PadLeft(11);
        }

        private static string Centrar(string texto)
        {
            if (texto.Length >= Ancho)
                return texto;

            return new string(' ', (Ancho - texto.Length) / 2) + texto;
        }

        private static string Limitar(string texto, int maximo)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return string.Empty;

            texto = texto.Trim();

            return texto.Length <= maximo
                ? texto
                : texto.Substring(0, maximo - 3) + "...";
        }

        private static string Seguro(string texto)
        {
            return string.IsNullOrWhiteSpace(texto) ? "-" : texto.Trim();
        }
    }
}
