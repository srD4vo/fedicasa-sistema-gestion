using System;
using System.Globalization;

namespace FediCasa.Models
{
    public class Venta
    {
        // =========================================================
        // IDENTIFICACIÓN DE LA VENTA
        // =========================================================

        public int IdVenta { get; set; }

        public string NumeroComprobante { get; set; }

        public string TipoComprobante { get; set; }

        // =========================================================
        // PRODUCTO
        // =========================================================

        public string CodigoProducto { get; set; }

        public string NombreProducto { get; set; }

        public int Cantidad { get; set; }

        public double Precio { get; set; }

        public double Subtotal { get; set; }

        // =========================================================
        // INFORMACIÓN COMERCIAL
        // =========================================================

        public double IGV { get; set; }

        public double Total { get; set; }

        public string FormaPago { get; set; }

        // =========================================================
        // CLIENTE
        // =========================================================

        public string DocumentoCliente { get; set; }

        public string NombreCliente { get; set; }

        // =========================================================
        // USUARIO RESPONSABLE
        // =========================================================

        public string UsuarioVendedor { get; set; }

        public string NombreVendedor { get; set; }

        // =========================================================
        // ESTADO
        // =========================================================

        public string Estado { get; set; }

        // =========================================================
        // FECHAS
        // =========================================================

        public DateTime Fecha { get; set; }

        public DateTime FechaConfirmacion { get; set; }

        // =========================================================
        // CONSTRUCTORES
        // =========================================================

        public Venta()
        {
            IdVenta = 0;

            NumeroComprobante = string.Empty;
            TipoComprobante = "Boleta";

            CodigoProducto = string.Empty;
            NombreProducto = string.Empty;

            Cantidad = 0;
            Precio = 0;
            Subtotal = 0;

            IGV = 0;
            Total = 0;

            FormaPago = "Efectivo";

            DocumentoCliente = string.Empty;
            NombreCliente = "Cliente general";

            UsuarioVendedor = string.Empty;
            NombreVendedor = string.Empty;

            Estado = "Registrada";

            Fecha = DateTime.Now;
            FechaConfirmacion = DateTime.MinValue;
        }

        public Venta(
            int idVenta,
            string codigoProducto,
            string nombreProducto,
            int cantidad,
            double precio)
        {
            IdVenta = idVenta;

            NumeroComprobante = string.Empty;
            TipoComprobante = "Boleta";

            CodigoProducto = codigoProducto;
            NombreProducto = nombreProducto;

            Cantidad = cantidad;
            Precio = precio;

            Subtotal =
                Math.Round(
                    cantidad * precio,
                    2);

            IGV =
                Math.Round(
                    Subtotal -
                    (Subtotal / 1.18),
                    2);

            Total =
                Math.Round(
                    Subtotal,
                    2);

            FormaPago = "Efectivo";

            DocumentoCliente = string.Empty;
            NombreCliente = "Cliente general";

            UsuarioVendedor = string.Empty;
            NombreVendedor = string.Empty;

            Estado = "Registrada";

            Fecha = DateTime.Now;
            FechaConfirmacion = DateTime.MinValue;
        }

        // =========================================================
        // CÁLCULOS
        // =========================================================

        public void CalcularImportes()
        {
            Subtotal =
                Math.Round(
                    Cantidad * Precio,
                    2);

            double valorVenta =
                Subtotal / 1.18;

            IGV =
                Math.Round(
                    Subtotal - valorVenta,
                    2);

            Total =
                Math.Round(
                    Subtotal,
                    2);
        }

        public double ObtenerSubtotal()
        {
            return Math.Round(
                Cantidad * Precio,
                2);
        }

        public double ObtenerValorVenta()
        {
            return Math.Round(
                Total / 1.18,
                2);
        }

        public double ObtenerIGV()
        {
            return Math.Round(
                Total - ObtenerValorVenta(),
                2);
        }

        // =========================================================
        // ESTADO DE LA VENTA
        // =========================================================

        public bool EstaRegistrada()
        {
            return Estado.Equals(
                "Registrada",
                StringComparison.OrdinalIgnoreCase);
        }

        public bool EstaConfirmada()
        {
            return Estado.Equals(
                "Confirmada",
                StringComparison.OrdinalIgnoreCase);
        }

        public bool EstaAnulada()
        {
            return Estado.Equals(
                "Anulada",
                StringComparison.OrdinalIgnoreCase);
        }

        public void Confirmar()
        {
            Estado = "Confirmada";

            FechaConfirmacion =
                DateTime.Now;
        }

        public void Anular()
        {
            Estado = "Anulada";
        }

        // =========================================================
        // INFORMACIÓN DEL CLIENTE
        // =========================================================

        public void AsignarCliente(
            string documento,
            string nombre)
        {
            DocumentoCliente =
                documento?.Trim() ?? string.Empty;

            NombreCliente =
                string.IsNullOrWhiteSpace(nombre)
                    ? "Cliente general"
                    : nombre.Trim();
        }

        // =========================================================
        // INFORMACIÓN DEL VENDEDOR
        // =========================================================

        public void AsignarVendedor(
            string usuario,
            string nombre)
        {
            UsuarioVendedor =
                usuario?.Trim() ?? string.Empty;

            NombreVendedor =
                nombre?.Trim() ?? string.Empty;
        }

        // =========================================================
        // COMPROBANTE
        // =========================================================

        public void AsignarComprobante(
            string tipo,
            string numero)
        {
            TipoComprobante =
                string.IsNullOrWhiteSpace(tipo)
                    ? "Boleta"
                    : tipo.Trim();

            NumeroComprobante =
                numero?.Trim() ?? string.Empty;
        }

        // =========================================================
        // FORMA DE PAGO
        // =========================================================

        public void AsignarFormaPago(
            string formaPago)
        {
            FormaPago =
                string.IsNullOrWhiteSpace(formaPago)
                    ? "Efectivo"
                    : formaPago.Trim();
        }

        // =========================================================
        // DESCRIPCIÓN
        // =========================================================

        public string ObtenerDescripcion()
        {
            return
                $"{CodigoProducto} - " +
                $"{NombreProducto} x {Cantidad}";
        }

        public string ObtenerResumen()
        {
            return
                $"Venta #{IdVenta} | " +
                $"{NombreProducto} | " +
                $"Cantidad: {Cantidad} | " +
                $"Total: S/ {Total.ToString(
                    "0.00",
                    CultureInfo.InvariantCulture)}";
        }

        // =========================================================
        // SERIALIZACIÓN
        // =========================================================

        public override string ToString()
        {
            return string.Join(
                "|",

                IdVenta,

                CodigoProducto,
                NombreProducto,

                Cantidad,

                Precio.ToString(
                    CultureInfo.InvariantCulture),

                Subtotal.ToString(
                    CultureInfo.InvariantCulture),

                Fecha.ToString(
                    "O",
                    CultureInfo.InvariantCulture),

                NumeroComprobante,
                TipoComprobante,

                IGV.ToString(
                    CultureInfo.InvariantCulture),

                Total.ToString(
                    CultureInfo.InvariantCulture),

                FormaPago,

                DocumentoCliente,
                NombreCliente,

                UsuarioVendedor,
                NombreVendedor,

                Estado,

                FechaConfirmacion.ToString(
                    "O",
                    CultureInfo.InvariantCulture));
        }

        // =========================================================
        // LECTURA DESDE ARCHIVO
        // =========================================================

        public static Venta DesdeTexto(
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
                // IdVenta|Codigo|Nombre|Cantidad|
                // Precio|Subtotal|Fecha
                // -------------------------------------------------

                if (partes.Length == 7)
                {
                    Venta venta =
                        new Venta
                        {
                            IdVenta =
                                int.Parse(partes[0]),

                            CodigoProducto =
                                partes[1],

                            NombreProducto =
                                partes[2],

                            Cantidad =
                                int.Parse(partes[3]),

                            Precio =
                                double.Parse(
                                    partes[4],
                                    CultureInfo.InvariantCulture),

                            Subtotal =
                                double.Parse(
                                    partes[5],
                                    CultureInfo.InvariantCulture),

                            Fecha =
                                DateTime.TryParseExact(
                                    partes[6],
                                    "dd/MM/yyyy HH:mm:ss",
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None,
                                    out DateTime fechaAntigua)
                                    ? fechaAntigua
                                    : DateTime.Now,

                            NumeroComprobante =
                                string.Empty,

                            TipoComprobante =
                                "Boleta",

                            FormaPago =
                                "Efectivo",

                            DocumentoCliente =
                                string.Empty,

                            NombreCliente =
                                "Cliente general",

                            UsuarioVendedor =
                                string.Empty,

                            NombreVendedor =
                                string.Empty,

                            Estado =
                                "Registrada",

                            FechaConfirmacion =
                                DateTime.MinValue
                        };

                    venta.IGV =
                        Math.Round(
                            venta.Subtotal -
                            (venta.Subtotal / 1.18),
                            2);

                    venta.Total =
                        venta.Subtotal;

                    return venta;
                }

                // -------------------------------------------------
                // FORMATO NUEVO
                // -------------------------------------------------

                if (partes.Length >= 18)
                {
                    DateTime fecha;
                    DateTime fechaConfirmacion;

                    if (!DateTime.TryParse(
                        partes[6],
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind,
                        out fecha))
                    {
                        fecha = DateTime.Now;
                    }

                    if (!DateTime.TryParse(
                        partes[17],
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.RoundtripKind,
                        out fechaConfirmacion))
                    {
                        fechaConfirmacion =
                            DateTime.MinValue;
                    }

                    Venta venta =
                        new Venta
                        {
                            IdVenta =
                                int.Parse(partes[0]),

                            CodigoProducto =
                                partes[1],

                            NombreProducto =
                                partes[2],

                            Cantidad =
                                int.Parse(partes[3]),

                            Precio =
                                double.Parse(
                                    partes[4],
                                    CultureInfo.InvariantCulture),

                            Subtotal =
                                double.Parse(
                                    partes[5],
                                    CultureInfo.InvariantCulture),

                            Fecha = fecha,

                            NumeroComprobante =
                                partes[7],

                            TipoComprobante =
                                partes[8],

                            IGV =
                                double.Parse(
                                    partes[9],
                                    CultureInfo.InvariantCulture),

                            Total =
                                double.Parse(
                                    partes[10],
                                    CultureInfo.InvariantCulture),

                            FormaPago =
                                partes[11],

                            DocumentoCliente =
                                partes[12],

                            NombreCliente =
                                partes[13],

                            UsuarioVendedor =
                                partes[14],

                            NombreVendedor =
                                partes[15],

                            Estado =
                                partes[16],

                            FechaConfirmacion =
                                fechaConfirmacion
                        };

                    return venta;
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
