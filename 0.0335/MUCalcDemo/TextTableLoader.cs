using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace MUCalcDemo
{
    class TextTableLoader:ITableLoader
    {
        public int columnNumber { get; private set; }
        public int rowNumber { get; private set; }

        public List<string> columnHeaders { get; private set; }

        public List<string> rowHeaders { get; private set; }

        public void Load(string _fileName)
        {
            //inicjalizacja
            int length = 0;
            this.rowNumber = 0;
            this.columnNumber = 0;
            this.columnHeaders = new List<string>();
            this.rowHeaders = new List<string>();
            bool isCreated = false; //czy utworzono juz nowy el listy
            char[] tmp; //tymczsowa tablica znakow (bufor) do ktorej jest wczytywany plik
            string tmp2 = ""; //wynikowy string dla nazw wierszy(tymczasowy)
            string tmp3 = ""; //wynikowy string dla nazw kolumn(tymczasowy)
            string tmp4 = ""; //wynikowy string dla danych(tymczasowy)

            StreamReader sr = new StreamReader(_fileName);

            length = sr.ReadToEnd().Length;

            tmp = new char[length];

            sr.Close();

            sr = new StreamReader(_fileName);

            sr.Read(tmp, 0, length);

            #region loading Row Names
            bool newLine = false;

            for (int i = 0; i < length; i++)
            {
                if (this.rowHeaders.Count == 0 && i > 0 && tmp[i - 1] == '\n' && tmp[i] != ' ' && tmp[i] != '\t')
                {
                    if (isCreated)
                    {
                        this.rowHeaders[this.rowHeaders.Count - 1] += tmp[i];
                    }
                    else //jezeli nazwa 1-znakowa(brak el na liscie)
                    {
                        this.rowHeaders.Add(tmp[i].ToString());
                        isCreated = true;
                    }
                    if (tmp[i + 1] == '\t' || tmp[i + 1] == ' ')
                    {
                        newLine = false;
                    }
                }
                else if (i > 0 && (i + 1) < length && (tmp[i - 1] == '\n' || (this.rowHeaders.Count > 0 && this.rowHeaders[this.rowHeaders.Count - 1].Length > 0 && tmp[i - 1] == this.rowHeaders[this.rowHeaders.Count - 1][this.rowHeaders[this.rowHeaders.Count - 1].Length - 1]) || tmp[i - 1] == ' ') && tmp[i] != '\t' && tmp[i] != ' ' && newLine)
                {
                    if (tmp[i + 1] == '\t' || tmp[i + 1] == ' ')
                    {
                        if (isCreated)
                        {
                            this.rowHeaders[this.rowHeaders.Count - 1] += tmp[i];
                        }
                        else //jezeli nazwa 1-znakowa
                        {
                            this.rowHeaders.Add(tmp[i].ToString());
                            isCreated = true;
                        }
                        newLine = false;
                    }
                    else
                    {
                        if (!isCreated) //jesli nie dodano niczego do listy
                        {
                            this.rowHeaders.Add(tmp[i].ToString());
                            isCreated = true;
                        }
                        else
                        {
                            this.rowHeaders[this.rowHeaders.Count - 1] += tmp[i];
                        }
                    }
                }
                else if (tmp[i] == '\n')
                {
                    newLine = true;
                    if (isCreated)
                    {
                        isCreated = false;
                    }
                }
            }

            this.rowNumber = this.rowHeaders.Count;
            #endregion

            #region loading Column Names
            isCreated = false;

            for (int i = 0; i < length; i++)
            {
                if (tmp[i] != '\n')
                {
                    if (tmp[i] != ' ' && tmp[i] != '\t' && tmp[i] != '\r')
                    {
                        if (tmp[i + 1] == ' ' || tmp[i + 1] == '\t')
                        {
                            if (isCreated)
                            {
                                this.columnHeaders[this.columnHeaders.Count - 1] += tmp[i];
                            }
                            else
                            {
                                this.columnHeaders.Add(tmp[i].ToString());
                                isCreated = true;
                            }
                        }
                        else
                        {
                            if (isCreated)
                            {
                                this.columnHeaders[this.columnHeaders.Count - 1] += tmp[i];
                            }
                            else
                            {
                                this.columnHeaders.Add(tmp[i].ToString());
                                isCreated = true;
                            }
                        }
                    }
                    else
                    {
                        if (isCreated)
                        {
                            isCreated = false;
                        }
                    }
                }
                else
                {
                    break;
                }
            }

            this.columnNumber = this.columnHeaders.Count;
            #endregion

            #region loading Row Data //TO DO

            bool firstNewLineSign = false;

            bool firstPauseAfterSignInLine = false;

            for (int i = 0; i < length; i++)
            {
                if (firstNewLineSign && firstPauseAfterSignInLine && tmp[i] != ' ' && tmp[i] != '\t' && tmp[i] != '\n')
                {
                    if (tmp[i + 1] == ' ' || tmp[i + 1] == '\t')
                    {
                        tmp4 += tmp[i] + " ";
                    }
                    else
                    {
                        tmp4 += tmp[i];
                    }
                }
                else if (!firstNewLineSign)
                {
                    if (tmp[i] == '\n')
                    {
                        firstNewLineSign = true;
                    }
                }
                else if (tmp[i] == '\n')
                {
                    firstPauseAfterSignInLine = false;
                    tmp4 += '\n';
                }
                else if (!firstPauseAfterSignInLine && (tmp[i] == ' ' || tmp[i] == '\t') && tmp[i - 1] != '\n' && tmp[i - 1] != ' ' && tmp[i - 1] != '\t')
                {
                    firstPauseAfterSignInLine = true;
                }
            }

            #endregion

            foreach (string rowName in this.rowHeaders)
            {
                tmp2 += rowName + " ";
            }

            foreach (string columnName in this.columnHeaders)
            {
                tmp3 += columnName + " ";
            }

            //wyniki(tymczasowe)
            if (MessageBox.Show(tmp2, "Wiersze: " + this.rowNumber.ToString()) == DialogResult.OK)
            {
                if (MessageBox.Show(tmp3, "Kolumny: " + this.columnNumber.ToString()) == DialogResult.OK)
                {
                    MessageBox.Show(tmp4, "Dane"); 
                }
            }
        }
    }
}
