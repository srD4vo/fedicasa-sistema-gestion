using System;
using System.Globalization;

namespace FediCasa.Models
{
    /// <summary>
    /// Solicitud o incidencia creada por un trabajador y revisada
    /// por el Administrador (o derivada al usuario maestro).
    /// </summary>
    public class Solicitud
    {
        // Tipos
        public const string Ingreso = "Ingreso";
        public const string Salida = "Salida";
        public const string Diferencia = "Diferencia";
        public const string CambioProducto = "Cambio de producto";
        public const string Incidencia = "Incidencia";

        // Estados
        public const string Pendiente = "Pendiente";
        public const string Aprobada = "Aprobada";
        public const string Rechazada = "Rechazada";
        public const string Derivada = "Derivada a admin";
        public const string Resuelta = "Resuelta";

        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; }
        public string CodigoProducto { get; set; }
        public int Cantidad { get; set; }
        public string Motivo { get; set; }
        public string Estado { get; set; }
        public string Solicitante { get; set; }
        public string Revisor { get; set; }
        public string Comentario { get; set; }

        public Solicitud()
        {
            Fecha = DateTime.Now;
            Tipo = string.Empty;
            CodigoProducto = string.Empty;
            Motivo = string.Empty;
            Estado = Pendiente;
            Solicitante = string.Empty;
            Revisor = string.Empty;
            Comentario = string.Empty;
        }

        // Ingreso, Salida y Diferencia modifican el stock al aprobarse.
        public bool EsOperacionDeStock()
        {
            return Tipo == Ingreso ||
                   Tipo == Salida ||
                   Tipo == Diferencia;
        }

        public bool EstaAbierta()
        {
            return Estado == Pendiente || Estado == Derivada;
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
                Id,
                Fecha.ToString("O", CultureInfo.InvariantCulture),
                Limpiar(Tipo),
                Limpiar(CodigoProducto),
                Cantidad,
                Limpiar(Motivo),
                Limpiar(Estado),
                Limpiar(Solicitante),
                Limpiar(Revisor),
                Limpiar(Comentario));
        }

        public static Solicitud DesdeTexto(string linea)
        {
            if (string.IsNullOrWhiteSpace(linea))
                return null;

            string[] p = linea.Split('|');

            if (p.Length < 10)
                return null;

            if (!int.TryParse(p[0], out int id) ||
                !int.TryParse(p[4], out int cantidad) ||
                !DateTime.TryParse(
                    p[1],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out DateTime fecha))
                return null;

            return new Solicitud
            {
                Id = id,
                Fecha = fecha,
                Tipo = p[2],
                CodigoProducto = p[3],
                Cantidad = cantidad,
                Motivo = p[5],
                Estado = p[6],
                Solicitante = p[7],
                Revisor = p[8],
                Comentario = p[9]
            };
        }
    }
}
