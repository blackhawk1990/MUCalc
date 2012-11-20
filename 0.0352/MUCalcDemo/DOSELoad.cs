using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvilDICOM.Core.IO.Reading;
using EvilDICOM.Core;
using EvilDICOM.Core.Element;
using EvilDICOM.Core.Interfaces;

namespace MUCalcDemo
{
    class DOSELoad
    {
        private DICOMObject dcm;
        public bool success { get; set; }

        public DOSELoad(string _filePath)
        {
            if (_filePath != "")
            {
                this.dcm = DICOMFileReader.Read(_filePath);
                this.success = true;
            }
            else
            {
                this.success = false;
            }
        }

        public List<string> tagName(string _tagName)
        {
            List<string> test = new List<string>();

            var tagDatas = dcm.FindAll(_tagName);

            List<CodeString> tagValues = new List<CodeString>();

            foreach (IDICOMElement tagData in tagDatas)
            {
                tagValues.Add(tagData as EvilDICOM.Core.Element.CodeString);
            }

            foreach(CodeString tagValue in tagValues)
            {
                test.Add(tagValue.Data);
            }

            return test;
        }

        public List<double> tagValue(string _tagName)
        {
            List<double> test = new List<double>();

            var tagDatas = dcm.FindAll(_tagName);

            List<DecimalString> tagValues = new List<DecimalString>();

            foreach (IDICOMElement tagData in tagDatas)
            {
                tagValues.Add(tagData as EvilDICOM.Core.Element.DecimalString);
            }

            foreach (DecimalString tagValue in tagValues)
            {
                test.Add(tagValue.Data[0]);
            }

            return test;
        }
    }
}