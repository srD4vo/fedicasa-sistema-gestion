using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using FediCasa.Models;

namespace FediCasa.Helpers
{
    /// <summary>
    /// Clase auxiliar encargada de generar y consultar
    /// los reportes del sistema FediCasa.
    ///
    /// Los reportes se almacenan como archivos TXT dentro
    /// de la carpeta Data/Reportes.
    ///
    /// Esta clase no utiliza consola.
    /// La interfaz gráfica será responsabilidad de FrmReportes.
    /// </summary>
    public static class ReporteHelper
    {
        // ─────────────────────────────────────────────
        // CONFIGURACIÓN
        // ─────────────────────────────────────────────

        private const int LineasPorPagina = 30;

        private static readonly string CarpetaReportes =
            Path.Combine(
                AppContext.BaseDirectory,
                "Data",
                "Reportes"
            );


        // ─────────────────────────────────────────────
        // INICIALIZACIÓN
        // ─────────────────────────────────────────────

        /// <summary>
        /// Crea la carpeta de reportes si todavía no existe.
        /// </summary>
        private static void InicializarCarpetaReportes()
        {
            if (!Directory.Exists(CarpetaReportes))
            {
                Directory.CreateDirectory(
                    CarpetaReportes
                );
            }
        }


        // ─────────────────────────────────────────────
        // GENERAR REPORTE COMPLETO
        // ─────────────────────────────────────────────

        /// <summary>
        /// Genera un reporte completo de FediCasa.
        ///
        /// El reporte incluye:
        /// - Datos del negocio.
        /// - Fecha y hora de generación.
        /// - Empleado que genera el reporte.
        /// - Total de ventas.
        /// - Importe total vendido.
        /// - Producto más vendido.
        /// - Top 3 productos por ingresos.
        /// - Productos con bajo stock.
        /// - Inventario activo.
        /// - Detalle de ventas.
        ///
        /// Devuelve la ruta del archivo generado.
        /// </summary>
        public static string GenerarReporteCompleto(
            List<Venta> historialVentas,
            List<Producto> productos,
            Empleado generadoPor)
        {
            InicializarCarpetaReportes();

            if (historialVentas == null)
            {
                historialVentas = new List<Venta>();
            }

            if (productos == null)
            {
                productos = new List<Producto>();
            }

            DateTime fechaGeneracion = DateTime.Now;

            string nombreArchivo =
                "Reporte_" +
                fechaGeneracion.ToString(
                    "dd-MM-yyyy_HH-mm-ss"
                ) +
                ".txt";

            string rutaArchivo =
                Path.Combine(
                    CarpetaReportes,
                    nombreArchivo
                );

            string contenido =
                ConstruirContenidoReporte(
                    historialVentas,
                    productos,
                    generadoPor,
                    fechaGeneracion
                );

            File.WriteAllText(
                rutaArchivo,
                contenido,
                Encoding.UTF8
            );

            return rutaArchivo;
        }


        // ─────────────────────────────────────────────
        // CONSTRUIR CONTENIDO
        // ─────────────────────────────────────────────

        /// <summary>
        /// Construye todo el contenido textual del reporte.
        /// </summary>
        private static string ConstruirContenidoReporte(
            List<Venta> ventas,
            List<Producto> productos,
            Empleado generadoPor,
            DateTime fechaGeneracion)
        {
            StringBuilder sb =
                new StringBuilder();

            double totalVendido =
                ventas.Sum(v => v.Subtotal);

            // ─────────────────────────────────────────
            // ENCABEZADO
            // ─────────────────────────────────────────

            sb.AppendLine(
                "============================================================"
            );

            sb.AppendLine(
                "                         FEDICASA"
            );

            sb.AppendLine(
                "                  REPORTE DEL SISTEMA"
            );

            sb.AppendLine(
                "============================================================"
            );

            sb.AppendLine();

            sb.AppendLine(
                "RUC       : 10458963271"
            );

            sb.AppendLine(
                "Dirección : Av. Las Palmeras 5201, Los Olivos 15304"
            );

            sb.AppendLine(
                "Celular   : 998 186 179"
            );

            sb.AppendLine();

            sb.AppendLine(
                "Fecha de generación: " +
                fechaGeneracion.ToString(
                    "dd/MM/yyyy HH:mm:ss"
                )
            );

            if (generadoPor != null)
            {
                sb.AppendLine(
                    "Generado por       : " +
                    generadoPor.NombreCompleto
                );

                sb.AppendLine(
                    "Usuario             : " +
                    generadoPor.Usuario
                );

                sb.AppendLine(
                    "Cargo               : " +
                    generadoPor.Cargo
                );
            }
            else
            {
                sb.AppendLine(
                    "Generado por       : Sistema"
                );
            }

            sb.AppendLine();

            // ─────────────────────────────────────────
            // RESUMEN DE VENTAS
            // ─────────────────────────────────────────

            sb.AppendLine(
                "============================================================"
            );

            sb.AppendLine(
                "                     RESUMEN DE VENTAS"
            );

            sb.AppendLine(
                "============================================================"
            );

            sb.AppendLine();

            sb.AppendLine(
                "Registros de venta : " +
                ventas.Count
            );

            sb.AppendLine(
                "Unidades vendidas  : " +
                ventas.Sum(v => v.Cantidad)
            );

            sb.AppendLine(
                "Total vendido      : S/ " +
                totalVendido.ToString(
                    "0.00",
                    CultureInfo.InvariantCulture
                )
            );

            sb.AppendLine();

            // ─────────────────────────────────────────
            // PRODUCTO MÁS VENDIDO
            // ─────────────────────────────────────────

            sb.AppendLine(
                "---------------- PRODUCTO MÁS VENDIDO ----------------"
            );

            if (ventas.Count > 0)
            {
                var productoMasVendido =
                    ventas
                    .GroupBy(v => new
                    {
                        v.CodigoProducto,
                        v.NombreProducto
                    })
                    .Select(g => new
                    {
                        g.Key.CodigoProducto,
                        g.Key.NombreProducto,
                        Cantidad = g.Sum(
                            v => v.Cantidad
                        )
                    })
                    .OrderByDescending(
                        x => x.Cantidad
                    )
                    .FirstOrDefault();

                if (productoMasVendido != null)
                {
                    sb.AppendLine(
                        "Código    : " +
                        productoMasVendido.CodigoProducto
                    );

                    sb.AppendLine(
                        "Producto  : " +
                        productoMasVendido.NombreProducto
                    );

                    sb.AppendLine(
                        "Unidades  : " +
                        productoMasVendido.Cantidad
                    );
                }
            }
            else
            {
                sb.AppendLine(
                    "No existen ventas registradas."
                );
            }

            sb.AppendLine();

            // ─────────────────────────────────────────
            // TOP 3 POR INGRESOS
            // ─────────────────────────────────────────

            sb.AppendLine(
                "---------------- TOP 3 POR INGRESOS ----------------"
            );

            if (ventas.Count > 0)
            {
                var topProductos =
                    ventas
                    .GroupBy(v => new
                    {
                        v.CodigoProducto,
                        v.NombreProducto
                    })
                    .Select(g => new
                    {
                        g.Key.CodigoProducto,
                        g.Key.NombreProducto,
                        Ingreso = g.Sum(
                            v => v.Subtotal
                        )
                    })
                    .OrderByDescending(
                        x => x.Ingreso
                    )
                    .Take(3)
                    .ToList();

                int posicion = 1;

                foreach (var producto in topProductos)
                {
                    sb.AppendLine(
                        posicion +
                        ". " +
                        producto.NombreProducto +
                        " (" +
                        producto.CodigoProducto +
                        ") - S/ " +
                        producto.Ingreso.ToString(
                            "0.00",
                            CultureInfo.InvariantCulture
                        )
                    );

                    posicion++;
                }
            }
            else
            {
                sb.AppendLine(
                    "No existen ventas registradas."
                );
            }

            sb.AppendLine();

            // ─────────────────────────────────────────
            // PRODUCTOS CON BAJO STOCK
            // ─────────────────────────────────────────

            sb.AppendLine(
                "---------------- PRODUCTOS CON BAJO STOCK ----------------"
            );

            List<Producto> bajoStock =
                productos
                .Where(p =>
                    p.Activo &&
                    p.Stock <= 5)
                .OrderBy(
                    p => p.Stock
                )
                .ToList();

            if (bajoStock.Count > 0)
            {
                foreach (Producto producto in bajoStock)
                {
                    sb.AppendLine(
                        producto.Codigo +
                        " | " +
                        producto.Nombre +
                        " | Stock: " +
                        producto.Stock
                    );
                }
            }
            else
            {
                sb.AppendLine(
                    "No existen productos con bajo stock."
                );
            }

            sb.AppendLine();

            // ─────────────────────────────────────────
            // INVENTARIO ACTIVO
            // ─────────────────────────────────────────

            sb.AppendLine(
                "============================================================"
            );

            sb.AppendLine(
                "                    INVENTARIO ACTIVO"
            );

            sb.AppendLine(
                "============================================================"
            );

            sb.AppendLine();

            List<Producto> productosActivos =
                productos
                .Where(p => p.Activo)
                .OrderBy(p => p.Codigo)
                .ToList();

            if (productosActivos.Count > 0)
            {
                sb.AppendLine(
                    "Código | Producto | Precio | Stock"
                );

                sb.AppendLine(
                    "------------------------------------------------------------"
                );

                foreach (Producto producto in productosActivos)
                {
                    sb.AppendLine(
                        producto.Codigo +
                        " | " +
                        producto.Nombre +
                        " | S/ " +
                        producto.Precio.ToString(
                            "0.00",
                            CultureInfo.InvariantCulture
                        ) +
                        " | " +
                        producto.Stock
                    );
                }
            }
            else
            {
                sb.AppendLine(
                    "No existen productos activos."
                );
            }

            sb.AppendLine();

            // ─────────────────────────────────────────
            // DETALLE DE VENTAS
            // ─────────────────────────────────────────

            sb.AppendLine(
                "============================================================"
            );

            sb.AppendLine(
                "                     DETALLE DE VENTAS"
            );

            sb.AppendLine(
                "============================================================"
            );

            sb.AppendLine();

            if (ventas.Count > 0)
            {
                foreach (Venta venta in ventas)
                {
                    sb.AppendLine(
                        "ID Venta    : " +
                        venta.IdVenta
                    );

                    sb.AppendLine(
                        "Fecha       : " +
                        venta.Fecha.ToString(
                            "dd/MM/yyyy HH:mm:ss"
                        )
                    );

                    sb.AppendLine(
                        "Código      : " +
                        venta.CodigoProducto
                    );

                    sb.AppendLine(
                        "Producto    : " +
                        venta.NombreProducto
                    );

                    sb.AppendLine(
                        "Cantidad    : " +
                        venta.Cantidad
                    );

                    sb.AppendLine(
                        "Precio      : S/ " +
                        venta.Precio.ToString(
                            "0.00",
                            CultureInfo.InvariantCulture
                        )
                    );

                    sb.AppendLine(
                        "Subtotal    : S/ " +
                        venta.Subtotal.ToString(
                            "0.00",
                            CultureInfo.InvariantCulture
                        )
                    );

                    sb.AppendLine(
                        "------------------------------------------------------------"
                    );
                }
            }
            else
            {
                sb.AppendLine(
                    "No existen ventas registradas."
                );
            }

            // ─────────────────────────────────────────
            // PIE DEL REPORTE
            // ─────────────────────────────────────────

            sb.AppendLine();

            sb.AppendLine(
                "============================================================"
            );

            sb.AppendLine(
                "              FIN DEL REPORTE - FEDICASA"
            );

            sb.AppendLine(
                "============================================================"
            );

            return sb.ToString();
        }


        // ─────────────────────────────────────────────
        // OBTENER REPORTES HISTÓRICOS
        // ─────────────────────────────────────────────

        /// <summary>
        /// Obtiene los nombres de todos los reportes
        /// históricos disponibles.
        /// </summary>
        public static List<string> ObtenerReportesHistoricos()
        {
            InicializarCarpetaReportes();

            List<string> reportes =
                new List<string>();

            string[] archivos =
                Directory.GetFiles(
                    CarpetaReportes,
                    "Reporte_*.txt"
                );

            foreach (string archivo in archivos)
            {
                reportes.Add(
                    Path.GetFileName(archivo)
                );
            }

            reportes.Sort(
                StringComparer.OrdinalIgnoreCase
            );

            reportes.Reverse();

            return reportes;
        }


        // ─────────────────────────────────────────────
        // LEER REPORTE
        // ─────────────────────────────────────────────

        /// <summary>
        /// Lee el contenido de un reporte histórico.
        ///
        /// El formulario puede utilizar este método para
        /// mostrar el contenido dentro de un RichTextBox.
        /// </summary>
        public static string LeerReporte(
            string nombreArchivo)
        {
            InicializarCarpetaReportes();

            if (string.IsNullOrWhiteSpace(nombreArchivo))
            {
                return string.Empty;
            }

            string nombreSeguro =
                Path.GetFileName(nombreArchivo);

            string ruta =
                Path.Combine(
                    CarpetaReportes,
                    nombreSeguro
                );

            if (!File.Exists(ruta))
            {
                return string.Empty;
            }

            return File.ReadAllText(
                ruta,
                Encoding.UTF8
            );
        }


        // ─────────────────────────────────────────────
        // RUTA DE REPORTES
        // ─────────────────────────────────────────────

        /// <summary>
        /// Devuelve la ruta de la carpeta donde
        /// se almacenan los reportes.
        /// </summary>
        public static string ObtenerCarpetaReportes()
        {
            InicializarCarpetaReportes();

            return CarpetaReportes;
        }

        // ─────────────────────────────────────────────
        // REPORTE DE TURNO (Ventas e Inventario)
        // ─────────────────────────────────────────────

        /// <summary>
        /// Resume lo que el usuario hizo hoy (su turno) según su rol.
        /// Usa las ventas, solicitudes y movimientos ya registrados.
        /// </summary>
        public static string GenerarReporteTurno(
            Empleado empleado,
            List<Venta> ventas,
            List<Solicitud> solicitudes,
            List<Movimiento> movimientos)
        {
            InicializarCarpetaReportes();

            DateTime ahora = DateTime.Now;
            DateTime hoy = ahora.Date;
            string usuario = empleado.Usuario;
            bool esVentas = empleado.Cargo.Equals(
                "Ventas", StringComparison.OrdinalIgnoreCase);

            StringBuilder sb = new StringBuilder();
            string raya = new string('=', 70);

            sb.AppendLine(raya);
            sb.AppendLine("FEDICASA - REPORTE DE TURNO");
            sb.AppendLine(raya);
            sb.AppendLine("Usuario : " + usuario);
            sb.AppendLine("Nombre  : " + empleado.NombreCompleto);
            sb.AppendLine("Rol     : " + empleado.Cargo);
            sb.AppendLine("Turno   : " + hoy.ToString("dd/MM/yyyy") +
                          " (00:00 hasta " + ahora.ToString("HH:mm:ss") + ")");
            sb.AppendLine("Generado: " + ahora.ToString("dd/MM/yyyy HH:mm:ss"));
            sb.AppendLine();

            if (esVentas)
            {
                // ----- VENTAS -----
                var misVentas = ventas
                    .Where(v => v.UsuarioVendedor.Equals(
                                    usuario,
                                    StringComparison.OrdinalIgnoreCase) &&
                                v.Fecha.Date == hoy)
                    .GroupBy(v => v.IdVenta)
                    .OrderBy(g => g.First().Fecha)
                    .ToList();

                sb.AppendLine("VENTAS DEL TURNO");
                sb.AppendLine(new string('-', 70));

                if (misVentas.Count == 0)
                    sb.AppendLine("Sin ventas registradas en este turno.");

                int confirmadas = 0, anuladas = 0, registradas = 0;
                double montoConfirmado = 0;

                foreach (var g in misVentas)
                {
                    Venta primera = g.First();
                    double total = g.Sum(v => v.Total);

                    if (primera.Estado == "Confirmada")
                    {
                        confirmadas++;
                        montoConfirmado += total;
                    }
                    else if (primera.Estado == "Anulada")
                        anuladas++;
                    else
                        registradas++;

                    sb.AppendLine(
                        primera.Fecha.ToString("HH:mm:ss") +
                        " | Venta N.° " + g.Key +
                        " | " + primera.Estado +
                        " | Cliente: " + primera.NombreCliente +
                        " | Pago: " + primera.FormaPago +
                        " | Total: S/ " + total.ToString("0.00"));

                    foreach (Venta v in g)
                    {
                        sb.AppendLine(
                            "      - " + v.CodigoProducto + " " +
                            v.NombreProducto + " x " + v.Cantidad +
                            " = S/ " + v.Subtotal.ToString("0.00"));
                    }
                }

                sb.AppendLine();
                sb.AppendLine("Ventas confirmadas : " + confirmadas +
                              " (S/ " + montoConfirmado.ToString("0.00") + ")");
                sb.AppendLine("Ventas pendientes : " + registradas);
                sb.AppendLine("Ventas anuladas   : " + anuladas);
                sb.AppendLine();
            }
            else
            {
                // ----- INVENTARIO: operaciones de stock aplicadas -----
                var ops = movimientos
                    .Where(m => m.Fecha.Date == hoy &&
                                (m.Tipo == "Ingreso" ||
                                 m.Tipo == "Salida" ||
                                 m.Tipo == "Diferencia") &&
                                (m.Usuario.Equals(
                                     usuario,
                                     StringComparison.OrdinalIgnoreCase) ||
                                 m.Detalle.Contains(
                                     "solicitado por: " + usuario + ")")))
                    .OrderBy(m => m.Fecha)
                    .ToList();

                sb.AppendLine("OPERACIONES DE STOCK APLICADAS");
                sb.AppendLine(new string('-', 70));

                if (ops.Count == 0)
                    sb.AppendLine("Sin operaciones de stock en este turno.");

                foreach (Movimiento m in ops)
                {
                    sb.AppendLine(
                        m.Fecha.ToString("HH:mm:ss") +
                        " | " + m.Tipo +
                        " | " + m.CodigoProducto + " " + m.NombreProducto +
                        " | Cant.: " + m.Cantidad.ToString("+0;-0;0") +
                        " | Stock: " + m.StockResultante);
                    sb.AppendLine("      " + m.Detalle);
                }

                sb.AppendLine();
            }

            // ----- SOLICITUDES E INCIDENCIAS (ambos roles) -----
            var misSolicitudes = solicitudes
                .Where(s => s.Solicitante.Equals(
                                usuario,
                                StringComparison.OrdinalIgnoreCase) &&
                            s.Fecha.Date == hoy)
                .OrderBy(s => s.Fecha)
                .ToList();

            sb.AppendLine("SOLICITUDES E INCIDENCIAS ENVIADAS");
            sb.AppendLine(new string('-', 70));

            if (misSolicitudes.Count == 0)
                sb.AppendLine("Sin solicitudes ni incidencias en este turno.");

            foreach (Solicitud s in misSolicitudes)
            {
                sb.AppendLine(
                    s.Fecha.ToString("HH:mm:ss") +
                    " | N.° " + s.Id +
                    " | " + s.Tipo +
                    " | " + s.Estado +
                    (s.CodigoProducto != string.Empty
                        ? " | Prod.: " + s.CodigoProducto + " x " + s.Cantidad
                        : ""));
                sb.AppendLine("      Motivo: " + s.Motivo);

                if (s.Comentario != string.Empty)
                    sb.AppendLine("      Revisión (" + s.Revisor + "): " +
                                  s.Comentario);
            }

            sb.AppendLine();
            sb.AppendLine(raya);
            sb.AppendLine("FIN DEL REPORTE");

            string archivo =
                "Reporte_Turno_" + usuario + "_" +
                ahora.ToString("dd-MM-yyyy_HH-mm-ss") + ".txt";

            File.WriteAllText(
                Path.Combine(CarpetaReportes, archivo),
                sb.ToString(),
                Encoding.UTF8);

            return Path.Combine(CarpetaReportes, archivo);
        }
    }
}
