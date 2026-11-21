using System;
using System.Collections.Generic;
using System.Linq;
using FediCasa.Helpers;
using FediCasa.Models;

namespace FediCasa.Services
{
    public static class ProductoService
    {
        private static readonly List<Producto> productos =
            new List<Producto>();

        private static bool inicializado = false;

        // =========================================================
        // INICIALIZACIÓN
        // =========================================================

        public static void Inicializar()
        {
            if (inicializado)
                return;

            productos.Clear();

            if (ArchivoHelper.ExisteArchivoProductos())
            {
                List<Producto> productosGuardados =
                    ArchivoHelper.LeerProductos();

                productos.AddRange(
                    productosGuardados);
            }
            else
            {
                CrearProductosIniciales();

                Guardar();
            }

            inicializado = true;
        }

        // =========================================================
        // PRODUCTOS INICIALES
        // =========================================================

        private static void CrearProductosIniciales()
        {
            productos.Add(
                CrearProductoInicial(
                    "FER001",
                    "Taladro eléctrico 650W",
                    189.90,
                    10,
                    3));

            productos.Add(
                CrearProductoInicial(
                    "FER002",
                    "Martillo de una 16 oz",
                    35.90,
                    15,
                    5));

            productos.Add(
                CrearProductoInicial(
                    "FER003",
                    "Destornillador plano 6 pulgadas",
                    12.50,
                    20,
                    5));

            productos.Add(
                CrearProductoInicial(
                    "FER004",
                    "Destornillador Phillips 6 pulgadas",
                    13.50,
                    20,
                    5));

            productos.Add(
                CrearProductoInicial(
                    "FER005",
                    "Alicate universal 8 pulgadas",
                    28.90,
                    12,
                    4));

            productos.Add(
                CrearProductoInicial(
                    "FER006",
                    "Cinta métrica 5 metros",
                    18.90,
                    15,
                    5));

            productos.Add(
                CrearProductoInicial(
                    "FER007",
                    "Juego de llaves Allen",
                    24.90,
                    10,
                    3));

            productos.Add(
                CrearProductoInicial(
                    "FER008",
                    "Cinta aislante negra",
                    5.50,
                    30,
                    10));

            productos.Add(
                CrearProductoInicial(
                    "FER009",
                    "Tubo PVC 1/2 pulgada",
                    8.90,
                    25,
                    8));

            productos.Add(
                CrearProductoInicial(
                    "FER010",
                    "Pegamento PVC 1/4 galón",
                    22.90,
                    12,
                    4));

            productos.Add(
                CrearProductoInicial(
                    "FER011",
                    "Pintura látex 1 galón",
                    49.90,
                    10,
                    3));

            productos.Add(
                CrearProductoInicial(
                    "FER012",
                    "Tornillos surtidos",
                    15.90,
                    25,
                    8));
        }

        private static Producto CrearProductoInicial(
            string codigo,
            string nombre,
            double precio,
            int stock,
            int stockMinimo)
        {
            return new Producto(
                codigo,
                nombre,
                precio,
                stock,
                stockMinimo,
                "Unidad",
                true);
        }

        // =========================================================
        // CONSULTAS
        // =========================================================

        public static List<Producto> ObtenerProductos()
        {
            Inicializar();

            return productos
                .Select(CopiarProducto)
                .ToList();
        }

        public static List<Producto> ObtenerProductosActivos()
        {
            Inicializar();

            return productos
                .Where(p => p.Activo)
                .Select(CopiarProducto)
                .ToList();
        }

        public static List<Producto> ObtenerProductosInactivos()
        {
            Inicializar();

            return productos
                .Where(p => !p.Activo)
                .Select(CopiarProducto)
                .ToList();
        }

        public static Producto BuscarPorCodigo(
            string codigo)
        {
            Inicializar();

            if (string.IsNullOrWhiteSpace(codigo))
                return null;

            Producto producto =
                productos.FirstOrDefault(p =>
                    p.Codigo.Equals(
                        codigo.Trim(),
                        StringComparison.OrdinalIgnoreCase));

            return producto == null
                ? null
                : CopiarProducto(producto);
        }

        public static List<Producto> Buscar(
            string texto)
        {
            Inicializar();

            if (string.IsNullOrWhiteSpace(texto))
                return ObtenerProductosActivos();

            texto = texto.Trim();

            return productos
                .Where(p =>
                    p.Activo &&
                    (
                        p.Codigo.Contains(
                            texto,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        p.Nombre.Contains(
                            texto,
                            StringComparison.OrdinalIgnoreCase)
                    ))
                .Select(CopiarProducto)
                .ToList();
        }

        public static List<Producto> BuscarTodos(
            string texto)
        {
            Inicializar();

            if (string.IsNullOrWhiteSpace(texto))
                return ObtenerProductos();

            texto = texto.Trim();

            return productos
                .Where(p =>
                    p.Codigo.Contains(
                        texto,
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    p.Nombre.Contains(
                        texto,
                        StringComparison.OrdinalIgnoreCase))
                .Select(CopiarProducto)
                .ToList();
        }

        // =========================================================
        // VALIDACIONES
        // =========================================================

        public static bool ExisteCodigo(
            string codigo)
        {
            Inicializar();

            if (string.IsNullOrWhiteSpace(codigo))
                return false;

            return productos.Any(p =>
                p.Codigo.Equals(
                    codigo.Trim(),
                    StringComparison.OrdinalIgnoreCase));
        }

        public static bool ExisteNombre(
            string nombre,
            string codigoExcluir = "")
        {
            Inicializar();

            if (string.IsNullOrWhiteSpace(nombre))
                return false;

            return productos.Any(p =>
                !p.Codigo.Equals(
                    codigoExcluir ?? string.Empty,
                    StringComparison.OrdinalIgnoreCase)
                &&
                p.Nombre.Equals(
                    nombre.Trim(),
                    StringComparison.OrdinalIgnoreCase));
        }

        // =========================================================
        // REGISTRAR PRODUCTO
        // =========================================================

        public static bool RegistrarProducto(
            Producto producto,
            out string mensaje)
        {
            mensaje = string.Empty;

            Inicializar();

            if (producto == null)
            {
                mensaje =
                    "El producto no puede ser nulo.";

                return false;
            }

            producto.Codigo =
                producto.Codigo?.Trim()
                .ToUpperInvariant() ?? string.Empty;

            producto.Nombre =
                producto.Nombre?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(
                producto.Codigo))
            {
                mensaje =
                    "El código del producto es obligatorio.";

                return false;
            }

            if (string.IsNullOrWhiteSpace(
                producto.Nombre))
            {
                mensaje =
                    "El nombre del producto es obligatorio.";

                return false;
            }

            if (producto.Precio <= 0)
            {
                mensaje =
                    "El precio debe ser mayor que cero.";

                return false;
            }

            if (producto.Stock < 0)
            {
                mensaje =
                    "El stock no puede ser negativo.";

                return false;
            }

            if (producto.StockMinimo < 0)
            {
                mensaje =
                    "El stock mínimo no puede ser negativo.";

                return false;
            }

            if (ExisteCodigo(producto.Codigo))
            {
                mensaje =
                    "Ya existe un producto con ese código.";

                return false;
            }

            if (ExisteNombre(producto.Nombre))
            {
                mensaje =
                    "Ya existe un producto con ese nombre.";

                return false;
            }

            producto.Activo = true;

            if (string.IsNullOrWhiteSpace(
                producto.UnidadMedida))
            {
                producto.UnidadMedida = "Unidad";
            }

            producto.FechaRegistro =
                DateTime.Now;

            producto.FechaActualizacion =
                DateTime.Now;

            productos.Add(producto);

            Guardar();

            return true;
        }

        // =========================================================
        // MODIFICAR PRODUCTO
        // =========================================================

        public static bool ModificarProducto(
            string codigoOriginal,
            Producto datosActualizados,
            out string mensaje)
        {
            mensaje = string.Empty;

            Inicializar();

            if (string.IsNullOrWhiteSpace(
                codigoOriginal))
            {
                mensaje =
                    "No se indicó el producto a modificar.";

                return false;
            }

            if (datosActualizados == null)
            {
                mensaje =
                    "No se recibieron datos del producto.";

                return false;
            }

            Producto producto =
                productos.FirstOrDefault(p =>
                    p.Codigo.Equals(
                        codigoOriginal.Trim(),
                        StringComparison.OrdinalIgnoreCase));

            if (producto == null)
            {
                mensaje =
                    "No se encontró el producto.";

                return false;
            }

            string nuevoCodigo =
                datosActualizados.Codigo?.Trim()
                .ToUpperInvariant() ?? string.Empty;

            string nuevoNombre =
                datosActualizados.Nombre?.Trim()
                ?? string.Empty;

            if (string.IsNullOrWhiteSpace(nuevoCodigo))
            {
                mensaje =
                    "El código es obligatorio.";

                return false;
            }

            if (string.IsNullOrWhiteSpace(nuevoNombre))
            {
                mensaje =
                    "El nombre es obligatorio.";

                return false;
            }

            if (datosActualizados.Precio <= 0)
            {
                mensaje =
                    "El precio debe ser mayor que cero.";

                return false;
            }

            if (datosActualizados.Stock < 0)
            {
                mensaje =
                    "El stock no puede ser negativo.";

                return false;
            }

            if (datosActualizados.StockMinimo < 0)
            {
                mensaje =
                    "El stock mínimo no puede ser negativo.";

                return false;
            }

            bool codigoDuplicado =
                productos.Any(p =>
                    !p.Codigo.Equals(
                        codigoOriginal.Trim(),
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    p.Codigo.Equals(
                        nuevoCodigo,
                        StringComparison.OrdinalIgnoreCase));

            if (codigoDuplicado)
            {
                mensaje =
                    "El nuevo código ya está siendo utilizado.";

                return false;
            }

            bool nombreDuplicado =
                productos.Any(p =>
                    !p.Codigo.Equals(
                        codigoOriginal.Trim(),
                        StringComparison.OrdinalIgnoreCase)
                    &&
                    p.Nombre.Equals(
                        nuevoNombre,
                        StringComparison.OrdinalIgnoreCase));

            if (nombreDuplicado)
            {
                mensaje =
                    "El nuevo nombre ya está siendo utilizado.";

                return false;
            }

            producto.Codigo =
                nuevoCodigo;

            producto.Nombre =
                nuevoNombre;

            producto.Precio =
                datosActualizados.Precio;

            producto.Stock =
                datosActualizados.Stock;

            producto.StockMinimo =
                datosActualizados.StockMinimo;

            producto.UnidadMedida =
                string.IsNullOrWhiteSpace(
                    datosActualizados.UnidadMedida)
                    ? "Unidad"
                    : datosActualizados.UnidadMedida.Trim();

            producto.FechaActualizacion =
                DateTime.Now;

            Guardar();

            return true;
        }

        // =========================================================
        // DESACTIVAR PRODUCTO
        // =========================================================

        public static bool DesactivarProducto(
            string codigo,
            out string mensaje)
        {
            mensaje = string.Empty;

            Inicializar();

            Producto producto =
                productos.FirstOrDefault(p =>
                    p.Codigo.Equals(
                        codigo?.Trim() ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase));

            if (producto == null)
            {
                mensaje =
                    "No se encontró el producto.";

                return false;
            }

            if (!producto.Activo)
            {
                mensaje =
                    "El producto ya se encuentra inactivo.";

                return false;
            }

            producto.Desactivar();

            Guardar();

            return true;
        }

        // =========================================================
        // REACTIVAR PRODUCTO
        // =========================================================

        public static bool ReactivarProducto(
            string codigo,
            out string mensaje)
        {
            mensaje = string.Empty;

            Inicializar();

            Producto producto =
                productos.FirstOrDefault(p =>
                    p.Codigo.Equals(
                        codigo?.Trim() ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase));

            if (producto == null)
            {
                mensaje =
                    "No se encontró el producto.";

                return false;
            }

            if (producto.Activo)
            {
                mensaje =
                    "El producto ya se encuentra activo.";

                return false;
            }

            producto.Activar();

            Guardar();

            return true;
        }

        // =========================================================
        // STOCK
        // =========================================================

        public static bool HayStockSuficiente(
            string codigo,
            int cantidad)
        {
            Inicializar();

            if (cantidad <= 0)
                return false;

            Producto producto =
                productos.FirstOrDefault(p =>
                    p.Codigo.Equals(
                        codigo?.Trim() ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase));

            if (producto == null)
                return false;

            return producto.TieneStockSuficiente(
                cantidad);
        }

        public static bool ReducirStock(
            string codigo,
            int cantidad,
            out string mensaje)
        {
            mensaje = string.Empty;

            Inicializar();

            if (cantidad <= 0)
            {
                mensaje =
                    "La cantidad debe ser mayor que cero.";

                return false;
            }

            Producto producto =
                productos.FirstOrDefault(p =>
                    p.Codigo.Equals(
                        codigo?.Trim() ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase));

            if (producto == null)
            {
                mensaje =
                    "No se encontró el producto.";

                return false;
            }

            if (!producto.Activo)
            {
                mensaje =
                    "El producto se encuentra inactivo.";

                return false;
            }

            if (producto.Stock < cantidad)
            {
                mensaje =
                    $"Stock insuficiente. Stock disponible: {producto.Stock}.";

                return false;
            }

            if (!producto.ReducirStock(cantidad))
            {
                mensaje =
                    "No fue posible actualizar el stock.";

                return false;
            }

            Guardar();

            return true;
        }

        public static bool AumentarStock(
            string codigo,
            int cantidad,
            out string mensaje)
        {
            mensaje = string.Empty;

            Inicializar();

            if (cantidad <= 0)
            {
                mensaje =
                    "La cantidad debe ser mayor que cero.";

                return false;
            }

            Producto producto =
                productos.FirstOrDefault(p =>
                    p.Codigo.Equals(
                        codigo?.Trim() ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase));

            if (producto == null)
            {
                mensaje =
                    "No se encontró el producto.";

                return false;
            }

            if (!producto.Activo)
            {
                mensaje =
                    "No se puede aumentar el stock de un producto inactivo.";

                return false;
            }

            if (!producto.AumentarStock(cantidad))
            {
                mensaje =
                    "No fue posible actualizar el stock.";

                return false;
            }

            Guardar();

            return true;
        }

        // =========================================================
        // CONSULTAS DE STOCK
        // =========================================================

        public static List<Producto> ObtenerProductosStockBajo()
        {
            Inicializar();

            return productos
                .Where(p =>
                    p.Activo &&
                    p.Stock <= p.StockMinimo)
                .Select(CopiarProducto)
                .ToList();
        }

        public static List<Producto> ObtenerProductosAgotados()
        {
            Inicializar();

            return productos
                .Where(p =>
                    p.Activo &&
                    p.Stock <= 0)
                .Select(CopiarProducto)
                .ToList();
        }

        public static int ObtenerCantidadProductosActivos()
        {
            Inicializar();

            return productos.Count(
                p => p.Activo);
        }

        public static int ObtenerCantidadProductosInactivos()
        {
            Inicializar();

            return productos.Count(
                p => !p.Activo);
        }

        public static int ObtenerStockTotal()
        {
            Inicializar();

            return productos
                .Where(p => p.Activo)
                .Sum(p => p.Stock);
        }

        public static double ObtenerValorInventario()
        {
            Inicializar();

            return Math.Round(
                productos
                    .Where(p => p.Activo)
                    .Sum(p =>
                        p.Precio * p.Stock),
                2);
        }

        // =========================================================
        // PERSISTENCIA
        // =========================================================

        public static void Guardar()
        {
            InicializarSoloMemoria();

            ArchivoHelper.GuardarProductos(
                productos);
        }

        // =========================================================
        // UTILIDADES INTERNAS
        // =========================================================

        private static void InicializarSoloMemoria()
        {
            if (inicializado)
                return;

            if (ArchivoHelper.ExisteArchivoProductos())
            {
                List<Producto> guardados =
                    ArchivoHelper.LeerProductos();

                productos.Clear();

                productos.AddRange(
                    guardados);
            }
            else
            {
                productos.Clear();

                CrearProductosIniciales();
            }

            inicializado = true;
        }

        private static Producto CopiarProducto(
            Producto producto)
        {
            return new Producto(
                producto.Codigo,
                producto.Nombre,
                producto.Precio,
                producto.Stock,
                producto.StockMinimo,
                producto.UnidadMedida,
                producto.Activo)
            {
                FechaRegistro =
                    producto.FechaRegistro,

                FechaActualizacion =
                    producto.FechaActualizacion
            };
        }
    }
}
