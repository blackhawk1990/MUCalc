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
        public int columnNumber
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int rowNumber
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string[] columnHeaders
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string[] rowHeaders
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Load(string _fileName)
        {
            //inicjalizacja
            int length = 0;
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
                if (tmp2 == "" && i > 0 && tmp[i - 1] == '\n' && tmp[i] != ' ')
                {
                    if (tmp[i + 1] == '\t' || tmp[i + 1] == ' ')
                    {
                        tmp2 += tmp[i] + " ";
                        newLine = false;
                    }
                    else
                    {
                        tmp2 += tmp[i];
                    }
                }
                else if (i > 0 && (i + 1) < length && (tmp[i - 1] == '\n' || (tmp2.Length > 0 && tmp[i - 1] == tmp2[tmp2.Length - 1]) || tmp[i - 1] == ' ') && tmp[i] != '\t' && tmp[i] != ' ' && newLine)
                {
                    if (tmp[i + 1] == '\t' || tmp[i + 1] == ' ')
                    {
                        tmp2 += tmp[i] + " ";
                        newLine = false;
                    }
                    else
                    {
                        tmp2 += tmp[i];
                    }
                }
                else if (tmp[i] == '\n')
                {
                    newLine = true;
                }
            }
            #endregion

            #region loading Column Names

            for (int i = 0; i < length; i++)
            {
                if (tmp[i] != ' ' && tmp[i] != '\t' && tmp[i] != '\n')
                {
                    if (tmp[i + 1] == ' ' || tmp[i + 1] == '\t')
                    {
                        tmp3 += tmp[i] + " ";
                    }
                    else
                    {
                        tmp3 += tmp[i];
                    }
                }
                else if (tmp[i] == '\n')
                {
                    break;
                }
            }

            #endregion

            #region loading Row Data

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

            //wyniki(tymczasowe)
            if (MessageBox.Show(tmp2, "Wiersze") == DialogResult.OK)
            {
                if (MessageBox.Show(tmp3, "Kolumny") == DialogResult.OK)
                {
                    MessageBox.Show(tmp4, "Dane"); 
                }
            }
        }
    }
}
