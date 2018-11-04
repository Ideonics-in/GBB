using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestBenchApp.Entity
{
    public class ReportRecord
    {
        public bool IsModified { get; set; }
        public int SlNo { get; set; }
        public String Model { get; set; }
        public String Main_Frame_Barcode { get; set; }
        public String Main_Body_Barcode { get; set; }
        public String Integrated_Barcode { get; set; }
        public String Combination_Barcode { get; set; }
        public String ROFilterBarcode { get; set; }
        public String Status { get; set; }
        
       

        public ReportRecord()
        {
            IsModified = false;
        }
    }
}
