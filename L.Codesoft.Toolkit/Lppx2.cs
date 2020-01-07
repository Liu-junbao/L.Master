using LabelManager2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace System
{
    class Lppx2
    {
        private const string LPPX2PROID = "Lppx2.Application";
        private Application mcsApp;
        public Lppx2()
        {
            try
            {
                Object obj = null;
                try
                {
                    //#if NET450
                    //                     obj = Marshal.GetActiveObject(LPPX2PROID);
                    //#elif NETCOREAPP

                    //#endif
                    obj = Marshal.GetActiveObject(LPPX2PROID);
                }
                catch
                {
                    obj = null;
                };
                if (obj==null)
                {
                    mcsApp = new Application();
                }
                else
                {
                    mcsApp = (Application)obj;
                }
            }
            catch (Exception e)
            {
                throw new Exception("codesoft接口初始化失败",e);
            }
        }
        Document this[string fileName]
        {
            get
            {
                return mcsApp.Documents.Item(fileName);
            }
        }
        public IEnumerable<string> PrinterNames()
        {
            PrinterSystem PrnSystem = mcsApp.PrinterSystem();
            Strings names = PrnSystem.Printers(enumKindOfPrinters.lppxAllPrinters);
            for (int i = 1; i <= names.Count; i++)
            {
                yield return names.Item(i);
            }
            Marshal.ReleaseComObject(names);
            Marshal.ReleaseComObject(PrnSystem);
        }
        public Document Open(string fileName, Dictionary<string,string> values)
        {
            var doc = mcsApp.Documents.Open(fileName);
            if (values != null)
            {
                foreach (var item in values)
                {
                    doc.Variables.Item(item.Key).Value = item.Value;
                }
            }
            return doc;
        }
        public void CloseAll()
        {
            mcsApp.Documents.CloseAll(false);
        }
        public bool SwitchPrinter(Document doc,string printerName)
        {
            PrinterSystem PrnSystem = mcsApp.PrinterSystem();
            Strings names = PrnSystem.Printers(enumKindOfPrinters.lppxAllPrinters);
            string currentName = doc.Printer.Name;
            string fullName = doc.Printer.FullName;
            string eachName = "";
            if (currentName != printerName && fullName != printerName)
            {
                short count = names.Count;
                for (short i = 1; i <= count; i++)
                {
                    fullName = names.Item(i);
                    int pos = fullName.LastIndexOf(',');
                    if (pos != -1)
                    {
                        eachName = fullName.Substring(0, pos);
                        if (eachName == printerName||fullName==printerName)
                        {
                            bool bDirectAccess = false;
                            string PortName = fullName.Substring(pos + 1);
                            if (PortName.StartsWith("->"))
                            {
                                PortName = PortName.Substring(2);
                                bDirectAccess = true;
                            }
                            var printer = doc.Printer;
                            printer.SwitchTo(printerName,PortName,bDirectAccess);
                            Marshal.ReleaseComObject(printer);
                            return true;
                        }
                    }
                }
                Marshal.ReleaseComObject(names);
                Marshal.ReleaseComObject(PrnSystem);
                return false;
            }
            return true;
        }

    }
    static class LppxEx
    {
        public static System.Drawing.Bitmap Preview(this Document doc)
        {
            object obj = doc.GetPreview(true, true, 100);
            if (obj is Array)
            {
                byte[] data = (byte[])obj;
                System.Drawing.Bitmap img;
                using (var stream = new MemoryStream(data))
                {
                    img = new System.Drawing.Bitmap(stream);
                }
                return img;
            }
            return null;
        }
    }
}
