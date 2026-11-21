using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using FediCasa.Models;

namespace FediCasa.Helpers
{
    public static class ArchivoHelper
    {
        private static readonly string CarpetaData =
            Path.Combine(AppContext.BaseDirectory, "Data");

        private static readonly string ArchivoProductos =
            Path.Combine(CarpetaData, "productos.txt");

        private static readonly string ArchivoVentas =
            Path.Combine(CarpetaData, "ventas.txt");

        private static readonly string ArchivoEmpleados =
            Path.Combine(CarpetaData, "empleados.txt");

        // =========================================================
        // INICIALIZACIÓN
        // =========================================================

        public static void InicializarCarpetaData()
        {
            if (!Directory.Exists(CarpetaData))
            {
                Directory.CreateDirectory(CarpetaData);
            }
        }

        // =========================================================
        // USO COMPARTIDO ENTRE VARIAS INSTANCIAS DEL PROGRAMA
        // =========================================================

        // Candado entre procesos: mientras una instancia modifica
        // inventario o ventas, las demás esperan su turno.
        private static readonly Mutex candadoDatos =
            new Mutex(false, "FediCasa_Datos_Compartidos");

        private class Candado : IDisposable
        {
            private bool tomado;

            public Candado()
            {
                try
                {
                    tomado = candadoDatos.WaitOne(5000);
                }
                catch (AbandonedMutexException)
                {
                    tomado = true;
                }
            }

            public void Dispose()
            {
                if (tomado)
                {
                    tomado = false;
                    candadoDatos.ReleaseMutex();
                }
            }
        }

        // Uso: using (ArchivoHelper.BloquearDatos()) { ... }
        public static IDisposable BloquearDatos()
        {
            return new Candado();
        }

        // Identifica la versión de un archivo (fecha de cambio + tamaño).
        // Si cambia, otra instancia lo modificó.
        private static string Firma(string ruta)
        {
            if (!File.Exists(ruta))
                return string.Empty;

            FileInfo info = new FileInfo(ruta);

            return info.LastWriteTimeUtc.Ticks + "-" + info.Length;
        }

        public static string FirmaArchivoProductos()
        {
            return Firma(ArchivoProductos);
        }

        public static string FirmaArchivoVentas()
        {
            return Firma(ArchivoVentas);
        }

        // Lee un archivo que otra instancia podría estar escribiendo.
        private static string[] LeerLineasSeguro(string ruta)
        {
            for (int intento = 0; ; intento++)
            {
                try
                {
                    return File.ReadAllLines(ruta);
                }
                catch (IOException) when (intento < 5)
                {
                    Thread.Sleep(50);
                }
            }
        }

        // Escribe en un archivo temporal y lo reemplaza de una sola vez,
        // para que otra instancia nunca lea un archivo a medio escribir.
        private static void EscribirLineasSeguro(
            string ruta,
            IEnumerable<string> lineas)
        {
            string temporal = ruta + ".tmp";

            File.WriteAllLines(temporal, lineas);

            for (int intento = 0; ; intento++)
            {
                try
                {
                    File.Move(temporal, ruta, true);
                    return;
                }
                catch (IOException) when (intento < 5)
                {
                    Thread.Sleep(50);
                }
            }
        }

        // =========================================================
        // PRODUCTOS
        // =========================================================

        public static void GuardarProductos(List<Producto> productos)
        {
            InicializarCarpetaData();

            EscribirLineasSeguro(
                ArchivoProductos,
                productos.Select(p => p.ToString()));
        }

        public static List<Producto> LeerProductos()
        {
            InicializarCarpetaData();

            List<Producto> productos = new List<Producto>();

            if (!File.Exists(ArchivoProductos))
                return productos;

            foreach (string linea in LeerLineasSeguro(ArchivoProductos))
            {
                // DesdeTexto conserva stock mínimo, unidad y fechas.
                Producto producto = Producto.DesdeTexto(linea);

                if (producto != null)
                    productos.Add(producto);
            }

            return productos;
        }

        public static bool ExisteArchivoProductos()
        {
            return File.Exists(ArchivoProductos);
        }

        // =========================================================
        // VENTAS
        // =========================================================

        public static void GuardarVentasNuevas(List<Venta> ventas)
        {
            InicializarCarpetaData();

            using StreamWriter escritor = new StreamWriter(
                ArchivoVentas,
                true);

            foreach (Venta venta in ventas)
            {
                escritor.WriteLine(venta.ToString());
            }
        }

        // Reescribe todo el archivo (necesario para confirmar/anular).
        public static void GuardarVentas(List<Venta> ventas)
        {
            InicializarCarpetaData();

            EscribirLineasSeguro(
                ArchivoVentas,
                ventas.Select(v => v.ToString()));
        }

        public static List<Venta> LeerVentas()
        {
            InicializarCarpetaData();

            List<Venta> ventas = new List<Venta>();

            if (!File.Exists(ArchivoVentas))
                return ventas;

            foreach (string linea in LeerLineasSeguro(ArchivoVentas))
            {
                // DesdeTexto conserva estado, cliente, vendedor, etc.
                Venta venta = Venta.DesdeTexto(linea);

                if (venta != null)
                    ventas.Add(venta);
            }

            return ventas;
        }

        public static int ObtenerUltimoIdVenta()
        {
            List<Venta> ventas = LeerVentas();

            if (ventas.Count == 0)
                return 0;

            int mayorId = 0;

            foreach (Venta venta in ventas)
            {
                if (venta.IdVenta > mayorId)
                {
                    mayorId = venta.IdVenta;
                }
            }

            return mayorId;
        }

        // =========================================================
        // MOVIMIENTOS (HISTORIAL)
        // =========================================================

        private static readonly string ArchivoMovimientos =
            Path.Combine(CarpetaData, "movimientos.txt");

        public static void GuardarMovimiento(Movimiento movimiento)
        {
            InicializarCarpetaData();

            File.AppendAllText(
                ArchivoMovimientos,
                movimiento.ToString() + Environment.NewLine);
        }

        public static List<Movimiento> LeerMovimientos()
        {
            InicializarCarpetaData();

            List<Movimiento> lista = new List<Movimiento>();

            if (!File.Exists(ArchivoMovimientos))
                return lista;

            foreach (string linea in File.ReadAllLines(ArchivoMovimientos))
            {
                Movimiento m = Movimiento.DesdeTexto(linea);

                if (m != null)
                    lista.Add(m);
            }

            return lista;
        }

        // =========================================================
        // ACTIVIDAD ADMINISTRATIVA (para reportes de turno)
        // =========================================================

        private static readonly string ArchivoActividad =
            Path.Combine(CarpetaData, "actividad.txt");

        public static void GuardarActividad(Actividad actividad)
        {
            InicializarCarpetaData();

            File.AppendAllText(
                ArchivoActividad,
                actividad.ToString() + Environment.NewLine);
        }

        public static List<Actividad> LeerActividades()
        {
            InicializarCarpetaData();

            List<Actividad> lista = new List<Actividad>();

            if (!File.Exists(ArchivoActividad))
                return lista;

            foreach (string linea in File.ReadAllLines(ArchivoActividad))
            {
                Actividad a = Actividad.DesdeTexto(linea);

                if (a != null)
                    lista.Add(a);
            }

            return lista;
        }

        // =========================================================
        // SOLICITUDES E INCIDENCIAS
        // =========================================================

        private static readonly string ArchivoSolicitudes =
            Path.Combine(CarpetaData, "solicitudes.txt");

        public static void GuardarSolicitudes(List<Solicitud> solicitudes)
        {
            InicializarCarpetaData();

            using StreamWriter escritor = new StreamWriter(
                ArchivoSolicitudes,
                false);

            foreach (Solicitud s in solicitudes)
            {
                escritor.WriteLine(s.ToString());
            }
        }

        public static List<Solicitud> LeerSolicitudes()
        {
            InicializarCarpetaData();

            List<Solicitud> lista = new List<Solicitud>();

            if (!File.Exists(ArchivoSolicitudes))
                return lista;

            foreach (string linea in File.ReadAllLines(ArchivoSolicitudes))
            {
                Solicitud s = Solicitud.DesdeTexto(linea);

                if (s != null)
                    lista.Add(s);
            }

            return lista;
        }

        // =========================================================
        // EMPLEADOS / USUARIOS
        // =========================================================

        public static void GuardarEmpleados(List<Empleado> empleados)
        {
            InicializarCarpetaData();

            using StreamWriter escritor = new StreamWriter(
                ArchivoEmpleados,
                false);

            foreach (Empleado empleado in empleados)
            {
                escritor.WriteLine(
                    $"{empleado.Usuario}|{empleado.Contraseña}|{empleado.NombreCompleto}|{empleado.Cargo}|{empleado.EsUsuarioMaestro}");
            }
        }

        public static List<Empleado> LeerEmpleados()
        {
            InicializarCarpetaData();

            List<Empleado> empleados = new List<Empleado>();

            if (!File.Exists(ArchivoEmpleados))
                return empleados;

            string[] lineas = File.ReadAllLines(ArchivoEmpleados);

            foreach (string linea in lineas)
            {
                if (string.IsNullOrWhiteSpace(linea))
                    continue;

                string[] partes = linea.Split('|');

                // Formato nuevo:
                // Usuario|Contraseña|Nombre|Cargo|EsUsuarioMaestro
                if (partes.Length >= 5)
                {
                    bool esUsuarioMaestro = false;

                    bool.TryParse(
                        partes[4].Trim(),
                        out esUsuarioMaestro);

                    Empleado empleado = new Empleado
                    {
                        Usuario = partes[0].Trim(),
                        Contraseña = partes[1],
                        NombreCompleto = partes[2].Trim(),
                        Cargo = partes[3].Trim(),
                        EsUsuarioMaestro = esUsuarioMaestro
                    };

                    empleados.Add(empleado);

                    continue;
                }

                // Compatibilidad con el formato anterior:
                // Usuario|Contraseña|Nombre|Cargo
                if (partes.Length >= 4)
                {
                    Empleado empleado = new Empleado
                    {
                        Usuario = partes[0].Trim(),
                        Contraseña = partes[1],
                        NombreCompleto = partes[2].Trim(),
                        Cargo = partes[3].Trim(),
                        EsUsuarioMaestro =
                            partes[0].Trim().Equals(
                                "admin",
                                StringComparison.OrdinalIgnoreCase)
                    };

                    empleados.Add(empleado);
                }
            }

            return empleados;
        }

        public static bool ExisteArchivoEmpleados()
        {
            return File.Exists(ArchivoEmpleados);
        }

        public static void CrearEmpleadosIniciales()
        {
            InicializarCarpetaData();

            if (File.Exists(ArchivoEmpleados))
                return;

            List<Empleado> empleados = new List<Empleado>
            {
                // USUARIO MAESTRO
                new Empleado(
                    "admin",
                    "1234",
                    "Usuario Maestro FediCasa",
                    "Administrador",
                    true),

                // ACTOR: ADMINISTRADOR
                new Empleado(
                    "administrador",
                    "1234",
                    "Administrador FediCasa",
                    "Administrador",
                    false),

                // ACTOR: INVENTARIO
                new Empleado(
                    "inventario",
                    "1234",
                    "Responsable de Inventario",
                    "Inventario",
                    false),

                // ACTOR: VENTAS
                new Empleado(
                    "ventas",
                    "1234",
                    "Personal de Ventas",
                    "Ventas",
                    false)
            };

            GuardarEmpleados(empleados);
        }
    }
}
