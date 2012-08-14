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

        private double beamDoseSum;

        private double[] beamDoses;

        private double TPR;

        public double MU { get; private set; }

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
            //inicjalizacja
            this.beamDoseSum = 0; //suma dawek poszczegolnych wiazek
            double [,] maxDose = new double [1, 2]; //maksymalna dawka i numer wiazki dla tej dawki
            maxDose[0, 0] = 0;
            maxDose[0, 1] = 0;
            double tmpDose = 0; //tymczasowa zmienna przechowujaca ostatnia dawke
            this.TPR = 0;
            double[] nominalBeamEnergy;

            DICOMLoad dcmload = new DICOMLoad();

            Dataset ds = dcmload.Load(fileName);

            //w przypadku pomyślnego załadowania pliku
            if (ds != null)
            {
                MessageBox.Show("Pomyślnie załadowano plik!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);

                #region wiązki
                Dataset dsFractionGroupSequence = ds.GetItem(Tags.FractionGroupSeq, 0);

                this.numberOfBeams = Convert.ToInt16(dsFractionGroupSequence.GetString(Tags.NumberOfBeams, 0));

                //inicjalizacja
                this.beamDoses = new double[this.numberOfBeams];
                nominalBeamEnergy = new double[this.numberOfBeams];

                DcmElement dcmeReferencedBeamSequence = dsFractionGroupSequence.Get(Tags.RefBeamSeq);

                string message = "Liczba wiązek: " + numberOfBeams.ToString() + Environment.NewLine;

                Dataset dsReferencedBeams = null;

                for (int i = 0; i < numberOfBeams; i++)
                {
                    dsReferencedBeams = dcmeReferencedBeamSequence.GetItem(i);
                    message += "Wiązka " + i + " = " + dsReferencedBeams.GetString(Tags.BeamDose, 0) + Environment.NewLine;
                    tmpDose = Convert.ToDouble(dsReferencedBeams.GetString(Tags.BeamDose, 0).Replace('.', ','));
                    this.beamDoseSum += tmpDose;

                    //wpisanie dawek do tablicy
                    this.beamDoses[i] = tmpDose;

                    //maksymalna dawka
                    if (tmpDose > maxDose[0, 0])
                    {
                        maxDose[0, 0] = tmpDose;
                        maxDose[0, 1] = i; //numer wiazki
                    }
                }

                message += "Suma dawek wszystkich wiązek: " + Math.Round(this.beamDoseSum, 3).ToString() + Environment.NewLine;
                message += "Maksymalna dawka: " + Math.Round(maxDose[0, 0], 3).ToString() + ", wiązka nr " + maxDose[0, 1].ToString() + Environment.NewLine;
                #endregion

                #region listki i nominalna energia wiązek

                DcmElement dcmeBeamSequence = ds.Get(Tags.BeamSeq);

                for (int i = 0; i < this.numberOfBeams; i++)
                {
                    Dataset dsBeamParams = dcmeBeamSequence.GetItem(i);

                    DcmElement dcmeBeamLimitingDeviceSequence = dsBeamParams.Get(Tags.BeamLimitingDeviceSeq);

                    Dataset dsLeafs = dcmeBeamLimitingDeviceSequence.GetItem(1); //bez zmian

                    //Dataset dsLeafPositionBoundaries = dsLeafs.GetItem(Tags.LeafPositionBoundaries, 0);

                    message += Environment.NewLine + "Listki" + Environment.NewLine;

                    //string[] test = dsLeafs.GetStrings(Tags.LeafPositionBoundaries);

                    message += "Parametry " + i + ": " + dsLeafs.GetString(Tags.LeafPositionBoundaries);

                    //nominal beam energy
                    DcmElement dcmeControlPointSequence = dsBeamParams.Get(Tags.ControlPointSeq);

                    Dataset dsnominalBeamEnergy = dcmeControlPointSequence.GetItem(0);

                    nominalBeamEnergy[i] = Convert.ToDouble(dsnominalBeamEnergy.GetString(Tags.NominalBeamEnergy).Replace('.', ','));

                    //message += "nominalBeamEnergy:  " + nominalBeamEnergy.ToString() + Environment.NewLine;
                }

                #endregion

                #region obliczanie TPR
                for (int i = 0; i < numberOfBeams; i++)
                {
                    this.TPR += nominalBeamEnergy[i] / this.beamDoses[i];
                }
                //message += "TPR:  " + this.TPR.ToString() + Environment.NewLine;
                #endregion

                logTextbox.Text = message;
            } //w przeciwnym wypadku
            else
            {
                MessageBox.Show("Błąd ładowania pliku!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        //metoda licząca MU
        private double calculateMU(double doseSum, double tissuePhantomRatio, double offAxisFactor, double refFieldCal)
        {
            double MU = doseSum / Math.Abs(tissuePhantomRatio * offAxisFactor * refFieldCal);

            return MU;
        }

        private void countMUButton_Click(object sender, EventArgs e)
        {
            this.MU = this.calculateMU(this.beamDoseSum, this.TPR, 1, 100);

            MessageBox.Show("MU = " + this.MU, "Wynik", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
