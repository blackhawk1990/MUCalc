using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MUCalcDemo
{
    interface ITableLoader
    {
        int columnNumber { get; set; }
        int rowNumber { get; set; }
        string[] columnHeaders { get; set; }
        string[] rowHeaders { get; set; }

        void Load(string _fileName);
    }
}
