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

        private const string defaultTablePath6 = "./Data/PRIMUS 6MV/"; //domyslna sciezka do plikow z tabelami dla aparatu 6MV

        private const string defaultTablePath15 = "./Data/PRIMUS 15MV/"; //domyslna sciezka do plikow z tabelami dla aparatu 15MV

        private int numberOfBeams;

        private int numberOfFractions;

        private double[] beamDoses; //dawki(cGy)

        private double[] TPR;

        private double[] beamMetersets; //zapisane w pliku MU kazdej wiazki

        private double[] SSD; //SSD dla kazdej wiazki(cm)

        private double[] SSDDepth; //glebokosc izocentrum dla kazdej wiazki(cm)

        private double[] radDepth; //glebokosc radiacyjna dla kazdej wiazki(cm)

        private double[] nominalBeamEnergy;

        private double[ , ] leafPositionBoundaries; //granice x listkow dla kazdej wiazki(mm)

        private double[,] leafJawPositionsX; //pozycje x szczek

        private double[,] leafJawPositionsY; //pozycje y szczek

        public double[] MU { get; private set; } //tablica z wynikami MU dla poszczególnych wiazek

        private TextTableLoader tlWyd; //obiekt przechowujacy dane z tabeli wydajnosci danego aparatu(cGy/MU)

        private TextTableLoader tlPDG; //obiekt przechowujacy dane z tabeli pdg danego aparatu

        private bool properRTPlanLoaded;

        public Form1(string _progVersion)
        {
            InitializeComponent();

            this.Text += " " + _progVersion;

            this.progVersion = _progVersion;
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

        private void fileLoad(string _filePath)
        {
            DICOMLoad dcmload = new DICOMLoad();

            Dataset ds = dcmload.Load(_filePath);

            //w przypadku pomyślnego załadowania pliku
            if (ds != null)
            {
                MessageBox.Show("Pomyślnie załadowano plik DICOM!", "Sukces", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.mainOperations(ds);
            } //w przeciwnym wypadku
            else
            {
                MessageBox.Show("Błąd ładowania pliku DICOM!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DOSELoad doseFileLoad(string _filePath)
        {
            DOSELoad doseload = new DOSELoad("");

            if (_filePath != "")
            {
                doseload = new DOSELoad(_filePath);
            }

            return doseload;
        }

        private void mainOperations(Dataset _ds) //glowna funkcja
        {
            //inicjalizacja
            double[,] maxDose = new double[1, 2]; //maksymalna dawka i numer wiazki dla tej dawki
            maxDose[0, 0] = 0;
            maxDose[0, 1] = 0;
            double tmpDose = 0; //tymczasowa zmienna przechowujaca ostatnia dawke
            string[] tmpLeafPostionBoundariesOrJaws;

            Dataset dsFractionGroupSequence = _ds.GetItem(Tags.FractionGroupSeq, 0);

            if (dsFractionGroupSequence != null)
            {
                try
                {
                    #region wiązki
                    this.numberOfBeams = Convert.ToInt16(dsFractionGroupSequence.GetString(Tags.NumberOfBeams, 0));

                    this.numberOfFractions = Convert.ToInt16(dsFractionGroupSequence.GetString(Tags.NumberOfFractionsPlanned, 0));

                    //inicjalizacja
                    this.beamDoses = new double[this.numberOfBeams];
                    this.TPR = new double[this.numberOfBeams];
                    this.MU = new double[this.numberOfBeams];
                    this.SSD = new double[this.numberOfBeams];
                    this.SSDDepth = new double[this.numberOfBeams];
                    this.radDepth = new double[this.numberOfBeams];
                    this.beamMetersets = new double[this.numberOfBeams];
                    this.nominalBeamEnergy = new double[this.numberOfBeams];

                    DcmElement dcmeReferencedBeamSequence = dsFractionGroupSequence.Get(Tags.RefBeamSeq);

                    string message = "Liczba wiązek: " + numberOfBeams.ToString() + Environment.NewLine;

                    message += "Frakcje: " + this.numberOfFractions + Environment.NewLine;

                    Dataset dsReferencedBeams = null;

                    for (int i = 0; i < numberOfBeams; i++)
                    {
                        dsReferencedBeams = dcmeReferencedBeamSequence.GetItem(i);
                        tmpDose = Convert.ToDouble(dsReferencedBeams.GetString(Tags.BeamDose, 0).Replace('.', ',')) * 100; //zamiana dawki na cGy
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
                        //GetUpperBound(poziom) - indeks ostatniego el na danym poziomie tablicy
                        if (this.leafPositionBoundaries == null || (this.leafPositionBoundaries.GetUpperBound(0) + 1) != this.numberOfBeams || (this.leafPositionBoundaries.GetUpperBound(1) + 1) != tmpLeafPostionBoundariesOrJaws.Length)
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
                        if (this.leafJawPositionsX == null || (this.leafJawPositionsX.GetUpperBound(0) + 1) != this.numberOfBeams || (this.leafJawPositionsX.GetUpperBound(1) + 1) != tmpLeafPostionBoundariesOrJaws.Length)
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
                        if (this.leafJawPositionsY == null || (this.leafJawPositionsY.GetUpperBound(0) + 1) != this.numberOfBeams || (this.leafJawPositionsY.GetUpperBound(1) + 1) != tmpLeafPostionBoundariesOrJaws.Length)
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

                    this.properRTPlanLoaded = true;

                    logTextbox.Text = message;
                }
                catch (NullReferenceException) //wyłapywanie braku którychś danych
                {
                    MessageBox.Show("Ten plik nie zawiera wszystkich potrzebnych do obliczeń danych!" + Environment.NewLine + "Spróbuj załadować prawidłowy plik!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.properRTPlanLoaded = false;
                }

                #region wczytywanie parametrow z plikow DOSE
                //inicjalizacja
                bool[] doseLoaded = new bool[this.numberOfBeams];

                if (MessageBox.Show("Czy chcesz załadować pliki RT Dose?", "Ładowanie RT Dose", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    #region automatyczne wczytywanie plikow DOSE

                    string RTPlanPath = Path.GetDirectoryName(fileNameTextbox.Text);

                    IEnumerable<string> RTPlanDirDoseFiles = Directory.EnumerateFiles(RTPlanPath, "RD*.dcm");

                    int j = 0;

                    foreach (string doseFileName in RTPlanDirDoseFiles)
                    {
                        DOSELoad doseload = this.doseFileLoad(doseFileName);

                        List<List<string>> doseData = doseload.beamData();

                        if (doseload.success)
                        {
                            if (doseData[1].Count > 0) //sprawdzenie, czy potrzebne dane sa w pliku
                            {
                                this.SSD[doseload.beamNumber - 1] = Convert.ToDouble(doseData[1][3].Replace('.', ',')) / 10;
                                this.SSDDepth[doseload.beamNumber - 1] = Convert.ToDouble(doseData[1][4].Replace('.', ',')) / 10;
                                this.radDepth[doseload.beamNumber - 1] = Convert.ToDouble(doseData[1][5].Replace('.', ',')) / 10;

                                doseLoaded[j] = true;
                            }
                            else
                            {
                                j--; //zmniejszenie licznika aby zaladowac wlasciwy plik dla kazdej wiazki
                            }
                        }
                        else
                        {
                            doseLoaded[j] = false;
                        }

                        j++;
                    }

                    if (j > 0)
                    {
                        MessageBox.Show("Załadowano " + j.ToString() + " plików RT Dose", "Pliki RT Dose", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Nie znaleziono w katalogu RT Planu plików RT Dose", "Pliki RT Dose", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }

                    #endregion
                }
                else
                {
                    MessageBox.Show("W związku z niewybraniem plików RT Dose niektóre wyniki mogą być nieprecyzyjne!", "Ostrzeżenie", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                #endregion

                #region automatyczne ladowanie tabeli wydajnosci i pdg dla danego aparatu

                bool properLoadWyd = false;
                bool properLoadPDG = false;

                if (nominalBeamEnergy[0].ToString() == "6")
                {
                    properLoadWyd = this.loadTables(ref this.tlWyd, defaultTablePath6 + "wyd.dat");
                    properLoadPDG = this.loadTables(ref this.tlPDG, defaultTablePath6 + "pdg.dat");
                }
                else if (nominalBeamEnergy[0].ToString() == "15")
                {
                    properLoadWyd = this.loadTables(ref this.tlWyd, defaultTablePath15 + "wyd.dat");
                    properLoadPDG = this.loadTables(ref this.tlPDG, defaultTablePath15 + "pdg.dat");
                }

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
            else //w przypadku, gdy nie jest to RT Plan
            {
                MessageBox.Show("Ten plik nie zawiera potrzebnych danych!" + Environment.NewLine + "Najprawdopodobniej nie jest to RTPlan - spróbuj załadować prawidłowy plik!", "Błąd", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

            return returnedValue;
        }

        //metoda liczaca boki pola
        private double countSideSize(double[] _leafJawPositions)
        {
            double[] jawsAreas;
            double[] jawsPlus;
            double[] jawsMinus;
            double jawsAreaSum = 0;
            int numberOfJawsPairs = 0;
            double sideSize = 0;

            #region liczenie roznic miedzy parami listkow
            //policzenie liczby par listkow
            if (_leafJawPositions.Length % 2 == 0) //jesli istnieje parzysta liczba elementow(kazdy ma pare)
            {
                numberOfJawsPairs = _leafJawPositions.Length / 2;
            }
            else
            {
                numberOfJawsPairs = (_leafJawPositions.Length + 1) / 2;
            }

            //inicjalizacja
            jawsAreas = new double[numberOfJawsPairs];
            for (int i = 0; i < numberOfJawsPairs; i++)
            {
                jawsAreas[i] = 0;
            }

            jawsPlus = new double[numberOfJawsPairs];
            for (int i = 0; i < numberOfJawsPairs; i++)
            {
                jawsPlus[i] = 0;
            }

            jawsMinus = new double[numberOfJawsPairs];
            for (int i = 0; i < numberOfJawsPairs; i++)
            {
                jawsMinus[i] = 0;
            }

            #region podzial na listki lewe i prawe
            int j = 0;//dodatkowy licznik

            //listki lewe
            for (int i = 0; i < numberOfJawsPairs; i++)
            {
                jawsMinus[i] = _leafJawPositions[i];
            }

            //listki prawe
            for (int i = numberOfJawsPairs; i < _leafJawPositions.Length; i++)
            {
                jawsPlus[j] = _leafJawPositions[i];
                j++;
            }
            #endregion

            for (int i = 0; i < numberOfJawsPairs; i++)
            {
                if ((jawsMinus[i] != jawsPlus[i]) && jawsMinus[i] != 0 && jawsPlus[i] != 0)
                {
                    if (jawsMinus[i] > jawsPlus[i])
                    {
                        jawsAreas[i] = Math.Abs(jawsMinus[i] - jawsPlus[i]);
                    }
                    else
                    {
                        jawsAreas[i] = Math.Abs(jawsPlus[i] - jawsMinus[i]);
                    }
                }
                else
                {
                    if (jawsMinus[i] != 0)
                    {
                        jawsAreas[i] = jawsMinus[i] + 200;
                    }
                    else if (jawsPlus[i] != 0)
                    {
                        jawsAreas[i] = jawsPlus[i] + 200;
                    }
                    else
                    {
                        jawsAreas[i] = 0;
                    }
                }
            }

            for (int i = 0; i < numberOfJawsPairs; i++)
            {
                jawsAreaSum += jawsAreas[i];
            }

            sideSize = (jawsAreaSum / numberOfJawsPairs) / 10;
            #endregion

            return sideSize;
        }

        private Form countedMUForm(int _beamNumber, double[,] _beamParams, string[] _headers)
        {
            Form countedMU = new Form();
            double[] percentDiff = new double[this.numberOfBeams];

            #region parametry formatki

            countedMU.Text = "Wynik dla " + _beamNumber.ToString() + " wiązek";
            countedMU.Size = new Size(500, 230);
            countedMU.StartPosition = FormStartPosition.CenterScreen;
            //countedMU.ControlBox = false; //bez kontrolek na pasku tytulowym
            countedMU.ShowIcon = true;
            countedMU.Icon = SystemIcons.Information;

            #endregion

            #region parametry el formatki

            ListView beamParamValuesListview = new ListView();
            beamParamValuesListview.Size = new Size(475, 130);
            beamParamValuesListview.Location = new Point(5, 5);
            beamParamValuesListview.Scrollable = true;
            beamParamValuesListview.FullRowSelect = true;
            beamParamValuesListview.AllowColumnReorder = true;
            beamParamValuesListview.View = View.Details;

            ColumnHeader headers;

            headers = new ColumnHeader();
            headers.Width = 70;
            headers.Text = _headers[0];
            beamParamValuesListview.Columns.Add(headers);

            for (int i = 1; i < _headers.Length; i++)
            {
                headers = new ColumnHeader();
                headers.Width = _headers[i].Length * 8;
                headers.Text = _headers[i];
                beamParamValuesListview.Columns.Add(headers);
            }

            ListViewItem MUValueListviewItem;

            for (int i = 0; i < _beamNumber; i++)
            {
                percentDiff[i] = Math.Round(((Math.Abs(_beamParams[i, 0] - _beamParams[i, 1])) / _beamParams[i, 1]) * 100, 2);
                MUValueListviewItem = new ListViewItem("Wiązka " + (i + 1).ToString());
                MUValueListviewItem.SubItems.Add(new ListViewItem.ListViewSubItem(MUValueListviewItem, _beamParams[i, 0].ToString()));
                MUValueListviewItem.SubItems.Add(new ListViewItem.ListViewSubItem(MUValueListviewItem, _beamParams[i, 1].ToString()));
                MUValueListviewItem.SubItems.Add(new ListViewItem.ListViewSubItem(MUValueListviewItem, percentDiff[i].ToString()));
                MUValueListviewItem.SubItems.Add(new ListViewItem.ListViewSubItem(MUValueListviewItem, _beamParams[i, 2].ToString()));
                MUValueListviewItem.SubItems.Add(new ListViewItem.ListViewSubItem(MUValueListviewItem, _beamParams[i, 3].ToString()));
                MUValueListviewItem.SubItems.Add(new ListViewItem.ListViewSubItem(MUValueListviewItem, _beamParams[i, 4].ToString()));
                MUValueListviewItem.SubItems.Add(new ListViewItem.ListViewSubItem(MUValueListviewItem, _beamParams[i, 5].ToString()));
                MUValueListviewItem.SubItems.Add(new ListViewItem.ListViewSubItem(MUValueListviewItem, _beamParams[i, 6].ToString()));

                #region kolorowanie roznicy
                if (percentDiff[i] < 3)
                {
                    MUValueListviewItem.BackColor = Color.Green;
                }
                else if (percentDiff[i] >= 3 && percentDiff[i] < 5)
                {
                    MUValueListviewItem.BackColor = Color.Yellow;
                }
                else if (percentDiff[i] >= 5)
                {
                    MUValueListviewItem.BackColor = Color.Red;
                }
                #endregion

                beamParamValuesListview.Items.Add(MUValueListviewItem);
            }

            countedMU.Controls.Add(beamParamValuesListview);

            Button acceptButton = new Button();
            acceptButton.Text = "OK";
            acceptButton.Size = new Size(80, 50);
            acceptButton.Location = new Point(210, 140);
            acceptButton.TextAlign = ContentAlignment.MiddleCenter;
            acceptButton.Name = "acceptButton";
            acceptButton.DialogResult = DialogResult.OK;
            countedMU.Controls.Add(acceptButton);

            //Button cancelButton = new Button();
            //cancelButton.Text = "Anuluj";
            //cancelButton.Size = new Size(80, 50);
            //cancelButton.Location = new Point(155, 80);
            //cancelButton.TextAlign = ContentAlignment.MiddleCenter;
            //cancelButton.Name = "cancelButton";
            //cancelButton.DialogResult = DialogResult.Cancel;
            //areaSize.Controls.Add(cancelButton);

            #endregion

            return countedMU;
        }

        private void countMUButton_Click(object sender, EventArgs e)
        {
            string tmpMsg = ""; //tymczasowo przechowywany tekst, wyskakujacy w okienku
            double[] machinePerfVal = new double[this.numberOfBeams]; //wydajnosci aparatu dla wiazek
            double[] percDepthDoses = new double[this.numberOfBeams]; //procentowe dawki glebokosci dla wiazek
            double tmpNormSideLength; //znormalizowana dlugosc boku pola kwadratowego(wartosc tymczasowa)
            double tmpPDGRatio;
            double[] tmpLeafJawPositions;
            int tmpNumberOfLeafJaws = 0;
            double[,] beamParams = new double[this.numberOfBeams, 7];
            string[] paramHeaders = { "", "Dawka obl.(MU)", "Dawka(MU)", "Różnica(%)", "X(cm)", "Y(cm)", "SSD(cm)", "Głębokość(cm)", "Głębokość radiacyjna(cm)" };

            //Form areaSize;
            string x;
            string y;

            if (this.properRTPlanLoaded) //jezeli zaladowano prawidlowy plik
            {
                for (int i = 0; i < this.numberOfBeams; i++)
                {
                    beamParams[i, 1] = Math.Round(this.beamMetersets[i], 2);

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

                    //zapisanie do tablicy parametrow wiazki
                    beamParams[i, 2] = Math.Round(Convert.ToDouble(x.Replace('.', ',')), 2);
                    beamParams[i, 3] = Math.Round(Convert.ToDouble(y.Replace('.', ',')), 2);
                    beamParams[i, 4] = Math.Round(this.SSD[i], 2);
                    beamParams[i, 5] = Math.Round(this.SSDDepth[i], 2);
                    beamParams[i, 6] = Math.Round(this.radDepth[i], 2);

                        if (x != "" && y != "") //sprawdzamy kompletnosc danych
                        {
                            //jezeli brak podanej glebokosci radiacyjnej, to bierzemy glebokosc zwykla wiazki
                            if (this.radDepth[i] == 0)
                            {
                                this.radDepth[i] = this.SSDDepth[i];
                            }

                            //wybor odpowiedniej wartosci wydajnosci aparatu
                            machinePerfVal[i] = this.machinePerformanceValue(Convert.ToDouble(x), Convert.ToDouble(y));

                            #region obliczanie TPR

                            tmpNormSideLength = Convert.ToDouble((2 * Convert.ToDouble(x) * Convert.ToDouble(y)) / (Convert.ToDouble(x) + Convert.ToDouble(y)));

                            percDepthDoses[i] = this.percentageDepthDoseValue(10, ((tmpNormSideLength * 90) / 100));

                            tmpPDGRatio = this.percentageDepthDoseValue(this.radDepth[i], ((tmpNormSideLength * 90) / (90 + this.radDepth[i]))) / percDepthDoses[i];

                            this.TPR[i] = tmpPDGRatio * Math.Pow((90 + this.radDepth[i]) / 100, 2);
                            //tmpMsg += "TPR " + (i + 1) + ":  " + Math.Round(this.TPR[i], 2).ToString() + Environment.NewLine;

                            #endregion

                            this.MU[i] = this.calculateMU(this.beamDoses[i], this.TPR[i], 1, machinePerfVal[i]);
                            tmpMsg += "Wiązka " + (i + 1) + " MU = " + Math.Round(this.MU[i], 2) + Environment.NewLine;

                            beamParams[i, 0] = Math.Round(this.MU[i], 2);
                        }
                }

                if (tmpMsg != "") //jezeli cokolwiek policzono
                {
                    //MessageBox.Show(tmpMsg, "Wynik", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    Form countedMUForm = this.countedMUForm(this.numberOfBeams, beamParams, paramHeaders);

                    if (countedMUForm.ShowDialog() == DialogResult.OK)
                    {
                        countedMUForm.Close();
                    }
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
                //string tmp2 = ""; //wynikowy string dla nazw wierszy(tymczasowy)
                //string tmp3 = ""; //wynikowy string dla nazw kolumn(tymczasowy)
                //string tmp4 = ""; //wynikowy string dla danych(tymczasowy)

                //foreach (string rowName in this.tlWyd.rowHeaders)
                //{
                //    tmp2 += rowName + " ";
                //}

                //foreach (string columnName in this.tlWyd.columnHeaders)
                //{
                //    tmp3 += columnName + " ";
                //}

                //foreach (List<string> row in this.tlWyd.rowsData)
                //{
                //    foreach (string rowData in row)
                //    {
                //        tmp4 += rowData + " ";
                //    }

                //    tmp4 += '\n';
                //}
                #endregion

                ////wyniki(tymczasowe)
                //string fileName = this.openTableFileDialog.SafeFileName;

                //if (MessageBox.Show(tmp2, fileName + " Wiersze: " + this.tlWyd.rowNumber.ToString()) == DialogResult.OK)
                //{
                //    if (MessageBox.Show(tmp3, fileName + " Kolumny: " + this.tlWyd.columnNumber.ToString()) == DialogResult.OK)
                //    {
                //        MessageBox.Show(tmp4, fileName + " Dane");
                //    }
                //}
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
            }
        }

        private void wyjścieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void oProgramieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("MUCalc " + this.progVersion + "\n\nZmiany:\n- poprawiono bug z nieprawidłowym ładowaniem niektórych plików RT Dose\n- udoskonalono licznie pola, gdu mamy nieparzystą liczbę pozycji listków\n- poprawiono bug związany z ładowaniem niektórych nieprawidłowych RT Plan-ów\n\n(c) Łukasz Traczewski 2012", "Info o programie", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
