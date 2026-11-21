using System;
using System.Collections.Generic;
using System.Linq;
using FediCasa.Helpers;
using FediCasa.Models;

namespace FediCasa.Services
{
    public static class MovimientoService
    {
        public static void Registrar(
            string tipo,
            string codigoProducto,
            int cantidad,
            string detalle)
        {
            Producto producto =
                ProductoService.BuscarPorCodigo(codigoProducto);

            Movimiento movimiento = new Movimiento
            {
                Fecha = DateTime.Now,
                Tipo = tipo,
                CodigoProducto = codigoProducto,
                NombreProducto = producto?.Nombre ?? string.Empty,
                Cantidad = cantidad,
                StockResultante = producto?.Stock ?? 0,
                Usuario = SesionService.ObtenerUsuarioActual(),
                Detalle = detalle ?? string.Empty
            };

            ArchivoHelper.GuardarMovimiento(movimiento);
        }

        // Más recientes primero.
        public static List<Movimiento> Obtener()
        {
            return ArchivoHelper.LeerMovimientos()
                .OrderByDescending(m => m.Fecha)
                .ToList();
        }
    }
}
