using System;
using TestBenchApp.Entity;

namespace TestBenchApp
{
    public class TestRecord
    {
        public string Barcode { get; set; }
        public string Model { get; set; }
        public float ECT { get; set; }
        public float HV { get; set; }
        public float IR { get; set; }
        public float LC { get; set; }
        public float Voltage { get; set; }
        public float Current { get; set; }
        public float Power { get; set; }
        public string Status { get; set; }

        public void UpdateStatus(string barcode,string model,string status)
        {
            Barcode = barcode;
            Status = status;
            Model = model;
        }
    }



    public class PerformanceTestEventArgs : EventArgs
    {
        public UnitAssociation Unit { get; set; }


        public PerformanceTestEventArgs(UnitAssociation u)
        {
            Unit = u;
        }
    }
}