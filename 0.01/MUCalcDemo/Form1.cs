using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using org.dicomcs.data;
using org.dicomcs.dict;

namespace MUCalcDemo
{
    public partial class Form1 : Form
    {
        private int numberOfBeams;

        public Form1()
        {
            InitializeComponent();
        }

        private void openRTPlanButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if(openFileDialog.FileName != "")
                {
                    fileNameTextbox.Text = openFileDialog.FileName;
                    fileLoad(fileNameTextbox.Text);
                }
            }
        }

        private void fileLoad(string fileName)
        {
            DICOMLoad dcmload = new DICOMLoad();

            Dataset ds = dcmload.Load(fileName);

            //w przypadku pomyślnego załadowania pliku
            if (ds != null)
            {
                MessageBox.Show("Pomyślnie załadowano plik!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);

                #region wiązki
                Dataset dsFractionGroupSequence = ds.GetItem(Tags.FractionGroupSeq, 0);

                this.numberOfBeams = Convert.ToInt16(dsFractionGroupSequence.GetString(Tags.NumberOfBeams, 0));

                DcmElement dcmeReferencedBeamSequence = dsFractionGroupSequence.Get(Tags.RefBeamSeq);

                string message = "Liczba wiązek: " + numberOfBeams.ToString() + Environment.NewLine;

                Dataset dsReferencedBeams = null;

                for (int i = 0; i < numberOfBeams; i++)
                {
                    dsReferencedBeams = dcmeReferencedBeamSequence.GetItem(i);
                    message += "Wiązka " + i + " = " + dsReferencedBeams.GetString(Tags.BeamDose, 0) + Environment.NewLine;
                }
                #endregion

                #region listki

                DcmElement dcmeBeamSequence = ds.Get(Tags.BeamSeq);

                for (int i = 0; i < this.numberOfBeams; i++)
                {
                    Dataset dsBeamParams = dcmeBeamSequence.GetItem(i);

                    DcmElement dsBeamLimitingDeviceSequence = dsBeamParams.Get(Tags.BeamLimitingDeviceSeq);

                    Dataset dsLeafs = dsBeamLimitingDeviceSequence.GetItem(1); //bez zmian

                    //Dataset dsLeafPositionBoundaries = dsLeafs.GetItem(Tags.LeafPositionBoundaries, 0);

                    message += Environment.NewLine + "Listki" + Environment.NewLine;

                    //string[] test = dsLeafs.GetStrings(Tags.LeafPositionBoundaries);

                    message += "Parametry " + i + ": " + dsLeafs.GetString(Tags.LeafPositionBoundaries);
                }

                #endregion

                logTextbox.Text = message;
            } //w przeciwnym wypadku
            else
            {
                MessageBox.Show("Błąd ładowania pliku!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
