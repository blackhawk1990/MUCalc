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
        enum TableType {wyd, pdg};

        private string progVersion; //wersja programu

        private const string defaultTablePath = "./Data/PRIMUS 15MV/"; //domyslna sciezka do plikow z tabelami dla danego aparatu

        private int numberOfBeams;

        private double[] beamDoses; //dawki(cGy)

        private double[] TPR;

        private double[] beamMetersets; //zapisane w pliku MU kazdej wiazki

        private double[] SSD; //SSD dla kazdej wiazki(cm)

        private double[] SSDDepth; //glebokosc izocentrum dla kazdej wiazki(cm)

        private double[ , ] leafPositionBoundaries; //granice x listkow dla kazdej wiazki(mm)

        private double[,] leafJawPositionsX; //pozycje x szczek

        private double[,] leafJawPositionsY; //pozycje y szczek

        public double[] MU { get; private set; } //tablica z wynikami MU dla poszczególnych wiazek

        private TextTableLoader tlWyd; //obiekt przechowujacy dane z tabeli wydajnosci danego aparatu(cGy/MU)

        private TextTableLoader tlPDG; //obiekt przechowujacy dane z tabeli pdg danego aparatu

        public Form1(string _progVersion)
        {
            InitializeComponent();

            this.Text += " " + _progVersion;

            this.progVersion = _progVersion;

            #region ladowanie tabeli wydajnosci i pdg

            bool properLoadWyd = this.loadTables(ref this.tlWyd, defaultTablePath + "wyd.dat");
            bool properLoadPDG = this.loadTables(ref this.tlPDG, defaultTablePath + "pdg.dat");

            if (properLoadWyd && properLoadPDG)
            {
                MessageBox.Show("Pomyślnie załadowano pliki tabel wydajności i pdg!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (properLoadWyd)
            {
                MessageBox.Show("Pomyślnie załadowano plik tabeli wydajności!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MessageBox.Show("Błąd ładowania pliku tabeli pdg!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (properLoadPDG)
            {
                MessageBox.Show("Pomyślnie załadowano plik tabeli pdg!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MessageBox.Show("Błąd ładowania pliku tabeli wydajności!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Błąd ładowania plików tabel wydajności i pdg!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            #endregion
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
            string[] tmpLeafPostionBoundariesOrJaws;

            #region wiązki
            Dataset dsFractionGroupSequence = _ds.GetItem(Tags.FractionGroupSeq, 0);

            this.numberOfBeams = Convert.ToInt16(dsFractionGroupSequence.GetString(Tags.NumberOfBeams, 0));

            //inicjalizacja
            this.beamDoses = new double[this.numberOfBeams];
            this.TPR = new double[this.numberOfBeams];
            this.MU = new double[this.numberOfBeams];
            this.SSD = new double[this.numberOfBeams];
            this.SSDDepth = new double[this.numberOfBeams];
            this.beamMetersets = new double[this.numberOfBeams];
            nominalBeamEnergy = new double[this.numberOfBeams];

            DcmElement dcmeReferencedBeamSequence = dsFractionGroupSequence.Get(Tags.RefBeamSeq);

            string message = "Liczba wiązek: " + numberOfBeams.ToString() + Environment.NewLine;

            Dataset dsReferencedBeams = null;

            for (int i = 0; i < numberOfBeams; i++)
            {
                dsReferencedBeams = dcmeReferencedBeamSequence.GetItem(i);
                tmpDose = Convert.ToDouble(dsReferencedBeams.GetString(Tags.BeamDose, 0).Replace('.', ',')) * 100; //aut zamiana dawki na cGy
                message += "Wiązka " + (i + 1) + " = " + Math.Round(tmpDose, 2).ToString() + " cGy" + Environment.NewLine;

                //wpisanie dawek do tablicy
                this.beamDoses[i] = tmpDose;

                //wpisanie MU do tablicy
                this.beamMetersets[i] = Convert.ToDouble(dsReferencedBeams.GetString(Tags.BeamMeterset, 0).Replace('.', ','));

                message += "Zapisane MU = " + Math.Round(this.beamMetersets[i], 2).ToString() + Environment.NewLine; //wyświetlenie

                //maksymalna dawka
                if (tmpDose > maxDose[0, 0])
                {
                    maxDose[0, 0] = tmpDose;
                    maxDose[0, 1] = i; //numer wiazki
                }
            }

            message += "Maksymalna dawka: " + Math.Round(maxDose[0, 0], 3).ToString() + ", wiązka nr " + maxDose[0, 1].ToString() + Environment.NewLine;
            #endregion

            #region listki, nominalna energia wiązek i SSD

            DcmElement dcmeBeamSequence = _ds.Get(Tags.BeamSeq);

            for (int i = 0; i < this.numberOfBeams; i++)
            {
                Dataset dsBeamParams = dcmeBeamSequence.GetItem(i);

                DcmElement dcmeBeamLimitingDeviceSequence = dsBeamParams.Get(Tags.BeamLimitingDeviceSeq);

                Dataset dsLeafs = dcmeBeamLimitingDeviceSequence.GetItem(1);

                message += Environment.NewLine + "Listki" + Environment.NewLine;

                //string[] test = dsLeafs.GetStrings(Tags.LeafPositionBoundaries);

                tmpLeafPostionBoundariesOrJaws = dsLeafs.GetString(Tags.LeafPositionBoundaries).Split('\\'); //wczytanie wsp x listkow

                message += "Parametry " + i + ": " + dsLeafs.GetString(Tags.LeafPositionBoundaries);

                //jesli nie istnieje tablica tworzymy ja lub (jesli istnieje) zmieniamy jej rozmiar, gdy jest za maly/duzy dla nowego pliku
                if (this.leafPositionBoundaries == null || (this.leafPositionBoundaries.Length / tmpLeafPostionBoundariesOrJaws.Length) != this.numberOfBeams || (this.leafPositionBoundaries.Length / this.numberOfBeams) < tmpLeafPostionBoundariesOrJaws.Length)
                {
                    this.leafPositionBoundaries = new double[this.numberOfBeams, tmpLeafPostionBoundariesOrJaws.Length];
                }

                for (int j = 0; j < tmpLeafPostionBoundariesOrJaws.Length; j++)
                {
                    this.leafPositionBoundaries[i, j] = Convert.ToDouble(tmpLeafPostionBoundariesOrJaws[j].Replace('.', ','));
                }

                DcmElement dcmeControlPointSequence = dsBeamParams.Get(Tags.ControlPointSeq);

                Dataset dsControlPointSequence = dcmeControlPointSequence.GetItem(0);

                #region nominal beam energy
                nominalBeamEnergy[i] = Convert.ToDouble(dsControlPointSequence.GetString(Tags.NominalBeamEnergy).Replace('.', ','));

                message += Environment.NewLine + "nominalBeamEnergy:  " + nominalBeamEnergy[i].ToString() + Environment.NewLine;
                #endregion

                #region leaf jaw positions
                DcmElement dcmeBeamLimitingDevicePositionSequence = dsControlPointSequence.Get(Tags.BeamLimitingDevicePositionSeq);

                //wsp x
                Dataset dsLeafJawPositionsX = dcmeBeamLimitingDevicePositionSequence.GetItem(1);

                message += Environment.NewLine + "LeafJawPositions(X):  " + dsLeafJawPositionsX.GetString(Tags.LeafJawPositions) + Environment.NewLine;

                #region zapisanie do tablicy
                tmpLeafPostionBoundariesOrJaws = dsLeafJawPositionsX.GetString(Tags.LeafJawPositions).Split('\\'); //tymczasowa tablica

                //jesli nie istnieje tablica tworzymy ja lub (jesli istnieje) zmieniamy jej rozmiar, gdy jest za maly/duzy dla nowego pliku
                if (this.leafJawPositionsX == null || (this.leafJawPositionsX.Length / tmpLeafPostionBoundariesOrJaws.Length) != this.numberOfBeams || (this.leafJawPositionsX.Length / this.numberOfBeams) < tmpLeafPostionBoundariesOrJaws.Length)
                {
                    this.leafJawPositionsX = new double[this.numberOfBeams, tmpLeafPostionBoundariesOrJaws.Length];
                }

                for (int j = 0; j < tmpLeafPostionBoundariesOrJaws.Length; j++)
                {
                    this.leafJawPositionsX[i, j] = Convert.ToDouble(tmpLeafPostionBoundariesOrJaws[j].Replace('.', ','));
                }
                #endregion

                //wsp y
                Dataset dsLeafJawPositionsY = dcmeBeamLimitingDevicePositionSequence.GetItem(0);

                message += Environment.NewLine + "LeafJawPositions(Y):  " + dsLeafJawPositionsY.GetString(Tags.LeafJawPositions) + Environment.NewLine;

                #region zapisanie do tablicy
                tmpLeafPostionBoundariesOrJaws = dsLeafJawPositionsY.GetString(Tags.LeafJawPositions).Split('\\'); //tymczasowa tablica

                //jesli nie istnieje tablica tworzymy ja lub (jesli istnieje) zmieniamy jej rozmiar, gdy jest za maly/duzy dla nowego pliku
                if (this.leafJawPositionsY == null || (this.leafJawPositionsY.Length / tmpLeafPostionBoundariesOrJaws.Length) != this.numberOfBeams || (this.leafJawPositionsY.Length / this.numberOfBeams) < tmpLeafPostionBoundariesOrJaws.Length)
                {
                    this.leafJawPositionsY = new double[this.numberOfBeams, tmpLeafPostionBoundariesOrJaws.Length];
                }

                for (int j = 0; j < tmpLeafPostionBoundariesOrJaws.Length; j++)
                {
                    this.leafJawPositionsY[i, j] = Convert.ToDouble(tmpLeafPostionBoundariesOrJaws[j].Replace('.', ','));
                }
                #endregion

                #endregion

                #region SSD i glebokosc izocentrum

                Dataset dsSourceAxisDistance = dcmeBeamSequence.GetItem(i);

                SSD[i] = Convert.ToDouble(dsControlPointSequence.GetString(Tags.SourceToSurfaceDistance).Replace('.', ',')) / 10;
                SSDDepth[i] = (Convert.ToDouble(dsSourceAxisDistance.GetString(Tags.SourceAxisDistance).Replace('.', ',')) / 10) - SSD[i];

                message += Environment.NewLine + "SSD(cm):  " + SSD[i].ToString() + Environment.NewLine;
                message += "Głębokość izocentrum(cm): " + SSDDepth[i].ToString() + Environment.NewLine;

                #endregion
            }

            #endregion

            logTextbox.Text = message;
        }

        //metoda liczaca MU
        private double calculateMU(double beamDose, double tissuePhantomRatio, double offAxisFactor, double refFieldCal)
        {
            double MU = beamDose / Math.Abs(tissuePhantomRatio * offAxisFactor * refFieldCal);

            return MU;
        }

        private bool loadTables(ref TextTableLoader _textTableLoader, string _tablePath)
        {
            _textTableLoader = new TextTableLoader();

            bool properLoaded = _textTableLoader.Load(_tablePath);

            return properLoaded;
        }

        private double machinePerformanceValue(double _xLength, double _yLength)
        {
            double minSpaceBeetweenColumns = 0;
            double minSpaceBeetweenRows = 0;
            bool firstValue = false;
            int minSpaceBeetweenRowsPos = 0;
            int minSpaceBeetweenColumnsPos = 0;

            double returnedValue = -1;

            for (int i = 0; i < this.tlWyd.columnNumber; i++)
            {
                if (firstValue)
                {
                    if (minSpaceBeetweenColumns >= Math.Abs(_xLength - Convert.ToDouble(this.tlWyd.columnHeaders[i].Replace('.', ','))))
                    {
                        minSpaceBeetweenColumnsPos = i;
                        minSpaceBeetweenColumns = Math.Abs(_xLength - Convert.ToDouble(this.tlWyd.columnHeaders[i].Replace('.', ',')));
                    }
                }

                if (i == 0)
                {
                    firstValue = true;
                    minSpaceBeetweenColumns = Math.Abs(_xLength - Convert.ToDouble(this.tlWyd.columnHeaders[i].Replace('.', ',')));
                }
            }

            firstValue = false;

            for (int j = 0; j < this.tlWyd.rowNumber; j++)
            {
                if (firstValue)
                {
                    if (minSpaceBeetweenRows >= Math.Abs(_yLength - Convert.ToDouble(this.tlWyd.rowHeaders[j].Replace('.', ','))))
                    {
                        minSpaceBeetweenRowsPos = j;
                        minSpaceBeetweenRows = Math.Abs(_yLength - Convert.ToDouble(this.tlWyd.rowHeaders[j].Replace('.', ',')));
                    }
                }

                if (j == 0)
                {
                    firstValue = true;
                    minSpaceBeetweenRows = Math.Abs(_yLength - Convert.ToDouble(this.tlWyd.rowHeaders[j].Replace('.', ',')));
                }
            }

            if (firstValue)
            {
                returnedValue = Convert.ToDouble(this.tlWyd.rowsData[minSpaceBeetweenRowsPos][minSpaceBeetweenColumnsPos].Replace('.', ','));
            }

            return returnedValue;
        }

        private double percentageDepthDoseValue(double _depth, double _rectFieldSideValue)
        {
            double minSpaceBeetweenColumns = 0;
            double minSpaceBeetweenRows = 0;
            bool firstValue = false;
            int minSpaceBeetweenRowsPos = 0;
            int minSpaceBeetweenColumnsPos = 0;

            double returnedValue = -1;

            for (int i = 0; i < this.tlPDG.columnNumber; i++)
            {
                if (firstValue)
                {
                    if (minSpaceBeetweenColumns >= Math.Abs(_rectFieldSideValue - Convert.ToDouble(this.tlPDG.columnHeaders[i].Replace('.', ','))))
                    {
                        minSpaceBeetweenColumnsPos = i;
                        minSpaceBeetweenColumns = Math.Abs(_rectFieldSideValue - Convert.ToDouble(this.tlPDG.columnHeaders[i].Replace('.', ',')));
                    }
                }

                if (i == 0)
                {
                    firstValue = true;
                    minSpaceBeetweenColumns = Math.Abs(_rectFieldSideValue - Convert.ToDouble(this.tlPDG.columnHeaders[i].Replace('.', ',')));
                }
            }

            firstValue = false;

            for (int j = 0; j < this.tlPDG.rowNumber; j++)
            {
                if (firstValue)
                {
                    if (minSpaceBeetweenRows >= Math.Abs(_depth - Convert.ToDouble(this.tlPDG.rowHeaders[j].Replace('.', ','))))
                    {
                        minSpaceBeetweenRowsPos = j;
                        minSpaceBeetweenRows = Math.Abs(_depth - Convert.ToDouble(this.tlPDG.rowHeaders[j].Replace('.', ',')));
                    }
                }

                if (j == 0)
                {
                    firstValue = true;
                    minSpaceBeetweenRows = Math.Abs(_depth - Convert.ToDouble(this.tlPDG.rowHeaders[j].Replace('.', ',')));
                }
            }

            if (firstValue)
            {
                returnedValue = Convert.ToDouble(this.tlPDG.rowsData[minSpaceBeetweenRowsPos][minSpaceBeetweenColumnsPos].Replace('.', ','));
            }

            //MessageBox.Show(minSpaceBeetweenColumnsPos.ToString(), "Kolumny");
            //MessageBox.Show(minSpaceBeetweenRowsPos.ToString(), "Wiersze");

            return returnedValue;
        }

        //metoda liczaca boki pola
        private double countSideSize(double[] _leafJawPositions)
        {
            int numberOfJawsPlus = 0;
            int numberOfJawsMinus = 0;
            double averagePlus = 0;
            double averageMinus = 0;
            double sideSize = 0;
            double jawPlusPositionsSum = 0; //suma pozycji dodatnich szczek
            double jawMinusPositionsSum = 0; //suma pozycji ujemnych szczek

            #region liczenie sredniej
            for (int i = 0; i < _leafJawPositions.Length; i++)
            {
                if (_leafJawPositions[i] < 0)
                {
                    jawMinusPositionsSum += Math.Abs(_leafJawPositions[i]);
                    numberOfJawsMinus++;
                }
                else if (_leafJawPositions[i] > 0)
                {
                    jawPlusPositionsSum += Math.Abs(_leafJawPositions[i]);
                    numberOfJawsPlus++;
                }
            }

            averageMinus = jawMinusPositionsSum / numberOfJawsMinus;
            averagePlus = jawPlusPositionsSum / numberOfJawsPlus;
            #endregion

            sideSize = (averageMinus + averagePlus) / 10;

            return sideSize;
        }

        private Form areaSizeForm(int _beamNumber)
        {
            Form areaSize = new Form();

            #region parametry formatki

            areaSize.Text = "Podaj wymiary obszaru wiązki " + _beamNumber.ToString();
            areaSize.Size = new Size(300, 200);
            areaSize.StartPosition = FormStartPosition.CenterScreen;
            areaSize.ControlBox = false; //bez kontrolek na pasku tytulowym

            #endregion

            #region parametry el formatki

            TextBox xLengthTextbox = new TextBox();
            xLengthTextbox.Size = new Size(50, 20);
            xLengthTextbox.Location = new Point(125, 20);
            xLengthTextbox.TextAlign = HorizontalAlignment.Center;
            xLengthTextbox.Name = "xLengthTextbox";
            areaSize.Controls.Add(xLengthTextbox);

            Label xLengthLabel = new Label();
            xLengthLabel.Size = new Size(50, 20);
            xLengthLabel.Location = new Point(75, 20);
            xLengthLabel.TextAlign = ContentAlignment.MiddleCenter;
            xLengthLabel.Name = "xLengthLabel";
            xLengthLabel.Text = "X(cm)";
            areaSize.Controls.Add(xLengthLabel);
 
            TextBox yLengthTextbox = new TextBox();
            yLengthTextbox.Size = new Size(50, 20);
            yLengthTextbox.Location = new Point(125, 45);
            yLengthTextbox.TextAlign = HorizontalAlignment.Center;
            yLengthTextbox.Name = "yLengthTextbox";
            areaSize.Controls.Add(yLengthTextbox);

            Label yLengthLabel = new Label();
            yLengthLabel.Size = new Size(50, 20);
            yLengthLabel.Location = new Point(75, 45);
            yLengthLabel.TextAlign = ContentAlignment.MiddleCenter;
            yLengthLabel.Name = "yLengthLabel";
            yLengthLabel.Text = "Y(cm)";
            areaSize.Controls.Add(yLengthLabel);

            Button acceptButton = new Button();
            acceptButton.Text = "OK";
            acceptButton.Size = new Size(80, 50);
            acceptButton.Location = new Point(65, 80);
            acceptButton.TextAlign = ContentAlignment.MiddleCenter;
            acceptButton.Name = "acceptButton";
            acceptButton.DialogResult = DialogResult.OK;
            areaSize.Controls.Add(acceptButton);

            Button cancelButton = new Button();
            cancelButton.Text = "Anuluj";
            cancelButton.Size = new Size(80, 50);
            cancelButton.Location = new Point(155, 80);
            cancelButton.TextAlign = ContentAlignment.MiddleCenter;
            cancelButton.Name = "cancelButton";
            cancelButton.DialogResult = DialogResult.Cancel;
            areaSize.Controls.Add(cancelButton);

            #endregion

            return areaSize;
        }

        private void countMUButton_Click(object sender, EventArgs e)
        {
            string tmpMsg = ""; //tymczasowo przechowywany tekst, wyskakujacy w okienku
            double[] machinePerfVal = new double[this.numberOfBeams]; //wydajnosci aparatu dla wiazek
            double[] percDepthDoses = new double[this.numberOfBeams]; //procentowe dawki glebokosci dla wiazek
            double[] tmpLeafJawPositions;
            int tmpNumberOfLeafJaws = 0;

            //Form areaSize;
            string x;
            string y;

            if (this.numberOfBeams > 0) //jezeli zaladowano wiazki
            {
                for (int i = 0; i < this.numberOfBeams; i++)
                {
                    //areaSize = areaSizeForm(i + 1); //tworzymy formatke

                    //DialogResult areaSizeFormDec = areaSize.ShowDialog(); //decyzja z formatki

                    //if (areaSizeFormDec == DialogResult.OK) //czekamy do czasu zatwierdzenia danych
                    //{
                    //    x = areaSize.Controls["xLengthTextbox"].Text;
                    //    y = areaSize.Controls["yLengthTextbox"].Text;

                    #region przepisanie wsp x
                    tmpNumberOfLeafJaws = this.leafJawPositionsX.Length / this.numberOfBeams;

                    tmpLeafJawPositions = new double[tmpNumberOfLeafJaws];

                    for (int j = 0; j < tmpNumberOfLeafJaws; j++)
                    {
                        tmpLeafJawPositions[j] = this.leafJawPositionsX[i, j];
                    }
                    #endregion

                    x = this.countSideSize(tmpLeafJawPositions).ToString();

                    #region przepisanie wsp y
                    tmpNumberOfLeafJaws = this.leafJawPositionsY.Length / this.numberOfBeams;

                    tmpLeafJawPositions = new double[tmpNumberOfLeafJaws];

                    for (int j = 0; j < tmpNumberOfLeafJaws; j++)
                    {
                        tmpLeafJawPositions[j] = this.leafJawPositionsY[i, j];
                    }
                    #endregion

                    y = this.countSideSize(tmpLeafJawPositions).ToString();

                        if (x != "" && y != "") //sprawdzamy kompletnosc danych
                        {
                            //wybor odpowiedniej wartosci wydajnosci aparatu
                            machinePerfVal[i] = this.machinePerformanceValue(Convert.ToDouble(x), Convert.ToDouble(y));

                            #region obliczanie TPR

                            percDepthDoses[i] = this.percentageDepthDoseValue(10, Convert.ToDouble((2 * Convert.ToDouble(x) * Convert.ToDouble(y)) / (Convert.ToDouble(x) + Convert.ToDouble(y))));

                            this.TPR[i] = this.percentageDepthDoseValue(SSDDepth[i], Convert.ToDouble((2 * Convert.ToDouble(x) * Convert.ToDouble(y)) / (Convert.ToDouble(x) + Convert.ToDouble(y)))) / percDepthDoses[i];
                            tmpMsg += "TPR " + (i + 1) + ":  " + Math.Round(this.TPR[i], 2).ToString() + Environment.NewLine;

                            #endregion

                            this.MU[i] = this.calculateMU(this.beamDoses[i], this.TPR[i], 1, machinePerfVal[i]);
                            tmpMsg += "Wiązka " + (i + 1) + " MU = " + Math.Round(this.MU[i], 2) + Environment.NewLine;
                        }
                    //    else
                    //    {
                    //        if (MessageBox.Show("Nie wypełniono wszystkich pól!", "Ostrzeżenie", MessageBoxButtons.OK, MessageBoxIcon.Warning) == DialogResult.OK)
                    //        {
                    //            areaSizeFormDec = areaSize.ShowDialog(); //decyzja z formatki

                    //            if (areaSizeFormDec == DialogResult.OK) //czekamy do czasu zatwierdzenia danych
                    //            {
                    //                x = areaSize.Controls["xLengthTextbox"].Text;
                    //                y = areaSize.Controls["yLengthTextbox"].Text;

                    //                if (x != "" && y != "") //sprawdzamy kompletnosc danych
                    //                {
                    //                    //wybor odpowiedniej wartosci wydajnosci aparatu
                    //                    machinePerfVal[i] = machinePerformanceValue(Convert.ToInt16(x), Convert.ToInt16(y));

                    //                    #region obliczanie TPR

                    //                    percDepthDoses[i] = this.percentageDepthDoseValue(10, Convert.ToDouble((2 * Convert.ToDouble(x) * Convert.ToDouble(y)) / (Convert.ToDouble(x) + Convert.ToDouble(y))));

                    //                    this.TPR[i] = this.percentageDepthDoseValue(SSDDepth[i], Convert.ToDouble((2 * Convert.ToDouble(x) * Convert.ToDouble(y)) / (Convert.ToDouble(x) + Convert.ToDouble(y)))) / percDepthDoses[i];
                    //                    tmpMsg += "TPR " + (i + 1) + ":  " + Math.Round(this.TPR[i], 2).ToString() + Environment.NewLine;

                    //                    #endregion

                    //                    this.MU[i] = this.calculateMU(this.beamDoses[i], this.TPR[i], 1, machinePerfVal[i]);
                    //                    tmpMsg += "Wiązka " + (i + 1) + " MU = " + Math.Round(this.MU[i], 2) + Environment.NewLine;
                    //                }
                    //                else
                    //                {
                    //                    if (MessageBox.Show("Nie wypełniono wszystkich pól!", "Ostrzeżenie", MessageBoxButtons.OK, MessageBoxIcon.Warning) == DialogResult.OK)
                    //                    {
                    //                        this.countMUButton_Click(null, EventArgs.Empty);
                    //                    }
                    //                }
                    //            }
                    //            else if (areaSizeFormDec == DialogResult.Cancel)
                    //            {
                    //                //jezeli chcemy zakonczyc wprowadzanie danych
                    //                if (MessageBox.Show("Czy na pewno chcesz zakończyć wprowadzanie danych?", "Pytanie", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    //                {
                    //                    break; //przerywamy petle
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //else if (areaSizeFormDec == DialogResult.Cancel)
                    //{
                    //    //jezeli chcemy zakonczyc wprowadzanie danych
                    //    if (MessageBox.Show("Czy na pewno chcesz zakończyć wprowadzanie danych?", "Pytanie", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    //    {
                    //        break; //przerywamy petle
                    //    }
                    //}
                }

                if (tmpMsg != "") //jezeli cokolwiek policzono
                {
                    MessageBox.Show(tmpMsg, "Wynik", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else //jezeli nie - komunikat
            {
                MessageBox.Show("Nie załadowano pliku z danymi lub nie został on załadowany poprawnie!", "Ostrzeżenie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void wczytajWydajnościToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.openTableFileDialog.ShowDialog() == DialogResult.OK)
            {
                bool properLoad = this.loadTables(ref this.tlWyd, this.openTableFileDialog.FileName);

                //MessageBox.Show(this.openTableFileDialog.FileName);

                if (properLoad)
                {
                    MessageBox.Show("Pomyślnie załadowano plik tabeli!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Błąd ładowania pliku tabeli!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                #region tymczasowe wczytywanie danych z list do stringow
                string tmp2 = ""; //wynikowy string dla nazw wierszy(tymczasowy)
                string tmp3 = ""; //wynikowy string dla nazw kolumn(tymczasowy)
                string tmp4 = ""; //wynikowy string dla danych(tymczasowy)

                foreach (string rowName in this.tlWyd.rowHeaders)
                {
                    tmp2 += rowName + " ";
                }

                foreach (string columnName in this.tlWyd.columnHeaders)
                {
                    tmp3 += columnName + " ";
                }

                foreach (List<string> row in this.tlWyd.rowsData)
                {
                    foreach (string rowData in row)
                    {
                        tmp4 += rowData + " ";
                    }

                    tmp4 += '\n';
                }
                #endregion

                //wyniki(tymczasowe)
                string fileName = this.openTableFileDialog.SafeFileName;

                if (MessageBox.Show(tmp2, fileName + " Wiersze: " + this.tlWyd.rowNumber.ToString()) == DialogResult.OK)
                {
                    if (MessageBox.Show(tmp3, fileName + " Kolumny: " + this.tlWyd.columnNumber.ToString()) == DialogResult.OK)
                    {
                        MessageBox.Show(tmp4, fileName + " Dane");
                    }
                }
            }
        }

        private void wczytajPdgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.openTableFileDialog.ShowDialog() == DialogResult.OK)
            {
                bool properLoad = this.loadTables(ref this.tlPDG, this.openTableFileDialog.FileName);

                //MessageBox.Show(this.openTableFileDialog.FileName);

                if (properLoad)
                {
                    MessageBox.Show("Pomyślnie załadowano plik tabeli!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Błąd ładowania pliku tabeli!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                #region tymczasowe wczytywanie danych z list do stringow
                string tmp2 = ""; //wynikowy string dla nazw wierszy(tymczasowy)
                string tmp3 = ""; //wynikowy string dla nazw kolumn(tymczasowy)
                string tmp4 = ""; //wynikowy string dla danych(tymczasowy)

                foreach (string rowName in this.tlPDG.rowHeaders)
                {
                    tmp2 += rowName + " ";
                }

                foreach (string columnName in this.tlPDG.columnHeaders)
                {
                    tmp3 += columnName + " ";
                }

                foreach (List<string> row in this.tlPDG.rowsData)
                {
                    foreach (string rowData in row)
                    {
                        tmp4 += rowData + " ";
                    }

                    tmp4 += '\n';
                }
                #endregion

                //wyniki(tymczasowe)
                string fileName = this.openTableFileDialog.SafeFileName;

                if (MessageBox.Show(tmp2, fileName + " Wiersze: " + this.tlPDG.rowNumber.ToString()) == DialogResult.OK)
                {
                    if (MessageBox.Show(tmp3, fileName + " Kolumny: " + this.tlPDG.columnNumber.ToString()) == DialogResult.OK)
                    {
                        MessageBox.Show(tmp4, fileName + " Dane");
                    }
                }
            }
        }

        private void wyjścieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void oProgramieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("MUCalc " + this.progVersion + "\n(c) Łukasz Traczewski 2012", "Info o programie", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void testbutton_Click(object sender, EventArgs e)
        {
            double x = 15.1;
            double y = 20;
            double d = 8.5;
            double dose = 65;

            MessageBox.Show(this.calculateMU(dose, (this.percentageDepthDoseValue(d, 2 * x * y / (x + y))) / (this.percentageDepthDoseValue(10, 2 * x * y / (x + y))), 1, this.machinePerformanceValue(x, y)).ToString());
        }
    }
}
