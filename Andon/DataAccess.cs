using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Collections;
using System.Data.Sql;
using System.ComponentModel;
using System.Collections.ObjectModel;
using TestBenchApp.Entity;
using shared.Entity;
using shared;
using System.Data.OleDb;


namespace TestBenchApp
{
    public class DataAccess
    {
        public enum TransactionStatus
        {
            NONE = 0, OPEN = 1, INPROCESS = 2,
            COMPLETE = 3, TIMEOUT = 4
        };
        public static String conStr;
        public static String TestDBConnectionString;
        #region CONSTRUCTORS
        public DataAccess()
        {
            if (conStr == String.Empty)
            {
                conStr = ConfigurationSettings.AppSettings["DBConStr"];
                TestDBConnectionString = ConfigurationSettings.AppSettings["TestDBConStr"];
            }
        }

        public DataAccess(String conStr)
        {
            DataAccess.conStr = conStr;
        }

        public DataAccess(String conStr, String testdbStr)
        {
            DataAccess.conStr = conStr;
            DataAccess.TestDBConnectionString = testdbStr;
        }

        #endregion

        #region LINEMANAGEMENT
        public lineCollection getLines()
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            lineCollection lines = new lineCollection();

            String qry = String.Empty;
            qry = @"SELECT id , description FROM lines ORDER BY id";

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                lines.add(new line((int)dt.Rows[i]["id"], (string)dt.Rows[i]["description"]));
            }

            con.Close();
            con.Dispose();
            return lines;
        }


        public void addLine(int ID, string description)
        {

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"insert into lines(id ,description) values({0},'{1}')";
            qry = String.Format(qry, ID, description);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }


        public void deleteLine(int ID)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"delete from lines where id={0}";
            qry = String.Format(qry, ID);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        public void updateLine(int currentID, int modifiedID, string description)
        {

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"update lines set id = {0} , description ='{1}' where id ={2}";
            qry = String.Format(qry, modifiedID, description, currentID);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }



        public StationCollection getStations(int line)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            StationCollection stations = new StationCollection();

            String qry = String.Empty;
            qry = @"SELECT id , description FROM stations where line={0} ORDER by id";

            qry = String.Format(qry, line);

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                stations.Add(new Station(line, (int)dt.Rows[i]["id"], (string)dt.Rows[i]["description"]));
            }

            con.Close();
            con.Dispose();
            return stations;
        }


        public void removeStation(int line, int station)
        {

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"delete from stations where line = {0} and id= {1}";
            qry = String.Format(qry, line, station);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();

            qry = @"delete from class where line ={0} and id = {0}";
            qry = String.Format(qry, line, station);
            cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();

            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        public void addStation(int line, int ID, string description)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"insert into stations(line , id , description)
                    values({0},{1},'{2}')";
            qry = String.Format(qry, line, ID, description);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        public ClassCollection getClass(int line, int station, int department)
        {

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            ClassCollection Class = new ClassCollection();

            String qry = String.Empty;
            qry = @"SELECT id , description FROM class where line={0} and station = {1} and
                department={2} ORDER by id";

            qry = String.Format(qry, line, station, department);

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Class.Add(new Class(department, (int)dt.Rows[i]["id"], (string)dt.Rows[i]["description"]));
            }

            con.Close();
            con.Dispose();
            return Class;
        }

        public void addClass(int line, int station, int department, int code, string description)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"insert into class(line ,station, department , id, description)
                    values({0},{1},{2},{3},'{4}')";
            qry = String.Format(qry, line, station, department, code, description);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        public void removeClass(int line, int station, int department, int code)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"delete from class where line={0} and station={1}
                    and department={2} and  id = {3}";
            qry = String.Format(qry, line, station, department, code);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        #endregion

        #region USERS
        public Users GetUsers()
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            Users users = new Users();

            String qry = String.Empty;
            qry = @"SELECT * FROM Users ";

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                users.Add(new User((string)dt.Rows[i]["Name"], (string)dt.Rows[i]["password"]));
            }

            con.Close();
            con.Dispose();
            return users;
        }

        public bool Login(String UserName, String Password)
        {
            SqlConnection cn;
            SqlCommand cmd;

            cn = new SqlConnection(conStr);
            String query = @"SELECT [Password] FROM [Users] WHERE  Name='{0}'";
            query = String.Format(query, UserName);

            cmd = new SqlCommand(query, cn);
            cn.Open();
            cmd.Connection = cn;
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cn.Close();
            if (dt.Rows.Count > 0)
            {
                if (((string)dt.Rows[0]["Password"]) == Password)
                    return true;


            }

            return false;

        }


        #endregion

        #region MODEL

        public ObservableCollection<Model> GetModels(MODEL_TYPE type)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"SELECT * from  Models where [Type] = {0} order by 'Product'";

            qry = String.Format(qry, (int)type);

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            ObservableCollection<Model> models = new ObservableCollection<Model>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                models.Add(new Model
                {
                    SlNo = (int)dt.Rows[i]["SlNo"],
                    Product = (string)dt.Rows[i]["Product"],
                    
                    ProductNumber = (string)dt.Rows[i]["ProductNumber"],
                    Name = (string)dt.Rows[i]["Name"],
                    Code = (string)dt.Rows[i]["Code"],
                    Volume = (dt.Rows[i]["Volume"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["Volume"],
                    Power = (dt.Rows[i]["Power"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["Power"],
                    
                    MRP = (dt.Rows[i]["MRP"] == DBNull.Value) ? 0 : (int)dt.Rows[i]["MRP"],
                    

                    
                    //CustomerCare = (string)dt.Rows[i]["CustomerCare"],
                    //Email = (string)dt.Rows[i]["Email"],

                    //Width = (int)dt.Rows[i]["Width"],
                    //Height = (int)dt.Rows[i]["Height"],
                    //Depth = (int)dt.Rows[i]["Depth"],
                    //ModelType = (MODEL_TYPE)dt.Rows[i]["Type"],
                    //ROTag = dt.Rows[i]["ROTag"] == DBNull.Value ? "" : (string)dt.Rows[i]["ROTag"],
                    //ByPassFunctionalTest = dt.Rows[i]["ByPassFunctionalTest"] == DBNull.Value ? true : ((bool)dt.Rows[i]["ByPassFunctionalTest"]),
                    ByPassPerformanceTest = dt.Rows[i]["ByPassPerformanceTest"] == DBNull.Value ? true : ((bool)dt.Rows[i]["ByPassPerformanceTest"]),

                    StdgLoss = (dt.Rows[i]["StdgLoss"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["StdgLoss"],

                    Vmax = (dt.Rows[i]["Vmax"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["Vmax"],
                    Vmin = (dt.Rows[i]["Vmin"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["Vmin"],
                    Imax = (dt.Rows[i]["Imax"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["Imax"],
                    Imin = (dt.Rows[i]["Imin"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["Imin"],
                    Pmax = (dt.Rows[i]["Pmax"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["Pmax"],
                    Pmin = (dt.Rows[i]["Pmin"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["Pmin"]
                     
                     
                });
            }


            con.Close();
            con.Dispose();

            return models;
        }
        public ObservableCollection<Model> GetModels()
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"SELECT * from  Models order by 'Name'";



            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            ObservableCollection<Model> models = new ObservableCollection<Model>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                models.Add(new Model
                {
                    SlNo = (int)dt.Rows[i]["SlNo"],
                    Product = dt.Rows[i]["Product"] == DBNull.Value ? "" : (string)dt.Rows[i]["Product"],




                    ProductNumber = dt.Rows[i]["ProductNumber"] == DBNull.Value ? "" : (string)dt.Rows[i]["ProductNumber"],
                    Name = dt.Rows[i]["Name"] == DBNull.Value ? "" : (string)dt.Rows[i]["Name"],
                    Code = dt.Rows[i]["Code"] == DBNull.Value ? "" : (string)dt.Rows[i]["Code"],
                    Volume = dt.Rows[i]["Volume"] == DBNull.Value ? 0.0 : (double)dt.Rows[i]["Volume"],
                    Power = dt.Rows[i]["Power"] == DBNull.Value ? 0.0 : (double)dt.Rows[i]["Power"],
                    MRP = dt.Rows[i]["MRP"] == DBNull.Value ? 0: (int)dt.Rows[i]["MRP"],
                    CustomerCare = dt.Rows[i]["CustomerCare"] == DBNull.Value ? "" : (string)dt.Rows[i]["CustomerCare"],
                    Email = dt.Rows[i]["Email"] == DBNull.Value ? "" : (string)dt.Rows[i]["Email"],

                    Width = dt.Rows[i]["Width"] == DBNull.Value ? 0 : (int)dt.Rows[i]["Width"],
                    Height = dt.Rows[i]["Height"] == DBNull.Value ? 0 : (int)dt.Rows[i]["Height"],
                    Depth = dt.Rows[i]["Depth"] == DBNull.Value ? 0 : (int)dt.Rows[i]["Depth"],
                    //ModelType = (MODEL_TYPE)dt.Rows[i]["Type"],
                    ROTag = dt.Rows[i]["ROTag"] == DBNull.Value ? "" : (string)dt.Rows[i]["ROTag"],
                    ByPassFunctionalTest = dt.Rows[i]["ByPassFunctionalTest"] == DBNull.Value ? true : ((bool)dt.Rows[i]["ByPassFunctionalTest"]),
                    ByPassPerformanceTest = dt.Rows[i]["ByPassPerformanceTest"] == DBNull.Value ? true : ((bool)dt.Rows[i]["ByPassPerformanceTest"]),

                    StdgLoss = (dt.Rows[i]["StdgLoss"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["StdgLoss"],

                    Vmax = (dt.Rows[i]["Vmax"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["Vmax"],
                    Vmin = (dt.Rows[i]["Vmin"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["Vmin"],
                    Imax = (dt.Rows[i]["Imax"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["Imax"],
                    Imin = (dt.Rows[i]["Imin"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["Imin"],
                    Pmax = (dt.Rows[i]["Pmax"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["Pmax"],
                    Pmin = (dt.Rows[i]["Pmin"] == DBNull.Value) ? 0.00 : (double)dt.Rows[i]["Pmin"]
                });
            }


            con.Close();
            con.Dispose();

            return models;
        }

        internal void InsertModel(Model m)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            qry = @"insert into [Models] ( Product, 
                                            ProductNumber, 
                                            Name,
                                            Power,
                                            Volume,
                        Code,MRP,StdgLoss,Vmax,Vmin,Imax,Imin,Pmax,Pmin,ByPassPerformanceTest)
                    values('{0}', '{1}', '{2}',{3}, {4}, '{5}',{6},{7},{8},{9},{10},{11},{12},{13},'{14}')";


            qry = String.Format(qry,
                m.Product, m.ProductNumber, m.Name, m.Power, m.Volume, m.Code, m.MRP,m.StdgLoss, m.Vmax,
                m.Vmin, m.Imax, m.Imin, m.Pmax, m.Pmin,m.ByPassPerformanceTest);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        internal void UpdateModel(Model m)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            qry = @"Update [Models] set Product ='{0}' , ProductNumber = '{1}', Name =  '{2}' ,
                    Power = {3} ,Volume ={4} ,
                        Code = '{5}',MRP={6},StdgLoss={7},Vmax={8},
                        Vmin = {9},Imax = {10},Imin = {11},Pmax={12},Pmin = {13},
                        ByPassPerformanceTest='{14}' 
                        where SlNo = {15}";

            qry = String.Format(qry,
                m.Product, m.ProductNumber, m.Name, m.Power, m.Volume, m.Code, m.MRP, m.StdgLoss,
                m.Vmax, m.Vmin, m.Imax, m.Imin, m.Pmax, m.Pmin,m.ByPassPerformanceTest, m.SlNo);


            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        internal void DeleteModel(Model model)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            qry = @"delete from [Models] where slNo = {0}";
            qry = String.Format(qry, model.SlNo);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        #endregion

        #region PLAN
        public List<Plan> GetPlans(Model.Type unitType)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"SELECT * from  Plans inner join Models on [Plans].Model = [Models].Code where timestamp > '{0}'
                    and timestamp < '{1}' and UnitType = {2}";

            qry = String.Format(qry, DateTime.Now.ToString("yyyy-MM-dd"), DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"), (int)unitType);


            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            List<Plan> plans = new List<Plan>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                plans.Add(new Plan
                {
                    slNumber = (int)dt.Rows[i]["slNo"],
                    ModelCode = (dt.Rows[i]["Model"] == DBNull.Value) ? "" : (string)dt.Rows[i]["Model"],
                    Actual = (dt.Rows[i]["Actual"] == DBNull.Value) ? 0 : (int)dt.Rows[i]["Actual"],
                    Quantity = (dt.Rows[i]["Quantity"] == DBNull.Value) ? 0 : (int)dt.Rows[i]["Quantity"],
                    BSerialNo = (dt.Rows[i]["BSerial"] == DBNull.Value) ? 0 : (int)dt.Rows[i]["BSerial"],
                    FSerialNo = (dt.Rows[i]["FSerial"] == DBNull.Value) ? 0 : (int)dt.Rows[i]["FSerial"],
                    IntegratedSerialNo = (dt.Rows[i]["IntegrationSerial"] == DBNull.Value) ? 0 : (int)dt.Rows[i]["IntegrationSerial"],
                    CombinationSerialNo = (dt.Rows[i]["CombinationSerial"] == DBNull.Value) ? 0 : (int)dt.Rows[i]["CombinationSerial"],
                    BStatus = (dt.Rows[i]["BStatus"] == DBNull.Value) ? false : (bool)dt.Rows[i]["BStatus"],
                    FStatus = (dt.Rows[i]["FStatus"] == DBNull.Value) ? false : (bool)dt.Rows[i]["FStatus"],
                    ModelName = (dt.Rows[i]["Product"] == DBNull.Value) ? "" : (String)dt.Rows[i]["Product"],
                    UnitType = (dt.Rows[i]["UnitType"] == DBNull.Value) ? Model.Type.NONE : (Model.Type)dt.Rows[i]["UnitType"]
                });
            }


            con.Close();
            con.Dispose();

            return plans;
        }


        public void UpdatePlan(Plan p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"update [Plans] set Actual = {0}, [BStatus] = '{1}',[FStatus] = '{2}',
                    BSerial={3},FSerial={4},CombinationSerial={5}
                    where SlNo={6}";
            qry = String.Format(qry, p.Actual, p.BStatus, p.FStatus, p.BSerialNo, p.FSerialNo, p.CombinationSerialNo, p.slNumber);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        public void ModifyPlan(Plan p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"update [Plans] set Quantity = {0} , FSerial = {1} , CombinationSerial = {2} , Actual = {2}
                    where SlNo={3}";
            qry = String.Format(qry, p.Quantity,p.FSerialNo,p.CombinationSerialNo, p.slNumber);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }



        public void UpdateFSerial(Plan p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"update [Plans] set FSerial = {0}
                    where SlNo={1}";
            qry = String.Format(qry, p.FSerialNo, p.slNumber);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }


        public void UpdateCSerial(Plan p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"update [Plans] set CombinationSerial = {0}
                    where SlNo={1}";
            qry = String.Format(qry, p.CombinationSerialNo, p.slNumber);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        public void UpdateISerial(Plan p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"update [Plans] set IntegrationSerial = {0}
                    where SlNo={1}";
            qry = String.Format(qry, p.IntegratedSerialNo, p.slNumber);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        public void UpdateBSerial(Plan p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"update [Plans] set BSerial = {0}
                    where SlNo={1}";
            qry = String.Format(qry, p.BSerialNo, p.slNumber);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        public void UpdatePlanQuantity(Plan p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"update [Plans] set Quantity = {0}
                    where SlNo={1}";
            qry = String.Format(qry, p.Quantity, p.slNumber);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        public void UpdateBPlanStatus(Plan p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"update [Plans] set BStatus = '{0}'
                    where SlNo={1}";
            qry = String.Format(qry, p.BStatus, p.slNumber);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        public void UpdateFPlanStatus(Plan p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"update [Plans] set FStatus = '{0}'
                    where SlNo={1}";
            qry = String.Format(qry, p.FStatus, p.slNumber);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        internal void InsertPlan(Plan p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            qry = @"insert into [Plans] ( Model, Quantity, timestamp,UnitType)
                    values('{0}', {1}, '{2}',{3})";
            qry = String.Format(qry, p.ModelCode, p.Quantity, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), (int)p.UnitType);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();


        }


        internal void DeletePlan(Plan p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            qry = @"delete from [Plans] where slNo = {0}";
            qry = String.Format(qry, p.slNumber);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        internal void UpdateActual(int actual, int p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"update [Plans] set Actual = '{0}'
                    where SlNo={1}";
            qry = String.Format(qry, actual, p);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }
        #endregion


        #region TRACKING
        public void InsertUnit(String model, Model.Type type, int serialNo)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            qry = @"insert into [Unit](Model,SerialNo,[Type],Status,[Timestamp],Barcode)
                        values('{0}',{1},{2},'{3}','{4}','{5}')";

            DateTime ts = DateTime.Now;
            String barcode = model + ((type == Model.Type.BODY) ? "A" : "") + ts.ToString("yyMMdd") + serialNo.ToString("D4");

            qry = String.Format(qry, model, serialNo, (int)type, "NG", ts.ToString("yyyy-MM-dd HH:mm:ss"), barcode);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();


            con.Close();
            con.Dispose();

            //InsertUnitAssociation(model, barcode, Model.Type.FRAME);
        }

        public void InsertUnitAssociation(String model, String barcode, Model.Type type)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            DateTime ts = DateTime.Now;
            if (type == Model.Type.BODY)
            {
                qry = @"begin
                        insert into [UnitAssociation] (BCode,BTimestamp,Model,Timestamp)
                        values('{0}','{1}','{2}','{1}')
                       
                        END";
            }
            else if (type == Model.Type.FRAME)
            {
                qry = @"begin
                        insert into [UnitAssociation] (FCode,ICode,FTimestamp,Model,Timestamp)
                        values('{0}','{0}','{1}','{2}','{1}')
                      
                        END";
            }




            qry = String.Format(qry, barcode, ts.ToString("yyyy-MM-dd HH:mm:ss"), model);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();


            con.Close();
            con.Dispose();
        }
        public void UpdateUnit(String barcode)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"bEGIN
                IF(SELECT COUNT(*) FROM Unit WHERE Status <>  'OK' AND Barcode = '{1}') > 0

                UPDATE [Unit] SET Status = 'OK', Timestamp = '{0}' where  Barcode= '{1}'
                END";
            qry = String.Format(qry, DateTime.Now.ToString("MM-dd-yyyy HH:mm:ss"), barcode);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        public void UpdateUnit(string status,String barcode)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"bEGIN
               

                UPDATE [Unit] SET [Status] = '{0}' where  Barcode= '{1}'
                END";
            qry = String.Format(qry, status, barcode);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();
        }

        internal bool UnitExists(string barcode)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            DateTime ts = DateTime.Now;

            qry = @"Select [Barcode] from [Unit] where [Barcode]='{0}'";

            qry = String.Format(qry, barcode);

            SqlCommand cmd = new SqlCommand(qry, con);

            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();
            con.Close();
            con.Dispose();

            if (dt.Rows.Count <= 0 || dt.Rows.Count > 1) return false;
            if (dt.Rows[0][0] == DBNull.Value) return false;

            if ((String)dt.Rows[0][0] == barcode) return true;
            else return false;
        }


        internal bool CheckOKStatus(string barcode)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            bool result = false;

            String qry = String.Empty;
            qry = @"SELECT [Status] from [Unit] where Barcode='{0}'";
            qry = String.Format(qry, barcode);

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            if (dt.Rows.Count == 0) return false;
            if (dt.Rows.Count == 1)
            {
                if (dt.Rows[0][0] == DBNull.Value)
                    return false;

                return ((String)dt.Rows[0][0] == "OK");
            }
            else return false;

        }

        internal bool CheckFcodeSerialNo(string barcode,int slNo)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            bool result = false;

            String qry = String.Empty;
            qry = @"SELECT [SlNo] from [UnitAssociation] where FCode='{0}'";
            qry = String.Format(qry, barcode);

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            if (dt.Rows.Count != 1) return false;

            if ((int)dt.Rows[0][0] == slNo) return true;

            else return false;

        }

        internal bool CheckBcodeSerialNo(string barcode, int slNo)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            bool result = false;

            String qry = String.Empty;
            qry = @"SELECT [SlNo] from [UnitAssociation] where BCode='{0}'";
            qry = String.Format(qry, barcode);

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            if (dt.Rows.Count != 1) return false;

            if ((int)dt.Rows[0][0] == slNo) return true;

            else return false;

        }

        internal bool CheckIcodeSerialNo(string barcode, int slNo)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            bool result = false;

            String qry = String.Empty;
            qry = @"SELECT [SlNo] from [UnitAssociation] where ICode='{0}'";
            qry = String.Format(qry, barcode);

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            if (dt.Rows.Count != 1) return false;

            if ((int)dt.Rows[0][0] == slNo) return true;

            else return false;

        }

        internal bool CheckCcodeSerialNo(string barcode, int slNo)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            bool result = false;

            String qry = String.Empty;
            qry = @"SELECT [SlNo] from [UnitAssociation] where CCode='{0}'";
            qry = String.Format(qry, barcode);

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            if (dt.Rows.Count != 1) return false;

            if ((int)dt.Rows[0][0] == slNo) return true;

            else return false;

        }

        internal bool  CheckROcodeSerialNo(string barcode, int slNo)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            bool result = false;

            String qry = String.Empty;
            qry = @"SELECT [SlNo] from [UnitAssociation] where [ROFilter] ='{0}'";
            qry = String.Format(qry, barcode);

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();


            if (dt.Rows.Count == 0) return true;

            else return false;
            

        }

        internal string UnitAssociated(Model.Type type, String model, int duration)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            bool result = false;

            String qry = String.Empty;
            if (type == Model.Type.BODY)
            {
                qry = @"SELECT [FCode] from [UnitAssociation]  where FCode is not null and BCode is null 
                            and Model = '{0}' and DATEDIFF(ss,FTimestamp,GETDATE()) < {1} ";
            }
            else if (type == Model.Type.FRAME)
            {
                qry = @"SELECT [BCode] from [UnitAssociation] where BCode is not null and FCode is null 
                        and Model = '{0}' and DATEDIFF(ss,BTimestamp,GETDATE()) < {1}";
            }

            qry = String.Format(qry, model, duration);
            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            if (dt.Rows.Count == 0) return String.Empty;
            if (dt.Rows.Count == 1)
            {
                if (dt.Rows[0][0] == DBNull.Value)
                    return String.Empty;

                return (String)dt.Rows[0][0];
            }
            else return String.Empty;
        }

        internal int UpdateAssociation(string barcode, Model.Type type, string assocationBarcode, string iCode)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            DateTime ts = DateTime.Now;

            int result = 0;

            if (type == Model.Type.BODY)
            {
                qry = @"begin
                        update [UnitAssociation] set BCode='{0}',BTimestamp = '{1}',Timestamp='{1}',[ICode]='{3}'
                        where [FCode]='{2}'
                         IF(SELECT COUNT(*) FROM Unit WHERE Status <>  'OK' AND Barcode = '{0}') > 0

                        UPDATE [Unit] SET Status = 'OK', Timestamp = '{1}' where  Barcode= '{0}'
                        IF(SELECT COUNT(*) FROM Unit WHERE Status <>  'OK' AND Barcode = '{2}') > 0

                        UPDATE [Unit] SET Status = 'OK', Timestamp = '{1}' where  Barcode= '{2}'
                        END";
                qry = String.Format(qry, barcode, ts.ToString("yyyy-MM-dd HH:mm:ss"), assocationBarcode, iCode);
            }
            else if (type == Model.Type.FRAME)
            {
                qry = @"begin
                        update [UnitAssociation] set FCode='{0}',FTimestamp = '{1}',Timestamp='{1}',[ICode]='{3}'
                        where [BCode]='{2}'
                         IF(SELECT COUNT(*) FROM Unit WHERE Status <>  'OK' AND Barcode = '{0}') > 0

                        UPDATE [Unit] SET Status = 'OK', Timestamp = '{1}' where  Barcode= '{0}'

                        IF(SELECT COUNT(*) FROM Unit WHERE Status <>  'OK' AND Barcode = '{2}') > 0

                        UPDATE [Unit] SET Status = 'OK', Timestamp = '{1}' where  Barcode= '{2}'
                        END";
                qry = String.Format(qry, barcode, ts.ToString("yyyy-MM-dd HH:mm:ss"), assocationBarcode, iCode);
            }
            else if (type == Model.Type.COMBINED)
            {
                qry = @"begin
                        update [UnitAssociation] set CCode='{0}',CTimestamp = '{1}',Timestamp='{1}'
                        where [FCode]='{2}' and CCode is null

                        END";
                qry = String.Format(qry, barcode, ts.ToString("yyyy-MM-dd HH:mm:ss"), iCode);
            }


            SqlCommand cmd = new SqlCommand(qry, con);

            result = cmd.ExecuteNonQuery();
            cmd.Dispose();


            con.Close();
            con.Dispose();

            return result;
        }

        internal int CheckRecordAssociation(ReportRecord r)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            DateTime ts = DateTime.Now;

           
            if (r.Main_Frame_Barcode != String.Empty)
                if (!CheckFcodeSerialNo(r.Main_Frame_Barcode,r.SlNo)) return 1;
            if (r.Main_Body_Barcode != String.Empty)
                if (!CheckBcodeSerialNo(r.Main_Body_Barcode, r.SlNo)) return 2;
            if (r.Integrated_Barcode != String.Empty)
                if (!CheckIcodeSerialNo(r.Integrated_Barcode, r.SlNo)) return 3;
            if (r.Combination_Barcode != String.Empty)
                if (!CheckCcodeSerialNo(r.Combination_Barcode, r.SlNo)) return 4;
            if (r.ROFilterBarcode != String.Empty)
                if (CheckROcodeSerialNo(r.ROFilterBarcode, r.SlNo)) return 0; 

                
            
                else return 5;
            else return 6;
                    
                
        }

           
               

        internal String CheckIntegrationStatus(string p , out bool exists , out String CCode)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();
            exists = false;
            CCode = String.Empty;
            String FCode = String.Empty;
            
            String qry = String.Empty;
            DateTime ts = DateTime.Now;

            qry = @"Select [Barcode] from [Unit] where [Barcode]='{0}' ";

            qry = String.Format(qry, p);

            SqlCommand cmd = new SqlCommand(qry, con);

            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();
            

            

            if (dt.Rows.Count <= 0 || dt.Rows.Count > 1) return "";
            if (dt.Rows[0][0] == DBNull.Value) return "";

            

            if ((String)dt.Rows[0]["Barcode"] == p)
            {
                exists = true;
                FCode = p;
            }
            else return "";


            qry = @"Select [CCode] from [UnitAssociation] where [FCode]='{0}' ";

            qry = String.Format(qry, p);

            cmd = new SqlCommand(qry, con);

            dr = cmd.ExecuteReader();
            dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();
          



            if (dt.Rows.Count <= 0 || dt.Rows.Count > 1) return FCode;
            if (dt.Rows[0][0] == DBNull.Value) return FCode;

            else CCode = (String)dt.Rows[0]["CCode"];

            con.Close();
            con.Dispose();

            return FCode;



        }

        internal bool CheckWaitingStatus(string p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            DateTime ts = DateTime.Now;

            qry = @"Select [CCode] from [UnitAssociation] where [CCode]='{1}'  and [Status] = 'Waiting'";

            qry = String.Format(qry, ts.ToString("yyyy-MM-dd HH:mm:ss"), p);

            SqlCommand cmd = new SqlCommand(qry, con);

            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();
            con.Close();
            con.Dispose();

            if (dt.Rows.Count <= 0 || dt.Rows.Count > 1) return false;
            if (dt.Rows[0][0] == DBNull.Value) return false;

            if ((String)dt.Rows[0][0] == p) return true;
            else return false;



        }

        internal int CheckBarcodeStatus(string p)
        {
            if (CheckAssociationStatus(p)) return 1;
            if (CheckROStatus(p)) return 2;

            return 0;
        }

        internal bool CheckAssociationStatus(string p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            DateTime ts = DateTime.Now;

            qry = @"Select [CCode] from [UnitAssociation] where [CCode]='{1}'  and [Status] = 'OK'";

            qry = String.Format(qry, ts.ToString("yyyy-MM-dd HH:mm:ss"), p);

            SqlCommand cmd = new SqlCommand(qry, con);

            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();
            con.Close();
            con.Dispose();

            if (dt.Rows.Count == 0) return false;
            if ((String)dt.Rows[0][0] == p) return true;
            else return false;
        }

        internal bool CheckROStatus(string p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            DateTime ts = DateTime.Now;

            qry = @"Select [ROFilter] from [UnitAssociation] where [CCode]='{1}'  and [Status] = 'OK'";

            qry = String.Format(qry, ts.ToString("yyyy-MM-dd HH:mm:ss"), p);

            SqlCommand cmd = new SqlCommand(qry, con);

            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();
            con.Close();
            con.Dispose();

            if (dt.Rows.Count == 0) return false;
            if ((String)dt.Rows[0][0] == p) return true;
            else return false;
        }



        internal bool UpdateWaitingStatus(String combinationCode)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            DateTime ts = DateTime.Now;

            qry = @"update [UnitAssociation] set Status='Waiting'
                     where [CCode]='{0}' and [Status] is null";

            qry = String.Format(qry, combinationCode);

            SqlCommand cmd = new SqlCommand(qry, con);
            int result = cmd.ExecuteNonQuery();
            cmd.Dispose();


            con.Close();
            con.Dispose();
            return (result == 1);
        }

        internal bool UpdateAsscociationStatus( String combinationCode)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            DateTime ts = DateTime.Now;

            qry = @"update [UnitAssociation] set Status='OK',FGTimestamp = '{0}',Timestamp='{0}'
                     where [CCode]='{1}' ";

            qry = String.Format(qry, ts.ToString("yyyy-MM-dd HH:mm:ss"), combinationCode);

            SqlCommand cmd = new SqlCommand(qry, con);
            int result = cmd.ExecuteNonQuery();
            cmd.Dispose();


            con.Close();
            con.Dispose();
            return (result == 1);
        }

        internal bool UnitProcessed(string barcode)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            bool result = false;

            String qry = String.Empty;
            qry = @"SELECT [Status] from [Unit] where Barcode='{0}'";
            qry = String.Format(qry, barcode);

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            if (dt.Rows.Count == 0) return false;
            if (dt.Rows.Count == 1)
            {
                if (dt.Rows[0][0] == DBNull.Value)
                    return false;

                return ((String)dt.Rows[0][0] == "OK");
            }
            else return false;

        }

        internal void DeleteAssociationTimeouts(int timeout)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = @"delete from [UnitAssociation] where (DATEDIFF(ss,FTimestamp,GETDATE()) > {0} and BTimestamp is null)
                    or  (DATEDIFF(ss,BTimestamp,GETDATE()) > {0} and FTimestamp is null)";
            qry = String.Format(qry, timeout);
            SqlCommand cmd3 = new SqlCommand(qry, con);


            int result = cmd3.ExecuteNonQuery();
            cmd3.Dispose();


            con.Close();
            con.Dispose();

            return;
        }

        internal DataTable GetAssociationData()
        {

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"
                select  
                FCode as [Frame Barcode],BCode as [Body Barcode],
                 ROFilter as [RO Filter],
                Status=
                case [Status] 
                when 'OK' then 'Completed'
                else 'Pending'
                end


                from UnitAssociation where
                  (CTimestamp >  CONVERT(nvarchar,GETDATE(),101) or FTimestamp > CONVERT(nvarchar,GETDATE(),101)
                    or BTimestamp > CONVERT(nvarchar,GETDATE(),101))";



            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();
            return dt;
        }

        internal ReportRecord GetAssociationData(String fcode, String bcode, String icode,String ccode,string rofilter)
        {
            ReportRecord r = null;
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"select * from UnitAssociation where ";

            if (fcode != String.Empty) { qry += @" FCode = '{0}'"; qry = String.Format(qry, fcode); }
            else if (bcode != String.Empty) { qry += @" BCode = '{0}'"; qry = String.Format(qry, bcode); }
            else if (icode != String.Empty) { qry += @" ICode = '{0}'"; qry = String.Format(qry, icode); }
            else if (ccode != String.Empty) { qry += @" CCode = '{0}'"; qry = String.Format(qry, ccode); }
            else if (rofilter != String.Empty) { qry += @" ROFilter = '{0}'"; qry = String.Format(qry, rofilter); }

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();
            if(dt.Rows.Count != 1) return null;
            r = new ReportRecord
            {
                SlNo = dt.Rows[0]["SlNo"] == DBNull.Value ? -1 : (int)dt.Rows[0]["SlNo"],
                Model = dt.Rows[0]["Model"] == DBNull.Value ? ("") : (String)dt.Rows[0]["Model"],
                Main_Frame_Barcode = dt.Rows[0]["FCode"] == DBNull.Value ? ("") : (String)dt.Rows[0]["FCode"],
                Main_Body_Barcode = dt.Rows[0]["BCode"] == DBNull.Value ? ("") : (String)dt.Rows[0]["BCode"],
                Integrated_Barcode = dt.Rows[0]["ICode"] == DBNull.Value ? ("") : (String)dt.Rows[0]["ICode"],
                Combination_Barcode = dt.Rows[0]["CCode"] == DBNull.Value ? ("") : (String)dt.Rows[0]["CCode"],
                ROFilterBarcode = dt.Rows[0]["ROFilter"] == DBNull.Value ? ("") : (String)dt.Rows[0]["ROFilter"],
                Status = dt.Rows[0]["Status"] == DBNull.Value ? ("") : (String)dt.Rows[0]["Status"]
            };

            return r;
        }
        #endregion

        #region REPRINT
        internal DataTable GetReprintSerialNos(string p)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;

            switch (p)
            {
                case "F1":
                    qry = @"select Name as Model, Barcode  from [Unit] 
                            inner join Models on Models.Code = [Unit].Model
                              where [status] = 'NG'  and [Unit].[Type] = {0}  and Timestamp >'{1}' 
                                and Timestamp <'{2}' order by Barcode,Model ";
                    qry = String.Format(qry, (int)Model.Type.FRAME, DateTime.Today.ToString("yyyy-MM-dd")
                        , DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"));

                    break;
                case "M1":
                    qry = @"select Name as Model, Barcode  from [Unit] 
                            inner join Models on Models.Code = [Unit].Model
                              where [status] = 'NG'  and [Unit].[Type] = {0}  and Timestamp >'{1}'
                                and Timestamp <'{2}'  order by Barcode,Model  ";
                    qry = String.Format(qry, (int)Model.Type.BODY, DateTime.Today.ToString("yyyy-MM-dd")
                        , DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"));
                    break;
                case "CS":
                    qry = @"select CCode as 'Barcode' from [UnitAssociation] where ([status] = 'NG' or [status] is null)
                                  and CTimestamp >'{0}' and CTimestamp <'{1}'  order by Barcode ";
                    qry = String.Format(qry, DateTime.Today.ToString("yyyy-MM-dd")
                        , DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"));
                    break;

                case "TOK":
                    qry = @"select FCode as 'Barcode' from [UnitAssociation] where ( [CCode] is null)
                                  and FTimestamp >'{0}' and FTimestamp <'{1}' and BTimestamp >'{0}'
                                    and BTimestamp <'{1}'  order by Barcode";
                    qry = String.Format(qry, DateTime.Today.ToString("yyyy-MM-dd")
                        , DateTime.Today.AddDays(1).ToString("yyyy-MM-dd"));
                    break;
            }

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();

            dt.Load(dr);
            dr.Close();
            return dt;

        }




        #endregion

        #region ReportTab

        public DataTable GetReportData(DateTime from, DateTime to)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            to = to.AddDays(1);
            String qry = @"select Substring(Convert(nvarchar,[Timestamp],113),0,21) as [Date-Time],
            Models.Name as Model,FCode as [Unit Barcode] , CCode as [Combination Barcode]

            from UnitAssociation
            inner join Models on UnitAssociation.Model = Models.Code
            and [Timestamp] > '{0}' and [Timestamp] < '{1}' order by [Timestamp] desc";

            qry = String.Format(qry, from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"));

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);

            dr.Close();

            return dt;


        }


        public DataTable GetOpenIssues()
        {
            SqlConnection localCon = new SqlConnection(conStr);
            String qry = @"select distinct Substring(Convert(nvarchar,issues.timestamp,0),0,12) as DATE, 
                        issues.slNo as SlNo,
                        issues.station as STATION_NO,
                        lines.description as LINE , 
                        stations.description as STATION_NAME,
                        departments.description as ISSUE , 
                        issues.data as DETAILS,
                        CONVERT(TIME(0), issues.timestamp,0) as RAISED 
                        from issues
                        LEFT OUTER JOIN stations on (stations.id = issues.station and stations.line = issues.line)
                        inner join lines on lines.id = issues.line
                        left outer join departments on issues.department = departments.id
                        where status='raised'";


            localCon.Open();

            SqlCommand cmd = new SqlCommand(qry, localCon);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);

            dr.Close();

            localCon.Close();
            localCon.Dispose();

            return dt;
        }





        #endregion

        #region ManualIntegration
        public ObservableCollection<ReportRecord> GetRecords(DateTime from, DateTime to)
        {
            ObservableCollection<ReportRecord> records = new ObservableCollection<ReportRecord>();
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            to = to.AddDays(1);
            String qry = @"select Substring(Convert(nvarchar,[Timestamp],113),0,21) as [Date_Time],
            Models.Name as Model,FCode as [Main_Frame_Barcode] , BCode as [Main_Body_Barcode],
            ICode as [Integrated_Barcode], CCode as [Combination_Barcode],
            ROFilter as [RO_Barcode]
           
                    
            
            from UnitAssociation
            inner join Models on UnitAssociation.Model = Models.Code
            and [Timestamp] > '{0}' and [Timestamp] < '{1}'

            union

            select Substring(Convert(nvarchar,[Timestamp],113),0,21) as [Date_Time],
            Models.Name as Model,Barcode as [Main_Frame_Barcode] , '' as [Main_Body_Barcode],'' as [RO_Barcode],
            '' as [Integrated_Barcode], ''  as [Combination_Barcode]
            
            from Unit 
            inner join Models on Unit.Model = Models.Code where Unit.[Type] = 2 and Status <> 'OK'
            and [Timestamp] > '{0}' and [Timestamp] < '{1}' 

            union

            select Substring(Convert(nvarchar,[Timestamp],113),0,21) as [Date_Time],
            Models.Name as Model, '' as [Main_Frame_Barcode], Barcode as [Main_Body_Barcode],'' as [RO_Barcode],
            '' as [Integrated_Barcode], ''  as [Combination_Barcode]
            
            from Unit 
            inner join Models on Unit.Model = Models.Code where Unit.[Type] = 1 and Status <> 'OK'
            and [Timestamp] > '{0}' and [Timestamp] < '{1}'
            order by [Date_Time] Desc";

            qry = String.Format(qry, from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"));

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);

            dr.Close();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                records.Add(new Entity.ReportRecord
                {
                    Model = (dt.Rows[i]["Model"]==DBNull.Value)?"":(String)dt.Rows[i]["Model"],
                    ROFilterBarcode = (dt.Rows[i]["RO_Barcode"] == DBNull.Value) ? "" : (String)dt.Rows[i]["RO_Barcode"],
                    Combination_Barcode = (dt.Rows[i]["Combination_Barcode"] == DBNull.Value) ? "" : (String)dt.Rows[i]["Combination_Barcode"],
                    
                    Integrated_Barcode = (dt.Rows[i]["Integrated_Barcode"] == DBNull.Value) ? "" : (String)dt.Rows[i]["Integrated_Barcode"],
                    Main_Body_Barcode = (dt.Rows[i]["Main_Body_Barcode"] == DBNull.Value) ? "" : (String)dt.Rows[i]["Main_Body_Barcode"],
                    Main_Frame_Barcode = (dt.Rows[i]["Main_Frame_Barcode"] == DBNull.Value) ? "" : (String)dt.Rows[i]["Main_Frame_Barcode"],
                });
            }

            return records;
        }

       

        public void UpdateRecords(List<ReportRecord> Records)
        {
            foreach (ReportRecord r in Records)
            {
                if (r.IsModified == true)
                {
                  //  UpdateAsscociationStatus(r.RO_Barcode, r.Combination_Barcode);

                }
            }
        }


        internal bool UpdateAssociation(ReportRecord reportRecord)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            DateTime ts = DateTime.Now;

            qry = @"update [UnitAssociation] set [FCode]='{0}', [BCode]='{1}',[ICode]='{2}',[CCode]='{3}',[ROFilter]='{4}',[Status]='MANUAL'
                    where SlNo={5}";

            qry = String.Format(qry, reportRecord.Main_Frame_Barcode,reportRecord.Main_Body_Barcode,reportRecord.Integrated_Barcode,
                reportRecord.Combination_Barcode,reportRecord.ROFilterBarcode,reportRecord.SlNo);

            SqlCommand cmd = new SqlCommand(qry, con);
            int result = cmd.ExecuteNonQuery();
            cmd.Dispose();


            con.Close();
            con.Dispose();
            return (result == 1);
        }
        #endregion


        #region TEST_DATA_ACCESS

        internal bool UnitFoundInPerformanceTestTable(string iCode)
        {
            bool result = false;




            using (SqlConnection connection = new SqlConnection(TestDBConnectionString))
            {
                connection.Open();



                String qry = "SELECT  * from  Test_Result where [Serial_No] = '{0}' ";
                qry = String.Format(qry, iCode+"\x0D");

                SqlCommand command = new SqlCommand(qry, connection);
                SqlDataReader dr = command.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dr);
                dr.Close();
                command.Dispose();

                if (dt.Rows.Count == 0)
                    return false;
                else return true;

            }


        }

        internal bool HasPassedPerformanceTest(string iCode)
        {
            bool result = false;
            



            using (SqlConnection connection = new SqlConnection(TestDBConnectionString))
            {
                connection.Open();

                

                String qry = "SELECT  * from  Test_Result where [Serial_No] = '{0}' and Result='Pass' ";
                qry = String.Format(qry, iCode + "\x0D");

                SqlCommand command = new SqlCommand(qry, connection);
                SqlDataReader dr = command.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dr);
                dr.Close();
                command.Dispose();

                if (dt.Rows.Count == 0)
                    return false;
                else return true;

            }

            
        }


        internal bool HasPassedFunctionalTest(string iCode)
        {
            bool result = false;
            String table = "MBAF_data";

           

            using (OleDbConnection connection = new OleDbConnection(TestDBConnectionString))
            {
                connection.Open();
                OleDbDataReader reader = null;
                String qry = "SELECT Top 1 * from  {1} where [Part_No] = '{0}' and MBAF_Result = 'PASS' ";
                qry = String.Format(qry, iCode, table);

                OleDbCommand command = new OleDbCommand(qry, connection);
                reader = command.ExecuteReader();
                if (reader.Read())
                {
                    String partNo = reader["Part_No"].ToString();
                    String testResult = reader["MBAF_Result"].ToString();
                    if (testResult == "PASS") result = true;
                    else result = false;
                }

            }

            return result;
        }


        internal int HasPassedFunctional_PerformanceTest(string CCode,bool byPassFunctionalTest,bool byPassPerformanceTest, out String FCode, out String ICode)
        {

            FCode = String.Empty;
            ICode = String.Empty;

            using (SqlConnection con = new SqlConnection(conStr))
            {
                con.Open();

                String qry = String.Empty;
                qry = @"select  [FCode],[ICode] from UnitAssociation where CCode='{0}'";
                qry = String.Format(qry, CCode);
                SqlCommand cmd = new SqlCommand(qry, con);
                SqlDataReader dr = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(dr);
                dr.Close();
                cmd.Dispose();

                if ((dt.Rows.Count == 0) || (dt.Rows.Count > 1)) return 0;
                if ((dt.Rows[0]["FCode"] == DBNull.Value) || (dt.Rows[0]["ICode"] == DBNull.Value)) return 0;
                FCode = (String)dt.Rows[0]["FCode"];
                ICode = (String)dt.Rows[0]["ICode"];
            }

            if (!byPassFunctionalTest)
            {

                if (!HasPassedFunctionalTest(FCode)) return 1;
            }
            if (!byPassPerformanceTest)
            {

                using (OleDbConnection connection = new OleDbConnection(TestDBConnectionString))
                {
                    connection.Open();
                    OleDbDataReader reader = null;
                    String qry = "SELECT Top 1 * from  Mbaf_data where [Part_No] = '{0}' and MBAF_Result='PASS'";
                    qry = String.Format(qry, ICode);

                    OleDbCommand command = new OleDbCommand(qry, connection);
                    reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        String partNo = reader["Part_No"].ToString();
                        String testResult = reader["MBAF_Result"].ToString();
                        if (testResult == "PASS") return 3;
                        else return 2;
                    }

                }
            }

            return 0;
        }




        #endregion

        #region LOG

        internal ObservableCollection<LogRecord> GetTodayLog()
        {
            ObservableCollection<LogRecord> Log = new ObservableCollection<LogRecord>();
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"Select * from Log where  DATEDIFF(DD,timestamp,GETDATE()) < 1 order by [Timestamp] asc";

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Log.Insert(0,new LogRecord
                {
                    Timestamp = (DateTime)dt.Rows[i]["Timestamp"],
                    Stage = (String)dt.Rows[i]["Stage"],
                    Remarks = (String)dt.Rows[i]["Remarks"],
                    Barcode = (String)dt.Rows[i]["Barcode"]
                });
            }



            return Log;
        }

        internal void InsertLogRecord(LogRecord lr)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            DateTime ts = DateTime.Now;
            qry = @"begin
                        insert into [Log] (Timestamp,Stage,Remarks,Barcode)
                        values('{0}','{1}','{2}','{3}')
                       
                        END";




            String tsStr = lr.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

            qry = String.Format(qry, tsStr ,lr.Stage,lr.Remarks,lr.Barcode);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();


            con.Close();
            con.Dispose();
        }




        #endregion


        #region TEST_DATA

        internal void InsertTestRecord(TestRecord r)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();


            String qry = String.Empty;
            qry = @"insert into [TestRecord] ( Barcode,Model, ECT,HV,IR,LC,Voltage,[Current],Power,Status)
                    values('{0}', '{1}', {2},{3},{4},{5},{6},{7},{8},'{9}')";
            qry = String.Format(qry, r.Barcode,r.Model,r.ECT,r.HV,r.IR,r.LC,r.Voltage,r.Current,r.Power,r.Status);
            SqlCommand cmd = new SqlCommand(qry, con);

            cmd.ExecuteNonQuery();
            cmd.Dispose();

            con.Close();
            con.Dispose();


        }

        #endregion

        public Queue<int> getDeviceQ()
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"SELECT ID from  deviceAssociation";


            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            Queue<int> deviceQ = new Queue<int>();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                deviceQ.Enqueue((int)dt.Rows[i]["ID"]);
            }


            con.Close();
            con.Dispose();

            return deviceQ;
        }


        public DeviceAssociation getDeviceAssociation(int id)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"select deviceAssociation.ID ,
                    deviceAssociation.Line ,
                    deviceAssociation.Station,
                    lines.header as Header,
                    lines.description as LineName , stations.description as StationName from deviceAssociation 
                    inner join lines on lines.id = deviceAssociation.line
                    left outer join stations on stations.id = deviceAssociation.station and lines.id = stations.line
                    where deviceAssociation.id = {0} ";

            qry = String.Format(qry, id);


            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            if (dt.Rows.Count == 0)
                return null;
            DeviceAssociation dA = null;
            if ((int)dt.Rows[0]["Station"] != 0)
            {
                dA = new DeviceAssociation((int)dt.Rows[0]["ID"],
                                                    (String)dt.Rows[0]["Header"],
                                                    (int)dt.Rows[0]["Line"],
                                                    (String)dt.Rows[0]["LineName"],
                                                    (int)dt.Rows[0]["Station"],
                                                    (String)dt.Rows[0]["StationName"]);
            }

            else
            {
                dA = new DeviceAssociation((int)dt.Rows[0]["ID"],
                                                    (String)dt.Rows[0]["Header"],
                                                      (int)dt.Rows[0]["Line"],
                                                    (String)dt.Rows[0]["LineName"]
                                                    );
            }


            con.Close();
            con.Dispose();

            return dA;
        }



        public DataTable getStationsInfo(String stations)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"SELECT stations.id , stations.description AS STATION_DESCRIPTION 
                    FROM stations ";
            String qry1 = String.Empty;
            if (stations != String.Empty)
            {
                qry1 = " where id in ({0})";
                qry1 = String.Format(qry1, stations);
            }

            qry = String.Format(qry, stations);
            qry += qry1;

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();

            con.Close();
            con.Dispose();

            return dt;
        }

        public DataTable getDepartmentInfo(String departments)
        {

            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = String.Empty;
            qry = @"select * from departments";
            if (departments != String.Empty)
            {
                qry += " where id in ({0})";
                qry = String.Format(qry, departments);
            }

            SqlCommand cmd = new SqlCommand(qry, con);
            SqlDataReader dr = cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(dr);
            dr.Close();
            cmd.Dispose();
            return dt;
        }

        internal void ChangePassword(User user)
        {
            SqlConnection con = new SqlConnection(conStr);
            con.Open();

            String qry = @"update [Users] set Password = '{0}'  where Name = '{1}'";
            qry = String.Format(qry, user.Password, user.Name);
            SqlCommand cmd3 = new SqlCommand(qry, con);


            int result = cmd3.ExecuteNonQuery();
            cmd3.Dispose();


            con.Close();
            con.Dispose();

            return;
        }




        ~DataAccess()
        {

        }

        public void close()
        {

        }


    }
}
