using System;
using System.Collections.Generic;
using System.Linq;
using FediCasa.Helpers;
using FediCasa.Models;

namespace FediCasa.Services
{
    public static class SolicitudService
    {
        // Administrador y maestro ven todo; los demás solo lo suyo.
        public static List<Solicitud> Obtener()
        {
            string usuario = SesionService.ObtenerUsuarioActual();
            bool veTodo = SesionService.PuedeAutorizarSolicitudes();

            return ArchivoHelper.LeerSolicitudes()
                .Where(s => veTodo ||
                            s.Solicitante.Equals(
                                usuario,
                                StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(s => s.Id)
                .ToList();
        }

        // ---------------------------------------------------------
        // CREAR
        // ---------------------------------------------------------

        public static bool Crear(
            string tipo,
            string codigo,
            int cantidad,
            string motivo,
            out string mensaje)
        {
            mensaje = string.Empty;

            if (!SesionService.PuedeCrearSolicitud())
            {
                mensaje = "Tu perfil no puede crear solicitudes.";
                return false;
            }

            // Ventas solo reporta incidencias.
            if (SesionService.EsVentas() && tipo != Solicitud.Incidencia)
            {
                mensaje = "Ventas solo puede reportar incidencias.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                mensaje = "Ingrese el motivo o la descripción.";
                return false;
            }

            codigo = codigo?.Trim().ToUpperInvariant() ?? string.Empty;

            bool necesitaProducto = tipo != Solicitud.Incidencia;

            if (necesitaProducto || codigo != string.Empty)
            {
                if (ProductoService.BuscarPorCodigo(codigo) == null)
                {
                    mensaje = "El código de producto no existe.";
                    return false;
                }
            }

            bool cantidadValida =
                tipo == Solicitud.CambioProducto ||
                tipo == Solicitud.Incidencia ||
                (tipo == Solicitud.Diferencia ? cantidad >= 0 : cantidad > 0);

            if (!cantidadValida)
            {
                mensaje = "Ingrese una cantidad válida.";
                return false;
            }

            List<Solicitud> todas = ArchivoHelper.LeerSolicitudes();

            todas.Add(new Solicitud
            {
                Id = todas.Count == 0 ? 1 : todas.Max(s => s.Id) + 1,
                Fecha = DateTime.Now,
                Tipo = tipo,
                CodigoProducto = codigo,
                Cantidad = cantidad,
                Motivo = motivo.Trim(),
                Estado = Solicitud.Pendiente,
                Solicitante = SesionService.ObtenerUsuarioActual()
            });

            ArchivoHelper.GuardarSolicitudes(todas);

            mensaje = "Solicitud enviada. Queda pendiente de revisión.";
            return true;
        }

        // ---------------------------------------------------------
        // APROBAR (aplica el stock una sola vez)
        // ---------------------------------------------------------

        public static bool Aprobar(
            int id,
            string comentario,
            out string mensaje)
        {
            mensaje = string.Empty;

            if (!SesionService.PuedeAutorizarSolicitudes())
            {
                mensaje = "Tu perfil no puede autorizar solicitudes.";
                return false;
            }

            List<Solicitud> todas = ArchivoHelper.LeerSolicitudes();
            Solicitud s = todas.FirstOrDefault(x => x.Id == id);

            if (s == null)
            {
                mensaje = "Seleccione una solicitud.";
                return false;
            }

            if (s.Estado != Solicitud.Pendiente)
            {
                mensaje = "Solo se pueden aprobar solicitudes pendientes.";
                return false;
            }

            if (s.Tipo == Solicitud.Incidencia)
            {
                mensaje = "Las incidencias se marcan como resueltas o se derivan.";
                return false;
            }

            if (s.Solicitante.Equals(
                    SesionService.ObtenerUsuarioActual(),
                    StringComparison.OrdinalIgnoreCase))
            {
                mensaje = "No puedes autorizar tu propia solicitud.";
                return false;
            }

            if (s.EsOperacionDeStock())
            {
                if (!EjecutarOperacion(
                        s.Tipo,
                        s.CodigoProducto,
                        s.Cantidad,
                        s.Motivo,
                        s.Solicitante,
                        out mensaje))
                {
                    return false;
                }
            }

            s.Estado = Solicitud.Aprobada;
            s.Revisor = SesionService.ObtenerUsuarioActual();
            s.Comentario = comentario?.Trim() ?? string.Empty;

            ArchivoHelper.GuardarSolicitudes(todas);

            ActividadService.Registrar(
                "Solicitud aprobada",
                DescribirSolicitud(s));

            mensaje = s.EsOperacionDeStock()
                ? "Solicitud aprobada y stock actualizado."
                : "Solicitud aprobada. Aplique el cambio en Productos.";

            return true;
        }

        private static string DescribirSolicitud(Solicitud s)
        {
            return "N.° " + s.Id + " - " + s.Tipo +
                   (s.CodigoProducto != string.Empty
                       ? " (" + s.CodigoProducto + ")"
                       : "") +
                   " de " + s.Solicitante;
        }

        // ---------------------------------------------------------
        // RECHAZAR / DERIVAR / RESOLVER
        // ---------------------------------------------------------

        public static bool Rechazar(int id, string comentario, out string mensaje)
        {
            return Cambiar(id, comentario, Solicitud.Rechazada, out mensaje);
        }

        public static bool Derivar(int id, string comentario, out string mensaje)
        {
            return Cambiar(id, comentario, Solicitud.Derivada, out mensaje);
        }

        public static bool Resolver(int id, string comentario, out string mensaje)
        {
            return Cambiar(id, comentario, Solicitud.Resuelta, out mensaje);
        }

        private static bool Cambiar(
            int id,
            string comentario,
            string nuevoEstado,
            out string mensaje)
        {
            mensaje = string.Empty;

            if (!SesionService.PuedeAutorizarSolicitudes())
            {
                mensaje = "Tu perfil no puede revisar solicitudes.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(comentario))
            {
                mensaje = "Escriba un comentario.";
                return false;
            }

            List<Solicitud> todas = ArchivoHelper.LeerSolicitudes();
            Solicitud s = todas.FirstOrDefault(x => x.Id == id);

            if (s == null)
            {
                mensaje = "Seleccione una solicitud.";
                return false;
            }

            if (!s.EstaAbierta())
            {
                mensaje = "La solicitud ya fue cerrada.";
                return false;
            }

            bool esMaestro = SesionService.EsUsuarioMaestro();

            // Derivadas: solo el usuario maestro las atiende.
            if (s.Estado == Solicitud.Derivada && !esMaestro)
            {
                mensaje = "Esta incidencia fue derivada al usuario maestro.";
                return false;
            }

            if (nuevoEstado == Solicitud.Derivada)
            {
                if (s.Tipo != Solicitud.Incidencia ||
                    s.Estado != Solicitud.Pendiente)
                {
                    mensaje = "Solo se pueden derivar incidencias pendientes.";
                    return false;
                }
            }

            if (nuevoEstado == Solicitud.Resuelta &&
                s.Tipo != Solicitud.Incidencia)
            {
                mensaje = "Solo las incidencias se marcan como resueltas.";
                return false;
            }

            s.Estado = nuevoEstado;
            s.Revisor = SesionService.ObtenerUsuarioActual();
            s.Comentario = comentario.Trim();

            ArchivoHelper.GuardarSolicitudes(todas);

            ActividadService.Registrar(
                "Solicitud " + nuevoEstado.ToLower(),
                DescribirSolicitud(s));

            mensaje = "Solicitud actualizada: " + nuevoEstado + ".";
            return true;
        }

        // ---------------------------------------------------------
        // OPERACIÓN DE STOCK + MOVIMIENTO (usada al aprobar y por
        // el usuario maestro cuando opera directamente)
        // ---------------------------------------------------------

        public static bool EjecutarOperacion(
            string tipo,
            string codigo,
            int cantidad,
            string motivo,
            string solicitante,
            out string mensaje)
        {
            mensaje = string.Empty;

            Producto producto = ProductoService.BuscarPorCodigo(codigo);

            if (producto == null)
            {
                mensaje = "No se encontró el producto.";
                return false;
            }

            int movimiento;

            if (tipo == Solicitud.Ingreso)
            {
                movimiento = cantidad;

                if (!ProductoService.AumentarStock(
                        codigo, cantidad, out mensaje))
                    return false;
            }
            else if (tipo == Solicitud.Salida)
            {
                movimiento = -cantidad;

                if (!ProductoService.ReducirStock(
                        codigo, cantidad, out mensaje))
                    return false;
            }
            else if (tipo == Solicitud.Diferencia)
            {
                // cantidad = stock real contado
                movimiento = cantidad - producto.Stock;

                if (movimiento == 0)
                {
                    mensaje = "El stock contado coincide con el sistema.";
                    return false;
                }

                bool ok = movimiento > 0
                    ? ProductoService.AumentarStock(
                        codigo, movimiento, out mensaje)
                    : ProductoService.ReducirStock(
                        codigo, -movimiento, out mensaje);

                if (!ok)
                    return false;
            }
            else
            {
                mensaje = "Tipo de operación no válido.";
                return false;
            }

            MovimientoService.Registrar(
                tipo,
                codigo,
                movimiento,
                $"{motivo} (solicitado por: {solicitante})");

            return true;
        }
    }
}
