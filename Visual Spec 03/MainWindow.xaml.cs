using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.OleDb;
using System.Data;
using System.ComponentModel;
using System.IO;
using System.Data.SqlClient;
using System.Windows.Threading;

namespace Visual_Spec_03
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {
        DispatcherTimer zegarFilesCopy = new DispatcherTimer();
        BackgroundWorker getDataFromDB = new BackgroundWorker();
        BackgroundWorker filesCopyWorker = new BackgroundWorker();
        bool dbcopydone;

        DataTable dtDataGrid = new DataTable();

        itemPhotoLayout photoLayout = new itemPhotoLayout();
        itemManualLayout manualLayout = new itemManualLayout();
        itemEnergyLayoutREF energyLayoutREF = new itemEnergyLayoutREF();
        itemEnergyLayoutREF_NEW energyLayoutREF_NEW = new itemEnergyLayoutREF_NEW();
        itemEnergyLayoutWM energyLayoutWM = new itemEnergyLayoutWM();
        itemEnergyLayoutWM_NEW energyLayoutWM_NEW = new itemEnergyLayoutWM_NEW();
        itemEnergyLayoutWD_NEW energyLayoutWD_NEW = new itemEnergyLayoutWD_NEW();
        itemRatingWM ratingWM = new itemRatingWM();
        itemRatingWM_KIV ratingWM_KIV = new itemRatingWM_KIV();
        itemRatingWD ratingWD = new itemRatingWD();
        itemRatingWD_KIV ratingWD_KIV = new itemRatingWD_KIV();
        ImagePreview imagePreview = new ImagePreview();

        DateTime inspectionStart;
        TimeSpan inspectionTime;

        string readFromKey;
        string setID;
        string logFileCopy;
        public int actualPoint;

        string sqlConnString = @"Data Source=server_name;Initial Catalog=catalog_name;User ID=user_name;Password=password";

        string model;
        string lot;
        string sn;

        string BUYER_MODEL;
        string ENERGY_GRADE;
        string ENERGY_GRADE_NEW; // PR REF - 'ENERGY CONSUMPTION GRADE (New EU Label)', PR WM - 'Energy grade New'
        string ENERGY_GRADE_NEW_WD; // PR WM - 'Energy grade drying New'
        string ENERGY_CONSUMPTION;
        string ENERGY_CONSUMPTION_NEW; // PR REF - 'ENERGY CONSUMPTION (New EU Label)', PR WM - 'Energy consumption New' 
        string ENERGY_CONSUMPTION_NEW_WD; // PR WM - 'Energy consumption drying New' 
        string VOLUME_REF;
        string VOLUME_REF_NEW; // PR REF - 'Refrigerator Volume Total (New EU Energy Label)'
        string VOLUME_FREEZER;
        string NOISE;
        string NOISE_GRADE; //PR REF - 'NEW EU NOISE GRADE' zmiana na -> 'NOISE GRADE', PR WM - 'Noise grade'
        string WATER_CONS;
        string WATER_CONS_NEW; // PR WM - 'Weighted water consumption'
        string WATER_CONS_NEW_WD; // PR WM - 'Weighted water consumption drying'
        string PROGRAMME_DURATION; // PR WM - 'Programme duration'
        string PROGRAMME_DURATION_WD; // PR WM - 'Programme duration drying'
        string WEIGHT_CAPA_WASH;
        string WEIGHT_CAPA_DRY;
        string SPIN_GRADE;
        string SPIN_GRADE_NEW; // PR WM - 'Spin grade new'
        string NOISE_SPIN;
        string NOISE_SPIN_NEW; // PR WM - 'Spin noise New'
        string VOLTAGE;
        string FREQUENCY;
        string POWER_WASH;
        string POWER_DRY;
        string WIDTH_W;
        string DEEP_D;
        string HIGHT_H;


        public string[] specData;
        public string[] PART_NO_Data;
        public string[] ITEM_SPEC_Data;

        public string[] manualDescription;
        public string[] manualPartNO;
        public string[] manualSpec;

        public string[] inspectedArray;


        public MainWindow()
        {
            InitializeComponent();

            mainOkno.AddHandler(FrameworkElement.KeyDownEvent, new KeyEventHandler(keyDown), true);

            getDataFromDB.DoWork += getDataFromDB_DoWork;
            getDataFromDB.RunWorkerCompleted += getDataFromDB_RunWorkerCompleted;

            filesCopyWorker.DoWork += filesCopyWorker_DoWork;
            filesCopyWorker.RunWorkerCompleted += filesCopyWorker_RunWorkerCompleted;
           
        }

        private void workerDBcopy_DoWork(object sender, DoWorkEventArgs e)
        {
            DBcopy();
        }

        private void workerDBcopy_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (dbcopydone == true)
            {
                labelStatus.Content = "DB Update successfull";
            }
            else
            {
                labelStatus.Content = "DB update error";
            }
        }

        private void DBcopy()
        {
            if (File.Exists(@"DB_path.mdb"))
                {
                File.Copy(@"DB_path.mdb", Environment.CurrentDirectory + "\\QA_MCL_DATA.mdb", true);

                dbcopydone = true;
            }
            else
            {
                dbcopydone = false;
            }
        }

        private void zegarFilesCopy_Tick(object sender, EventArgs e)
        {
            try
            {
                filesCopyWorker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                zapiszLog("ERROR" + "\t" + "zegarFilesCopy_Tick: " + ex.Message);
            }
            
        }

        private void filesCopyWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            filesCopy("REF");
            filesCopy("WM");

        }
        private void filesCopyWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            zapiszLog("INFO" + "\t" + "File copy done");

        }
        private void getDataFromDB_DoWork(object sender, DoWorkEventArgs e)
        {
            DBdataLoad(readFromKey);
        }
        private void getDataFromDB_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dataGridPointList.ItemsSource = dtDataGrid.AsDataView();

            inspectedArray = new string[dataGridPointList.Items.Count];

            for (int i = 0; i < dataGridPointList.Items.Count; i++)
            {
                if (specData[i] == "NA")
                {
                    ((DataRowView)dataGridPointList.Items[i]).Row["Result"] = "OK";
                    inspectedArray[i] = "OK";
                }
            }

            labelPointQty.Content = dataGridPointList.Items.Count;

            labelModel.Content = model;
            labelBuyer.Content = BUYER_MODEL;
            labelLot.Content = lot;

            setID = readFromKey;
            readFromKey = null;


            try
            {               
                sn = setID.Substring(8, 5);
                labelSerial.Content = setID.Substring(8, 5);
            }
            catch
            {
                //labelStatus.Content = labelStatus.Content + ex.Message;
            }

            if (model == null)
            {
                dataGridPointList.ItemsSource = null;
                labelModel.Content = "no data";
                labelSerial.Content = "";
                labelStatus.Content = " no data for this scan ";
            }

            imageGif.Visibility = Visibility.Hidden;
            tabControl1.SelectedIndex = 0;

            zapiszLog("INFO" + "\t" + "getDataFromDB_RunWorkerComplete " + setID);
            inspectionStart = DateTime.Now;

        }

        private void DBdataLoad(string setID)//--------------------------------------------get data from db 
        {
            string sqlString = "SELECT [ID], [AREA_ENGLISH] AS AREA, [POINT_NAME_ENGLISH] AS POINT_NAME, [RESULT], [POINT_ID] FROM MCL_" + MCLsettings.Default.Division + "_CONTROL_POINTS_" + MCLsettings.Default.Area;
            
            SqlConnection connectionSQLSRV = new SqlConnection(sqlConnString);
            try
            {
                //---------------------------------------------------------------------load control Point list
                connectionSQLSRV.Open();
                SqlCommand cmd = connectionSQLSRV.CreateCommand();
                cmd.CommandText = sqlString;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                dtDataGrid = new DataTable();
                da.Fill(dtDataGrid);
                dtDataGrid.DefaultView.Sort = "ID";
                dtDataGrid = dtDataGrid.DefaultView.ToTable();
                cmd.Dispose();
                da.Dispose();

                //---------------------------------------------------------------------get model from scan setID

                SqlDataReader reader;

                switch (MCLsettings.Default.Division)
                {
                    case "REF":

                        sqlString = "SELECT [PART_NO], [WO] FROM View_REF_EAN WHERE SUBSTRING([WO], 5, 4)='" + setID.Substring(14, 4) + "' AND SUBSTRING([EANCODE], 8, 5)='" + setID.Substring(0, 5) + "'";

                        cmd.CommandText = sqlString;
                        reader = cmd.ExecuteReader();

                        while (reader.Read())
                        {
                            model = reader[0].ToString();
                            lot = reader[1].ToString();
                        }
                        cmd.Dispose();
                        reader.Close();

                        if (model != null)
                        {
                            zapiszLog("INFO" + "\t" + "get model from scan, table View_REF_EAN " + setID + " " + model + " " + lot);
                        }

                        else
                        {
                            sqlString = "SELECT [MODEL_SUFFIX], [WO_NAME] FROM REF_SET_ID WHERE SUBSTRING([MIN_SERIAL], 1, 8)= '" + setID.Substring(0, 8) + "' AND SUBSTRING([MIN_SERIAL], 14, 7)= '" + setID.Substring(13, 7) + "' AND '" + setID.Substring(8, 5) + "' >= SUBSTRING([MIN_SERIAL], 9 ,5) AND '" + setID.Substring(8, 5) + "' <= SUBSTRING([MAX_SERIAL], 9, 5)";
                            cmd.CommandText = sqlString;
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                model = reader[0].ToString();
                                lot = reader[1].ToString();
                            }
                            cmd.Dispose();
                            reader.Close();
                            zapiszLog("INFO" + "\t" + "get model from scan, table REF_SET_ID " + setID + " " + model + " "  + lot);
                        }
                        
                        //---------------------------------------------------------------------get data from PR
                        sqlString = "SELECT [Customer Model], [ENERGY CONSUMPTION GRADE], [ENERGY CONSUMPTION], [Refrigerator Volume Total (Energy Label)], [VOLUME FREEZER VALUE], [NOISE(dB)], [ENERGY CONSUMPTION GRADE (New EU Label)], [ENERGY CONSUMPTION (New EU Label)], [NOISE GRADE], [Refrigerator Volume Total (New EU Energy Label)] FROM REF_PR_Spec WHERE [Model Suffix]='" + model + "'";

                        cmd.CommandText = sqlString;
                        reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                BUYER_MODEL = reader[0].ToString();
                                ENERGY_GRADE = reader[1].ToString();
                                ENERGY_CONSUMPTION = reader[2].ToString();
                                VOLUME_REF = reader[3].ToString();
                                VOLUME_FREEZER = reader[4].ToString().Substring(0, 3);
                                NOISE = reader[5].ToString();
                                //value to new regulation label from PR
                                ENERGY_GRADE_NEW = reader[6].ToString();
                                ENERGY_CONSUMPTION_NEW = reader[7].ToString();
                                NOISE_GRADE = reader[8].ToString();
                                VOLUME_REF_NEW = reader[9].ToString();
                            }
                        }
                        else
                        {
                            zapiszLog("ERROR" + "\t" + "No PR data, model " + model);
                        }
                        cmd.Dispose();
                        reader.Close();
                       
                        break;

                    case "WM":

                        sqlString = "SELECT [Model] FROM View_WM_EAN WHERE [MID]='" + setID.Substring(0, 5) + "'";

                        cmd.CommandText = sqlString;
                        reader = cmd.ExecuteReader();

                        string[] modelQty = new string[2];

                        while (reader.Read())
                        {
                            if (reader[0].ToString().Substring(reader[0].ToString().Length - 9, 1) == "R" || reader[0].ToString().Substring(reader[0].ToString().Length - 10, 1) == "R")
                            {
                                modelQty[1] = reader[0].ToString();                            
                            }
                            else
                            {
                                modelQty[0] = reader[0].ToString();
                            }
                        }
                        cmd.Dispose();
                        reader.Close();


                        if (modelQty[1] != null)
                        {
                            string message = "Is it a model with new energy regulation? (*R*)" + Environment.NewLine + "YES:  " + modelQty[1] + Environment.NewLine + "NO:  " + modelQty[0];
                            string caption = "Choice model";
                            MessageBoxButton buttons = MessageBoxButton.YesNo;
                            MessageBoxImage icon = MessageBoxImage.Information;
                            if (MessageBox.Show(message, caption, buttons, icon) == MessageBoxResult.Yes)
                            {
                                model = modelQty[1];

                            }
                            else
                            {
                                model = modelQty[0];
                            }

                        }
                        else
                        {
                            model = modelQty[0];
                        }    
                        

                        if (model != null)
                        {
                            zapiszLog("INFO" + "\t" + "get model from scan, table View_WM_EAN " + setID + " " + model);
                        }

                        else
                        {
                            sqlString = "SELECT [MODEL_SUFFIX] FROM WM_SET_ID WHERE SUBSTRING([MIN_SERIAL], 1, 8)= '" + setID.Substring(0, 8) + "' AND SUBSTRING([MIN_SERIAL], 14, 7)= '" + setID.Substring(13, 7) + "' AND '" + setID.Substring(8, 5) + "' >= SUBSTRING([MIN_SERIAL], 9 ,5) AND '" + setID.Substring(8, 5) + "' <= SUBSTRING([MAX_SERIAL], 9, 5)";                   
                            cmd.CommandText = sqlString;
                            reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                model = reader[0].ToString();
                                //lot = reader[1].ToString();
                            }
                            cmd.Dispose();
                            reader.Close();
                            zapiszLog("INFO" + "\t" + "get model from scan, table WM_SET_ID " + setID + " " + model);
                        }

                        //---------------------------------------------------------------------get data from PR
                        sqlString = "SELECT [Customer Model], [Energy grade(EU/Iran)], [Annual energy consumption(WO)(EU)], [Annual water consumption(WO)(EU/Iran)], [Capacity_energy(Washing)]," +
                                        "[Spin grade(WO)], [Noise(Washing)], [Noise(Spin)], [Capacity (Drying)], [RATING1], [Max#Watt (Washing)], [Max#Watt (Drying)], [Product size (Width)]," +
                                        "[Product size (Depth)], [Product size (Height)], [Energy grade New], [Energy grade drying New], [Energy consumption New], [Energy consumption drying New]," +
                                        "[Capacity (Drying)], [Weighted water consumption], [Weighted water consumption drying], [Programme duration], [Programme duration drying], [Spin grade new], [Noise grade], [Spin noise New] " +
                                        "FROM WM_PR_Spec WHERE [Model#Suffix]='" + model + "'";

                        cmd.CommandText = sqlString;
                        reader = cmd.ExecuteReader();

                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                BUYER_MODEL = reader[0].ToString();
                                //energy value
                                ENERGY_GRADE = reader[1].ToString();
                                ENERGY_CONSUMPTION = reader[2].ToString();
                                WATER_CONS = reader[3].ToString();
                                WEIGHT_CAPA_WASH = reader[4].ToString();
                                SPIN_GRADE = reader[5].ToString();
                                NOISE = reader[6].ToString();
                                NOISE_SPIN = reader[7].ToString();

                                //rating value
                                WEIGHT_CAPA_DRY = reader[8].ToString();
                                VOLTAGE = reader[9].ToString();
                                FREQUENCY = reader[9].ToString();
                                POWER_WASH = reader[10].ToString();
                                POWER_DRY = reader[11].ToString();
                                WIDTH_W = reader[12].ToString();
                                DEEP_D = reader[13].ToString();
                                HIGHT_H = reader[14].ToString();

                                //new energy value
                                ENERGY_GRADE_NEW = reader[15].ToString();
                                ENERGY_GRADE_NEW_WD = reader[16].ToString();
                                ENERGY_CONSUMPTION_NEW = reader[17].ToString();
                                ENERGY_CONSUMPTION_NEW_WD = reader[18].ToString();
                                WEIGHT_CAPA_DRY = reader[19].ToString();
                                WATER_CONS_NEW = reader[20].ToString();
                                WATER_CONS_NEW_WD = reader[21].ToString();
                                PROGRAMME_DURATION = reader[22].ToString();
                                PROGRAMME_DURATION_WD = reader[23].ToString();
                                SPIN_GRADE_NEW = reader[24].ToString();
                                NOISE_GRADE = reader[25].ToString();
                                NOISE_SPIN_NEW = reader[26].ToString();

                                //MessageBox.Show(WEIGHT_CAPA_DRY);
                            }

                            WEIGHT_CAPA_WASH = WEIGHT_CAPA_WASH.Replace("kg", "");
                            WEIGHT_CAPA_DRY = WEIGHT_CAPA_DRY.Replace("kg", "");
                            VOLTAGE = VOLTAGE.Replace("/", "-");

                        }
                        else
                        {
                            zapiszLog("ERROR" + "\t" + "No PR data, model " + model);
                        }

                        cmd.Dispose();
                        reader.Close();

                        break;
                }

                //---------------------------------------------------------------------get spec from BOM
                specData = new string[dtDataGrid.Rows.Count];
                PART_NO_Data = new string[dtDataGrid.Rows.Count];
                ITEM_SPEC_Data = new string[dtDataGrid.Rows.Count];
                manualDescription = new string[10];
                manualPartNO = new string[10];
                manualSpec = new string[10];
                int j = 0;


                for (int i = 0; i < dtDataGrid.Rows.Count; i++)
                {
                    sqlString = "SELECT [MODEL_SUFFIX], [PART_NO], [ITEM_SPEC], [DESCRIPTION] ,[QTY], [POINT_INDEX], [KEY_NAME] ,[SPEC] FROM View_MCL_" + MCLsettings.Default.Division + "_SPEC WHERE [POINT_INDEX]='" + dtDataGrid.Rows[i]["POINT_ID"].ToString() + "' AND [MODEL_SUFFIX] = '" + model + "'";
                    cmd.CommandText = sqlString;
                    reader = cmd.ExecuteReader();
                    specData[i] = "NA";
                    
                    while (reader.Read())
                    {
                        if (reader[6].ToString() == "MANUAL_ASSEMBLY")
                        {
                            manualDescription[j] = reader[3].ToString();
                            manualPartNO[j] = reader[7].ToString();
                            manualSpec[j] = reader[2].ToString();
                            j++;
                            specData[i] = "MANUAL";
                        }
                        else if (reader[6].ToString() == "LABEL_ENERGY_LAYOUT" || reader[6].ToString() == "LABEL_ENERGY_LAYOUT_UK")
                        {
                            specData[i] = "ENERGY_" + MCLsettings.Default.Division;
                            PART_NO_Data[i] = reader[1].ToString();
                            ITEM_SPEC_Data[i] = reader[2].ToString();
                        }
                        // new energy label REF
                        else if (reader[6].ToString() == "LABEL_ENERGY_LAYOUT_NEW" || reader[6].ToString() == "LABEL_ENERGY_LAYOUT_UK_NEW")
                        {
                            specData[i] = "LABEL_ENERGY_LAYOUT_NEW";
                            PART_NO_Data[i] = reader[1].ToString();
                            ITEM_SPEC_Data[i] = reader[2].ToString();
                        }
                        // new energy label WM LABEL_ENERGY_LAYOUT3
                        else if (reader[6].ToString() == "LABEL_ENERGY_LAYOUT_NEW_WM" || reader[6].ToString() == "LABEL_ENERGY_LAYOUT_UK_NEW_WM") //energy new uk wm
                        {
                            specData[i] = "LABEL_ENERGY_LAYOUT_NEW_WM";
                            PART_NO_Data[i] = reader[1].ToString();
                            ITEM_SPEC_Data[i] = reader[2].ToString();
                        }
                        else if (reader[6].ToString() == "LABEL_RATING_LAYOUT")
                        {
                            specData[i] = "LABEL_RATING_LAYOUT";
                            PART_NO_Data[i] = reader[7].ToString();
                            ITEM_SPEC_Data[i] = reader[2].ToString();
                        }
                        else if (reader[6].ToString() != "MANUAL_ASSEMBLY")
                        {
                            specData[i] = reader[7].ToString();
                            PART_NO_Data[i] = reader[1].ToString();
                            ITEM_SPEC_Data[i] = reader[2].ToString();
                            //MessageBox.Show(""+specData[i]);
                        }
                    }
                    cmd.Dispose();
                    reader.Close();
                }
                connectionSQLSRV.Close();
            }
            catch (Exception ex)
            {
                if (connectionSQLSRV.State == ConnectionState.Open) { connectionSQLSRV.Close(); }
                zapiszLog("ERROR" + "\t" +  "DBdataLoad: " + ex.Message);
            }

        }

        private void loadPointList()
        {
            string sqlString = "SELECT [ID], [AREA_ENGLISH] AS AREA, [POINT_NAME_ENGLISH] AS POINT_NAME, [RESULT] FROM MCL_REF_CONTROL_POINTS";
            SqlConnection connectionSQLSRV = new SqlConnection(sqlConnString);
            try
            {

                connectionSQLSRV.Open();

                SqlCommand cmd = connectionSQLSRV.CreateCommand();
                cmd.CommandText = sqlString;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                dtDataGrid = new DataTable();
                da.Fill(dtDataGrid);

                connectionSQLSRV.Close();



            }
            catch (Exception ex)
            {
                if (connectionSQLSRV.State == ConnectionState.Open) { connectionSQLSRV.Close(); }
                zapiszLog("ERROR" + "\t" + "loadPointList: " + ex.Message);
                //MessageBox.Show("db error");
            }
        } 

        private void clearVariables()
        {
            //readFromKey = null;
            actualPoint = 0;

            setID = null;
            model = null;
            lot = null;
            sn = null;

            BUYER_MODEL = null;
            ENERGY_GRADE = null;
            ENERGY_GRADE_NEW = null;
            ENERGY_GRADE_NEW_WD = null;
            ENERGY_CONSUMPTION = null;
            ENERGY_CONSUMPTION_NEW = null;
            ENERGY_CONSUMPTION_NEW_WD = null;
            VOLUME_REF = null;
            VOLUME_REF_NEW = null;
            VOLUME_FREEZER = null;
            NOISE = null;
            NOISE_GRADE = null;
            WATER_CONS = null;
            WATER_CONS_NEW = null;
            WATER_CONS_NEW_WD = null;
            PROGRAMME_DURATION = null;
            PROGRAMME_DURATION_WD = null;
            WEIGHT_CAPA_WASH = null;
            WEIGHT_CAPA_DRY = null;
            SPIN_GRADE = null;
            SPIN_GRADE_NEW = null;
            NOISE_SPIN = null;
            NOISE_SPIN_NEW = null;
            VOLTAGE = null;
            FREQUENCY = null;
            POWER_WASH = null;
            POWER_DRY = null;
            WIDTH_W = null;
            DEEP_D = null;
            HIGHT_H = null;

            labelModel.Content = "-";
            labelBuyer.Content = "-";
            labelLot.Content = "-";
            labelSerial.Content = "-";

        }
        public void loadLayout(int point, bool bigSizePreview) //choose layout
        {
            switch (specData[point])
            {
                case "MANUAL":

                    manualLayout.listViewManual.Items.Clear();
                    manualLayout.manualPartNO = new string[10];
                    viewboxItem.Child = manualLayout;
                    for (int i = 0; i < 10; i++)
                    {
                        if (manualDescription[i] != null)
                        {
                            manualLayout.listViewManual.Items.Add(new manualItem { ID = i + 1, Description = manualDescription[i], PartNO = manualPartNO[i], Spec = manualSpec[i] });
                            manualLayout.manualPartNO[i] = manualPartNO[i];
                        }
                    }

                    break;

                case "LABEL_RATING_LAYOUT":

                    int userControlType = 0;

                    if (bigSizePreview == false) // toogle between main window and full screen mode
                    {

                        if (IF_WD(model) == false && model.Substring(model.Length - 3, 3) != "KIV") { viewboxItem.Child = ratingWM; userControlType = 1; }  // no WD and no KIV
                        if (IF_WD(model) == false && model.Substring(model.Length - 3, 3) == "KIV") { viewboxItem.Child = ratingWM_KIV; userControlType = 2; } // no WD and KIV
                        if (IF_WD(model) == true && model.Substring(model.Length - 3, 3) != "KIV") { viewboxItem.Child = ratingWD; userControlType = 3; } // WD and no KIV
                        if (IF_WD(model) == true && model.Substring(model.Length - 3, 3) == "KIV") { viewboxItem.Child = ratingWD_KIV; userControlType = 4; } // WD and KIV
                    }
                    else
                    {
                        viewboxItem.Child = null;

                        if (IF_WD(model) == false && model.Substring(model.Length - 3, 3) != "KIV") { imagePreview.viewBoxImagePreview.Child = ratingWM; }  //no WD and no KIV
                        if (IF_WD(model) == false && model.Substring(model.Length - 3, 3) == "KIV") { imagePreview.viewBoxImagePreview.Child = ratingWM_KIV; } //no WD and KIV
                        if (IF_WD(model) == true && model.Substring(model.Length - 3, 3) != "KIV") { imagePreview.viewBoxImagePreview.Child = ratingWD; } // WD and no KIV
                        if (IF_WD(model) == true && model.Substring(model.Length - 3, 3) == "KIV") { imagePreview.viewBoxImagePreview.Child = ratingWD_KIV; } // WD and KIV

                        imagePreview.Show();
                    }
                   

                    if (userControlType == 1) //no WD i no KIV
                    {
                        ratingWM.loadPicture(PART_NO_Data[point], "imageLABEL_RATING_LAYOUT", ".png", MCLsettings.Default.Division);

                        //ratingWM.EnergyLabelsLayoutVisibility(true);

                        ratingWM.labelBuyerModel.Content = BUYER_MODEL;
                        ratingWM.labelModel.Content = model;
                        try { ratingWM.labelVoltage.Content = VOLTAGE.Substring(0, 7); } catch { };
                        try { ratingWM.labelFrequency.Content = VOLTAGE.Substring(9, 2); } catch { };
                        ratingWM.labelPowerWM.Content = POWER_WASH;

                        //try { if (WEIGHT_CAPA_WASH.Substring(2, 1) == "0") { ratingWM.labelLoadCapa.Content = WEIGHT_CAPA_WASH.Substring(0, 1); } else { ratingWM.labelLoadCapa.Content = WEIGHT_CAPA_WASH; } } catch { };
                        ratingWM.labelLoadCapa.Content = WEIGHT_CAPA_WASH;

                        if (model.Substring(model.Length - 3, 3) == "WFS" || model.Substring(model.Length - 3, 3) == "EFS" || model.Substring(model.Length - 3, 3) == "WES")
                        {
                            ratingWM.labelLoadCapa.SetValue(Grid.ColumnProperty, 12);
                            ratingWM.labelLoadCapa.SetValue(Grid.ColumnSpanProperty, 2);
                        }
                        else
                        {
                            ratingWM.labelLoadCapa.SetValue(Grid.ColumnProperty, 7);
                            ratingWM.labelLoadCapa.SetValue(Grid.ColumnSpanProperty, 2);
                        }
                        
                        ratingWM.labelW.Content = WIDTH_W;
                        ratingWM.labelD.Content = DEEP_D;
                        ratingWM.labelH.Content = HIGHT_H;

                    }

                    else if (userControlType == 2) //no WD i KIV
                    {
                        ratingWM_KIV.loadPicture(PART_NO_Data[point], "imageLABEL_RATING_LAYOUT", ".png", MCLsettings.Default.Division);

                        //ratingWM.EnergyLabelsLayoutVisibility(true);

                        ratingWM_KIV.labelBuyerModel.Content = BUYER_MODEL;
                        ratingWM_KIV.labelModel.Content = model;
                        try { ratingWM_KIV.labelVoltage.Content = VOLTAGE.Substring(0, 7); } catch { };
                        try { ratingWM_KIV.labelFrequency.Content = VOLTAGE.Substring(9, 2); } catch { };
                        if (IF_SLIM(model) == true) { ratingWM_KIV.labelCurrent.Content = "8"; } else { ratingWM_KIV.labelCurrent.Content = "10"; }
                        ratingWM_KIV.labelPowerWM.Content = POWER_WASH;
                        //try { if (WEIGHT_CAPA_WASH.Substring(2, 1) == "0") { ratingWM_KIV.labelLoadCapa.Content = WEIGHT_CAPA_WASH.Substring(0, 1); } else { ratingWM_KIV.labelLoadCapa.Content = WEIGHT_CAPA_WASH; } } catch { };
                        ratingWM_KIV.labelLoadCapa.Content = WEIGHT_CAPA_WASH;
                        ratingWM_KIV.labelW.Content = WIDTH_W;
                        ratingWM_KIV.labelD.Content = DEEP_D;
                        ratingWM_KIV.labelH.Content = HIGHT_H;

                    }

                    else if (userControlType == 3) //WD i no KIV
                    {
                        ratingWD.loadPicture(PART_NO_Data[point], "imageLABEL_RATING_LAYOUT", ".png", MCLsettings.Default.Division);

                        ratingWD.labelBuyerModel.Content = BUYER_MODEL;
                        ratingWD.labelModel.Content = model;
                        try { ratingWD.labelVoltage.Content = VOLTAGE.Substring(0, 7); } catch { };
                        try { ratingWD.labelFrequency.Content = VOLTAGE.Substring(9, 2); } catch { };
                        //try { if (WEIGHT_CAPA_WASH.Substring(2, 1) == "0") { ratingWD.labelLoadCapa.Content = WEIGHT_CAPA_WASH.Substring(0, 1); } else { ratingWD.labelLoadCapa.Content = WEIGHT_CAPA_WASH; } } catch { };
                        ratingWD.labelLoadCapa.Content = WEIGHT_CAPA_WASH;
                        ratingWD.labelLoadCapaDry.Content = WEIGHT_CAPA_DRY;

                        if (model.Substring(model.Length - 3, 3) == "WIS" || model.Substring(model.Length - 3, 3) == "WPT")
                        {
                            ratingWD.labelLoadCapa.SetValue(Grid.ColumnProperty, 11);
                            ratingWD.labelLoadCapa.SetValue(Grid.ColumnSpanProperty, 1);

                            ratingWD.labelLoadCapaDry.SetValue(Grid.ColumnProperty, 11);
                            ratingWD.labelLoadCapaDry.SetValue(Grid.ColumnSpanProperty, 1);
                        }
                        else
                        {
                            ratingWD.labelLoadCapa.SetValue(Grid.ColumnProperty, 9);
                            ratingWD.labelLoadCapa.SetValue(Grid.ColumnSpanProperty, 2);

                            ratingWD.labelLoadCapaDry.SetValue(Grid.ColumnProperty, 9);
                            ratingWD.labelLoadCapaDry.SetValue(Grid.ColumnSpanProperty, 2);
                        }

                        ratingWD.labelW.Content = WIDTH_W;
                        ratingWD.labelD.Content = DEEP_D;
                        ratingWD.labelH.Content = HIGHT_H;
                    }

                    else if (userControlType == 4) //WD i KIV
                    {
                        ratingWD_KIV.loadPicture(PART_NO_Data[point], "imageLABEL_RATING_LAYOUT", ".png", MCLsettings.Default.Division);

                        ratingWD_KIV.labelBuyerModel.Content = BUYER_MODEL;
                        ratingWD_KIV.labelModel.Content = model;
                        try { ratingWD_KIV.labelVoltage.Content = VOLTAGE.Substring(0, 7); } catch { };
                        try { ratingWD_KIV.labelFrequency.Content = VOLTAGE.Substring(9, 2); } catch { };

                        ratingWD_KIV.labelCurrent.Content = "10";

                        //try { if (WEIGHT_CAPA_WASH.Substring(2, 1) == "0") { ratingWD_KIV.labelLoadCapa.Content = WEIGHT_CAPA_WASH.Substring(0, 1); } else { ratingWD_KIV.labelLoadCapa.Content = WEIGHT_CAPA_WASH; } } catch { };

                        ratingWD_KIV.labelLoadCapa.Content = WEIGHT_CAPA_WASH;
                        ratingWD_KIV.labelLoadCapaDry.Content = WEIGHT_CAPA_DRY;
                        ratingWD_KIV.labelW.Content = WIDTH_W;
                        ratingWD_KIV.labelD.Content = DEEP_D;
                        ratingWD_KIV.labelH.Content = HIGHT_H;
                    }

                    break;

                case "ENERGY_REF":

                    if (bigSizePreview == false) // toogle between main window and full screen mode
                    {
                        viewboxItem.Child = energyLayoutREF;
                    }
                    else
                    {
                        viewboxItem.Child = null;
                        imagePreview.viewBoxImagePreview.Child = energyLayoutREF;
                        imagePreview.Show();
                    }

                    if (model.Substring(model.Length - 3, 3) == "EUR" || model.Substring(model.Length - 3, 3) == "GSF" || model.Substring(model.Length - 3, 3) == "LGU" || model.Substring(model.Length - 3, 3) == "ESW" || model.Substring(model.Length - 3, 3) == "EUZ" || model.Substring(model.Length - 3, 3) == "FRA" || model.Substring(model.Length - 3, 3) == "EEP" || model.Substring(model.Length - 3, 3) == "EFS" || model.Substring(model.Length - 3, 3) == "EDG")
                    {
                        energyLayoutREF.loadPicture(PART_NO_Data[point], "imageLABEL_ENERGY_LAYOUT", ".png", MCLsettings.Default.Division);

                        energyLayoutREF.EnergyLabelsLayoutVisibility(true);

                        energyLayoutREF.labelBuyerModel.Content = BUYER_MODEL;
                        energyLayoutREF.labelEnergyGrade.Content = ENERGY_GRADE;
                        energyLayoutREF.labelEnergyCons.Content = ENERGY_CONSUMPTION;
                        energyLayoutREF.labelKWh.Content = "kWh/annum";
                        energyLayoutREF.labelRefVolume.Content = VOLUME_REF + " L";
                        energyLayoutREF.labelFreezerVolume.Content = VOLUME_FREEZER + " L";
                        energyLayoutREF.labelNoise.Content = NOISE + " dB";
                    }
                    else if (model.Substring(model.Length - 3, 3) == "UKR")
                    {
                        energyLayoutREF.loadPicture(PART_NO_Data[point], "imageLABEL_ENERGY_LAYOUT", ".png", MCLsettings.Default.Division);

                        energyLayoutREF.EnergyLabelsLayoutVisibility(true);

                        energyLayoutREF.labelBuyerModel.Content = BUYER_MODEL;
                        energyLayoutREF.labelEnergyGrade.Content = ENERGY_GRADE;
                        energyLayoutREF.labelEnergyCons.Content = ENERGY_CONSUMPTION;
                        energyLayoutREF.labelKWh.Content = "кВт·г/рік";
                        energyLayoutREF.labelRefVolume.Content = VOLUME_REF + " л";
                        energyLayoutREF.labelFreezerVolume.Content = VOLUME_FREEZER + " л";
                        energyLayoutREF.labelNoise.Content = NOISE + " дЪ";
                    }
                    else
                    {
                        energyLayoutREF.loadPicture(PART_NO_Data[point], "imageLABEL_ENERGY_LAYOUT", ".png", MCLsettings.Default.Division);
                        energyLayoutREF.EnergyLabelsLayoutVisibility(false);
                    }

                    break;
         //LABEL_ENERGY_LAYOUT_NEW
                case "LABEL_ENERGY_LAYOUT_NEW":

                    if (bigSizePreview == false) // toogle between main window and full screen mode
                    {
                        viewboxItem.Child = energyLayoutREF_NEW;
                    }
                    else
                    {
                        viewboxItem.Child = null;
                        imagePreview.viewBoxImagePreview.Child = energyLayoutREF_NEW;
                        imagePreview.Show();
                    }

                    if (model.Substring(model.Length - 3, 3) == "EUR" || model.Substring(model.Length - 3, 3) == "GSF" || model.Substring(model.Length - 3, 3) == "LGU" || model.Substring(model.Length - 3, 3) == "ESW" || model.Substring(model.Length - 3, 3) == "EUZ" || model.Substring(model.Length - 3, 3) == "FRA" || model.Substring(model.Length - 3, 3) == "EEP" || model.Substring(model.Length - 3, 3) == "EFS" || model.Substring(model.Length - 3, 3) == "EDG")
                    {
                        energyLayoutREF_NEW.loadPicture(PART_NO_Data[point], "imageLABEL_ENERGY_LAYOUT", ".png", MCLsettings.Default.Division);

                        energyLayoutREF_NEW.EnergyLabelsLayoutVisibility(true);

                        energyLayoutREF_NEW.labelBuyerModel.Content = BUYER_MODEL;
                        energyLayoutREF_NEW.labelEnergyGrade.Content = ENERGY_GRADE_NEW;
                        energyLayoutREF_NEW.labelEnergyCons.Content = ENERGY_CONSUMPTION_NEW;
                        energyLayoutREF_NEW.labelKWh.Content = "kWh/annum";

                        //energyLayoutREF_NEW.labelRefVolume.Content = VOLUME_REF + " L";
                        //energyLayoutREF_NEW.labelFreezerVolume.Content = VOLUME_FREEZER + " L";

                        TextBlock tblabelRefVolume = new TextBlock();
                        tblabelRefVolume.Inlines.Add(new Run(VOLUME_REF_NEW) { FontWeight = FontWeights.SemiBold });
                        tblabelRefVolume.Inlines.Add(new Run(" L") { FontWeight = FontWeights.Normal });
                        energyLayoutREF_NEW.labelRefVolume.Content = tblabelRefVolume;

                        TextBlock tblabelFreezerVolume = new TextBlock();
                        tblabelFreezerVolume.Inlines.Add(new Run(VOLUME_FREEZER) { FontWeight = FontWeights.SemiBold });
                        tblabelFreezerVolume.Inlines.Add(new Run(" L") { FontWeight = FontWeights.Normal });
                        energyLayoutREF_NEW.labelFreezerVolume.Content = tblabelFreezerVolume;

                        TextBlock tbNoise = new TextBlock();
                        tbNoise.Inlines.Add(new Run(NOISE) { FontWeight = FontWeights.SemiBold});
                        tbNoise.Inlines.Add(new Run("dB") { FontWeight = FontWeights.Normal });
                        energyLayoutREF_NEW.labelNoise.Content = tbNoise;


                        if (NOISE_GRADE == "A")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("A") { FontWeight = FontWeights.SemiBold, FontSize = 24 });
                            tb.Inlines.Add(new Run("BCD") { FontWeight = FontWeights.Normal, FontSize = 16 });
                            energyLayoutREF_NEW.labelNoiseGrade.Content = tb;
                        }
                        else if (NOISE_GRADE == "B")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("A") { FontWeight = FontWeights.Normal, FontSize = 16 });
                            tb.Inlines.Add(new Run("B") { FontWeight = FontWeights.SemiBold, FontSize = 24 });
                            tb.Inlines.Add(new Run("CD") { FontWeight = FontWeights.Normal, FontSize = 16 });
                            energyLayoutREF_NEW.labelNoiseGrade.Content = tb;
                        }
                        else if (NOISE_GRADE == "C")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("AB") { FontWeight = FontWeights.Normal, FontSize = 16 });
                            tb.Inlines.Add(new Run("C") { FontWeight = FontWeights.SemiBold, FontSize = 24 });
                            tb.Inlines.Add(new Run("D") { FontWeight = FontWeights.Normal, FontSize = 16 });
                            energyLayoutREF_NEW.labelNoiseGrade.Content = tb;
                        }
                        else if (NOISE_GRADE == "D")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("ABC") { FontWeight = FontWeights.Normal, FontSize = 16 });
                            tb.Inlines.Add(new Run("D") { FontWeight = FontWeights.SemiBold, FontSize = 24 });
                            energyLayoutREF_NEW.labelNoiseGrade.Content = tb;
                        }

                    }
                    else
                    {
                        energyLayoutREF_NEW.loadPicture(PART_NO_Data[point], "imageLABEL_ENERGY_LAYOUT", ".png", MCLsettings.Default.Division);
                        energyLayoutREF_NEW.EnergyLabelsLayoutVisibility(false);
                    }

                    break;

                case "ENERGY_WM":

                    if (bigSizePreview == false) // toogle between main window and full screen mode
                    {
                        viewboxItem.Child = energyLayoutWM;
                    }
                    else
                    {
                        viewboxItem.Child = null;
                        imagePreview.viewBoxImagePreview.Child = energyLayoutWM;
                        imagePreview.Show();
                    }

                    if (model.Substring(model.Length - 3, 3) != "KIV" && model.Substring(model.Length - 3, 3) != "LTK" && IF_WD(model) == false)  // warunek że to nie jest WD i KIV i LTK
                    {
                        energyLayoutWM.loadPicture(PART_NO_Data[point], "imageLABEL_ENERGY_LAYOUT", ".png", MCLsettings.Default.Division);

                        energyLayoutWM.EnergyLabelsLayoutVisibility(true);

                        energyLayoutWM.labelBuyerModel.Content = BUYER_MODEL;
                        energyLayoutWM.labelEnergyGrade.Content = ENERGY_GRADE;
                        energyLayoutWM.labelEnergyCons.Content = ENERGY_CONSUMPTION;
                        energyLayoutWM.labelKWh.Content = "kWh/annum";
                        energyLayoutWM.labelWaterCons.Content = WATER_CONS;
                        energyLayoutWM.labelLannum.Content = "L/annum";
                        energyLayoutWM.labelWeightCapa.Content = WEIGHT_CAPA_WASH;
                        try { if (WEIGHT_CAPA_WASH.Length == 1) { energyLayoutWM.labelWeightCapa.Content = WEIGHT_CAPA_WASH + ",0"; ; } else { energyLayoutWM.labelWeightCapa.Content = WEIGHT_CAPA_WASH; } } catch { };
                        energyLayoutWM.labelKg.Content = "kg";
                        energyLayoutWM.labelNoiseWash.Content = NOISE + " dB";
                        energyLayoutWM.labelNoiseSpin.Content = NOISE_SPIN + " dB";

                        //text format spin grade
                        if (SPIN_GRADE=="A")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("A") {FontWeight = FontWeights.SemiBold, FontSize = 21 });
                            tb.Inlines.Add(new Run("BCDEFG") { FontWeight = FontWeights.Normal, FontSize = 16 });
                            energyLayoutWM.labelSpinGrade.Content = tb;
                        }
                        else if (SPIN_GRADE == "B")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("A") { FontWeight = FontWeights.Normal, FontSize = 16 });
                            tb.Inlines.Add(new Run("B") { FontWeight = FontWeights.SemiBold, FontSize = 21 });
                            tb.Inlines.Add(new Run("CDEFG") { FontWeight = FontWeights.Normal, FontSize = 16 });
                            energyLayoutWM.labelSpinGrade.Content = tb;
                        }


                    }
                    else if (model.Substring(model.Length - 3, 3) == "KIV" && IF_WD(model) == false)  //UKR i nie WD
                    {
                        energyLayoutWM.loadPicture(PART_NO_Data[point], "imageLABEL_ENERGY_LAYOUT", ".png", MCLsettings.Default.Division);

                        energyLayoutWM.EnergyLabelsLayoutVisibility(true);

                        energyLayoutWM.labelBuyerModel.Content = BUYER_MODEL;
                        energyLayoutWM.labelEnergyGrade.Content = ENERGY_GRADE;
                        energyLayoutWM.labelEnergyCons.Content = ENERGY_CONSUMPTION;
                        energyLayoutWM.labelKWh.Content = "кВт·г/рік";
                        energyLayoutWM.labelWaterCons.Content = WATER_CONS;
                        energyLayoutWM.labelLannum.Content = "л/рік";
                        energyLayoutWM.labelWeightCapa.Content = WEIGHT_CAPA_WASH;
                        energyLayoutWM.labelKg.Content = "кг";
                        energyLayoutWM.labelNoiseWash.Content = NOISE + " дб";
                        energyLayoutWM.labelNoiseSpin.Content = NOISE_SPIN + " дб";

                        //text format spin grade
                        if (SPIN_GRADE == "A")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("A") { FontWeight = FontWeights.SemiBold, FontSize = 21 });
                            tb.Inlines.Add(new Run("BCDEFG") { FontWeight = FontWeights.Normal, FontSize = 16 });
                            energyLayoutWM.labelSpinGrade.Content = tb;
                        }
                        else if (SPIN_GRADE == "B")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("A") { FontWeight = FontWeights.Normal, FontSize = 16 });
                            tb.Inlines.Add(new Run("B") { FontWeight = FontWeights.SemiBold, FontSize = 21 });
                            tb.Inlines.Add(new Run("CDEFG") { FontWeight = FontWeights.Normal, FontSize = 16 });
                            energyLayoutWM.labelSpinGrade.Content = tb;
                        }

                    }
                    else if (model.Substring(model.Length - 3, 3) == "LTK" && IF_WD(model) == false)  //Turkey and not WD
                    {
                        energyLayoutWM.loadPicture(PART_NO_Data[point], "imageLABEL_ENERGY_LAYOUT", ".png", MCLsettings.Default.Division);

                        energyLayoutWM.EnergyLabelsLayoutVisibility(true);

                        energyLayoutWM.labelBuyerModel.Content = BUYER_MODEL;
                        energyLayoutWM.labelEnergyGrade.Content = ENERGY_GRADE;
                        energyLayoutWM.labelEnergyCons.Content = ENERGY_CONSUMPTION;
                        energyLayoutWM.labelKWh.Content = "kWh/yıl";
                        energyLayoutWM.labelWaterCons.Content = WATER_CONS;
                        energyLayoutWM.labelLannum.Content = "L/yıl";
                        energyLayoutWM.labelWeightCapa.Content = WEIGHT_CAPA_WASH;
                        try { if (WEIGHT_CAPA_WASH.Length == 1) { energyLayoutWM.labelWeightCapa.Content = WEIGHT_CAPA_WASH + ",0"; ; } else { energyLayoutWM.labelWeightCapa.Content = WEIGHT_CAPA_WASH; } } catch { };
                        energyLayoutWM.labelKg.Content = "kg";
                        energyLayoutWM.labelNoiseWash.Content = NOISE + " dB";
                        energyLayoutWM.labelNoiseSpin.Content = NOISE_SPIN + " dB";

                        //text format spin grade
                        if (SPIN_GRADE == "A")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("A") { FontWeight = FontWeights.SemiBold, FontSize = 21 });
                            tb.Inlines.Add(new Run("BCDEFG") { FontWeight = FontWeights.Normal, FontSize = 16 });
                            energyLayoutWM.labelSpinGrade.Content = tb;
                        }
                        else if (SPIN_GRADE == "B")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("A") { FontWeight = FontWeights.Normal, FontSize = 16 });
                            tb.Inlines.Add(new Run("B") { FontWeight = FontWeights.SemiBold, FontSize = 21 });
                            tb.Inlines.Add(new Run("CDEFG") { FontWeight = FontWeights.Normal, FontSize = 16 });
                            energyLayoutWM.labelSpinGrade.Content = tb;
                        }

                    }
                    else if (IF_WD(model) == true)
                    {
                        energyLayoutWM.loadPicture(PART_NO_Data[point], "imageLABEL_ENERGY_LAYOUT", ".jpg", MCLsettings.Default.Division);
                        energyLayoutWM.EnergyLabelsLayoutVisibility(false);
                    }

                    break;

                //LABEL_ENERGY_LAYOUT_NEW_WM
                case "LABEL_ENERGY_LAYOUT_NEW_WM":

                    if (IF_WD(model) == false) // layout if WM
                    {
                        if (bigSizePreview == false) // toogle between main window and full screen mode
                        {
                            viewboxItem.Child = energyLayoutWM_NEW;
                        }
                        else
                        {
                            viewboxItem.Child = null;
                            imagePreview.viewBoxImagePreview.Child = energyLayoutWM_NEW;
                            imagePreview.Show();
                        }

                            energyLayoutWM_NEW.loadPicture(PART_NO_Data[point], "imageLABEL_ENERGY_LAYOUT", ".png", MCLsettings.Default.Division);

                            energyLayoutWM_NEW.EnergyLabelsLayoutVisibility(true);

                            energyLayoutWM_NEW.labelBuyerModel.Content = BUYER_MODEL;
                            energyLayoutWM_NEW.labelEnergyGrade.Content = ENERGY_GRADE_NEW;
                            energyLayoutWM_NEW.labelEnergyCons.Content = ENERGY_CONSUMPTION_NEW;
                            try { if (WEIGHT_CAPA_WASH.Length == 1) { energyLayoutWM_NEW.labelWeightCapa.Content = WEIGHT_CAPA_WASH + ",0"; ; } else { energyLayoutWM_NEW.labelWeightCapa.Content = WEIGHT_CAPA_WASH; } } catch { };
                            energyLayoutWM_NEW.labelDuration.Content = PROGRAMME_DURATION;
                            energyLayoutWM_NEW.labelWaterCons.Content = WATER_CONS_NEW;
                            //energyLayoutWM_NEW.labelSpinGrade.Content = SPIN_GRADE_NEW;
                            energyLayoutWM_NEW.labelNoise.Content = NOISE_SPIN_NEW;
                            //energyLayoutWM_NEW.labelNoiseGrade.Content = NOISE_GRADE;

                            //text format spin grade
                            if (SPIN_GRADE_NEW == "A")
                            {
                                TextBlock tb = new TextBlock();
                                tb.Inlines.Add(new Run("A") { FontWeight = FontWeights.Bold, FontSize = 28 });
                                tb.Inlines.Add(new Run("BCDEFG") { FontWeight = FontWeights.Normal, FontSize = 18 });
                                energyLayoutWM_NEW.labelSpinGrade.Content = tb;
                            }
                            else if (SPIN_GRADE_NEW == "B")
                            {
                                TextBlock tb = new TextBlock();
                                tb.Inlines.Add(new Run("A") { FontWeight = FontWeights.Normal, FontSize = 18 });
                                tb.Inlines.Add(new Run("B") { FontWeight = FontWeights.Bold, FontSize = 28 });
                                tb.Inlines.Add(new Run("CDEFG") { FontWeight = FontWeights.Normal, FontSize = 18 });
                                energyLayoutWM_NEW.labelSpinGrade.Content = tb;
                            }
                            else if (SPIN_GRADE_NEW == "C")
                            {
                                TextBlock tb = new TextBlock();
                                tb.Inlines.Add(new Run("AB") { FontWeight = FontWeights.Normal, FontSize = 18 });
                                tb.Inlines.Add(new Run("C") { FontWeight = FontWeights.Bold, FontSize = 28 });
                                tb.Inlines.Add(new Run("DEFG") { FontWeight = FontWeights.Normal, FontSize = 18 });
                                energyLayoutWM_NEW.labelSpinGrade.Content = tb;
                            }
                            //text format noise grade
                            if (NOISE_GRADE == "A")
                            {
                                TextBlock tb = new TextBlock();
                                tb.Inlines.Add(new Run("A") { FontWeight = FontWeights.Bold, FontSize = 28 });
                                tb.Inlines.Add(new Run("BCD") { FontWeight = FontWeights.Normal, FontSize = 18 });
                                energyLayoutWM_NEW.labelNoiseGrade.Content = tb;
                            }
                            else if (NOISE_GRADE == "B")
                            {
                                TextBlock tb = new TextBlock();
                                tb.Inlines.Add(new Run("A") { FontWeight = FontWeights.Normal, FontSize = 18 });
                                tb.Inlines.Add(new Run("B") { FontWeight = FontWeights.Bold, FontSize = 28 });
                                tb.Inlines.Add(new Run("CD") { FontWeight = FontWeights.Normal, FontSize = 18 });
                                energyLayoutWM_NEW.labelNoiseGrade.Content = tb;
                            }
                            else if (NOISE_GRADE == "C")
                            {
                                TextBlock tb = new TextBlock();
                                tb.Inlines.Add(new Run("AB") { FontWeight = FontWeights.Normal, FontSize = 18 });
                                tb.Inlines.Add(new Run("C") { FontWeight = FontWeights.Bold, FontSize = 28 });
                                tb.Inlines.Add(new Run("D") { FontWeight = FontWeights.Normal, FontSize = 18 });
                                energyLayoutWM_NEW.labelNoiseGrade.Content = tb;
                            }
                            else if (NOISE_GRADE == "D")
                            {
                                TextBlock tb = new TextBlock();
                                tb.Inlines.Add(new Run("ABCD") { FontWeight = FontWeights.Normal, FontSize = 18 });
                                tb.Inlines.Add(new Run("D") { FontWeight = FontWeights.Bold, FontSize = 28 });
                                energyLayoutWM_NEW.labelNoiseGrade.Content = tb;
                            }
                     
                    }
                    else // layout if WD
                    {
                        if (bigSizePreview == false) // toogle between main window and full screen mode
                        {
                            viewboxItem.Child = energyLayoutWD_NEW;
                        }
                        else
                        {
                            viewboxItem.Child = null;
                            imagePreview.viewBoxImagePreview.Child = energyLayoutWD_NEW;
                            imagePreview.Show();
                        }

                        energyLayoutWD_NEW.loadPicture(PART_NO_Data[point], "imageLABEL_ENERGY_LAYOUT", ".png", MCLsettings.Default.Division);

                        energyLayoutWD_NEW.EnergyLabelsLayoutVisibility(true);

                        energyLayoutWD_NEW.labelBuyerModel.Content = BUYER_MODEL;
                        energyLayoutWD_NEW.labelEnergyGrade.Content = ENERGY_GRADE_NEW;
                        energyLayoutWD_NEW.labelEnergyGradeWD.Content = ENERGY_GRADE_NEW_WD;
                        energyLayoutWD_NEW.labelEnergyCons.Content = ENERGY_CONSUMPTION_NEW;
                        energyLayoutWD_NEW.labelEnergyConsDry.Content = ENERGY_CONSUMPTION_NEW_WD;
                        try { if (WEIGHT_CAPA_WASH.Length == 1) { energyLayoutWD_NEW.labelWeightCapa.Content = WEIGHT_CAPA_WASH + ",0"; ; } else { energyLayoutWD_NEW.labelWeightCapa.Content = WEIGHT_CAPA_WASH; } } catch { };
                        try { if (WEIGHT_CAPA_DRY.Length == 1) { energyLayoutWD_NEW.labelWeightCapaDry.Content = WEIGHT_CAPA_DRY + ",0"; ; } else { energyLayoutWD_NEW.labelWeightCapaDry.Content = WEIGHT_CAPA_DRY; } } catch { };
                        energyLayoutWD_NEW.labelWaterCons.Content = WATER_CONS_NEW;
                        energyLayoutWD_NEW.labelWaterConsDry.Content = WATER_CONS_NEW_WD;
                        energyLayoutWD_NEW.labelDuration.Content = PROGRAMME_DURATION;
                        energyLayoutWD_NEW.labelDurationDry.Content = PROGRAMME_DURATION_WD;
                        //energyLayoutWM_NEW.labelSpinGrade.Content = SPIN_GRADE_NEW;
                        energyLayoutWD_NEW.labelNoise.Content = NOISE_SPIN_NEW;
                        //energyLayoutWD_NEW.labelNoiseGrade.Content = NOISE_GRADE;

                        //text format spin grade
                        if (SPIN_GRADE_NEW == "A")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("A") { FontWeight = FontWeights.Bold, FontSize = 28 });
                            tb.Inlines.Add(new Run("BCDEFG") { FontWeight = FontWeights.Normal, FontSize = 18 });
                            energyLayoutWD_NEW.labelSpinGrade.Content = tb;
                        }
                        else if (SPIN_GRADE_NEW == "B")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("A") { FontWeight = FontWeights.Normal, FontSize = 18 });
                            tb.Inlines.Add(new Run("B") { FontWeight = FontWeights.Bold, FontSize = 28 });
                            tb.Inlines.Add(new Run("CDEFG") { FontWeight = FontWeights.Normal, FontSize = 18 });
                            energyLayoutWD_NEW.labelSpinGrade.Content = tb;
                        }
                        else if (SPIN_GRADE_NEW == "C")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("AB") { FontWeight = FontWeights.Normal, FontSize = 18 });
                            tb.Inlines.Add(new Run("C") { FontWeight = FontWeights.Bold, FontSize = 28 });
                            tb.Inlines.Add(new Run("DEFG") { FontWeight = FontWeights.Normal, FontSize = 18 });
                            energyLayoutWD_NEW.labelSpinGrade.Content = tb;
                        }
                        //text format noise grade
                        if (NOISE_GRADE == "A")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("A") { FontWeight = FontWeights.Bold, FontSize = 28 });
                            tb.Inlines.Add(new Run("BCD") { FontWeight = FontWeights.Normal, FontSize = 18 });
                            energyLayoutWD_NEW.labelNoiseGrade.Content = tb;
                        }
                        else if (NOISE_GRADE == "B")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("A") { FontWeight = FontWeights.Normal, FontSize = 18 });
                            tb.Inlines.Add(new Run("B") { FontWeight = FontWeights.Bold, FontSize = 28 });
                            tb.Inlines.Add(new Run("CD") { FontWeight = FontWeights.Normal, FontSize = 18 });
                            energyLayoutWD_NEW.labelNoiseGrade.Content = tb;
                        }
                        else if (NOISE_GRADE == "C")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("AB") { FontWeight = FontWeights.Normal, FontSize = 18 });
                            tb.Inlines.Add(new Run("C") { FontWeight = FontWeights.Bold, FontSize = 28 });
                            tb.Inlines.Add(new Run("D") { FontWeight = FontWeights.Normal, FontSize = 18 });
                            energyLayoutWD_NEW.labelNoiseGrade.Content = tb;
                        }
                        else if (NOISE_GRADE == "D")
                        {
                            TextBlock tb = new TextBlock();
                            tb.Inlines.Add(new Run("ABCD") { FontWeight = FontWeights.Normal, FontSize = 18 });
                            tb.Inlines.Add(new Run("D") { FontWeight = FontWeights.Bold, FontSize = 28 });
                            energyLayoutWD_NEW.labelNoiseGrade.Content = tb;
                        }
                    }
                    
                    break;

                default:

                    viewboxItem.Child = null;
                    viewboxItem.Child = photoLayout;
                    photoLayout.loadPicture(specData[point], "imageItem", ".jpg", MCLsettings.Default.Division);

                    break;
            }
        }


        private void button_Click(object sender, RoutedEventArgs e)
        {

            //readFromKey = "43795908KM6597036941"; //ref
            readFromKey = "50786002L7119A051429"; //wm
            //readFromKey = "54327909LG221A047652"; //wm
            //readFromKey = "54320910LA677A048621"; //wm kiv
            //readFromKey = "58336911K7313A048234"; //wm WMR
            //readFromKey = "30644911KV012A049116"; // wm WFS
            //readFromKey = "57143910L7628A047444"; // wm WES
            //readFromKey = "58094911KN653A048376"; // wd WMR
            //readFromKey = "26308909LS393A047805"; // wd WIS
            //readFromKey = "54309909LS803A047800"; // wd KIV
            //readFromKey = "01836911K9145A048979";

            //MessageBox.Show(divisionScanType(readFromKey));

            if (divisionScanType(readFromKey) == "REF" && MCLsettings.Default.Division == "WM")
            {
                MessageBox.Show("This is scan from REF division. Change division in option to REF", "Change Division", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else if (divisionScanType(readFromKey) == "WM" && MCLsettings.Default.Division == "REF")
            {
                MessageBox.Show("This is scan from WM division. Change division in option to WM", "Change Division", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else //if (divisionScanType(readFromKey) == MCLsettings.Default.Division)
            {
                dataGridPointList.ItemsSource = null;
                tabControl1.SelectedIndex = 0;
                imageGif.Visibility = Visibility.Visible;
                getDataFromDB.RunWorkerAsync();
            }

        }
        private void ButtonGrid_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                DataRowView row = dataGridPointList.SelectedItem as DataRowView;

                actualPoint = Int16.Parse(row.Row.ItemArray[0].ToString()) - 1;

                labelPointName.Content = row.Row.ItemArray[0].ToString() + ". " + row.Row.ItemArray[2].ToString();
                labelBOMspec.Content = new TextBlock() { Text = ITEM_SPEC_Data[actualPoint], TextWrapping = TextWrapping.Wrap };
                labelBOMpn.Content = PART_NO_Data[actualPoint];

                loadLayout(actualPoint, false);

                if (inspectedArray[actualPoint] == "OK")
                {
                    labelOK.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0)); ;
                    labelNG.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
                }
                else if (inspectedArray[actualPoint] == "NG")
                {
                    labelOK.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
                    labelNG.Background = new SolidColorBrush(Color.FromRgb(255, 1, 1));
                }
                else
                {
                    labelOK.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
                    labelNG.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
                }

                tabControl1.SelectedIndex = 1;

            }
            catch
            { }

        }
        private void pointSum()
        {
            int sum = 0;
            for (int i = 0; i < inspectedArray.Length; i++)
            {
                if (inspectedArray[i] == "OK" || inspectedArray[i] == "NG")
                {
                    sum = sum + 1;
                }

            }
            labelPointQty.Content = inspectedArray.Length - sum;
            if (sum == inspectedArray.Length)
            {
                buttonSave.IsEnabled = true;
                labelPointQty.Content = "Now you can save result !";
            }
        }

        private void labelOK_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            labelOK.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0)); ;
            labelNG.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));

            ((DataRowView)dataGridPointList.Items[actualPoint]).Row["Result"] = "OK";

            inspectedArray[actualPoint] = "OK";
            pointSum();
        }

        private void labelNG_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            labelOK.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
            labelNG.Background = new SolidColorBrush(Color.FromRgb(255, 1, 1));

            ((DataRowView)dataGridPointList.Items[actualPoint]).Row["Result"] = "NG";

            inspectedArray[actualPoint] = "NG";
            pointSum();
        }

        private void buttonNext_Click(object sender, RoutedEventArgs e)
        {
            if (actualPoint < dataGridPointList.Items.Count - 1)
            {
                DataRowView row = dataGridPointList.Items[actualPoint + 1] as DataRowView;

                labelPointName.Content = row.Row.ItemArray[0].ToString() + ". " + row.Row.ItemArray[2].ToString();
                labelBOMspec.Content = new TextBlock() { Text = ITEM_SPEC_Data[actualPoint + 1], TextWrapping = TextWrapping.Wrap };
                labelBOMpn.Content = PART_NO_Data[actualPoint + 1];

                loadLayout(actualPoint + 1, false);

                actualPoint = actualPoint + 1;

                if (inspectedArray[actualPoint] == "OK")
                {
                    labelOK.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0)); ;
                    labelNG.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
                }
                else if (inspectedArray[actualPoint] == "NG")
                {
                    labelOK.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
                    labelNG.Background = new SolidColorBrush(Color.FromRgb(255, 1, 1));
                }
                else
                {
                    labelOK.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
                    labelNG.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
                }

            }
            else
            {
                changeTab(0);
            }
        }

        private void buttonPrev_Click(object sender, RoutedEventArgs e)
        {
            if (actualPoint != 0)
            {

                DataRowView row = dataGridPointList.Items[actualPoint - 1] as DataRowView;

                labelPointName.Content = row.Row.ItemArray[0].ToString() + ". " + row.Row.ItemArray[2].ToString();
                labelBOMspec.Content = new TextBlock() { Text = ITEM_SPEC_Data[actualPoint - 1], TextWrapping = TextWrapping.Wrap };
                labelBOMpn.Content = PART_NO_Data[actualPoint - 1];

                loadLayout(actualPoint - 1, false);

                actualPoint = actualPoint - 1;

                if (inspectedArray[actualPoint] == "OK")
                {
                    labelOK.Background = new SolidColorBrush(Color.FromRgb(0, 255, 0)); ;
                    labelNG.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
                }
                else if (inspectedArray[actualPoint] == "NG")
                {
                    labelOK.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
                    labelNG.Background = new SolidColorBrush(Color.FromRgb(255, 1, 1));
                }
                else
                {
                    labelOK.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
                    labelNG.Background = new SolidColorBrush(Color.FromRgb(203, 203, 203));
                }
            }
            else
            {
                changeTab(0);
            }
        }

        private void buttonReturnToList_Click(object sender, RoutedEventArgs e)
        {
            changeTab(0);
        }

        DataGridCell GetCell(DataGrid dg, int rowIndex, int columnIndex)
        {
            var dr = dg.ItemContainerGenerator.ContainerFromIndex(rowIndex) as DataGridRow;
            var dc = dg.Columns[columnIndex].GetCellContent(dr);
            return dc.Parent as DataGridCell;

        }

        private void cellColor()
        {
            try
            {
                for (int i = 0; i < dataGridPointList.Items.Count; i++)
                {
                    DataGridRow row = (DataGridRow)dataGridPointList.ItemContainerGenerator.ContainerFromIndex(i);
                    TextBlock cellContent = dataGridPointList.Columns[3].GetCellContent(row) as TextBlock;
                    if (cellContent != null && cellContent.Text.Equals("OK"))
                    {
                        object item = dataGridPointList.Items[i];
                        GetCell(dataGridPointList, i, 3).Background = Brushes.LawnGreen;
                        GetCell(dataGridPointList, i, 3).FontWeight = FontWeights.Bold;
                    }
                    else if (cellContent != null && cellContent.Text.Equals("NG"))
                    {
                        object item = dataGridPointList.Items[i];
                        GetCell(dataGridPointList, i, 3).Background = Brushes.Red;
                    }
                }
            }
            catch { }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            cellColor();
        }

        public void changeTab(int tabIndex)
        {
            tabControl1.SelectedIndex = tabIndex;
        }

        public bool IF_WD(string modelName)
        {
            if (modelName.Substring(5, 1) == "G" || modelName.Substring(5, 1) == "M")
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }

        public bool IF_SLIM (string modelName)
        {
            if (modelName.Substring(4, 1) == "H" || modelName.Substring(4, 1) == "W" || modelName.Substring(4, 1) == "N")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void zapiszLog(string tresc)
        {
            if (CheckNet() == true)
            {
                try
                {
                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"\\172.26.85.148\departments\ref_qa\QA Systems\VSI\03\Logs\VSI_03_logs_ID_" + "1" + "_" + Environment.MachineName + ".txt", true))
                    {
                        file.WriteLine(DateTime.Now.ToString("yyy-MM-dd") + "\t" + DateTime.Now.ToLongTimeString() + "\t" + Environment.MachineName + "\t" + Environment.UserName + "\t" + "AppID:" + "1" + "\t" + tresc);
                    }
                }
                catch (Exception)
                {
                    //nothing
                }
            }

        }

        public bool CheckNet()
        {
            bool stats;
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() == true)
            {
                stats = true;
            }
            else
            {
                stats = false;
            }
            return stats;
        }

        private void dataGridPointList_LayoutUpdated(object sender, EventArgs e)
        {
            cellColor();

        }

        private void closeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            closeButton.Background = Brushes.IndianRed;
        }

        private void closeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            closeButton.Background = Brushes.Transparent;
        }

        private void normalButton_MouseEnter(object sender, MouseEventArgs e)
        {
            normalButton.Background = Brushes.Gray;
        }

        private void normalButton_MouseLeave(object sender, MouseEventArgs e)
        {
            normalButton.Background = Brushes.Transparent;
        }

        private void minButton_MouseEnter(object sender, MouseEventArgs e)
        {
            minButton.Background = Brushes.Gray;
        }

        private void minButton_MouseLeave(object sender, MouseEventArgs e)
        {
            minButton.Background = Brushes.Transparent;
        }

        private void closeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            zapiszLog("INFO" + "\t" + "Application Close");
            Application.Current.Shutdown();

        }

        private void normalButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void minButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void labelTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            zapiszLog("INFO" + "\t" + "Application Start");

            if (!Directory.Exists(@"C:\LGEWR_IMAGES\MCL\REF"))
            {
                Directory.CreateDirectory(@"C:\LGEWR_IMAGES\MCL\REF");
                zapiszLog("INFO" + "\t" + "pictureUpdate: " + @"Utworzono katalog [C:\LGEWR_IMAGES\MCL\REF]");
            }

            if (!Directory.Exists(@"C:\LGEWR_IMAGES\MCL\WM"))
            {
                Directory.CreateDirectory(@"C:\LGEWR_IMAGES\MCL\WM");
                zapiszLog("INFO" + "\t" + "pictureUpdate: " + @"Utworzono katalog [C:\LGEWR_IMAGES\MCL\WM]");
            }

            filesCopyWorker.RunWorkerAsync();

            zegarFilesCopy.Tick += new EventHandler(zegarFilesCopy_Tick);
            zegarFilesCopy.Interval = TimeSpan.FromMinutes(5);
            zegarFilesCopy.Start();

            imagePreview.Owner = this;
            imageGif.Visibility = Visibility.Hidden;
            tab0.Visibility = Visibility.Hidden;
            tab1.Visibility = Visibility.Hidden;



        }

        private void keyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D1 || e.Key == Key.NumPad1)
            { readFromKey = readFromKey + "1"; }
            else if (e.Key == Key.D2 || e.Key == Key.NumPad2) { readFromKey = readFromKey + "2"; }
            else if (e.Key == Key.D3 || e.Key == Key.NumPad3) { readFromKey = readFromKey + "3"; }
            else if (e.Key == Key.D4 || e.Key == Key.NumPad4) { readFromKey = readFromKey + "4"; }
            else if (e.Key == Key.D5 || e.Key == Key.NumPad5) { readFromKey = readFromKey + "5"; }
            else if (e.Key == Key.D6 || e.Key == Key.NumPad6) { readFromKey = readFromKey + "6"; }
            else if (e.Key == Key.D7 || e.Key == Key.NumPad7) { readFromKey = readFromKey + "7"; }
            else if (e.Key == Key.D8 || e.Key == Key.NumPad8) { readFromKey = readFromKey + "8"; }
            else if (e.Key == Key.D9 || e.Key == Key.NumPad9) { readFromKey = readFromKey + "9"; }
            else if (e.Key == Key.D0 || e.Key == Key.NumPad0) { readFromKey = readFromKey + "0"; }
            else if (e.Key != Key.D0 || e.Key != Key.D1 || e.Key != Key.D2 || e.Key != Key.D3 || e.Key != Key.D4 || e.Key != Key.D5 || e.Key != Key.D6 || e.Key != Key.D7 || e.Key != Key.D8 || e.Key != Key.D9 || e.Key != Key.D0 || e.Key != Key.NumPad1 || e.Key != Key.NumPad2 || e.Key != Key.NumPad3 || e.Key != Key.NumPad4 || e.Key != Key.NumPad5 || e.Key != Key.NumPad6 || e.Key != Key.NumPad7 || e.Key != Key.NumPad8 || e.Key != Key.NumPad9 || e.Key != Key.NumPad0 || e.Key != Key.Enter || e.Key != Key.Return)
            {
                readFromKey = readFromKey + e.Key.ToString();
                //readFromKey = readFromKey.Replace("Return", "");
                readFromKey = readFromKey.Replace("Shift", "");
                readFromKey = readFromKey.Replace("Key", "");
                readFromKey = readFromKey.Replace("Control", "");
                readFromKey = readFromKey.Replace("Capital", "");
                readFromKey = readFromKey.Replace("Left", "");
                readFromKey = readFromKey.Replace("Right", "");
            }

            if (readFromKey.Length > 6)
            {
                if (readFromKey.Substring(readFromKey.Length - 6, 6) == "Return")
                {
                    readFromKey = readFromKey.Replace("Return", "");


                    if (readFromKey.Length >= 20)
                    {

                        if (divisionScanType(readFromKey) == "REF" && MCLsettings.Default.Division == "WM")
                        {
                            MessageBox.Show("This is scan from REF division. Change division in option to REF", "Change Division", MessageBoxButton.OK, MessageBoxImage.Information);
                            readFromKey = null;
                        }
                        else if (divisionScanType(readFromKey) == "WM" && MCLsettings.Default.Division == "REF")
                        {
                            MessageBox.Show("This is scan from WM division. Change division in option to WM", "Change Division", MessageBoxButton.OK, MessageBoxImage.Information);
                            readFromKey = null;
                        }
                        else
                        {
                            clearVariables();
                            dataGridPointList.ItemsSource = null;
                            tabControl1.SelectedIndex = 0;
                            imageGif.Visibility = Visibility.Visible;
                            buttonSave.IsEnabled = false;
                            labelPointQty.Content = "-";

                            try
                            {
                                readFromKey = readFromKey.Substring(readFromKey.Length - 20, 20);
                                getDataFromDB.RunWorkerAsync();
                            }
                            catch (Exception ex)
                            {
                                labelStatus.Content = "ERROR: " + ex.Message;
                                zapiszLog("ERROR" + "\t" + "keyDown: " + ex.Message);
                                //MessageBox.Show("db error");
                            }
                        }
                    }
                    labelStatus.Content = readFromKey;
                }
            }
        }

        private string divisionScanType (string scan)
        {
            if (scan.Substring(13, 1) == "A")
            {
                return "WM";
            }
            else
            {
                return "REF";
            }
                 
        }

        private void viewboxItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataRowView row = dataGridPointList.Items[actualPoint] as DataRowView;

            if (specData[actualPoint] == "ENERGY_" + MCLsettings.Default.Division)
            {
                imagePreview.ActualPoint = actualPoint;
                loadLayout(actualPoint, true);
                imagePreview.labelPointName.Content = row.Row.ItemArray[0].ToString() + ". " + row.Row.ItemArray[2].ToString();
                imagePreview.Show();
            }
            else if (specData[actualPoint] == "LABEL_ENERGY_LAYOUT_NEW")
            {
                imagePreview.ActualPoint = actualPoint;
                loadLayout(actualPoint, true);
                imagePreview.labelPointName.Content = row.Row.ItemArray[0].ToString() + ". " + row.Row.ItemArray[2].ToString();
                imagePreview.Show();
            }
            else if (specData[actualPoint] == "LABEL_ENERGY_LAYOUT_NEW_WM")
            {
                imagePreview.ActualPoint = actualPoint;
                loadLayout(actualPoint, true);
                imagePreview.labelPointName.Content = row.Row.ItemArray[0].ToString() + ". " + row.Row.ItemArray[2].ToString();
                imagePreview.Show();
            }
            else if (specData[actualPoint] == "LABEL_RATING_LAYOUT")
            {
                imagePreview.ActualPoint = actualPoint;
                loadLayout(actualPoint, true);
                imagePreview.labelPointName.Content = row.Row.ItemArray[0].ToString() + ". " + row.Row.ItemArray[2].ToString();
                imagePreview.Show();
            }
            else if (specData[actualPoint] != "MANUAL")
            {
                imagePreview.ActualPoint = actualPoint;
                imagePreview.loadPicture(specData[actualPoint], "imageFullScreen", ".jpg", MCLsettings.Default.Division);
                imagePreview.labelPointName.Content = row.Row.ItemArray[0].ToString() + ". " + row.Row.ItemArray[2].ToString();
                imagePreview.Show();
            }

        }

        private void filesCopy(string div)
        {
            try
            {
                string sourcePatch = @"\\172.26.85.148\departments\ref_qa\QA Systems\VSI\03\Pictures\" + div + @"\";
                string destinationPatch = @"C:\LGEWR_IMAGES\MCL\" + div + @"\";

                DirectoryInfo source = new DirectoryInfo(sourcePatch);

                foreach (var file in source.GetFiles("*"))
                {
                    if (!File.Exists(destinationPatch + file))
                    {
                        File.Copy(sourcePatch + file, destinationPatch + file);
                        zapiszLog("INFO" + "\t" + div + " File copied  " + file);
                    }
                    else if ((File.GetLastWriteTime(sourcePatch + file) > File.GetLastWriteTime(destinationPatch + file)))
                    {
                        File.Copy(sourcePatch + file, destinationPatch + file, true);
                        zapiszLog("INFO" + "\t" + div + " File copied [new ver]  " + file);
                    }
                }

                source = new DirectoryInfo(destinationPatch);

                foreach (var file in source.GetFiles("*"))
                {
                    if (!File.Exists(sourcePatch + file))
                    {
                        File.Delete(destinationPatch + file);
                        zapiszLog("INFO" + "\t" + div + " Deleted file  " + file);
                    }
                }
            }
            catch (Exception ex)
            {
                zapiszLog("ERROR" + "\t" + "filesCopy: " + ex.Message);
            }

        }

        private void button2_Click_1(object sender, RoutedEventArgs e)
        {
            OptionWindow optionWindow = new OptionWindow();
            optionWindow.Show();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            labelDivision.Content = "Division: " + MCLsettings.Default.Division;
            labelInspector.Content = MCLsettings.Default.InspectorsNames.ToString();
        }

        private void buttonSave_Click(object sender, RoutedEventArgs e)
        {

            int OK_Result = 0;
            int NG_Result = 0;
            string finalResult = "OK";
            string NGPoint = ", NG Points: ";

            for (int i = 0; i < inspectedArray.Length; i++)
            {
                if (inspectedArray[i] == "NG")
                {
                    NG_Result++;
                    finalResult = "NG";

                    DataRowView row = dataGridPointList.Items[i] as DataRowView;
                    NGPoint = NGPoint + row.Row.ItemArray[2].ToString() + ", ";
                }
                else
                {
                    OK_Result++;
                }
            }
            if (NG_Result == 0) { NGPoint = NGPoint.Replace(", NG Points: ", ""); }

            string sqlString = "INSERT INTO MCL_INSPECTION_RESULTS ([DATE], [TIME], [DIVISION], [SET_ID], [MODEL_SUFFIX], [WO],[SERIAL], [RESULT], [REMARKS], [ADD_INFO], [INSPECTOR])" +
                               "VALUES ('" + DateTime.Now.ToString("yyy-MM-dd") + "', '" + DateTime.Now.ToLongTimeString() + "' , '" +
                                         MCLsettings.Default.Division + "' , '" +
                                         setID.Substring(0,20) + "' , '" + //dodano substring bo skaner na in line ref dodaje ctrlj do stringa
                                         model + "' , '" +
                                         lot + "' , '" +
                                         sn + "' , '" +
                                         finalResult + "' , '" +
                                         //"" + "' , '" +
                                         "Total point: " + dataGridPointList.Items.Count + ", OK: " + OK_Result + ", NG: " + NG_Result + NGPoint + "' , '" +
                                         "Type of inspection: " + MCLsettings.Default.Area + "' , '" + MCLsettings.Default.InspectorsNames + "')";

            try
            {
                SqlConnection connectionSQLSRV = new SqlConnection(sqlConnString);
                SqlCommand command = new SqlCommand(sqlString, connectionSQLSRV);  
                connectionSQLSRV.Open();
                command.ExecuteNonQuery();
                connectionSQLSRV.Close();

                inspectionTime = DateTime.Now - inspectionStart;
                labelPointQty.Content = "-";
                labelStatus.Content = "Save successful, result: " + finalResult;
                MessageBox.Show("Save successful, result: " + finalResult, "Save Result", MessageBoxButton.OK, MessageBoxImage.Information);

                dataGridPointList.ItemsSource = null;
                tabControl1.SelectedIndex = 0;
                //readFromKey = null;
                buttonSave.IsEnabled = false;

                zapiszLog("INFO" + "\t" + "Save inspection: Division " + MCLsettings.Default.Division + ", Result " + finalResult + ", Model " + model + 
                            ", Serial " + sn + ", Inspector: " + MCLsettings.Default.InspectorsNames + ", Inspection time: " + inspectionTime.ToString(@"hh\:mm\:ss"));

                clearVariables();
            }

            catch (Exception ex)
            {
                zapiszLog("ERROR" + "\t" + "saveResult: " + ex.Message);
                labelStatus.Content = "ERROR: " + ex.Message;
                MessageBox.Show("Save ERROR: " + ex.Message, "Save Result", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
    }

}
public class manualItem
{
    public int ID { get; set; }

    public string Description { get; set; }

    public string  PartNO { get; set; }

    public string Spec { get; set; }
}
