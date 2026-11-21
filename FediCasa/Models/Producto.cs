using System;
using System.Globalization;

namespace FediCasa.Models
{
    public class Producto
    {
        // =========================================================
        // IDENTIFICACIÓN
        // =========================================================

        public string Codigo { get; set; }

        public string Nombre { get; set; }

        // =========================================================
        // INFORMACIÓN COMERCIAL
        // =========================================================

        public double Precio { get; set; }

        public string UnidadMedida { get; set; }

        // =========================================================
        // INVENTARIO
        // =========================================================

        public int Stock { get; set; }

        public int StockMinimo { get; set; }

        // =========================================================
        // ESTADO
        // =========================================================

        public bool Activo { get; set; }

        // =========================================================
        // CONTROL
        // =========================================================

        public DateTime FechaRegistro { get; set; }

        public DateTime FechaActualizacion { get; set; }

        // =========================================================
        // CONSTRUCTORES
        // =========================================================

        public Producto()
        {
            Codigo = string.Empty;
            Nombre = string.Empty;

            Precio = 0;

            UnidadMedida = "Unidad";

            Stock = 0;
            StockMinimo = 5;

            Activo = true;

            FechaRegistro = DateTime.Now;
            FechaActualizacion = DateTime.Now;
        }

        public Producto(
            string codigo,
            string nombre,
            double precio,
            int stock,
            bool activo = true)
        {
            Codigo = codigo;
            Nombre = nombre;

            Precio = precio;

            UnidadMedida = "Unidad";

            Stock = stock;
            StockMinimo = 5;

            Activo = activo;

            FechaRegistro = DateTime.Now;
            FechaActualizacion = DateTime.Now;
        }

        public Producto(
            string codigo,
            string nombre,
            double precio,
            int stock,
            int stockMinimo,
            string unidadMedida,
            bool activo = true)
        {
            Codigo = codigo;
            Nombre = nombre;

            Precio = precio;

            Stock = stock;
            StockMinimo = stockMinimo;

            UnidadMedida =
                string.IsNullOrWhiteSpace(unidadMedida)
                    ? "Unidad"
                    : unidadMedida;

            Activo = activo;

            FechaRegistro = DateTime.Now;
            FechaActualizacion = DateTime.Now;
        }

        // =========================================================
        // ESTADO DEL STOCK
        // =========================================================

        public bool TieneStockDisponible()
        {
            return Activo && Stock > 0;
        }

        public bool TieneStockSuficiente(
            int cantidad)
        {
            return cantidad > 0 &&
                   Activo &&
                   Stock >= cantidad;
        }

        public bool EstaEnStockBajo()
        {
            return Activo &&
                   Stock <= StockMinimo;
        }

        public bool EstaAgotado()
        {
            return Activo &&
                   Stock <= 0;
        }

        // =========================================================
        // ACTUALIZACIÓN DE STOCK
        // =========================================================

        public bool AumentarStock(
            int cantidad)
        {
            if (cantidad <= 0)
                return false;

            Stock += cantidad;

            FechaActualizacion =
                DateTime.Now;

            return true;
        }

        public bool ReducirStock(
            int cantidad)
        {
            if (cantidad <= 0)
                return false;

            if (cantidad > Stock)
                return false;

            Stock -= cantidad;

            FechaActualizacion =
                DateTime.Now;

            return true;
        }

        // =========================================================
        // ACTUALIZACIÓN DE DATOS
        // =========================================================

        public void ActualizarDatos(
            string nombre,
            double precio,
            int stockMinimo,
            string unidadMedida)
        {
            Nombre =
                nombre?.Trim() ?? string.Empty;

            Precio = precio;

            StockMinimo =
                stockMinimo < 0
                    ? 0
                    : stockMinimo;

            UnidadMedida =
                string.IsNullOrWhiteSpace(unidadMedida)
                    ? "Unidad"
                    : unidadMedida.Trim();

            FechaActualizacion =
                DateTime.Now;
        }

        // =========================================================
        // ACTIVACIÓN / DESACTIVACIÓN
        // =========================================================

        public void Activar()
        {
            Activo = true;

            FechaActualizacion =
                DateTime.Now;
        }

        public void Desactivar()
        {
            Activo = false;

            FechaActualizacion =
                DateTime.Now;
        }

        // =========================================================
        // INFORMACIÓN DEL PRODUCTO
        // =========================================================

        public string ObtenerEstadoStock()
        {
            if (!Activo)
                return "Inactivo";

            if (Stock <= 0)
                return "Agotado";

            if (Stock <= StockMinimo)
                return "Stock bajo";

            return "Disponible";
        }

        public string ObtenerDescripcionCompleta()
        {
            return
                $"{Codigo} - {Nombre} | " +
                $"S/ {Precio.ToString("0.00", CultureInfo.InvariantCulture)} | " +
                $"Stock: {Stock} {UnidadMedida}";
        }

        // =========================================================
        // SERIALIZACIÓN
        // =========================================================

        public override string ToString()
        {
            return string.Join(
                "|",
                Codigo,
                Nombre,
                Precio.ToString(
                    CultureInfo.InvariantCulture),
                Stock,
                Activo,
                StockMinimo,
                UnidadMedida,
                FechaRegistro.ToString(
                    "O",
                    CultureInfo.InvariantCulture),
                FechaActualizacion.ToString(
                    "O",
                    CultureInfo.InvariantCulture));
        }

        // =========================================================
        // COMPATIBILIDAD CON DATOS ANTERIORES
        // =========================================================

        public static Producto DesdeTexto(
            string linea)
        {
            if (string.IsNullOrWhiteSpace(linea))
                return null;

            string[] partes =
                linea.Split('|');

            try
            {
                // -------------------------------------------------
                // FORMATO ANTIGUO
                //
                // Codigo|Nombre|Precio|Stock|Activo
                // -------------------------------------------------

                if (partes.Length == 5)
                {
                    Producto producto =
                        new Producto
                        {
                            Codigo = partes[0],
                            Nombre = partes[1],
                            Precio =
                                double.Parse(
                                    partes[2],
                                    CultureInfo.InvariantCulture),
                            Stock =
                                int.Parse(partes[3]),
                            Activo =
                                bool.Parse(partes[4]),

                            StockMinimo = 5,
                            UnidadMedida = "Unidad",

                            FechaRegistro =
                                DateTime.Now,

                            FechaActualizacion =
                                DateTime.Now
                        };

                    return producto;
                }

                // -------------------------------------------------
                // FORMATO NUEVO
                //
                // Codigo|Nombre|Precio|Stock|Activo|
                // StockMinimo|UnidadMedida|FechaRegistro|
                // FechaActualizacion
                // -------------------------------------------------

                if (partes.Length >= 9)
                {
                    DateTime fechaRegistro;
                    DateTime fechaActualizacion;

                    if (!DateTime.TryParse(
                        partes[7],
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind,
                        out fechaRegistro))
                    {
                        fechaRegistro =
                            DateTime.Now;
                    }

                    if (!DateTime.TryParse(
                        partes[8],
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind,
                        out fechaActualizacion))
                    {
                        fechaActualizacion =
                            DateTime.Now;
                    }

                    Producto producto =
                        new Producto
                        {
                            Codigo = partes[0],
                            Nombre = partes[1],

                            Precio =
                                double.Parse(
                                    partes[2],
                                    CultureInfo.InvariantCulture),

                            Stock =
                                int.Parse(partes[3]),

                            Activo =
                                bool.Parse(partes[4]),

                            StockMinimo =
                                int.Parse(partes[5]),

                            UnidadMedida =
                                string.IsNullOrWhiteSpace(
                                    partes[6])
                                    ? "Unidad"
                                    : partes[6],

                            FechaRegistro =
                                fechaRegistro,

                            FechaActualizacion =
                                fechaActualizacion
                        };

                    return producto;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }
    }
}
