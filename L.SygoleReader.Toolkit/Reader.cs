using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class Reader
    {
        private Sygole.HFReader.HFReader _reader;
        public Reader(string address, int port)
        {
            Address = address;
            Port = port;
            _reader = new Sygole.HFReader.HFReader();
            _reader.AutoReadHandler += Reader_AutoReadHandler;
            IsConnected = _reader.Connect(address, port);
        }
        public bool IsConnected { get; }
        public string Address { get; }
        public int Port { get; }
        public string Read()
        {
            byte[] uid = new byte[8];
            if (_reader.Inventory(0x00, ref uid, Sygole.HFReader.Antenna_enum.ANT_1) == Sygole.HFReader.Status_enum.SUCCESS)
            {
                string sn = BytesToHexString(uid);
                Readed?.Invoke(sn);
                return sn;
            }
            return null;
        }
        private void Reader_AutoReadHandler(object sender, Sygole.HFReader.AutoReadEventArgs Args)
        {
            Readed?.Invoke(Args.tag_ID);
        }
        //将字节类型的数据转化为十六进制字符串
        private string BytesToHexString(byte[] data)
        {
            int length = data.Length;
            StringBuilder builder = new StringBuilder(length * 2);
            for (int i = 0; i < length; i++)
            {
                var b = data[i];
                if (b < 16) builder.Append("0");
                builder.Append(b.ToString("X"));
            }
            return builder.ToString();
        }
        public event ReadedHandler Readed;
    }

    public delegate void ReadedHandler(string text);
}

