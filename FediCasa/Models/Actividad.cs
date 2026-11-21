using System;
using System.Globalization;

namespace FediCasa.Models
{
    /// <summary>
    /// Acción administrativa realizada por un usuario (productos,
    /// usuarios, revisión de solicitudes). Se usa para los reportes
    /// de turno del Administrador y del usuario maestro.
    /// </summary>
    public class Actividad
    {
        public DateTime Fecha { get; set; }
        public string Usuario { get; set; }
        public string Accion { get; set; }
        public string Detalle { get; set; }

        public Actividad()
        {
            Fecha = DateTime.Now;
            Usuario = string.Empty;
            Accion = string.Empty;
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
                Limpiar(Usuario),
                Limpiar(Accion),
                Limpiar(Detalle));
        }

        public static Actividad DesdeTexto(string linea)
        {
            if (string.IsNullOrWhiteSpace(linea))
                return null;

            string[] p = linea.Split('|');

            if (p.Length < 4)
                return null;

            if (!DateTime.TryParse(
                    p[0],
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out DateTime fecha))
                return null;

            return new Actividad
            {
                Fecha = fecha,
                Usuario = p[1],
                Accion = p[2],
                Detalle = p[3]
            };
        }
    }
}
