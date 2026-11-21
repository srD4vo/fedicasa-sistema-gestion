using System;
using System.Windows.Forms;
using FediCasa.Forms;

namespace FediCasa
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal de la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Configuración inicial de Windows Forms.
            ApplicationConfiguration.Initialize();

            // Inicia FediCasa mostrando primero el formulario de Login.
            Application.Run(new FrmLogin());
        }
    }
}
