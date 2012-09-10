using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using org.dicomcs.data;
using org.dicomcs.dict;

namespace MUCalcDemo
{
    public class DICOMLoad
    {
        public DICOMLoad()
        {
        }

        public Dataset Load(String fileName)
        {
            return Load(new FileInfo(fileName));
        }

        public Dataset Load(FileInfo file)
        {
            Stream ins = null;
            DcmParser parser = null;
            Dataset ds = null;

            try
            {
                try
                {
                    ins = new BufferedStream(new FileStream(file.FullName, FileMode.Open, FileAccess.Read));
                    parser = new DcmParser(ins);
                    FileFormat format = parser.DetectFileFormat();
                    if (format != null)
                    {
                        ds = new Dataset();
                        parser.DcmHandler = ds.DcmHandler;
                        parser.ParseDcmFile(format, Tags.PixelData);

                        //MessageBox.Show("Pomyślnie!");

                        return ds;
                    }
                    else
                    {
                        //MessageBox.Show("failed!");
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.StackTrace);
                }
            }
            finally
            {
                if (ins != null)
                {
                    try
                    {
                        ins.Close();
                    }
                    catch (IOException)
                    {
                    }
                }
            }

            return null;
        }
    }
}
