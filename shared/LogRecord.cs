using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class LogRecord
    {
        public DateTime Timestamp { get; set; }
        public String Stage { get; set; }
        public String Remarks { get; set; }
        public String Barcode { get; set; }
    }
}
