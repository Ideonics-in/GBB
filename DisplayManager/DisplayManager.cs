using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DisplayManager
{
    public class CP5200Andon
    {
        CP5200 CP5200Instance;

        String _port;

        public  CP5200Andon(string port,int baud, int timeout)
        {
            _port = port;
            CP5200.CP5200_RS232_InitEx(Marshal.StringToHGlobalAnsi(_port), baud, timeout); 


        }

        public bool UpdateWindow(int window,String data )
        {
            int result = CP5200.CP5200_RS232_SendText(1, window, Marshal.StringToHGlobalAnsi(data), 0xFF, 8, 3, 0, 3, 0);
            return result >= 0;
        }
    }
}
