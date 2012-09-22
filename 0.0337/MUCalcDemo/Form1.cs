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

        private double[] beamDoses;

        private double[] TPR;

        private double[] beamMetersets; //zapisane w pliku MU kazdej wiazki

        private double[ , ] leafPositionBoundaries; //granice x listkow dla kazdej wiazki

        public double[] MU { get; private set; } //tablica z wynikami MU dla poszczególnych wiązek

        public Form1(string _progVersion)
        {
            InitializeComponent();

            this.Text += " " + _progVersion;
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

                this.mainOperations(ds);
            } //w przeciwnym wypadku
            else
            {
                MessageBox.Show("Błąd ładowania pliku!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void mainOperations(Dataset _ds) //glowna funkcja
        {
            //inicjalizacja
            double[,] maxDose = new double[1, 2]; //maksymalna dawka i numer wiazki dla tej dawki
            maxDose[0, 0] = 0;
            maxDose[0, 1] = 0;
            double tmpDose = 0; //tymczasowa zmienna przechowujaca ostatnia dawke
            double[] nominalBeamEnergy;
            string[] tmpLeafPostionBoundaries;

            #region wiązki
            Dataset dsFractionGroupSequence = _ds.GetItem(Tags.FractionGroupSeq, 0);

            this.numberOfBeams = Convert.ToInt16(dsFractionGroupSequence.GetString(Tags.NumberOfBeams, 0));

            //inicjalizacja
            this.beamDoses = new double[this.numberOfBeams];
            this.TPR = new double[this.numberOfBeams];
            this.MU = new double[this.numberOfBeams];
            this.beamMetersets = new double[this.numberOfBeams];
            nominalBeamEnergy = new double[this.numberOfBeams];

            DcmElement dcmeReferencedBeamSequence = dsFractionGroupSequence.Get(Tags.RefBeamSeq);

            string message = "Liczba wiązek: " + numberOfBeams.ToString() + Environment.NewLine;

            Dataset dsReferencedBeams = null;

            for (int i = 0; i < numberOfBeams; i++)
            {
                dsReferencedBeams = dcmeReferencedBeamSequence.GetItem(i);
                message += "Wiązka " + (i + 1) + " = " + dsReferencedBeams.GetString(Tags.BeamDose, 0) + Environment.NewLine;
                tmpDose = Convert.ToDouble(dsReferencedBeams.GetString(Tags.BeamDose, 0).Replace('.', ','));

                //wpisanie dawek do tablicy
                this.beamDoses[i] = tmpDose;

                //wpisanie MU do tablicy
                this.beamMetersets[i] = Convert.ToDouble(dsReferencedBeams.GetString(Tags.BeamMeterset, 0).Replace('.', ','));

                message += "Zapisane MU = " + this.beamMetersets[i].ToString() + Environment.NewLine; //wyświetlenie

                //maksymalna dawka
                if (tmpDose > maxDose[0, 0])
                {
                    maxDose[0, 0] = tmpDose;
                    maxDose[0, 1] = i; //numer wiazki
                }
            }

            message += "Maksymalna dawka: " + Math.Round(maxDose[0, 0], 3).ToString() + ", wiązka nr " + maxDose[0, 1].ToString() + Environment.NewLine;
            #endregion

            #region listki i nominalna energia wiązek

            DcmElement dcmeBeamSequence = _ds.Get(Tags.BeamSeq);

            for (int i = 0; i < this.numberOfBeams; i++)
            {
                Dataset dsBeamParams = dcmeBeamSequence.GetItem(i);

                DcmElement dcmeBeamLimitingDeviceSequence = dsBeamParams.Get(Tags.BeamLimitingDeviceSeq);

                Dataset dsLeafs = dcmeBeamLimitingDeviceSequence.GetItem(1); //bez zmian

                //Dataset dsLeafPositionBoundaries = dsLeafs.GetItem(Tags.LeafPositionBoundaries, 0);

                message += Environment.NewLine + "Listki" + Environment.NewLine;

                //string[] test = dsLeafs.GetStrings(Tags.LeafPositionBoundaries);

                tmpLeafPostionBoundaries = dsLeafs.GetString(Tags.LeafPositionBoundaries).Split('\\'); //wczytanie wsp x listkow

                message += "Parametry " + i + ": " + dsLeafs.GetString(Tags.LeafPositionBoundaries);

                //jesli nie istnieje tablica tworzymy ja
                if (this.leafPositionBoundaries == null)
                {
                    this.leafPositionBoundaries = new double[this.numberOfBeams, tmpLeafPostionBoundaries.Length];
                }

                for (int j = 0; j < tmpLeafPostionBoundaries.Length; j++)
                {
                    this.leafPositionBoundaries[i, j] = Convert.ToDouble(tmpLeafPostionBoundaries[j].Replace('.', ','));
                }

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
                this.TPR[i] = this.beamDoses[i] / nominalBeamEnergy[i];
            }
            //message += "TPR:  " + this.TPR.ToString() + Environment.NewLine;
            #endregion

            logTextbox.Text = message;
        }

        //metoda licząca MU
        private double calculateMU(double beamDose, double tissuePhantomRatio, double offAxisFactor, double refFieldCal)
        {
            double MU = beamDose / Math.Abs(tissuePhantomRatio * offAxisFactor * refFieldCal);

            return MU;
        }

        private bool loadTables(string _tablePath = "./Data/")
        {
            TextTableLoader tl = new TextTableLoader();

            bool properLoaded = tl.Load(_tablePath + "pdg.dat");

            #region tymczasowe wczytywanie danych z list do stringow
            string tmp2 = ""; //wynikowy string dla nazw wierszy(tymczasowy)
            string tmp3 = ""; //wynikowy string dla nazw kolumn(tymczasowy)
            string tmp4 = ""; //wynikowy string dla danych(tymczasowy)

            foreach (string rowName in tl.rowHeaders)
            {
                tmp2 += rowName + " ";
            }

            foreach (string columnName in tl.columnHeaders)
            {
                tmp3 += columnName + " ";
            }

            foreach (List<string> row in tl.rowsData)
            {
                foreach (string rowData in row)
                {
                    tmp4 += rowData + " ";
                }

                tmp4 += '\n';
            }
            #endregion

            //wyniki(tymczasowe)
            if (MessageBox.Show(tmp2, "Wiersze: " + tl.rowNumber.ToString()) == DialogResult.OK)
            {
                if (MessageBox.Show(tmp3, "Kolumny: " + tl.columnNumber.ToString()) == DialogResult.OK)
                {
                    MessageBox.Show(tmp4, "Dane");
                }
            }

            properLoaded = tl.Load(_tablePath + "wyd.dat");

            #region tymczasowe wczytywanie danych z list do stringow
            tmp2 = ""; //wynikowy string dla nazw wierszy(tymczasowy)
            tmp3 = ""; //wynikowy string dla nazw kolumn(tymczasowy)
            tmp4 = ""; //wynikowy string dla danych(tymczasowy)

            foreach (string rowName in tl.rowHeaders)
            {
                tmp2 += rowName + " ";
            }

            foreach (string columnName in tl.columnHeaders)
            {
                tmp3 += columnName + " ";
            }

            foreach (List<string> row in tl.rowsData)
            {
                foreach (string rowData in row)
                {
                    tmp4 += rowData + " ";
                }

                tmp4 += '\n';
            }
            #endregion

            //wyniki(tymczasowe)
            if (MessageBox.Show(tmp2, "Wiersze: " + tl.rowNumber.ToString()) == DialogResult.OK)
            {
                if (MessageBox.Show(tmp3, "Kolumny: " + tl.columnNumber.ToString()) == DialogResult.OK)
                {
                    MessageBox.Show(tmp4, "Dane");
                }
            }

            return properLoaded;
        }

        private void countMUButton_Click(object sender, EventArgs e)
        {
            string tmpMsg = ""; //tymczasowo przechowywany tekst, wyskakujacy w okienku

            //TO DO!!!!!!!
            #region ladowanie wydajnosci

            //bool properLoad = this.loadTables();

            #endregion

            for (int i = 0; i < this.numberOfBeams; i++)
            {
                this.MU[i] = this.calculateMU(this.beamDoses[i], this.TPR[i], 1, 100);
                tmpMsg += "Wiązka " + (i + 1) + " MU = " + this.MU[i] + Environment.NewLine;
            }

            MessageBox.Show(tmpMsg, "Wynik", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void uselessTextbox_TextChanged(object sender, EventArgs e)
        {
            if (this.openTableFileDialog.ShowDialog() == DialogResult.OK)
            {
                //this.uselessTextbox.Text = this.openTableFileDialog.FileName;
            }
        }
    }
}
