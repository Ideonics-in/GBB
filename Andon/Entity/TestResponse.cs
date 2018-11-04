using System;
using System.Runtime.InteropServices;

namespace TestBenchApp
{
   public class TestResponse
    {
        public byte SlaveID;
        public byte FunctionCode;
        public byte DataLength;
        public short TestStatus;
        public short EarthContactStatus;
        public short HighVoltageStatus;
        public short InsulationStatus;
        public short LeakageCurrent_LNStatus;
        public short LeakageCurrent_NLStatus;
        public float ECTResistance;
        public float HVTCurrent;
        public float ITResistance;
        public float LCT_LNCurrent;
        public float LCT_NLCurrent;
        public float Voltage_LN;
        public float Current_LN;
        public float Power_LN;
        public float Voltage_NL;
        public float Current_NL;
        public float Power_NL;
        public byte VAW_Meter_Error;

        public byte[] ResponsePacket;

        public  TestResponse()
        {
            ResponsePacket = new byte[75];

        }

        public int GetStatus()
        {
            byte[] tempBuff = new byte[2];
            tempBuff[0] = ResponsePacket[4];
            tempBuff[1] = ResponsePacket[3];
            TestStatus = BitConverter.ToInt16(tempBuff, 0);
            return TestStatus;
        }

        public TestRecord ParseResponse(String barcode,string model)
        {
            byte[] tempBuff = new byte[4];
            TestRecord tr = new TestRecord { Barcode = barcode,Model = model };
            
            SlaveID = ResponsePacket[0];
            FunctionCode = ResponsePacket[1];
            DataLength = ResponsePacket[2];

            tempBuff[0] = ResponsePacket[4];
            tempBuff[1] = ResponsePacket[3];
            TestStatus = BitConverter.ToInt16(tempBuff, 0);

            tempBuff[0] = ResponsePacket[6];
            tempBuff[1] = ResponsePacket[5];
            EarthContactStatus = BitConverter.ToInt16(tempBuff, 0);

            tempBuff[0] = ResponsePacket[8];
            tempBuff[1] = ResponsePacket[7];
            HighVoltageStatus = BitConverter.ToInt16(tempBuff, 0);

            tempBuff[0] = ResponsePacket[10];
            tempBuff[1] = ResponsePacket[9];
            InsulationStatus = BitConverter.ToInt16(tempBuff, 0);

            tempBuff[0] = ResponsePacket[12];
            tempBuff[1] = ResponsePacket[11];
            LeakageCurrent_LNStatus = BitConverter.ToInt16(tempBuff, 0);

            tempBuff[0] = ResponsePacket[14];
            tempBuff[1] = ResponsePacket[13];
            LeakageCurrent_NLStatus = BitConverter.ToInt16(tempBuff, 0);

            tempBuff[0] = ResponsePacket[16];
            tempBuff[1] = ResponsePacket[15];
            tempBuff[2] = ResponsePacket[18];
            tempBuff[3] = ResponsePacket[17];
            tr.ECT = BitConverter.ToSingle(tempBuff, 0);



            tempBuff[0] = ResponsePacket[20];
            tempBuff[1] = ResponsePacket[19];
            tempBuff[2] = ResponsePacket[22];
            tempBuff[3] = ResponsePacket[21];
            tr.HV =  BitConverter.ToSingle(tempBuff, 0);

            tempBuff[0] = ResponsePacket[24];
            tempBuff[1] = ResponsePacket[23];
            tempBuff[2] = ResponsePacket[26];
            tempBuff[3] = ResponsePacket[25];
            tr.IR = BitConverter.ToSingle(tempBuff, 0);


            tempBuff[0] = ResponsePacket[28];
            tempBuff[1] = ResponsePacket[27];
            tempBuff[2] = ResponsePacket[30];
            tempBuff[3] = ResponsePacket[29];
            tr.LC = BitConverter.ToSingle(tempBuff, 0);
           

            tempBuff[0] = ResponsePacket[32];
            tempBuff[1] = ResponsePacket[31];
            tempBuff[2] = ResponsePacket[34];
            tempBuff[3] = ResponsePacket[33];
            LCT_NLCurrent = BitConverter.ToSingle(tempBuff, 0);


            tempBuff[0] = ResponsePacket[36];
            tempBuff[1] = ResponsePacket[35];
            tempBuff[2] = ResponsePacket[38];
            tempBuff[3] = ResponsePacket[37];
            tr.Voltage = BitConverter.ToSingle(tempBuff, 0);

            tempBuff[0] = ResponsePacket[40];
            tempBuff[1] = ResponsePacket[39];
            tempBuff[2] = ResponsePacket[42];
            tempBuff[3] = ResponsePacket[41];
            tr.Current = BitConverter.ToSingle(tempBuff, 0);

            tempBuff[0] = ResponsePacket[44];
            tempBuff[1] = ResponsePacket[43];
            tempBuff[2] = ResponsePacket[46];
            tempBuff[3] = ResponsePacket[45];
            tr.Power = BitConverter.ToSingle(tempBuff, 0);


            tempBuff[0] = ResponsePacket[48];
            tempBuff[1] = ResponsePacket[47];
            tempBuff[2] = ResponsePacket[50];
            tempBuff[3] = ResponsePacket[49];
            Voltage_NL = BitConverter.ToSingle(tempBuff, 0);

            tempBuff[0] = ResponsePacket[52];
            tempBuff[1] = ResponsePacket[51];
            tempBuff[2] = ResponsePacket[54];
            tempBuff[3] = ResponsePacket[53];
            Current_NL = BitConverter.ToSingle(tempBuff, 0);


            tempBuff[0] = ResponsePacket[56];
            tempBuff[1] = ResponsePacket[55];
            tempBuff[2] = ResponsePacket[58];
            tempBuff[3] = ResponsePacket[57];
            Power_NL = BitConverter.ToSingle(tempBuff, 0);

            VAW_Meter_Error = ResponsePacket[59];

            tr.Status = (TestStatus == 3) ? "PASS" : "FAIL";

            return tr;
        }
    }
}