using System;

namespace FediCasa.Models
{
    /// <summary>
    /// Representa un producto agregado temporalmente
    /// al carrito de compras durante el proceso de venta.
    ///
    /// Este objeto NO representa todavía una venta registrada.
    /// Se convierte en una Venta cuando el usuario confirma
    /// la operación desde FrmVentas.
    /// </summary>
    public class ItemCarrito
    {
        // ─────────────────────────────────────────────
        // PROPIEDADES
        // ─────────────────────────────────────────────

        /// <summary>
        /// Código del producto agregado al carrito.
        /// </summary>
        public string CodigoProducto { get; set; }

        /// <summary>
        /// Nombre del producto.
        /// </summary>
        public string NombreProducto { get; set; }

        /// <summary>
        /// Precio unitario del producto.
        /// </summary>
        public double Precio { get; set; }

        /// <summary>
        /// Cantidad de unidades agregadas al carrito.
        /// </summary>
        public int Cantidad { get; set; }

        /// <summary>
        /// Subtotal del producto.
        /// Se obtiene multiplicando Precio × Cantidad.
        /// </summary>
        public double Subtotal
        {
            get
            {
                return Precio * Cantidad;
            }
        }


        // ─────────────────────────────────────────────
        // CONSTRUCTORES
        // ─────────────────────────────────────────────

        /// <summary>
        /// Constructor vacío.
        /// </summary>
        public ItemCarrito()
        {
            CodigoProducto = string.Empty;
            NombreProducto = string.Empty;
            Precio = 0;
            Cantidad = 0;
        }

        /// <summary>
        /// Constructor completo.
        /// </summary>
        public ItemCarrito(
            string codigoProducto,
            string nombreProducto,
            double precio,
            int cantidad)
        {
            CodigoProducto = codigoProducto;
            NombreProducto = nombreProducto;
            Precio = precio;
            Cantidad = cantidad;
        }


        // ─────────────────────────────────────────────
        // MÉTODOS
        // ─────────────────────────────────────────────

        /// <summary>
        /// Actualiza la cantidad del producto en el carrito.
        /// </summary>
        public void ActualizarCantidad(int nuevaCantidad)
        {
            if (nuevaCantidad > 0)
            {
                Cantidad = nuevaCantidad;
            }
        }

        /// <summary>
        /// Obtiene el subtotal del producto.
        /// </summary>
        public double ObtenerSubtotal()
        {
            return Precio * Cantidad;
        }

        /// <summary>
        /// Representación del producto para mostrar información
        /// dentro del sistema.
        /// </summary>
        public override string ToString()
        {
            return CodigoProducto + " - " +
                   NombreProducto + " x " +
                   Cantidad + " = S/ " +
                   Subtotal.ToString("0.00");
        }
    }
}

