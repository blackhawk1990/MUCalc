using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.dicomcs.data;
using org.dicomcs.dict;

namespace MUCalcDemo
{
    class DOSELoad
    {
        //beamData - zwracany format dla punktow: 0 - nazwa punktu, 1 - dawka w punkcie(Gy), 2 - SSD, 3 - glebokosc, 4 - glebokosc rad.
        private Dataset ds;
        public int beamNumber; //numer wiazki
        private int numberOfPoints; //liczba punktow wiazki
        private string[] pointDose; //dawka w punkcie
        private string[] ROIName;

        public bool success { get; set; }

        public DOSELoad(string _filePath)
        {
            if (_filePath != "")
            {
                DICOMLoad dl = new DICOMLoad();
                this.ds = dl.Load(_filePath);
                this.success = true;
            }
            else
            {
                this.success = false;
            }
        }

        public List<List<string>> beamData()
        {
            #region wczytywanie numeru wiazki
            DcmElement dcmeRefRTPlanSequence = ds.Get(Tags.RefRTPlanSeq);

            Dataset dsRefFractionGroupSequence = dcmeRefRTPlanSequence.GetItem(0);

            DcmElement dcmeRefFractionGroupSequence = dsRefFractionGroupSequence.Get(Tags.RefFractionGroupSeq);

            Dataset dsRefBeamSequence = dcmeRefFractionGroupSequence.GetItem(0);

            DcmElement dcmeRefBeamSequence = dsRefBeamSequence.Get(Tags.RefBeamSeq);

            Dataset dsRefBeamNumber = dcmeRefBeamSequence.GetItem(0);

            this.beamNumber = Convert.ToInt16(dsRefBeamNumber.GetString(Tags.RefBeamNumber, 0));
            #endregion

            List<List<string>> beamParams = new List<List<string>>();

            DcmElement dcmeRTDoseROISeq = ds.Get(Tags.RTDoseROISeq);

            this.numberOfPoints = dcmeRTDoseROISeq.Item.Size;

            this.ROIName = new string[this.numberOfPoints];
            this.pointDose = new string[this.numberOfPoints];

            List<DcmElement> dcmeBeamMeasurements = new List<DcmElement>();

            List<int> refROINumbers = new List<int>();

            for (int i = 0; i < this.numberOfPoints; i++)
            {
                beamParams.Add(new List<string>());
                dcmeBeamMeasurements.Add(dcmeRTDoseROISeq.GetItem(i).Get(0x30051026));
                refROINumbers.Add(Convert.ToInt16(dcmeRTDoseROISeq.GetItem(i).GetString(Tags.RefROINumber)));

                #region wczytywanie dawki w punkcie

                this.pointDose[i] = dcmeRTDoseROISeq.GetItem(i).GetString(Tags.DoseValue);

                #endregion
            }

            //List<Dataset> dsBeamMeasurements = new List<Dataset>();

            //foreach (DcmElement dcmeBeamMeasurement in dcmeBeamMeasurements)
            //{
            //    for (int i = 0; i < 3; i++)
            //    {
            //        if (dcmeBeamMeasurement != null)
            //        {
            //            dsBeamMeasurements.Add(dcmeBeamMeasurement.GetItem(i));
            //        }
            //    }
            //}

            #region wczytywanie nazw punktow
            DcmElement dcmeStructureSetROISequence = ds.Get(Tags.StructureSetROISeq);

            for (int i = 0; i < this.numberOfPoints; i++)
            {
                this.ROIName[i] = dcmeStructureSetROISequence.GetItem(i).GetString(Tags.ROIName);
            }
            #endregion

            if (dcmeBeamMeasurements[0] != null)
            {
                for (int i = 0; i < this.numberOfPoints; i++)
                {
                    beamParams[i].Add(this.ROIName[i]);

                    beamParams[i].Add(this.pointDose[i]);

                    for (int j = 0; j < 3;j++)
                    {
                        beamParams[i].Add(dcmeBeamMeasurements[i].GetItem(j).GetString(0x30051028));
                        //beamParams[i].Add(dcmeBeamMeasurements[i].GetItem(j).GetString(0x30051027)); //nazwa tagu
                    }
                }
            }

            return beamParams;
        }
    }
}