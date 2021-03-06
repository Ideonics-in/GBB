﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using System.Configuration;
using System.ComponentModel;
using System.IO.Ports;
using System.Collections;

using ias.devicedriver;

namespace ias.andonmanager
{
    
    public partial class AndonManager
    {
        public event EventHandler<AndonAlertEventArgs> andonAlertEvent;   //the handler for alerts
        public event EventHandler<BCScannerEventArgs> barcodeAlertEvent;
        public event EventHandler<CSScannerEventArgs> combStickerAlertEvent;
        public event EventHandler<actQtyScannerEventArgs> actQtyAlertEvent;

        public enum MODE { NONE = 0, MASTER = 1, SLAVE = 2 };

        SerialPortDriver spDriver = null;               //the serial port driver

        //Code added on 11 Nov
        SerialPortDriver bcScannerDriver = null;        // mainframe barcode scanner
        SerialPortDriver csScannerDriver = null;        // Combination sticker generator scanner
        SerialPortDriver actQtyScannerDriver = null;    // Final product scanner

        RS485Driver rs485Driver = null;                 //the rs485 driver

        XbeeDriver xbeeDriver = null;

        String communicationPort = String.Empty;
        public String CommunicationPort
        {
            get { return communicationPort; }
        }

        //Code added on 11 Nov BCPORT 
        String bccommunicationPort = String.Empty;
        public String bcCommunicationPort
        {
            get { return bccommunicationPort; }
        }

        String cscommunicationPort = String.Empty;
        public String csCommunicationPort
        {
            get { return cscommunicationPort; }
        }

        String actQtycommunicationPort = String.Empty;
        public String actQtyCommunicationPort
        {
            get { return actQtycommunicationPort; }
        }
       

        struct LineResponse
        {
            public int id;
            public DateTime timeStamp;  //response time stamp
            

            public List<Byte> data ;
 
            public LineResponse( DateTime tstp, int id , List<Byte> data)
            {
                timeStamp = tstp;

                this.id = id;
                this.data = data;
            }
        }

        List<Byte> txPacket = null;
        List<Byte> rxPacket = null;
        List<Byte> partialPacket = null;
        Queue<Byte> rxDataQ = null;

        String[] comLayers;
        String[] responsePacketFields;

        System.Timers.Timer transactionTimer = null;
        System.Timers.Timer simulationTimer = null;
        int responseTimeout = 50; //response timeout in milliseconds

        Queue<int> stations = null;
        // Queue<int> departments = null;

        String simulation = String.Empty;//simulation control

        private Queue<TransactionInfo> transactionQ = null;

        Byte RESP_SOF = 0xCC;    //start of FRAME
        Byte RESP_EOF = 0xDD;    //end of FRAME


        MODE mode = MODE.NONE;

        int retries = 0;
        int NO_OF_RETRIES = 3;
        String xbeeIdentifier = String.Empty;

        public AndonManager(Queue<int> stationList, Queue<int> departmentList , MODE mode)
        {
            try
            {
                responseTimeout = int.Parse(ConfigurationSettings.AppSettings["ResponseTimeout"]);
                this.mode = mode;

                transactionTimer = new System.Timers.Timer(responseTimeout);
                transactionTimer.Elapsed += new ElapsedEventHandler(transactionTimer_Elapsed);
                transactionTimer.AutoReset = false;

                simulation = ConfigurationSettings.AppSettings["SIMULATION"];

                if (simulation != "Yes")
                {
                    //spDriver = new SerialPortDriver(57600, 8, StopBits.One, Parity.None, Handshake.None);
                    //communicationPort = ConfigurationSettings.AppSettings["PORT"];

                    //Code added on 11 Nov BCPORT 
                    bcScannerDriver = new SerialPortDriver(9600,8,StopBits.One,Parity.None,Handshake.None);
                    bccommunicationPort = ConfigurationSettings.AppSettings["BCPORT"];

                    csScannerDriver = new SerialPortDriver(9600, 8, StopBits.One, Parity.None, Handshake.None);
                    cscommunicationPort = ConfigurationSettings.AppSettings["CSPORT"];

                    actQtyScannerDriver = new SerialPortDriver(9600, 8, StopBits.One, Parity.None, Handshake.None); ;
                    actQtycommunicationPort = ConfigurationSettings.AppSettings["ACTQTYPORT"];


                    comLayers = ConfigurationSettings.AppSettings["COM_LAYERS"].Split(',');
                    responsePacketFields = ConfigurationSettings.AppSettings["RESPONSE_PACKET_FIELDS"].Split(',');

                    if( comLayers.Contains<String>("RS485"))
	                    rs485Driver = new RS485Driver();
                    if( comLayers.Contains<String>("XBee"))
                    {
                        xbeeDriver = new XbeeDriver(XbeeDriver.COMMUNICATION_MODE.API_ESC);
                        xbeeIdentifier = ConfigurationSettings.AppSettings["XBeeIdentifier"];


                        //communicationPort = findXbeePort(xbeeIdentifier);   //find the port of xbee

                        //if (communicationPort == String.Empty)
                        //{
                        //    throw new AndonManagerException(" Error : Xbee Device Not Found ");
                        //}

                    }
	
	                stations = stationList;
	                //departments = departmentList;
	
	                
                    
                    
                }
                else 
                {
                	
                	simulationTimer = new System.Timers.Timer(2 * 1000);
                	simulationTimer.Elapsed += new ElapsedEventHandler(simulationTimer_Elapsed);
                    simulationTimer.AutoReset = false;
                	
                }
                transactionQ = new Queue<TransactionInfo>();
            }
            catch (Exception e)
            {
                throw new AndonManagerException("Andon Manager Initialization Error:"+e.Message);
            }
        }

        void simulationTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
        	simulationTimer.Stop();
            //List<int> departments = new List<int>();
            ////departments.Add(1);
            //departments.Add(14);
            //departments.Add(0);
        	
        	
        	
            //AndonAlertEventArgs alertEvent
            //    = new AndonAlertEventArgs(DateTime.Now,8,
            //                              createLog(departments));

            //if (andonAlertEvent != null)
            //{
            //    andonAlertEvent(this, alertEvent);
            //}
        	
        }

        public void OpenBCScanner()
        {
            bcScannerDriver = new SerialPortDriver(9600, 8, StopBits.One, Parity.None, Handshake.None);
            bccommunicationPort = ConfigurationSettings.AppSettings["BCPORT"];
            bcScannerDriver.open(bccommunicationPort);
            int i = 3;
            do
            {
                if (bcScannerDriver.IsOpen == false)
                {
                    bcScannerDriver.Close();
                    Thread.Sleep(500);
                }
            } while (--i > 0);

            if (bcScannerDriver.IsOpen == false)
                throw new Exception("unable to open barcode Serial Port");
        }

        public void OpenCSScanner()
        {
            csScannerDriver = new SerialPortDriver(9600, 8, StopBits.One, Parity.None, Handshake.None);
            cscommunicationPort = ConfigurationSettings.AppSettings["CSPORT"];
            csScannerDriver.open(cscommunicationPort);
            int i = 3;
            do
            {
                if (csScannerDriver.IsOpen == false)
                {
                    csScannerDriver.Close();
                    Thread.Sleep(500);
                }
            } while (--i > 0);

            if (csScannerDriver.IsOpen == false)
                throw new Exception("unable to open CS Serial Port");

            rxPacket = new List<byte>();
            partialPacket = new List<byte>();
            rxDataQ = new Queue<byte>();


            transactionTimer.Start();
        }

        public void start()
        {
            try
            {
                int i = 3;
            	if( simulation == "Yes")
            	{
            		simulationTimer.Start();
            		return;
            		
            	}
                //spDriver.open(communicationPort);

                //
                //do
                //{
                //    if (spDriver.IsOpen == false)
                //    {
                //        spDriver.Close();
                //        Thread.Sleep(500);
                //    }
                //} while (--i > 0);


                //Code added on 11 Nov BCPORT 
                bcScannerDriver.open(bccommunicationPort);
                i = 3;
                do
                {
                    if (bcScannerDriver.IsOpen == false)
                    {
                        bcScannerDriver.Close();
                        Thread.Sleep(500);
                    }
                } while (--i > 0);

                csScannerDriver.open(cscommunicationPort);
                i = 3;

                do
                {
                    if (csScannerDriver.IsOpen == false)
                    {
                        csScannerDriver.Close();
                        Thread.Sleep(500);
                    }
                } while (--i > 0);

                //actQtyScannerDriver.open(actQtycommunicationPort);
                //i = 3;

                //do
                //{
                //    if (actQtyScannerDriver.IsOpen == false)
                //    {
                //        actQtyScannerDriver.Close();
                //        Thread.Sleep(500);
                //    }
                //} while (--i > 0);


                //if (spDriver.IsOpen == false)
                //    throw new Exception("unable to open serial port");

                //Code added on 11 Nov BCPORT 
                if (bcScannerDriver.IsOpen == false)
                    throw new Exception("unable to open Scanner Serial Port");

                if (csScannerDriver.IsOpen == false)
                    throw new Exception("unable to open Scanner Serial Port");

                //if (actQtyScannerDriver.IsOpen == false)
                //    throw new Exception("unable to open Scanner serial Port");

                rxPacket = new List<byte>();
                partialPacket = new List<byte>();
                rxDataQ = new Queue<byte>();


                transactionTimer.Start();

            }
            catch (Exception e)
            {
                spDriver = null;
                bcScannerDriver = null;
                csScannerDriver = null;
                actQtyScannerDriver = null;
                throw new AndonManagerException("Unable to start Andon Manager:" + e.Message);
            }
        }


        public void stop()
        {
			    if( simulation == "Yes")
            	{
			    	simulationTimer.Stop();
            		return;
            		
            	}
            rxPacket = null;

            //if (spDriver != null)
            //{
            //    spDriver.abort = true;
            //    Thread.Sleep(10);
            //}

            //Code added on 11 Nov BCPORT
            if (bcScannerDriver != null)
            {
                bcScannerDriver.abort = true;
                Thread.Sleep(10);
            }

            if (csScannerDriver != null)
            {
                csScannerDriver.abort = true;
                Thread.Sleep(10);
            }

            if (actQtyScannerDriver != null)
            {
                actQtyScannerDriver.abort = true;
                Thread.Sleep(10);
            }

            if( transactionTimer.Enabled )
            transactionTimer.Stop();
            

            Thread.Sleep(100);
            
        }

        void transactionTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<Byte> packet = null;
            int startIndex= -1;
            int endIndex= -1;

            transactionTimer.Stop();

            //if (!spDriver.IsOpen)
            //{
            //    throw new AndonManagerException("Serial Port Closed");

            //}

            //Code added on 11 Nov BCPORT
            if (!bcScannerDriver.IsOpen)
                throw new AndonManagerException("Barcode Scanner Serial Port Closed");

            if (!csScannerDriver.IsOpen)
                throw new AndonManagerException("Combination Sticker Barcode Scanner Serial Port Closed");

            //if (!actQtyScannerDriver.IsOpen)
              //  throw new AndonManagerException("Final Stage Barcode Scanner Serial Port Closed");

            //int bytesReceived = spDriver.BytesToRead;

            //if (bytesReceived > 0)  //check whether bytes have been received
            //{
            //    Byte[] tempBuff = new Byte[spDriver.BytesToRead];


            //    spDriver.Read(tempBuff, 0, bytesReceived); //copy the received bytes into temp buffer

            //    for (int i = 0; i < bytesReceived; i++)   //copy from temp buffer to packet
            //    {
            //        rxPacket.Add(tempBuff[i]);
            //    }
            //    List<Byte> parsedRxPacket;
            //    if (comLayers.Contains<String>("XBee"))
            //    {
            //        parsedRxPacket = xbeeDriver.parseRxPacket(rxPacket);
            //    }
            //    else
            //    {
            //        parsedRxPacket = rxPacket;
            //    }


            //    if( parsedRxPacket == null )
            //    {
            //        retries++;
            //        if (retries >= NO_OF_RETRIES)
            //        {
            //            retries = 0;
            //            rxPacket.Clear();
            //            startTransaction();
            //            return;
            //        }
            //        transactionTimer.Start();
            //        return;
            //    }
            //    retries = 0;
            //    parsedRxPacket.InsertRange(0,partialPacket.GetRange(0,partialPacket.Count));
            //    partialPacket.Clear();

            //    while (parsedRxPacket.Contains(RESP_SOF) && parsedRxPacket.Contains(RESP_EOF))     // if the packet contains start of frame
            //    {

            //        startIndex = parsedRxPacket.FindIndex(findRespSof);

            //        endIndex = parsedRxPacket.FindIndex(findRespEof);

            //        if (startIndex > endIndex)
            //        {
            //            parsedRxPacket.RemoveRange(0, startIndex);
            //            continue;
            //        }

            //        packet = new List<byte>();
            //        packet.AddRange(parsedRxPacket.GetRange(startIndex, (endIndex - startIndex) + 1));

            //        processResponse(packet);      //process it

            //        parsedRxPacket.RemoveRange(0, endIndex + 1);

            //    }
            //    partialPacket.AddRange( parsedRxPacket.GetRange(0 , parsedRxPacket.Count));
            //}

            //Code added on 11 Nov BCPORT
            if (bcScannerDriver.BytesToRead > 0)
            {
                String bcScannerData = bcScannerDriver.ReadLine();
                if (bcScannerData != null)
                    ParsebcScannerData(bcScannerData);
            }

            if (csScannerDriver.BytesToRead > 0)
            {
                String csScannerData = csScannerDriver.ReadLine();
                if (csScannerData != null)
                    ParsecsScannerData(csScannerData);
            }

            //if (actQtyScannerDriver.BytesToRead > 0)
            //{
            //    String actQtyScannerData = actQtyScannerDriver.ReadLine();
            //    if (actQtyScannerData != null)
            //        ParseActQtyScannerData(actQtyScannerData);
            //}


            startTransaction();

            return;
        }

        //Code added on 11 Nov BCPORT
        private void ParseActQtyScannerData(string actQtyScannerData)
        {
            if (actQtyAlertEvent != null)
                actQtyAlertEvent(this, new actQtyScannerEventArgs(actQtyScannerData));
        }


        
        private void ParsecsScannerData(string csScannerData)
        {
            if (combStickerAlertEvent != null)
                combStickerAlertEvent(this, new CSScannerEventArgs(csScannerData));
        }


       
        private void ParsebcScannerData(string packet)
        {
           
            
            //packet.CopyTo(0, modelNo, 0, Convert.ToInt16(SCANNERPACKET.MODEL));
            //packet.CopyTo(Convert.ToInt16(SCANNERPACKET.MODEL), dateTime, 0, Convert.ToInt16(SCANNERPACKET.DATETIME));
            //packet.CopyTo(Convert.ToInt16(SCANNERPACKET.DATETIME) + Convert.ToInt16(SCANNERPACKET.MODEL), srNo, 0, Convert.ToInt16(SCANNERPACKET.SRNO));


            
            if (barcodeAlertEvent != null)
                barcodeAlertEvent(this, new BCScannerEventArgs(packet));
     

        }

        private bool findRespSof(Byte b)
        {
            if (b == (Byte)RESP_SOF)
                return true;
            else return false;
        }
        private bool findRespEof(Byte b)
        {
            if (b == (Byte)RESP_EOF)
                return true;
            else return false;
        }

        void startTransaction()
        {
            //if (mode == MODE.MASTER)
            //{
            //    List<Byte> rs485packet = null;
            //    List<Byte> txPacket = null;


            //    if (transactionQ.Count > 0)
            //    {
            //        TransactionInfo tInfo = transactionQ.Dequeue();
            //        rs485packet = rs485Driver.Packetize((Byte)0xFA, (Byte)tInfo.command, tInfo.data);

            //        if (rs485packet != null)
            //        {
            //            if (comLayers.Contains<string>("XBee"))
            //                txPacket = xbeeDriver.getTxPacket(tInfo.deviceId, rs485packet);
            //            else
            //            {
            //                txPacket = rs485packet;
            //            }
            //            byte[] txBuffer = txPacket.ToArray();
            //            //spDriver.WriteToPort(txPacket.ToArray());
            //            spDriver.Write(txBuffer, 0, txPacket.Count);

            //        }
            //    }
            //    else
            //    {


            //        byte curStation = (Byte)stations.Dequeue();



            //        if (comLayers.Contains<String>("RS485"))
            //        {
            //            rs485packet = rs485Driver.Packetize((Byte)0xFA, (Byte)AndonCommand.CMD_GET_STATUS, null);
            //            stations.Enqueue(curStation);


            //            if (rs485packet != null)
            //            {
            //                if (comLayers.Contains<string>("XBee"))
            //                    txPacket = xbeeDriver.getTxPacket(curStation, rs485packet);
            //                else
            //                {
            //                    txPacket = rs485packet;
            //                }
            //                byte[] txBuffer = txPacket.ToArray();
            //                //spDriver.WriteToPort(txPacket.ToArray());
            //                spDriver.Write(txBuffer, 0, txPacket.Count);

            //            }
            //        }
            //    }

            //}
            transactionTimer.Start();
          
        }

        void processResponse(List<Byte> packet)
        {

            if( (packet == null) || (packet.Count <= 0 ))
            return;
            int status = 0xFF;
            int deviceId = 0xFF;
            List<Byte> responseData = null;
            if (comLayers.Contains<String>("RS485"))
            {
                rs485Driver.Parse(packet, out status, out deviceId, out responseData);
            }

            //if (status == (Byte)AndonResponse.RES_COMM_OK)
            //{
                if (stations.Contains(deviceId) == false )
                    return;
                LineResponse lineReponse = new LineResponse();

                lineReponse.data = responseData;
                lineReponse.id = deviceId;
                lineReponse.timeStamp = DateTime.Now;
                updateStationStatus(lineReponse);
            //}
        }

        void updateStationStatus(LineResponse lineResponse)
        {
            try
            {
                if (lineResponse.data == null || (lineResponse.data.Count == 0))
                {
                    return;
                }

                List<LogEntry> log = parseResponse(lineResponse.data);


                if (log != null)
                {
                    AndonAlertEventArgs alertEvent
                        = new AndonAlertEventArgs(lineResponse.timeStamp, lineResponse.id, log);

                    if (andonAlertEvent != null)
                    {
                        andonAlertEvent(this, alertEvent);
                    }
                        
                }

                        
            }
            catch( Exception te)
            {
                    throw te;
            }

        }

        List<LogEntry> parseResponse(List<Byte> responseData)
        {
            List<LogEntry> log = new List<LogEntry>();

            LogEntry lgEntry = new LogEntry();

            if (responsePacketFields.Contains<String>("STATION"))
            {
                Byte[] tempBuff = { (Byte)(responseData[1] - '0'), (Byte)(responseData[0] - '0') };
                lgEntry.Station = tempBuff[1] * 10 + tempBuff[0];
                responseData.RemoveRange(0, 2);
            }

            if (responsePacketFields.Contains<String>("DEPARTMENT"))
            {
                
                lgEntry.Department =  responseData[0] - '0';
                responseData.RemoveRange(0, 1);
            }

            if( responseData.Count > 0 )
                lgEntry.Data = System.Text.Encoding.UTF8.GetString(responseData.ToArray());

           log.Add(lgEntry);       

            return log;
        }

        public void addTransaction(int deviceId, AndonCommand command, List<Byte> data)
        {
            transactionQ.Enqueue(new TransactionInfo(deviceId, command, data));
        }
        
        
        List<LogEntry> createLog(List<int> departments )
        {
        	List<LogEntry> log = new List<LogEntry>();
        	
        	foreach( int i in departments )
        	{
        		log.Add( new LogEntry(1 , i , "5" ));
        	}
        	return log;
        	
        }



       


        
      
    }
    

    public class AndonManagerException : Exception
    {
        public String message = String.Empty;
        public AndonManagerException(String msg )
        {
            message = msg;
        }
    }

    public class AndonAlertEventArgs : EventArgs
    {
        DateTime eventTstp;
        public DateTime EventTimeStamp
        {
            get { return eventTstp; }
        }

        int stationId = 0;
        public int StationId
        {
            get { return stationId; }
            set { stationId = value; }
        }
        List<LogEntry> stationLog = null;
        public List<LogEntry> StationLog
        {
            get { return stationLog; }
        }


        public AndonAlertEventArgs(DateTime eventTstp, int stationId,List<LogEntry> stnLog)
        {
            this.eventTstp = eventTstp;
            
            this.stationId = stationId;
            this.stationLog = stnLog;
                       

        }

        public void addLogEntry(LogEntry logEntry)
        {
            if (stationLog == null)
            {
                stationLog = new List<LogEntry>();
            }
            stationLog.Add(logEntry);


        }

    }

    public class BCScannerEventArgs : EventArgs
    {
        public String ModelNumber { get; set; } //alpha numeric
        public String Timestamp { get; set; }
        public int SerialNo { get; set; } //numeric
 
        public BCScannerEventArgs(String scanData)
        {
            try
            {
                if (scanData.Contains("A"))
                {
                    ModelNumber = scanData.Substring(0, 5);
                    Timestamp = scanData.Substring(5, 6);

                    SerialNo = Convert.ToInt32(scanData.Substring(11, 4));

                }
                else
                {
                    ModelNumber = scanData.Substring(0, 4);
                    Timestamp = scanData.Substring(4, 6);

                    SerialNo = Convert.ToInt32(scanData.Substring(10, 4));

                }
            }
            catch (Exception e)
            {
               
            }
        }

    }

    public class CSScannerEventArgs : EventArgs
    {
        public String ModelNumber { get; set; } //alpha numeric
        public String Timestamp { get; set; }

        
        public int SerialNo { get; set; } //numeric
        


        public CSScannerEventArgs(String scanData)
        {
            try
            {
                if (scanData.Contains("A"))
                {
                    ModelNumber = scanData.Substring(0, 5);
                    Timestamp = scanData.Substring(5, 6);

                    SerialNo = Convert.ToInt32(scanData.Substring(11, 4));

                }
                else
                {
                    ModelNumber = scanData.Substring(0, 4);
                    Timestamp = scanData.Substring(4, 6);

                    SerialNo = Convert.ToInt32(scanData.Substring(10, 4));

                }
            }
            catch (Exception e)
            {
            }

           
        }

    }

    public class actQtyScannerEventArgs : EventArgs
    {
        public String Barcode { get; set; }

        public actQtyScannerEventArgs(String scanData)
        {
            try
            {
                Barcode = scanData.Replace("\r","");
            }
            catch (Exception e)
            {
            }
        }

    }



    
}
