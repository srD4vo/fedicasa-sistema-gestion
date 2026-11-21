using System;

namespace FediCasa.Models
{
    public class Empleado
    {
        public string Usuario { get; set; }
        public string Contraseña { get; set; }
        public string NombreCompleto { get; set; }
        public string Cargo { get; set; }

        // Indica si esta cuenta es el usuario maestro del sistema.
        public bool EsUsuarioMaestro { get; set; }

        public Empleado()
        {
            Usuario = string.Empty;
            Contraseña = string.Empty;
            NombreCompleto = string.Empty;
            Cargo = string.Empty;
            EsUsuarioMaestro = false;
        }

        public Empleado(
            string usuario,
            string contraseña,
            string nombreCompleto,
            string cargo,
            bool esUsuarioMaestro = false)
        {
            Usuario = usuario;
            Contraseña = contraseña;
            NombreCompleto = nombreCompleto;
            Cargo = cargo;
            EsUsuarioMaestro = esUsuarioMaestro;
        }

        public override string ToString()
        {
            return $"{Usuario}|{Contraseña}|{NombreCompleto}|{Cargo}|{EsUsuarioMaestro}";
        }
    }
}
