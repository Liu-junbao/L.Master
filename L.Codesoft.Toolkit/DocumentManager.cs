using LabelManager2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class DocumentManager
    {
        private Lppx2 _lppx;
        public DocumentManager()
        {
            _lppx = new Lppx2();
        }
        private bool PrintDocument(Document doc, string printerName)
        {
            if (_lppx.SwitchPrinter(doc, printerName))
            {
                return doc.PrintDocument() >= 0;
            }
            return false;
        }
        public bool Print(string templateFullName, Dictionary<string, string> values, params string[] printerNames)
        {
            Document doc = _lppx.Open(templateFullName, values);
            try
            {
                if (printerNames != null)
                {
                    foreach (var item in printerNames)
                    {
                        if (PrintDocument(doc, item) == false) return false;
                    }
                }
                else return false;
                doc.Close();
            }
            finally
            {
                doc.Close();
            }
            return true;
        }
        public Bitmap Preview(string templateFullName, Dictionary<string, string> values)
        {
            return _lppx.Open(templateFullName, values).Preview();
        }
    }
}
