using System;
using System.Globalization;

namespace FediCasa.Models
{
    /// <summary>
    /// Registro del historial de operaciones: ingresos, salidas,
    /// ajustes, incidencias (diferencias de inventario) y ventas.
    /// </summary>
    public class Movimiento
    {
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public string CodigoProducto { get; set; }
        public string NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public int StockResultante { get; set; }
        public string Usuario { get; set; }
        public string Detalle { get; set; }

        public Movimiento()
        {
            Fecha = DateTime.Now;
            Tipo = string.Empty;
            CodigoProducto = string.Empty;
            NombreProducto = string.Empty;
            Usuario = string.Empty;
            Detalle = string.Empty;
        }

        private static string Limpiar(string texto)
        {
            if (string.IsNullOrEmpty(texto))
                return string.Empty;

            return texto.Replace("|", "/")
                        .Replace("\r", " ")
                        .Replace("\n", " ");
        }

        public override string ToString()
        {
            return string.Join(
                "|",
                Fecha.ToString("O", CultureInfo.InvariantCulture),
                Limpiar(Tipo),
                Limpiar(CodigoProducto),
                Limpiar(NombreProducto),
                Cantidad,
                StockResultante,
                Limpiar(Usuario),
                Limpiar(Detalle));
        }

        public static Movimiento DesdeTexto(string linea)
        {
            if (string.IsNullOrWhiteSpace(linea))
                return null;

            string[] p = linea.Split('|');

            if (p.Length < 8)
                return null;

            if (!DateTime.TryParse(
                    p[0],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out DateTime fecha))
                return null;

            if (!int.TryParse(p[4], out int cantidad) ||
                !int.TryParse(p[5], out int stock))
                return null;

            return new Movimiento
            {
                Fecha = fecha,
                Tipo = p[1],
                CodigoProducto = p[2],
                NombreProducto = p[3],
                Cantidad = cantidad,
                StockResultante = stock,
                Usuario = p[6],
                Detalle = p[7]
            };
        }
    }
}
