using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

//MUCalc v 0.0337
//written by: Łukasz Traczewski (c) 2012, 22-09-2012

namespace MUCalcDemo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1("0.0337 22-09-2012"));
        }
    }
}
