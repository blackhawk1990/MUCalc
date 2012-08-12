using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

//MUCalc Demo v 0.03
//written by: Łukasz Traczewski (c) 2012, 12-08-2012

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
            Application.Run(new Form1());
        }
    }
}
