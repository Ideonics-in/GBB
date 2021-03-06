﻿using HDC_COMMSERVER;
using prjParagon_WMS;
using shared.Entity;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Printer
{
    public class PrinterManager
    {
        #region PublicVariables

        public String CombinationTemplate { get; set; }
        public IPAddress IPAddress { get; set; }
        public int Port { get; set; }
        public String CombinationPrinterName { get; set;}
        public String TemplatePath { get; set; }

        #endregion


        Dictionary<String,BcilNetwork> Drivers;
        #region Constructor
        public PrinterManager(String templatePath)
        {

            Drivers = new Dictionary<string, BcilNetwork>();
            TemplatePath = templatePath;
            
        }
        #endregion

        #region Public Methods

        public void SetupDriver(String name, IPAddress ipaddr, int port, String template)
        {
            BcilNetwork Driver = new BcilNetwork {
                            PrinterIP = ipaddr, 
                            PrinterPort = port,
                            Template = template};
            
                Drivers.Add(name, Driver);
        }



        public bool PrintBarcode(String name, String Model,String ModelCode, String date,String serialno)
        {
            try
            {
                if (Drivers.ContainsKey(name))
                {

                    String BarcodeData = File.ReadAllText(Drivers[name].Template);
                    BarcodeData = BarcodeData.Replace("MODEL", Model);
                    BarcodeData = BarcodeData.Replace("B601A>51401010001", ModelCode + ">5" + date + serialno);
                    return Drivers[name].NetworkPrint(BarcodeData);
                }
                return false;


                
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool PrintBarcode(String name, String Model, String ModelCode, String date, String serialno, String Template)
        {
            try
            {
                if (Drivers.ContainsKey(name))
                {

                    String BarcodeData = File.ReadAllText(Template);
                    BarcodeData = BarcodeData.Replace("MODEL", Model);
                    BarcodeData = BarcodeData.Replace("B601A>51401010001", ModelCode + ">5" + date + serialno);
                    return Drivers[name].NetworkPrint(BarcodeData);
                }
                return false;



            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool PrintBarcode(String name, Model m,String date,String serialno)
        {
            try
            {
                if (Drivers.ContainsKey(name))
                {
                    if (m.Name.Contains("Dummy"))
                    {

                        String BarcodeData = File.ReadAllText("Dummy" + Drivers[name].Template);
                        BarcodeData = BarcodeData.Replace("MODEL", m.Name);
                        BarcodeData = BarcodeData.Replace("B601A>51401010001", m.Code + ">5" + date + serialno);
                        return Drivers[name].NetworkPrint(BarcodeData);
                    }
                }
                return false;



            }
            catch (Exception e)
            {
                return false;
            }
        }

       

        public bool PrintCombSticker(Model m, String barCode)
        {
            try
            {
                String CombStickerData = File.ReadAllText(TemplatePath+ m.Code+ "_" + m.Name + ".prn");
                CombStickerData = 
                    CombStickerData.Replace("PRODUCT", m.Product);

                CombStickerData = 
                    CombStickerData.Replace("PRODNO", m.ProductNumber);

                CombStickerData = 
                    CombStickerData.Replace("{MRP}", String.Format("{0:n}", m.MRP));

                CombStickerData = 
                    CombStickerData.Replace("MODELNAME", m.Name);

                CombStickerData = 
                    CombStickerData.Replace("STORAGECAPACITY", (Math.Round(m.Power,1)).ToString());

                CombStickerData = 
                    CombStickerData.Replace("NETQUANTITY", (Math.Round(m.Volume,1)).ToString());

                CombStickerData = 
                    CombStickerData.Replace("B163A>51401010001", m.Code + ">5" + barCode.Substring(4,6) + barCode.Substring(10,4));

                CombStickerData = 
                    CombStickerData.Replace("{H}X{D}X{W}", Convert.ToInt32(m.Height).ToString()
                    +" X "+Convert.ToInt32(m.Depth).ToString()+" X " +Convert.ToInt32(m.Width).ToString());

                CombStickerData = 
                    CombStickerData.Replace("MM/YYYY", barCode.Substring(6, 2) + "/" + "20" + barCode.Substring(4, 2));

                CombStickerData = 
                    CombStickerData.Replace("12345678901234", barCode);


                return clsPrint.SendStringToPrinter(CombinationPrinterName, CombStickerData);
            }
            catch
            {
                return false;
            }
        }


        public bool PrintCombSticker(Model m, String barCode,String template)
        {
            try
            {
                String CombStickerData = File.ReadAllText(TemplatePath + m.Code + "_" + m.Name + ".prn");
                CombStickerData = CombStickerData.Replace("PRODUCT", m.Product);
                CombStickerData = CombStickerData.Replace("PRODNO", m.ProductNumber);
                CombStickerData = CombStickerData.Replace("{MRP}", String.Format("{0:n}", m.MRP));
                CombStickerData = CombStickerData.Replace("MODELNAME", m.Name);
                CombStickerData = CombStickerData.Replace("STORAGECAPACITY", Convert.ToInt32(m.Power).ToString());
                CombStickerData = CombStickerData.Replace("NETQUANTITY", Convert.ToInt32(m.Volume).ToString());
                CombStickerData = CombStickerData.Replace("B163A>51401010001", m.Code + ">5" + barCode.Substring(4, 6) + barCode.Substring(10, 4));
                CombStickerData = CombStickerData.Replace("{H}X{D}X{W}", Convert.ToInt32(m.Height).ToString()
                    + " X " + Convert.ToInt32(m.Depth).ToString() + " X " + Convert.ToInt32(m.Width).ToString());
                CombStickerData = CombStickerData.Replace("MM/YYYY", barCode.Substring(6, 2) + "/" + "20" + barCode.Substring(4, 2));

                CombStickerData = CombStickerData.Replace("12345678901234", barCode);


                return clsPrint.SendStringToPrinter(CombinationPrinterName, CombStickerData);
            }
            catch
            {
                return false;
            }
        }



        #endregion
    }
}
