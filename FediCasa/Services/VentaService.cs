using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FediCasa.Helpers;
using FediCasa.Models;

namespace FediCasa.Services
{
    public static class VentaService
    {
        // ============================================================
        // DATOS EN MEMORIA
        // ============================================================

        private static List<Venta> ventas =
            new List<Venta>();

        private static List<ItemCarrito> carrito =
            new List<ItemCarrito>();

        private static bool inicializado = false;

        // Versión de ventas.txt que tenemos en memoria.
        private static string firmaVentas = string.Empty;

        // ============================================================
        // RUTAS
        // ============================================================

        private static string ObtenerRutaVentas()
        {
            string carpeta =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Data");

            Directory.CreateDirectory(carpeta);

            return Path.Combine(
                carpeta,
                "ventas.txt");
        }

        // ============================================================
        // INICIALIZACIÓN
        // ============================================================

        private static void Inicializar()
        {
            if (inicializado)
            {
                // Si otra instancia cambió ventas.txt, se vuelve a leer
                // (el carrito actual no se toca).
                string firma = ArchivoHelper.FirmaArchivoVentas();

                if (firma != firmaVentas)
                {
                    ventas =
                        ArchivoHelper.LeerVentas()
                        ?? new List<Venta>();

                    firmaVentas = firma;
                }

                return;
            }

            ventas =
                ArchivoHelper.LeerVentas()
                ?? new List<Venta>();

            carrito =
                new List<ItemCarrito>();

            firmaVentas = ArchivoHelper.FirmaArchivoVentas();

            inicializado = true;
        }

        // ============================================================
        // OBTENER VENTAS
        // ============================================================

        public static List<Venta> ObtenerVentas()
        {
            Inicializar();

            return ventas;
        }

        public static List<Venta> ObtenerVentasRegistradas()
        {
            Inicializar();

            return ventas
                .Where(v => v.EstaRegistrada())
                .ToList();
        }

        public static List<Venta> ObtenerVentasConfirmadas()
        {
            Inicializar();

            return ventas
                .Where(v => v.EstaConfirmada())
                .ToList();
        }

        public static List<Venta> ObtenerVentasPendientes()
        {
            Inicializar();

            return ventas
                .Where(v =>
                    v.EstaRegistrada() &&
                    !v.EstaConfirmada() &&
                    !v.EstaAnulada())
                .ToList();
        }

        public static List<Venta> ObtenerVentasAnuladas()
        {
            Inicializar();

            return ventas
                .Where(v => v.EstaAnulada())
                .ToList();
        }

        // ============================================================
        // BUSCAR VENTAS
        // ============================================================

        public static List<Venta> BuscarVentas(
            int idVenta)
        {
            Inicializar();

            return ventas
                .Where(v =>
                    v.IdVenta == idVenta)
                .Select(CopiarVenta)
                .ToList();
        }

        public static Venta BuscarVenta(
            int idVenta)
        {
            Inicializar();

            return ventas
                .FirstOrDefault(
                    v => v.IdVenta == idVenta);
        }

        public static List<Venta> BuscarPorDocumentoCliente(
            string documento)
        {
            Inicializar();

            if (string.IsNullOrWhiteSpace(documento))
            {
                return new List<Venta>();
            }

            return ventas
                .Where(v =>
                    !string.IsNullOrWhiteSpace(
                        v.DocumentoCliente) &&
                    v.DocumentoCliente.Equals(
                        documento.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                .Select(CopiarVenta)
                .ToList();
        }

        public static List<Venta> BuscarPorUsuario(
            string usuario)
        {
            Inicializar();

            if (string.IsNullOrWhiteSpace(usuario))
            {
                return new List<Venta>();
            }

            return ventas
                .Where(v =>
                    !string.IsNullOrWhiteSpace(
                        v.UsuarioVendedor) &&
                    v.UsuarioVendedor.Equals(
                        usuario.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                .Select(CopiarVenta)
                .ToList();
        }

        public static List<Venta> BuscarPorFecha(
            DateTime fecha)
        {
            Inicializar();

            return ventas
                .Where(v =>
                    v.Fecha.Date == fecha.Date)
                .Select(CopiarVenta)
                .ToList();
        }

        // ============================================================
        // CARRITO
        // ============================================================

        public static List<ItemCarrito> ObtenerCarrito()
        {
            Inicializar();

            return carrito;
        }

        // ------------------------------------------------------------
        // AGREGAR AL CARRITO
        // ------------------------------------------------------------

        public static bool AgregarAlCarrito(
            Producto producto,
            int cantidad,
            out string mensaje)
        {
            Inicializar();

            return AgregarAlCarrito(
                carrito,
                producto,
                cantidad,
                out mensaje);
        }

        public static bool AgregarAlCarrito(
            List<ItemCarrito> listaCarrito,
            Producto producto,
            int cantidad,
            out string mensaje)
        {
            mensaje = string.Empty;

            if (listaCarrito == null)
            {
                mensaje =
                    "El carrito no está disponible.";

                return false;
            }

            if (producto == null)
            {
                mensaje =
                    "El producto seleccionado no es válido.";

                return false;
            }

            if (!producto.Activo)
            {
                mensaje =
                    "El producto está inactivo.";

                return false;
            }

            if (cantidad <= 0)
            {
                mensaje =
                    "La cantidad debe ser mayor que cero.";

                return false;
            }

            if (!producto.TieneStockSuficiente(
                cantidad))
            {
                mensaje =
                    $"Stock insuficiente. " +
                    $"Disponible: {producto.Stock}.";

                return false;
            }

            ItemCarrito itemExistente =
                listaCarrito.FirstOrDefault(
                    i =>
                        i.CodigoProducto.Equals(
                            producto.Codigo,
                            StringComparison.OrdinalIgnoreCase));

            if (itemExistente != null)
            {
                int nuevaCantidad =
                    itemExistente.Cantidad + cantidad;

                if (!producto.TieneStockSuficiente(
                    nuevaCantidad))
                {
                    mensaje =
                        $"No se puede agregar esa cantidad. " +
                        $"Stock disponible: {producto.Stock}. " +
                        $"Cantidad solicitada en total: {nuevaCantidad}.";

                    return false;
                }

                itemExistente.ActualizarCantidad(
                    nuevaCantidad);
            }
            else
            {
                listaCarrito.Add(
                    new ItemCarrito(
                        producto.Codigo,
                        producto.Nombre,
                        producto.Precio,
                        cantidad));
            }

            mensaje =
                "Producto agregado al carrito.";

            return true;
        }

        // ------------------------------------------------------------
        // MODIFICAR CANTIDAD
        // ------------------------------------------------------------

        public static bool ModificarCantidadCarrito(
            string codigoProducto,
            int nuevaCantidad,
            out string mensaje)
        {
            Inicializar();

            return ModificarCantidadCarrito(
                carrito,
                codigoProducto,
                nuevaCantidad,
                out mensaje);
        }

        public static bool ModificarCantidadCarrito(
            List<ItemCarrito> listaCarrito,
            string codigoProducto,
            int nuevaCantidad,
            out string mensaje)
        {
            mensaje = string.Empty;

            if (listaCarrito == null)
            {
                mensaje =
                    "El carrito no está disponible.";

                return false;
            }

            if (string.IsNullOrWhiteSpace(
                codigoProducto))
            {
                mensaje =
                    "El código del producto es obligatorio.";

                return false;
            }

            if (nuevaCantidad <= 0)
            {
                mensaje =
                    "La cantidad debe ser mayor que cero.";

                return false;
            }

            ItemCarrito item =
                listaCarrito.FirstOrDefault(
                    i =>
                        i.CodigoProducto.Equals(
                            codigoProducto.Trim(),
                            StringComparison.OrdinalIgnoreCase));

            if (item == null)
            {
                mensaje =
                    "El producto no se encuentra en el carrito.";

                return false;
            }

            Producto producto =
                ProductoService.BuscarPorCodigo(
                    item.CodigoProducto);

            if (producto == null)
            {
                mensaje =
                    "El producto ya no existe en el inventario.";

                return false;
            }

            if (!producto.Activo)
            {
                mensaje =
                    "El producto está inactivo.";

                return false;
            }

            if (!producto.TieneStockSuficiente(
                nuevaCantidad))
            {
                mensaje =
                    $"Stock insuficiente. " +
                    $"Disponible: {producto.Stock}.";

                return false;
            }

            item.ActualizarCantidad(
                nuevaCantidad);

            mensaje =
                "Cantidad actualizada correctamente.";

            return true;
        }

        // ------------------------------------------------------------
        // ELIMINAR DEL CARRITO
        // ------------------------------------------------------------

        public static bool EliminarDelCarrito(
            string codigoProducto,
            out string mensaje)
        {
            Inicializar();

            return EliminarDelCarrito(
                carrito,
                codigoProducto,
                out mensaje);
        }

        public static bool EliminarDelCarrito(
            List<ItemCarrito> listaCarrito,
            string codigoProducto,
            out string mensaje)
        {
            mensaje = string.Empty;

            if (listaCarrito == null)
            {
                mensaje =
                    "El carrito no está disponible.";

                return false;
            }

            if (string.IsNullOrWhiteSpace(
                codigoProducto))
            {
                mensaje =
                    "El código del producto es obligatorio.";

                return false;
            }

            ItemCarrito item =
                listaCarrito.FirstOrDefault(
                    i =>
                        i.CodigoProducto.Equals(
                            codigoProducto.Trim(),
                            StringComparison.OrdinalIgnoreCase));

            if (item == null)
            {
                mensaje =
                    "El producto no se encuentra en el carrito.";

                return false;
            }

            listaCarrito.Remove(item);

            mensaje =
                "Producto eliminado del carrito.";

            return true;
        }

        // ------------------------------------------------------------
        // VACIAR CARRITO
        // ------------------------------------------------------------

        public static void VaciarCarrito()
        {
            Inicializar();

            VaciarCarrito(carrito);
        }

        public static void VaciarCarrito(
            List<ItemCarrito> listaCarrito)
        {
            if (listaCarrito == null)
                return;

            listaCarrito.Clear();
        }

        // ============================================================
        // CÁLCULOS DEL CARRITO
        // ============================================================

        public static double CalcularSubtotal()
        {
            Inicializar();

            return carrito.Sum(
                item => item.Subtotal);
        }

        public static double CalcularValorVenta()
        {
            double subtotal =
                CalcularSubtotal();

            return Math.Round(
                subtotal / 1.18,
                2);
        }

        public static double CalcularIGV()
        {
            double valorVenta =
                CalcularValorVenta();

            return Math.Round(
                valorVenta * 0.18,
                2);
        }

        public static double CalcularTotal()
        {
            double subtotal =
                CalcularSubtotal();

            return Math.Round(
                subtotal,
                2);
        }

        // ============================================================
        // REGISTRAR VENTA
        // ============================================================

        public static bool RegistrarVenta(
            string documentoCliente,
            string nombreCliente,
            string formaPago,
            out int idVenta,
            out string mensaje)
        {
            Inicializar();

            return RegistrarVenta(
                carrito,
                documentoCliente,
                nombreCliente,
                formaPago,
                out idVenta,
                out mensaje);
        }

        public static bool RegistrarVenta(
            List<ItemCarrito> listaCarrito,
            string documentoCliente,
            string nombreCliente,
            string formaPago,
            out int idVenta,
            out string mensaje)
        {
            using (ArchivoHelper.BloquearDatos())
            {
                return RegistrarVentaInterno(
                    listaCarrito,
                    documentoCliente,
                    nombreCliente,
                    formaPago,
                    out idVenta,
                    out mensaje);
            }
        }

        private static bool RegistrarVentaInterno(
            List<ItemCarrito> listaCarrito,
            string documentoCliente,
            string nombreCliente,
            string formaPago,
            out int idVenta,
            out string mensaje)
        {
            Inicializar();

            idVenta = 0;
            mensaje = string.Empty;

            if (listaCarrito == null ||
                listaCarrito.Count == 0)
            {
                mensaje =
                    "No existen productos en el carrito.";

                return false;
            }

            if (string.IsNullOrWhiteSpace(
                formaPago))
            {
                mensaje =
                    "Debe seleccionar una forma de pago.";

                return false;
            }

            // --------------------------------------------------------
            // VALIDAR TODOS LOS PRODUCTOS ANTES DE REGISTRAR
            // --------------------------------------------------------

            foreach (ItemCarrito item in listaCarrito)
            {
                Producto producto =
                    ProductoService.BuscarPorCodigo(
                        item.CodigoProducto);

                if (producto == null)
                {
                    mensaje =
                        $"El producto '{item.CodigoProducto}' " +
                        "ya no existe.";

                    return false;
                }

                if (!producto.Activo)
                {
                    mensaje =
                        $"El producto '{producto.Nombre}' " +
                        "está inactivo.";

                    return false;
                }

                if (!producto.TieneStockSuficiente(
                    item.Cantidad))
                {
                    mensaje =
                        $"Stock insuficiente para '{producto.Nombre}'. " +
                        $"Disponible: {producto.Stock}. " +
                        $"Solicitado: {item.Cantidad}.";

                    return false;
                }
            }

            // --------------------------------------------------------
            // GENERAR ID
            // --------------------------------------------------------

            idVenta =
                ObtenerSiguienteIdVenta();

            DateTime fecha =
                DateTime.Now;

            string usuarioVendedor =
                SesionService.EmpleadoActual?.Usuario
                ?? string.Empty;

            string nombreVendedor =
                SesionService.EmpleadoActual?.NombreCompleto
                ?? string.Empty;

            // --------------------------------------------------------
            // CREAR UNA VENTA POR CADA LÍNEA
            // --------------------------------------------------------

            foreach (ItemCarrito item in listaCarrito)
            {
                Venta venta =
                    new Venta(
                        idVenta,
                        item.CodigoProducto,
                        item.NombreProducto,
                        item.Cantidad,
                        item.Precio);

                venta.Fecha =
                    fecha;

                venta.AsignarCliente(
                    documentoCliente?.Trim() ?? string.Empty,
                    nombreCliente?.Trim() ?? string.Empty);

                venta.AsignarVendedor(
                    usuarioVendedor,
                    nombreVendedor);

                venta.AsignarFormaPago(
                    formaPago.Trim());

                venta.Estado =
                    "Registrada";

                venta.CalcularImportes();

                ventas.Add(venta);
            }

            // --------------------------------------------------------
            // GUARDAR
            // IMPORTANTE:
            // AQUÍ TODAVÍA NO SE DESCUENTA STOCK.
            // --------------------------------------------------------

            GuardarTodasLasVentas();

            mensaje =
                $"Venta N.° {idVenta} registrada correctamente.";

            return true;
        }

        // ============================================================
        // CONFIRMAR VENTA
        // ============================================================

        public static bool ConfirmarVenta(
            int idVenta,
            out string mensaje)
        {
            using (ArchivoHelper.BloquearDatos())
            {
                return ConfirmarVentaInterno(idVenta, out mensaje);
            }
        }

        private static bool ConfirmarVentaInterno(
            int idVenta,
            out string mensaje)
        {
            Inicializar();

            mensaje = string.Empty;

            if (idVenta <= 0)
            {
                mensaje =
                    "El número de venta no es válido.";

                return false;
            }

            List<Venta> ventasVenta =
                ventas
                    .Where(v =>
                        v.IdVenta == idVenta)
                    .ToList();

            if (ventasVenta.Count == 0)
            {
                mensaje =
                    "No se encontró la venta.";

                return false;
            }

            if (ventasVenta.Any(
                v => v.EstaAnulada()))
            {
                mensaje =
                    "La venta contiene operaciones anuladas.";

                return false;
            }

            if (ventasVenta.All(
                v => v.EstaConfirmada()))
            {
                mensaje =
                    "La venta ya se encuentra confirmada.";

                return false;
            }

            // --------------------------------------------------------
            // VALIDAR STOCK ANTES DE MODIFICAR NADA
            // --------------------------------------------------------

            foreach (Venta venta in ventasVenta)
            {
                Producto producto =
                    ProductoService.BuscarPorCodigo(
                        venta.CodigoProducto);

                if (producto == null)
                {
                    mensaje =
                        $"No se encontró el producto " +
                        $"'{venta.CodigoProducto}'.";

                    return false;
                }

                if (!producto.Activo)
                {
                    mensaje =
                        $"El producto '{producto.Nombre}' " +
                        "está inactivo.";

                    return false;
                }

                if (!producto.TieneStockSuficiente(
                    venta.Cantidad))
                {
                    mensaje =
                        $"No hay stock suficiente para " +
                        $"confirmar '{producto.Nombre}'.\n\n" +
                        $"Disponible: {producto.Stock}\n" +
                        $"Necesario: {venta.Cantidad}";

                    return false;
                }
            }

            // --------------------------------------------------------
            // DESCONTAR STOCK
            // --------------------------------------------------------

            List<Venta> ventasProcesadas =
                new List<Venta>();

            foreach (Venta venta in ventasVenta)
            {
                if (!ProductoService.ReducirStock(
                    venta.CodigoProducto,
                    venta.Cantidad,
                    out string mensajeStock))
                {
                    // ------------------------------------------------
                    // ROLLBACK
                    // Si una línea falla, devolvemos las anteriores.
                    // ------------------------------------------------

                    foreach (Venta procesada in ventasProcesadas)
                    {
                        ProductoService.AumentarStock(
                            procesada.CodigoProducto,
                            procesada.Cantidad,
                            out string mensajeReversion);
                    }

                    mensaje =
                        $"No se pudo confirmar la venta.\n\n" +
                        $"{mensajeStock}";

                    return false;
                }

                ventasProcesadas.Add(venta);
            }

            // --------------------------------------------------------
            // CONFIRMAR TODAS LAS LÍNEAS
            // --------------------------------------------------------

            DateTime fechaConfirmacion =
                DateTime.Now;

            foreach (Venta venta in ventasVenta)
            {
                venta.Confirmar();

                venta.FechaConfirmacion =
                    fechaConfirmacion;
            }

            ProductoService.Guardar();

            GuardarTodasLasVentas();

            foreach (Venta venta in ventasVenta)
            {
                MovimientoService.Registrar(
                    "Venta confirmada",
                    venta.CodigoProducto,
                    -venta.Cantidad,
                    $"Venta N.° {idVenta}");
            }

            // --------------------------------------------------------
            // LIMPIAR CARRITO
            // --------------------------------------------------------

            VaciarCarrito();

            mensaje =
                $"Venta N.° {idVenta} confirmada correctamente.\n\n" +
                "El stock fue actualizado.";

            return true;
        }

        // ============================================================
        // ANULAR VENTA
        // ============================================================

        public static bool AnularVenta(
            int idVenta,
            out string mensaje)
        {
            using (ArchivoHelper.BloquearDatos())
            {
                return AnularVentaInterno(idVenta, out mensaje);
            }
        }

        private static bool AnularVentaInterno(
            int idVenta,
            out string mensaje)
        {
            Inicializar();

            mensaje = string.Empty;

            List<Venta> ventasVenta =
                ventas
                    .Where(v =>
                        v.IdVenta == idVenta)
                    .ToList();

            if (ventasVenta.Count == 0)
            {
                mensaje =
                    "No se encontró la venta.";

                return false;
            }

            if (ventasVenta.Any(
                v => v.EstaAnulada()))
            {
                mensaje =
                    "La venta ya se encuentra anulada.";

                return false;
            }

            // --------------------------------------------------------
            // SI YA ESTABA CONFIRMADA:
            // DEVOLVER STOCK
            // --------------------------------------------------------

            bool estabaConfirmada =
                ventasVenta.Any(
                    v => v.EstaConfirmada());

            if (estabaConfirmada)
            {
                foreach (Venta venta in ventasVenta)
                {
                    if (!ProductoService.AumentarStock(
                        venta.CodigoProducto,
                        venta.Cantidad,
                        out string mensajeStock))
                    {
                        mensaje =
                            $"No se pudo devolver el stock " +
                            $"del producto '{venta.NombreProducto}'.\n\n" +
                            mensajeStock;

                        return false;
                    }
                }

                ProductoService.Guardar();
            }

            // --------------------------------------------------------
            // ANULAR
            // --------------------------------------------------------

            foreach (Venta venta in ventasVenta)
            {
                venta.Anular();
            }

            GuardarTodasLasVentas();

            foreach (Venta venta in ventasVenta)
            {
                MovimientoService.Registrar(
                    estabaConfirmada
                        ? "Venta anulada"
                        : "Venta registrada anulada",
                    venta.CodigoProducto,
                    estabaConfirmada ? venta.Cantidad : 0,
                    estabaConfirmada
                        ? $"Venta N.° {idVenta} anulada, stock devuelto"
                        : $"Venta N.° {idVenta} anulada sin afectar stock");
            }

            mensaje =
                $"Venta N.° {idVenta} anulada correctamente.";

            return true;
        }

        // ============================================================
        // OBTENER SIGUIENTE ID
        // ============================================================

        private static int ObtenerSiguienteIdVenta()
        {
            Inicializar();

            int ultimoId =
                ventas.Count == 0
                    ? 0
                    : ventas.Max(
                        v => v.IdVenta);

            return ultimoId + 1;
        }

        // ============================================================
        // GUARDAR TODAS LAS VENTAS
        // ============================================================

        private static void GuardarTodasLasVentas()
        {
            ArchivoHelper.GuardarVentas(ventas);

            firmaVentas = ArchivoHelper.FirmaArchivoVentas();
        }

        // ============================================================
        // ESTADÍSTICAS
        // ============================================================

        public static int ObtenerCantidadVentas()
        {
            Inicializar();

            return ventas
                .Select(v => v.IdVenta)
                .Distinct()
                .Count();
        }

        public static int ObtenerCantidadVentasConfirmadas()
        {
            Inicializar();

            return ventas
                .Where(v =>
                    v.EstaConfirmada())
                .Select(v => v.IdVenta)
                .Distinct()
                .Count();
        }

        public static int ObtenerCantidadVentasPendientes()
        {
            Inicializar();

            return ventas
                .Where(v =>
                    v.EstaRegistrada() &&
                    !v.EstaConfirmada() &&
                    !v.EstaAnulada())
                .Select(v => v.IdVenta)
                .Distinct()
                .Count();
        }

        public static double ObtenerTotalVentas()
        {
            Inicializar();

            return Math.Round(
                ventas
                    .Where(v =>
                        v.EstaConfirmada())
                    .Sum(v => v.Total),
                2);
        }

        public static double ObtenerTotalVentasRegistradas()
        {
            Inicializar();

            return Math.Round(
                ventas
                    .Where(v =>
                        !v.EstaAnulada())
                    .Sum(v => v.Total),
                2);
        }

        public static double ObtenerTotalVentasPorFecha(
            DateTime fecha)
        {
            Inicializar();

            return Math.Round(
                ventas
                    .Where(v =>
                        v.EstaConfirmada() &&
                        v.Fecha.Date == fecha.Date)
                    .Sum(v => v.Total),
                2);
        }

        public static double ObtenerTotalVentasDelMes(
            int año,
            int mes)
        {
            Inicializar();

            return Math.Round(
                ventas
                    .Where(v =>
                        v.EstaConfirmada() &&
                        v.Fecha.Year == año &&
                        v.Fecha.Month == mes)
                    .Sum(v => v.Total),
                2);
        }

        // ============================================================
        // PRODUCTOS MÁS VENDIDOS
        // ============================================================

        public static List<Venta> ObtenerDetalleVentasConfirmadas()
        {
            Inicializar();

            return ventas
                .Where(v =>
                    v.EstaConfirmada())
                .Select(CopiarVenta)
                .ToList();
        }

        public static List<string> ObtenerProductosMasVendidos()
        {
            Inicializar();

            return ventas
                .Where(v =>
                    v.EstaConfirmada())
                .GroupBy(
                    v => v.CodigoProducto)
                .OrderByDescending(
                    grupo =>
                        grupo.Sum(
                            v => v.Cantidad))
                .Take(10)
                .Select(
                    grupo =>
                        $"{grupo.Key} - " +
                        $"{grupo.First().NombreProducto} - " +
                        $"{grupo.Sum(v => v.Cantidad)} unidades")
                .ToList();
        }

        // ============================================================
        // RESUMEN DE VENTA
        // ============================================================

        public static string ObtenerResumenVenta(
            int idVenta)
        {
            List<Venta> detalle =
                BuscarVentas(idVenta);

            if (detalle.Count == 0)
            {
                return "Venta no encontrada.";
            }

            double total =
                detalle.Sum(v => v.Total);

            string estado =
                detalle.Any(v => v.EstaAnulada())
                    ? "ANULADA"
                    : detalle.All(v => v.EstaConfirmada())
                        ? "CONFIRMADA"
                        : "REGISTRADA";

            return
                $"Venta N.° {idVenta}\n" +
                $"Cliente: {detalle[0].NombreCliente}\n" +
                $"Documento: {detalle[0].DocumentoCliente}\n" +
                $"Vendedor: {detalle[0].NombreVendedor}\n" +
                $"Forma de pago: {detalle[0].FormaPago}\n" +
                $"Estado: {estado}\n" +
                $"Total: S/ {total:0.00}";
        }

        // ============================================================
        // COPIAR VENTA
        // ============================================================

        private static Venta CopiarVenta(
            Venta original)
        {
            Venta copia =
                new Venta(
                    original.IdVenta,
                    original.CodigoProducto,
                    original.NombreProducto,
                    original.Cantidad,
                    original.Precio);

            copia.NumeroComprobante =
                original.NumeroComprobante;

            copia.TipoComprobante =
                original.TipoComprobante;

            copia.Subtotal =
                original.Subtotal;

            copia.IGV =
                original.IGV;

            copia.Total =
                original.Total;

            copia.FormaPago =
                original.FormaPago;

            copia.DocumentoCliente =
                original.DocumentoCliente;

            copia.NombreCliente =
                original.NombreCliente;

            copia.UsuarioVendedor =
                original.UsuarioVendedor;

            copia.NombreVendedor =
                original.NombreVendedor;

            copia.Estado =
                original.Estado;

            copia.Fecha =
                original.Fecha;

            copia.FechaConfirmacion =
                original.FechaConfirmacion;

            return copia;
        }

        // ============================================================
        // REINICIAR SERVICIO
        // ============================================================

        public static void Recargar()
        {
            inicializado = false;

            ventas =
                new List<Venta>();

            carrito =
                new List<ItemCarrito>();

            Inicializar();
        }

        // ============================================================
        // CONSULTAS DE PRODUCTOS ORIENTADAS A LA VENTA
        // ============================================================

        public static Producto BuscarProductoActivo(string codigo)
        {
            return ProductoService.BuscarActivoPorCodigo(codigo);
        }

        public static List<Producto> BuscarProductosActivos(string texto)
        {
            return ProductoService.Buscar(texto);
        }
    }
}
