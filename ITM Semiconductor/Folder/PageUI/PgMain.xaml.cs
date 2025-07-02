using DeviceSource;
using DTO;
using FluentFTP;
using Mitsubishi;
using MvCamCtrl.NET;
using OpenCvSharp.Extensions;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using VisionInspection;
using static ITM_Semiconductor.MesSettings;
using Path = System.IO.Path;
namespace ITM_Semiconductor
{
    /// <summary>
    /// Interaction logic for PgMain.xaml
    /// </summary>
    public partial class PgMain : Page
    {

        //public bool resizeImage = false;
        private VisionInspectionStatus visionInspectionStatus;

        bool isAlarming = false;
        //Lock ojb
        private static Object objLock = new Object();

        //private Stopwatch stopWatch = new Stopwatch();
        private System.Timers.Timer cycleTimer;
        private System.Timers.Timer clock;
        private AppSettings appSettings;
        private ConnectionSettings connection;
        private bool PlcAlarm = false;
        private bool RB1Alarm = false;
        private bool RB2Alarm = false;
        private int lastAlarm = 0;
        private Boolean mesShow = false;
        private bool AutoCH2 = true;
        private bool ManualCH2 = false;
        private bool AutoCH1 = true;
        private bool ManualCH1 = false;

        private LotStatus lotStatus;

        public int MES_TIMEOUT = 4000;

        private bool repuireNewLot = false;
        private bool isRunning = false; // Signal Auto Run 
        private bool ErrFlag = false; // Signal Err when Err ocur
        private Thread runThread;
        List<int> D_7000_7900;
        List<bool> M_0_1000;
        List<bool> M_1000_2000;

        public string LightButton = "redOn.jpg";
        private bool resizeImage = false;
        private static MyLogger logger = new MyLogger("Main process");

        //Nam Em thêm ngày 01/11
        private HikCamDevice hikCamera = new HikCamDevice();
        MyCamera.MV_CC_DEVICE_INFO_LIST m_pDeviceList;
        CameraOperator m_pOperator = new CameraOperator();
        //private HikCam myCam1 = new HikCam();
        //private HikCam myCam2 = new HikCam();
        VisionAL vision1 = new VisionAL(VisionAL.Chanel.Ch1);
        VisionAL vision2 = new VisionAL(VisionAL.Chanel.Ch2);
        Boolean cameraReady = false;


        List<short> D_ListShortDevicePLC_0 = new List<short>();
        private bool hasClearedError = false;
        bool M611_Reset = false;
        bool M619_Reset = false;
        public PgMain()
        {
            InitializeComponent();
            InitializeErrorCodes();
            ActionClearAlarm.ClearErrorAction = ClearError;



            this.clock = new System.Timers.Timer(1000);
            this.cycleTimer = new System.Timers.Timer(100);
            this.cycleTimer.AutoReset = true;
            this.cycleTimer.Elapsed += this.CycleTimer_Elapsed;
            this.clock.AutoReset = true;
            this.clock.Elapsed += this.Clock_Elapsed;
            #region Main process event creat
            this.Loaded += this.MainWindow_Load;
            this.Unloaded += this.MainWindow_Unloaded;
            //Start CH2
            this.btnStartCH2.Click += BtnStartCH2_Click;
            //Start CH1
            this.btnStartCH1.Click += BtnStartCH1_Click;
            //Pause CH2
            this.btnPauseCH2.Click += BtnPauseCH2_Click;
            //Pause CH1
            this.btnPauseCH1.Click += BtnPauseCH1_Click;
            //Reset CH1
            this.btnResetCH1.Click += BtnResetCH1_Click;
            //Reset CH2
            this.btnResetCH2.Click += BtnResetCH2_Click;
            //HOME CH1
            this.btnHomeCH1.Click += BtnHomeCH1_Click;
            //HOME CH2
            this.btnHomeCH2.Click += BtnHomeCH2_Click;
            //Stop CH1
            this.btnStopCH1.Click += BtnStopCH1_Click;
            //Stop CH2
            this.btnStopCH2.Click += BtnStopCH2_Click;



            this.btLotin.Click += this.btLotin_Clicked;
            this.btLotend.Click += BtLotend_Click;
            //this.cbModel.SelectionChanged += CbModel_SelectionChanged;

            //Auto/Manual Vision
            this.btnAutoCH2.Click += BtnAuto_Click;
            this.btnManualCH2.Click += btnManual_Click;
            this.btnAutoCH1.Click += BtnAutoCH1_Click; ;
            this.btnManualCH1.Click += BtnManualCH1_Click;

            this.Img_Main_process_1.MouseDown += Grid_Image1_MouseDown;
            this.Img_Main_process_2.MouseDown += Grid_Image2_MouseDown;

            //Lot Status
            lotStatus = new LotStatus();

            Qr1Manager.Init();
            this.visionInspectionStatus = Qr1Manager.GetQr1Status();
            #endregion

        }

        #region UIEvent
        private void BtLotend_Click(object sender, RoutedEventArgs e)
        {
            UserManager.createUserLog(UserActions.MAIN_BUTTON_LOTIN);
            var lotData = UiManager.appSettings.lotData;
            if (new WndConfirm().DoComfirmYesNo("Do You Want to LotEnd, ?", System.Windows.Window.GetWindow(this)))
            {
                lotData.workGroup = "";
                lotData.deviceId = "";
                lotData.LotId = "";
                lotData.Config = "";
                lotData.lotQty = 0;
                this.lblEquimentID.Content = "";
                this.lblLotNo.Content = "";
                repuireNewLot = true;
                if(UiManager.PLC.isOpen())
                {
                  UiManager.PLC.WriteBit(DeviceCode.M, 532, true);
                }
                else
                {
                    MessageBox.Show("Write Bit LOTIN PLC thất bại Check lại kết nối PLC ");
                }    
 
                //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 532);
                updateLotDataUI();
                UiManager.SaveAppSetting();
                logger.Create(" -> Delete LOT", LogLevel.Information);
            }
        }
        //Stop CH2m
        private void BtnStopCH2_Click(object sender, RoutedEventArgs e)
        {
            if (new WndConfirm().DoComfirmYesNo("Do You Want to Stop Channel 2 Machine", System.Windows.Window.GetWindow(this)))
            {
                UiManager.PLC.WriteBit(DeviceCode.M, 87, true);
                //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 87);
                StopEvent();
            }
            return;
        }
        //Stop CH1
        private void BtnStopCH1_Click(object sender, RoutedEventArgs e)
        {
            if (new WndConfirm().DoComfirmYesNo("Do You Want to Stop Channel 1 Machine", System.Windows.Window.GetWindow(this)))
            {
                UiManager.PLC.WriteBit(DeviceCode.M, 37, true);
                //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 37);
                StopEvent();
            }
            return;
        }
        //Pause CH1
        private void BtnPauseCH1_Click(object sender, RoutedEventArgs e)
        {
            if (new WndConfirm().DoComfirmYesNo("Do You Want to Pause Channel 1 Machine", System.Windows.Window.GetWindow(this)))
            {
                UiManager.PLC.WriteBit(DeviceCode.M, 30, true);
                //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 30);
            }
            else
                return;
        }
        //Pause CH2
        private void BtnPauseCH2_Click(object sender, RoutedEventArgs e)
        {
            if (new WndConfirm().DoComfirmYesNo("Do You Want to Pause Channel 2 Machine", System.Windows.Window.GetWindow(this)))
            {
                UiManager.PLC.WriteBit(DeviceCode.M, 80, true);
                //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 80);
            }
            return;
        }
        //Reset CH2
        private void BtnResetCH2_Click(object sender, RoutedEventArgs e)
        {
            UiManager.PLC.WriteBit(DeviceCode.M, 65, true);
            this.ClearError();
            //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 65);
            if (!isRunning)
            {
                addLog("CHANNEL 2 STOP!!!");
            }
            else if (isRunning)
            {
                addLog("USER RESET CHANNEL 2");
               
            }
        }
        //Reset CH2
        private void BtnResetCH1_Click(object sender, RoutedEventArgs e)
        {
            UiManager.PLC.WriteBit(DeviceCode.M, 15, true);
            this.ClearError();
            //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 15);
            if (!isRunning)
            {
                addLog("CHANNEL 1 STOP!!!");
            }
            if (isRunning)
            {
                addLog("USER RESET CHANNEL 1!");
                
            }
        }

        //HOME CH2
        private void BtnHomeCH2_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                addLog("SYSTEM RUNING, PLEASE DON'T INITIAL!");
                return;
            }
            if (!isRunning)
            {
                UserManager.createUserLog(UserActions.MAIN_BUTTON_INIT);
                if (new WndConfirm().DoComfirmYesNo("Do You Want to Initial Machine Channel 2", System.Windows.Window.GetWindow(this)))
                {
                    var dbLot = DbRead.GetCurrentLotStatus();
                    if (dbLot != null)
                    {
                        logger.Create(String.Format(" -> Reset Current LOT!"), LogLevel.Information);
                        dbLot.InputCount = 0;
                        dbLot.TotalCount = 0;
                        dbLot.OkCount = 0;
                        dbLot.NgCount = 0;
                        dbLot.totalSeconds = 0;
                        DbWrite.updateLotStatus(this.lotStatus);
                    }
                    Qr1Manager.Reset();
                    addLog("USER CHOSE INITIAL CHANNEL 2!!!");
                    UiManager.PLC.WriteBit(DeviceCode.M, 70, true);
                    //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 70);
                    return;
                }
            }
        }
        //HOME CH1
        private void BtnHomeCH1_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                addLog("SYSTEM RUNING, PLEASE DON'T INITIAL!");
                return;
            }
            if (!isRunning)
            {
                UserManager.createUserLog(UserActions.MAIN_BUTTON_INIT);
                if (new WndConfirm().DoComfirmYesNo("Do You Want to Initial Machine Channel 1", System.Windows.Window.GetWindow(this)))
                {
                    var dbLot = DbRead.GetCurrentLotStatus();
                    if (dbLot != null)
                    {
                        logger.Create(String.Format(" -> Reset Current LOT!"), LogLevel.Error);
                        dbLot.InputCount = 0;
                        dbLot.TotalCount = 0;
                        dbLot.OkCount = 0;
                        dbLot.NgCount = 0;
                        dbLot.totalSeconds = 0;
                        DbWrite.updateLotStatus(this.lotStatus);
                    }
                    Qr1Manager.Reset();
                    addLog("USER CHOSE INITIAL CHANNEL 1!!!");
                    UiManager.PLC.WriteBit(DeviceCode.M, 20, true);
                    //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 20);
                    return;
                }
            }
        }
        private void Grid_Image2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2 && e.LeftButton == MouseButtonState.Pressed)
            {
                resizeControlCH2();
            }
        }
        private void Grid_Image1_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2 && e.LeftButton == MouseButtonState.Pressed)
            {
                resizeControlCH1();
            }
        }
        private void resizeControlCH1()
        {
            if (resizeImage == false)
            {
                Grid_Row_StatusInput.Height = new GridLength(0, GridUnitType.Star);
                Grid_Row_Status.Height = new GridLength(0, GridUnitType.Star);
                Grid_Image1.Width = new GridLength(0, GridUnitType.Star);
                Grid_ShowControl2.Height = new GridLength(0, GridUnitType.Star);
                Grid_ShowDataJigCh1.Width = new GridLength(0, GridUnitType.Star);
                resizeImage = true;
            }
            else if (resizeImage == true)
            {
                Grid_Row_StatusInput.Height = new GridLength(17, GridUnitType.Star);
                Grid_Row_Status.Height = new GridLength(3.5, GridUnitType.Star);
                Grid_Image1.Width = new GridLength(60, GridUnitType.Star);
                Grid_ShowControl2.Height = new GridLength(38, GridUnitType.Star);
                Grid_ShowDataJigCh1.Width = new GridLength(1, GridUnitType.Star);
                resizeImage = false;
            }
        }
        private void resizeControlCH2()
        {
            if (resizeImage == false)
            {
                Grid_Row_StatusInput.Height = new GridLength(0, GridUnitType.Star);
                Grid_Row_Status.Height = new GridLength(0, GridUnitType.Star);
                Grid_Image2.Width = new GridLength(0, GridUnitType.Star);
                Grid_ShowControl1.Height = new GridLength(0, GridUnitType.Star);
                Grid_ShowDataJigCh2.Width = new GridLength(0, GridUnitType.Star);
                resizeImage = true;
            }
            else if (resizeImage == true)
            {
                Grid_Row_StatusInput.Height = new GridLength(17, GridUnitType.Star);
                Grid_Row_Status.Height = new GridLength(3.5, GridUnitType.Star);
                Grid_Image2.Width = new GridLength(60, GridUnitType.Star);
                Grid_ShowControl1.Height = new GridLength(5, GridUnitType.Star);
                Grid_ShowDataJigCh2.Width = new GridLength(1, GridUnitType.Star);
                resizeImage = false;
            }
        }
        private void BtnAuto_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
                return;
            var converter = new BrushConverter();
            this.AutoCH2 = true;
            this.ManualCH2 = false;
            btnAutoCH2.Background = Brushes.LightYellow;
            btnManualCH2.Background = (Brush)converter.ConvertFromString("#D5D5D5");
            return;
        }
        private void btnManual_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
                return;
            if (isRunning)
                return;
            var converter = new BrushConverter();
            if (!M_0_1000[2])
            {
                this.AutoCH2 = false;
                this.ManualCH2 = true;
                btnManualCH2.Background = Brushes.LightYellow;
                btnAutoCH2.Background = (Brush)converter.ConvertFromString("#D5D5D5");
                this.AutoStart();
            }
            return;
        }
        private void BtnManualCH1_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
                return;
            if (isRunning)
                return;
            var converter = new BrushConverter();
            if (!M_0_1000[52])
            {
                this.AutoCH1 = false;
                this.ManualCH1 = true;
                btnManualCH1.Background = Brushes.LightYellow;
                btnAutoCH1.Background = (Brush)converter.ConvertFromString("#D5D5D5");
                this.AutoStart();
            }
            return;
        }
        private void BtnAutoCH1_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
                return;
            var converter = new BrushConverter();
            this.AutoCH1 = true;
            this.ManualCH1 = false;
            btnAutoCH1.Background = Brushes.LightYellow;
            btnManualCH1.Background = (Brush)converter.ConvertFromString("#D5D5D5");
            return;
        }

        #endregion

        #region Startup

        private void Innit()
        {
            //ON UI
            startUp();
        }
        private void btLotin_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                UserManager.createUserLog(UserActions.MAIN_BUTTON_LOTIN);

                var wnd = new WndLotIn();
                var lotData = new LotInData();
                lotData.workGroup = UiManager.appSettings.lotData.workGroup;

                var newSettings = wnd.DoSettings(System.Windows.Window.GetWindow(this), lotData);
                if (newSettings != null)
                {
                    UiManager.appSettings.lotData = newSettings.Clone();
                    this.lotStatus = new LotStatus(newSettings.lotId);
                    this.lotStatus.InputCount = newSettings.lotQty;
                  
                   

                    UiManager.appSettings.lotData = newSettings.Clone();
                    lotData.LotStart = DateTime.Now;
                    updateLotDataUI();
                    UiManager.SaveAppSetting();
                    
                    

                    UiManager.PLC.WriteBit(DeviceCode.M, 530, true);
                   

                    //this.lotStatus = new LotStatus(newSettings.lotId);
                    //this.lotStatus.InputCount = newSettings.lotQty;

                    //logger.Create(" -> Create/Override LOT data!", LogLevel.Information);

                    //// Write to DB:
                    //DbWrite.insertLot(this.lotStatus);

                    //// Read out from DB to get Id for later update:
                    //this.lotStatus = DbRead.GetCurrentLotStatus();
                    //repuireNewLot = false;
                

                }
            }
            catch (Exception ex)
            {
                logger.Create("BtLotIn_Click error: " + ex.Message, LogLevel.Error);
            }
        }
        private void updateLotDataUI()
        {

            this.lblModelCurrent.Content = UiManager.appSettings.connection.modelName;
            this.lblLotNo.Content = UiManager.appSettings.lotData.LotId.ToString();
            this.lblEquimentID.Content = UiManager.appSettings.lotData.Config;


            
            
        }
        private void startUp()
        {

            var runSettings = this.appSettings.run;
            var lotData = this.appSettings.lotData;
            //Main
            Task tsk = new Task(() =>
            {

                cycleTimer.Start();

                clock.Start();
               
            });
            tsk.Start();
        }



        #endregion

        #region Event Window 

        void Form_touchUp(object sender, TouchEventArgs e)
        {
            MessageBox.Show("Form Clicked");
        }
        private void lblEquimentID_textChange(object sender, RoutedEventArgs e)
        {
            //this.appSettings.connection.model = this.lblEquimentID.Text;
            //UiManager.SaveAppSettings();
        }


        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(TabItem))
                e.Handled = true;
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ((e.OriginalSource as TabControl).SelectedItem as TabItem).Focus();
            }
            catch { }
        }
        private void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            //Task tsk = new Task(() =>
            //{
                cycleTimer.Stop();
                clock.Stop();
                int D3605 = 0;
                UiManager.PLC.WriteWord(DeviceCode.D, 3605, D3605);
                //plcComm.WriteSingleWord(MCProtocol.DeviceCode.D, 3605, new int[] { D3605 });

                //if (UiManager.Cam1 != null)
                //{
                //    UiManager.Cam1.Close();
                //    UiManager.Cam1.DisPose();
                //}
                //if (UiManager.Cam2 != null)
                //{
                //    UiManager.Cam2.Close();
                //    UiManager.Cam2.DisPose();
                //}
            //});
            //tsk.Start();
          
            //clock.Stop();

        }

        #endregion

        #region Event Mainprocess
        private void MainWindow_Load(object sender, RoutedEventArgs e)
        {

            this.appSettings = UiManager.appSettings;
            this.connection = this.appSettings.connection;
            

            updateLotDataUI();
            if (this.lblLotNo.Content.ToString() == "")
            {
                repuireNewLot = true;
            }
            else
            {
                repuireNewLot = false;
            }
            Innit();
            Task tsk1 = new Task(() =>
            {
                cameraReady = AddeviceCam();
                //InnitialCamera1();
                //InnitialCamera2();
            });
            tsk1.Start();
            Task tsk2 = new Task(() => {
                vision1.deleteOldFiles(UiManager.appSettings.connection.image.CH1_path);
                vision2.deleteOldFiles(UiManager.appSettings.connection.image.CH2_path);
            });
            tsk2.Start();
       
        }
        private void BtPower_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
            Application.Current.Shutdown(0);
        }
        private void txt_lotId_Focus(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Process.Start("osk.exe");
        }
        private void txt_productId_Focus(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Process.Start("osk.exe");
        }
        private void txt_eqpID_Focus(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Process.Start("osk.exe");
        }
        private void resetDataLot(string lot)
        {
        }
        #endregion

        #region Auto process
        private void BtnStartCH2_Click(object sender, RoutedEventArgs e)
        {
            if (repuireNewLot)
            {
                WndMessenger ShowMessenger = new WndMessenger();
                ShowMessenger.MessengerShow("Messenger : LOT IN not entered \r Chưa nhập LotIN  ");
                return;
            }
            if (!UiManager.PLC.isOpen())
            {
                addLog("CH2: PLC Not Connect");
                return;
            }
            
            UserManager.createUserLog(UserActions.MAIN_BUTTON_START);
            if (isRunning)
            {
                addLog("Already RUNNING");
                //return;
            }
            if (new WndConfirm().DoComfirmYesNo("Do You Want to Start Auto Running", System.Windows.Window.GetWindow(this)))
            {
                UiManager.PLC.WriteBit(DeviceCode.M, 67, true);
                //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 67);
                var converter = new BrushConverter();
                this.AutoCH2 = true;
                this.ManualCH2 = false;
                btnAutoCH2.Background = Brushes.LightYellow;
                btnManualCH2.Background = (Brush)converter.ConvertFromString("#D5D5D5");
                this.AutoStart();
            }
            return;
        }

        private void BtnStartCH1_Click(object sender, RoutedEventArgs e)
        {
            if (repuireNewLot)
            {
                WndMessenger ShowMessenger = new WndMessenger();
                ShowMessenger.MessengerShow("Messenger : LOT IN not entered \r Chưa nhập LotIn. Kiểm tra lại   ");
                return;
            }
            if (!UiManager.PLC.isOpen())
            {
                addLog("CH1: PLC Not Connect");
                return;
            }
           
            UserManager.createUserLog(UserActions.MAIN_BUTTON_START);
            if (isRunning)
            {
                //addLog("Already RUNNING");
                //return;
            }
            if (new WndConfirm().DoComfirmYesNo("Do You Want to Start Auto Running", System.Windows.Window.GetWindow(this)))
            {
                UiManager.PLC.WriteBit(DeviceCode.M, 17, true);
                //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 17);
                var converter = new BrushConverter();
                this.AutoCH1 = true;
                this.ManualCH1 = false;
                btnAutoCH1.Background = Brushes.LightYellow;
                btnManualCH1.Background = (Brush)converter.ConvertFromString("#D5D5D5");
                this.AutoStart();
            }
            return;
        }
        private void btnReset_PreviewMouseDown(object sender, TouchEventArgs e)
        {
            //if (isRunning)
            //{
            addLog("USER RESET!");
            //}
            UiManager.PLC.WriteBit(DeviceCode.M, 65, true);
            //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 65);
        }
        private void AutoStart()
        {
            if (isRunning)
                return;
            //TEST 11/07/2021
            CheckReady();
        }
        private void btnMainStop_Click(object sender, RoutedEventArgs e)
        {
            StopEvent();
        }
        private void StopEvent()
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    //this.btnStart.IsEnabled = true;
                    //this.btnStop.IsEnabled = false;
                    isRunning = false;
                    //lblMainStatus.Content = "STOP";
                    //lblMainStatus.Background = new SolidColorBrush(Colors.Red);
                });
            }
            catch (Exception ex)
            {
                logger.Create("Main Start Err : " + ex.ToString(), LogLevel.Error);
            }
        }
        private void CheckReady()
        {
            if (!cameraReady)
                return;
            try
            {
                isRunning = true;
                ErrFlag = false;
                if (appSettings.connection.model.Length > 9)
                {
                    //displayAlarm(30022);
                    AddError(30022);
                    return;

                }
                //vision1.ROISet();
                //vision2.ROISet();
                runManager();
            }
            catch (Exception ex)
            {
                logger.Create("Check Ready Err: " + ex.ToString(), LogLevel.Error);
            }
        }
        private void runManager()
        {
            try
            {
                callThreadStartLoopCH1();
                callThreadStartLoopCH2();


                // Check Lot End:
                int remainCnt = this.lotStatus.InputCount - this.lotStatus.TotalCount;
                if ((this.lotStatus.InputCount > 0) && (remainCnt == 0))
                {
                    //displayAlarm(AlarmInfo.LOT_END);
                    AddError(31212);
                    repuireNewLot = true;
                    return;
                }

                //AUTO LOOP
            }
            catch (Exception ex)
            {
                logger.Create("Start Run Manager Err :" + ex.ToString(), LogLevel.Error);
            }
        }
        private void creatDir(string path)
        {
            try
            {
                if (Directory.Exists(path))
                    return;
                DirectoryInfo di = Directory.CreateDirectory(path);
            }
            catch (Exception ex)
            {
                logger.Create($"Can't creat dir for Image + {ex}", LogLevel.Error);
            }
        }
        #endregion

        #region Connector Class
        public class Connector
        {
            public string JigCode { get; set; }
            public string ConnectorCode { get; set; }
            public string Lot { get; set; }
            public string Result { get; set; }
            public string Position { get; set; }
            public string DateTime { get; set; }
            public Connector(String jigcode, String connectorCode, String lot, String result, String position, String datetime)
            {
                this.JigCode = jigcode;
                this.ConnectorCode = connectorCode;
                this.Lot = lot;
                this.Result = result;
                this.Position = position;
                this.DateTime = datetime;
            }
        }

        public bool IsRunningAuto()
        {
            return isRunning;
        }
        #endregion

        #region PLC Signal & Timer
      
        private void CycleTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                this.Dispatcher.Invoke(() => 
                {
                    this.bdConnectPLC.Background = Brushes.Red;
                    this.lblConnectPLC.Content = "Disconnect";
                });
                return;
              
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    this.bdConnectPLC.Background = Brushes.LightGreen;
                    this.lblConnectPLC.Content = "Connect";
                });
            }
            try
            {
                UiManager.PLC.ReadMultiWord(DeviceCode.D, 7920, 20, out D_ListShortDevicePLC_0);
                UiManager.PLC.ReadBit(DeviceCode.M, 611, out M611_Reset);
                UiManager.PLC.ReadBit(DeviceCode.M, 619, out M619_Reset);
                UiManager.PLC.ReadMultiBits(DeviceCode.M, 0, 1000, out M_0_1000);
                UiManager.PLC.ReadMultiBits(DeviceCode.M, 1000, 1000, out M_1000_2000);

                UiManager.PLC.ReadMultiDoubleWord(DeviceCode.D, 7000, 900, out D_7000_7900);
                UpdateError();

                int model = 1;
                if (UiManager.appSettings.connection.model == "X2833")
                {
                    model = 1;
                }
                else if (UiManager.appSettings.connection.model == "X2835")
                {
                    model = 2;
                }
                else if (UiManager.appSettings.connection.model == "X2836")
                {
                    model = 3;
                }
                UiManager.PLC.WriteWord(DeviceCode.D, 7500, model);
               
            }
            catch (Exception ex)
            {
                logger.Create("PLC Com Err" + ex.ToString(), LogLevel.Error);
                return;
            }
            return;
        }
        private void UpdateError()
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (UiManager.PLC.isOpen())
                {
                    if (D_ListShortDevicePLC_0.Count >= 1)
                    {
                        this.AddError(D_ListShortDevicePLC_0[0]);
                        this.AddError(D_ListShortDevicePLC_0[1]);
                        this.AddError(D_ListShortDevicePLC_0[2]);
                        this.AddError(D_ListShortDevicePLC_0[3]);
                        this.AddError(D_ListShortDevicePLC_0[4]);
                        this.AddError(D_ListShortDevicePLC_0[5]);
                        this.AddError(D_ListShortDevicePLC_0[6]);
                        this.AddError(D_ListShortDevicePLC_0[7]);
                        this.AddError(D_ListShortDevicePLC_0[8]);
                        this.AddError(D_ListShortDevicePLC_0[9]);
                        this.AddError(D_ListShortDevicePLC_0[10]);
                        this.AddError(D_ListShortDevicePLC_0[11]);
                        this.AddError(D_ListShortDevicePLC_0[12]);
                        this.AddError(D_ListShortDevicePLC_0[13]);
                        this.AddError(D_ListShortDevicePLC_0[14]);

                        if ((D_ListShortDevicePLC_0[0] == 0) && !hasClearedError)
                        {
                            this.ClearError();
                            hasClearedError = true; // Đặt cờ để ngăn chạy lại hàm này
                        }
                        else if (D_ListShortDevicePLC_0[0] != 0)
                        {
                            // Khi D_ListShortDevicePLC_0[200] khác 0, reset lại cờ
                            hasClearedError = false;
                        }
                        if (M611_Reset || M619_Reset)
                        {
                           this.ClearError();
                        }

                    }
                }

            });

        }
        private void UpdateUIData()
        {

            var converter = new BrushConverter();
            this.Dispatcher.Invoke(() =>
            {
              

                if(M_0_1000.Count >=1)
                {
                    if (M_0_1000[8])
                    {
                        if (lb1_status1.Text == "MACHINE READY..")
                        {
                            lb1_status1.Text = "MÁY SẴN SÁNG HOẠT ĐỘNG, ẤN NÚT START ĐỂ CHẠY AUTO..";
                        }
                        else
                        {
                            lb1_status1.Text = "MACHINE READY..";
                        }
                        lb1_status1.Background = Brushes.Lime;
                    }

                    else if (!M_0_1000[8] && !M_0_1000[16] && !M_0_1000[0] && !M_0_1000[28] && !M_0_1000[46]
                    && !M_0_1000[2])
                    {
                        if (lb1_status1.Text == "MACHINE NOT READY,PRESS INITIAL TO HOMING..")
                        {
                            lb1_status1.Text = "MÁY CHƯA VỀ GỐC,ẤN NÚT INITIAL ĐỂ VỀ GỐC MÁY..";
                        }
                        else
                        {
                            lb1_status1.Text = "MACHINE NOT READY,PRESS INITIAL TO HOMING..";
                        }

                        lb1_status1.Background = (Brush)converter.ConvertFromString("#FF4500");
                    }
                    //Auto
                    else if (M_0_1000[16])
                    {
                        lb1_status1.Text = "MACHINE  RUNING";
                        lb1_status1.Background = Brushes.Lime;
                    }

                    else if (M_0_1000[0])
                    {
                        if (lb1_status1.Text == "MACHINE HOMING..")
                        {
                            lb1_status1.Text = "MÁY ĐANG VỀ GỐC..";
                        }
                        else
                        {
                            lb1_status1.Text = "MACHINE HOMING..";
                        }
                        lb1_status1.Background = Brushes.Blue;
                    }
                    if (M_0_1000[46])
                    {
                        if (lb1_status1.Text == "SAFETY SENSOR IS DETECTED")
                        {
                            lb1_status1.Text = "CẢM BIẾN AN TOÀN ĐANG BỊ TÁC ĐỘNG";
                        }
                        else
                        {
                            lb1_status1.Text = "SAFETY SENSOR IS DETECTED";
                        }

                        lb1_status1.Background = (Brush)converter.ConvertFromString("#FF4500");
                    }

                    if (M_0_1000[28])
                    {
                        this.lb1_status1.Text = AlarmInfo.getMessage(lastAlarm);
                        lb1_status1.Background = (Brush)converter.ConvertFromString("#FF4500");
                    }

                    //CH2
                    if (M_0_1000[58])
                    {
                        if (lb1_status2.Text == "MACHINE READY..")
                        {
                            lb1_status2.Text = "MÁY SẴN SÁNG HOẠT ĐỘNG, ẤN NÚT START ĐỂ CHẠY AUTO..";
                        }
                        else
                        {
                            lb1_status2.Text = "MACHINE READY..";
                        }
                        lb1_status2.Background = Brushes.Lime;
                    }

                    else if (!M_0_1000[58] && !M_0_1000[66] && !M_0_1000[50] && !M_0_1000[78] && !M_0_1000[96]
                    && !M_0_1000[52])
                    {
                        if (lb1_status2.Text == "MACHINE NOT READY,PRESS INITIAL TO HOMING..")
                        {
                            lb1_status2.Text = "MÁY CHƯA VỀ GỐC,ẤN NÚT INITIAL ĐỂ VỀ GỐC MÁY..";
                        }
                        else
                        {
                            lb1_status2.Text = "MACHINE NOT READY,PRESS INITIAL TO HOMING..";
                        }
                        lb1_status2.Background = (Brush)converter.ConvertFromString("#FF4500");
                    }

                    //Auto
                    else if (M_0_1000[66])
                    {
                        lb1_status2.Text = "MACHINE  RUNING";
                        lb1_status2.Background = Brushes.Lime;
                    }

                    else if (M_0_1000[50])
                    {
                        if (lb1_status2.Text == "MACHINE HOMING..")
                        {
                            lb1_status2.Text = "MÁY ĐANG VỀ GỐC..";
                        }
                        else
                        {
                            lb1_status2.Text = "MACHINE HOMING..";
                        }
                        lb1_status2.Background = Brushes.Blue;
                    }

                    if (M_0_1000[96])
                    {
                        if (lb1_status2.Text == "SAFETY SENSOR IS DETECTED")
                        {
                            lb1_status2.Text = "CẢM BIẾN AN TOÀN ĐANG BỊ TÁC ĐỘNG";
                        }
                        else
                        {
                            lb1_status2.Text = "SAFETY SENSOR IS DETECTED";
                        }

                        lb1_status2.Background = (Brush)converter.ConvertFromString("#FF4500");
                    }
                    if (M_0_1000[78])
                    {
                        this.lb1_status2.Text = AlarmInfo.getMessage(lastAlarm);
                        lb1_status2.Background = (Brush)converter.ConvertFromString("#FF4500");
                    }
                }    
               
                if(M_1000_2000.Count >=1)
                {
                    #region bỏ check Mesage Robot PickUp
                    //Update Alarm Robot
                    if (!RB1Alarm)
                    {
                        if (M_1000_2000[868] && !M_1000_2000[872])
                        {
                            RB1Alarm = true;
                            if ((new WindowPopUp2()).DoConfirmYesNo("Error Drop PCB On Tool Robot Ch1 ." + Environment.NewLine + "Let select click !" + Environment.NewLine + "If you click button turn left Robot contiue running , if you click button turn right Stop Machine ", System.Windows.Window.GetWindow(this)))
                            {
                                UiManager.PLC.WriteBit(DeviceCode.M, 1840, true);
                                
                                RB1Alarm = false;
                            }
                            else
                                UiManager.PLC.WriteBit(DeviceCode.M, 30, true);
                           
                            RB1Alarm = false;
                        }
                        if (!M_1000_2000[868] && M_1000_2000[872])
                        {
                            RB1Alarm = true;
                            String mes = "Error Vacuum PCB On Tool Robot Ch1." + Environment.NewLine
                             + "Let select click ! " + Environment.NewLine
                             + "If you click button turn left Robot pick again , if you click button turn right Robot skip error vacuum ";
                            if ((new WindowPopUp()).DoConfirmYesNo(mes, System.Windows.Window.GetWindow(this)))
                            {
                                UiManager.PLC.WriteBit(DeviceCode.M, 1886, true);
                                //this.plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1886);
                                RB1Alarm = false;
                            }
                            else
                                UiManager.PLC.WriteBit(DeviceCode.M, 1887, true);
                            //this.plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1887);
                            RB1Alarm = false;
                        }
                        if (M_1000_2000[841])
                        {
                            RB1Alarm = true;
                            if ((new WindowPopUp3()).DoConfirmYesNo("Error Vacuum Panel On Tool Robot Ch1 ." + Environment.NewLine + "Let select click !" + Environment.NewLine + " If you click button turn left Robot pick again , if you click button turn right Stop Machine ", System.Windows.Window.GetWindow(this)))
                            {
                                UiManager.PLC.WriteBit(DeviceCode.M, 1869, true);
                                //this.plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1869);
                                RB1Alarm = false;
                            }
                            else
                                UiManager.PLC.WriteBit(DeviceCode.M, 1830, true);
                            //this.plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1830);
                            RB1Alarm = false; ;
                        }
                    }
                    if (!RB2Alarm)
                    {
                        if (M_1000_2000[968] && !M_1000_2000[972])
                        {
                            RB2Alarm = true;
                            if ((new WindowPopUp2()).DoConfirmYesNo("Error Drop PCB On Tool Robot Ch2 ." + Environment.NewLine + "Let select click !" + Environment.NewLine + "If you click button turn left Robot contiue running , if you click button turn right Stop Machine ", System.Windows.Window.GetWindow(this)))
                            {
                                UiManager.PLC.WriteBit(DeviceCode.M, 1940, true);
                                //this.plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1940);
                                RB2Alarm = false;
                            }
                            else
                                UiManager.PLC.WriteBit(DeviceCode.M, 80, true);
                            //this.plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 80);
                            RB2Alarm = false;
                        }
                        if (!M_1000_2000[968] && M_1000_2000[972])
                        {
                            RB2Alarm = true;
                            if ((new WindowPopUp()).DoConfirmYesNo("Error Vacuum PCB On Tool Robot Ch2 . " + Environment.NewLine + " Let select click ! " + Environment.NewLine + "If you click button turn left Robot pick again , if you click button turn right Robot skip error vacuum ", System.Windows.Window.GetWindow(this)))
                            {
                                UiManager.PLC.WriteBit(DeviceCode.M, 1992, true);
                                //this.plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1992);
                                RB2Alarm = false;
                            }
                            else
                                UiManager.PLC.WriteBit(DeviceCode.M, 1993, true);
                            //this.plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1993);
                            RB2Alarm = false;

                        }
                        if (M_1000_2000[941])
                        {
                            RB2Alarm = true;
                            if ((new WindowPopUp3()).DoConfirmYesNo("Error Vacuum Panel On Tool Robot Ch2 ." + Environment.NewLine + "Let select click !" + Environment.NewLine + " If you click button turn left Robot pick again , if you click button turn right Stop Machine ", System.Windows.Window.GetWindow(this)))
                            {
                                UiManager.PLC.WriteBit(DeviceCode.M, 1969, true);
                                //this.plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1969);
                                RB2Alarm = false;
                            }
                            else
                                UiManager.PLC.WriteBit(DeviceCode.M, 80, true);
                            //this.plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 80);
                            RB2Alarm = false;
                        }
                    }



                    //Display tabcontrol
                    #endregion
                }

                if (D_7000_7900.Count >=1)
                {
                    lblCycleTime1_CH1.Content = ((double)D_7000_7900[466] / 10).ToString();
                    lblCycleTime_CH2.Content = ((double)D_7000_7900[486] / 10).ToString();
                    lblLotCountOK_CH1.Content = ((double)D_7000_7900[426]).ToString();
                    lblLotCountOK_Ch2.Content = ((double)D_7000_7900[436]).ToString();
                    lblUPH_CH1.Content = ((double)D_7000_7900[542]).ToString();
                    lblUPH_CH2.Content = ((double)D_7000_7900[532]).ToString();


                    lblLotCountNG_CH1.Content = (((double)D_7000_7900[424]) - ((double)D_7000_7900[426])).ToString();
                    lblLotCountNG_CH2.Content = (((double)D_7000_7900[434]) - ((double)D_7000_7900[436])).ToString();

                    ////Time CH1
                    lblTotalCH1.Content = ((double)D_7000_7900[330]).ToString() + "h"
                    + ((double)D_7000_7900[328]).ToString() + "m"
                    + ((double)D_7000_7900[326]).ToString() + "s";

                    lblNormalCH1.Content = ((double)D_7000_7900[336]).ToString() + "h"
                    + ((double)D_7000_7900[334]).ToString() + "m"
                    + ((double)D_7000_7900[332]).ToString() + "s";

                    lblStopCH1.Content = ((double)D_7000_7900[342]).ToString() + "h"
                    + ((double)D_7000_7900[340]).ToString() + "m"
                    + ((double)D_7000_7900[338]).ToString() + "s";

                    lblAlarmCH1.Content = ((double)D_7000_7900[348]).ToString() + "h"
                    + ((double)D_7000_7900[346]).ToString() + "m"
                    + ((double)D_7000_7900[344]).ToString() + "s";

                    ////Time CH2
                    lblTotalCH2.Content = ((double)D_7000_7900[360]).ToString() + "h"
                    + ((double)D_7000_7900[358]).ToString() + "m"
                    + ((double)D_7000_7900[356]).ToString() + "s";

                    lblNormalCH2.Content = ((double)D_7000_7900[366]).ToString() + "h"
                    + ((double)D_7000_7900[364]).ToString() + "m"
                    + ((double)D_7000_7900[362]).ToString() + "s";

                    lblStopCH2.Content = ((double)D_7000_7900[372]).ToString() + "h"
                    + ((double)D_7000_7900[370]).ToString() + "m"
                    + ((double)D_7000_7900[368]).ToString() + "s";

                    lblAlarmCH2.Content = ((double)D_7000_7900[378]).ToString() + "h"
                    + ((double)D_7000_7900[376]).ToString() + "m"
                    + ((double)D_7000_7900[374]).ToString() + "s";

                }

            });
         
        }
      
        private void Clock_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (!UiManager.PLC.isOpen() || M_0_1000 == null)
                    return;
                var converter = new BrushConverter();
                this.Dispatcher.Invoke(() =>
                {
                   
                });

                // add 
                bool ccLinkMode = UiManager.appSettings.connection.SelectModeCOM;
                if (ccLinkMode)
                {
                    UiManager.PLC.WriteBit(DeviceCode.M, 508, true);
                    //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 508);
                }
                else
                {
                    UiManager.PLC.WriteBit(DeviceCode.M, 508, false);
                    //plcComm.RstBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 508);
                }
                // add
                UpdateUIData();
                if(M_0_1000.Count >=1)
                {
                    if (M_0_1000[16])
                    {
                        Qr1Manager.StartEvent();
                    }
                    else if (!M_0_1000[16])
                    {
                        Qr1Manager.StopEvent();
                    }
                    if (M_0_1000[66])
                    {
                        Qr1Manager.StartEventCH2();
                    }
                    else if (!M_0_1000[66])
                    {
                        Qr1Manager.StopEventCH2();
                    }
                }    
               
                this.Dispatcher.Invoke(() => {
                    if (this.lblLotNo.Content.ToString() == "")
                    {
                        repuireNewLot = true;
                    }
                    else
                    {
                        repuireNewLot = false;
                    }
                    //PLC Connect
                    if (!UiManager.PLC.isOpen())
                    {
                        if (lb1_status1.Background == Brushes.DarkGreen)
                        {
                            lb1_status1.Background = (Brush)converter.ConvertFromString("#FF4500");
                            lb1_status1.Text = "PLC DISCONNECT";
                        }
                        else
                        {
                            lb1_status1.Background = Brushes.DarkGreen;
                            lb1_status1.Text = "MẤT KẾT NỐI ĐẾN PLC";
                        }
                        if (lb1_status2.Background == Brushes.DarkGreen)
                        {
                            lb1_status2.Background = (Brush)converter.ConvertFromString("#FF4500");
                            lb1_status2.Text = "PLC DISCONNECT";
                        }
                        else
                        {
                            lb1_status2.Background = Brushes.DarkGreen;
                            lb1_status2.Text = "MẤT KẾT NỐI ĐẾN PLC";
                        }
                        Thread.Sleep(100);
                        return;
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Create("Clock_Elapsed" + ex.Message.ToString(), LogLevel.Error);
            }
        }
        #endregion

        #region Auto Thread
        Boolean AddeviceCam()
        {
            MyCamera.MV_CC_DEVICE_INFO device1 = UiManager.appSettings.connection.camera1.device;
            IntPtr buffer1 = Marshal.UnsafeAddrOfPinnedArrayElement(device1.SpecialInfo.stUsb3VInfo, 0);
            MyCamera.MV_USB3_DEVICE_INFO usbInfo1 = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer1, typeof(MyCamera.MV_USB3_DEVICE_INFO));
            MyCamera.MV_CC_DEVICE_INFO device2 = UiManager.appSettings.connection.camera2.device;
            IntPtr buffer2 = Marshal.UnsafeAddrOfPinnedArrayElement(device2.SpecialInfo.stUsb3VInfo, 0);
            MyCamera.MV_USB3_DEVICE_INFO usbInfo2 = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer2, typeof(MyCamera.MV_USB3_DEVICE_INFO));
            //hikCamera.DeviceListAcq();
            m_pDeviceList = UiManager.hikCamera.m_pDeviceList;
            for (int i = 0; i < m_pDeviceList.nDeviceNum; i++)
            {
                MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stUsb3VInfo, 0);
                MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                if (usbInfo.chSerialNumber == usbInfo1.chSerialNumber)
                {
                    device1 = device;
                    addLog("Finding Camera1 is OK");
                }

                else if (usbInfo.chSerialNumber == usbInfo2.chSerialNumber)
                {
                    device2 = device;
                    addLog("Finding Camera2 is OK");
                }
                else
                {
                    addLog(device.ToString());
                    addLog("Finding Camera is NG");
                    return false;
                }
            }
            InnitialCamera1(device1);
            InnitialCamera2(device2);
            return true;
        }

        private bool InnitialCamera1(MyCamera.MV_CC_DEVICE_INFO device1)
        {
            return true;
            //MyCamera.MV_CC_DEVICE_INFO device = UiManager.appSettings.connection.camera1.device;
            //int ret = myCam1.Open(device, HikCam.AquisMode.AcquisitionMode);
            //myCam1.SetExposeTime((int)UiManager.appSettings.connection.camera1.ExposeTime);
            //Thread.Sleep(2);
            //if (ret == MyCamera.MV_OK)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}

        }
        private bool InnitialCamera2(MyCamera.MV_CC_DEVICE_INFO device1)
        {
            //MyCamera.MV_CC_DEVICE_INFO device = UiManager.appSettings.connection.camera2.device;
            //int ret = myCam2.Open(device, HikCam.AquisMode.AcquisitionMode);

            //MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[indexCtr2], typeof(MyCamera.MV_CC_DEVICE_INFO));



            return true;
            //MyCamera.MV_CC_DEVICE_INFO device = UiManager.appSettings.connection.camera1.device;
            //int ret = myCam2.Open(device, HikCam.AquisMode.AcquisitionMode);
            //myCam2.SetExposeTime((int)UiManager.appSettings.connection.camera2.ExposeTime);
            //Thread.Sleep(2);
            //if (ret == MyCamera.MV_OK)
            //{
            //    return true;
            //}
            //else
            //{
            //    return false;
            //}
        }
        private void callThreadStartLoopCH1()
        {
            try
            {
                Thread startThread = new Thread(new ThreadStart(waitTriggerCH1));
                startThread.IsBackground = true;
                startThread.Start();
            }
            catch (Exception ex)
            {
                logger.Create("Start thread Auto loop Err : " + ex.ToString(), LogLevel.Error);
            }

        }
        private void callThreadStartLoopCH2()
        {
            try
            {
                Thread startThread = new Thread(new ThreadStart(waitTriggerCH2));
                startThread.IsBackground = true;
                startThread.Start();
            }
            catch (Exception ex)
            {
                logger.Create("Start thread Auto loop Err : " + ex.ToString(), LogLevel.Error);
            }

        }
        private void waitTriggerCH1()
        {
            while (true)
            {
                if (!isRunning)
                    return;

                //int remainCnt = this.lotStatus.InputCount - this.lotStatus.TotalCount;
                //if ((this.lotStatus.InputCount > 0) && (remainCnt == 0))
                //{
                //    //displayAlarm(AlarmInfo.LOT_END);
                //    AddError(31212);
                //    repuireNewLot = true;
                //    return;
                //}
                bool M500;
                UiManager.PLC.ReadBit(DeviceCode.M, 500, out M500);
                if (M500 && AutoCH1 == true) //Update Sau Bit Trigger
                {
                    addLog("Read Bit M500 = True");
                    UiManager.PLC.WriteBit(DeviceCode.M, 500, false);
                    addLog("Write Bit M500 = False");
                    TriggerCameraCH1();
                    return;
                }
                else if (ManualCH1 == true) //Update Sau Bit Trigger
                {
                    UiManager.PLC.WriteBit(DeviceCode.M, 500, false);
                    //plcComm.RstBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 500);
                    TriggerCameraCH1();
                    return;
                }
                Thread.Sleep(5);
            }
        }
        private void waitTriggerCH2()
        {
            while (true)
            {
                if (!isRunning)
                    return;
                //int remainCnt = this.lotStatus.InputCount - this.lotStatus.TotalCount;
                //if ((this.lotStatus.InputCount > 0) && (remainCnt == 0))
                //{
                //    //displayAlarm(AlarmInfo.LOT_END);
                //    AddError(31212);
                //    repuireNewLot = true;
                //    return;
                //}
                bool M520;
                UiManager.PLC.ReadBit(DeviceCode.M, 520, out M520);
                if (M520 && AutoCH2 == true) //Update Sau Bit Trigger
                {
                    addLog("Read Bit M520  = True");
                    UiManager.PLC.WriteBit(DeviceCode.M, 520, false);
                    addLog("Write Bit M520  = False");
                    
                    TriggerCameraCH2();
                    return;
                }
                else if (ManualCH2 == true) //Update Sau Bit Trigger
                {
                    UiManager.PLC.WriteBit(DeviceCode.M, 520, false);
                    
                    TriggerCameraCH2();
                    return;
                }
                Thread.Sleep(1);
            }
        }
        private void TriggerCameraCH1()
        {
            try
            {
                int CountOKJig = 0, CountNGJig = 0;
                OpenCvSharp.Mat src = UiManager.Cam1.CaptureImage();
                Thread.Sleep(10);
                if (src == null)
                {
                    src = UiManager.Cam1.CaptureImage();
                    Thread.Sleep(10);
                    if (src == null)
                    {
                        addLog("Image Camera is null");
                        //displayAlarm(30006);
                        AddError(30006);
                        StopEvent();
                        return;
                    }
                }
                src.SaveImage("temp1.bmp");
                Mat src1 = Cv2.ImRead("temp1.bmp", ImreadModes.Color);
                Stopwatch st = new Stopwatch();
                st.Start();
                vision1.Image1 = src1;
                List<bool> ret = vision1.visionCheck();
                st.Stop();
                this.Dispatcher.Invoke(() =>
                {
                    Img_Main_process_1.Source = vision1.Image1.ToWriteableBitmap(PixelFormats.Bgr24);
                    //Img_Main_process_2.Source = vision1.Image1.ToWriteableBitmap(PixelFormats.Bgr24);
                });
                if (ManualCH1 == false && AutoCH1 == true)
                {
                    if (!vision1.ImageLog(vision1.Image1, UiManager.appSettings.connection.image.CH1_path))
                        addLog("Không thể Log ảnh, Clear Log Data tại ổ đĩa C");
                }

                for (int i = 0; i < ret.Count; i++)
                {
                    if (ret[i])
                    {
                        UiManager.PLC.WriteBit(DeviceCode.M, 2800 + i, true);
                       
                        CountOKJig++;
                    }
                    else if (!ret[i])
                    {
                        UiManager.PLC.WriteBit(DeviceCode.M, 2800 + i, false);
                       
                        CountNGJig++;
                    }
                    UiManager.PLC.WriteBit(DeviceCode.M, 501, true);
                    
                    addLog("M501 Enter -> PLC");
                }
                if (AutoCH1 == true)
                {
                    //vision1.ImageLog(vision1.Image1, UiManager.appSettings.connection.image.okPath);
                    this.Dispatcher.Invoke(() => {
                        this.lbl_Ch1_TT.Content = st.Elapsed.TotalSeconds.ToString() + "(s)";
                        this.lbl_CountOKJig_Ch1.Content = CountOKJig.ToString();
                        this.lbl_CountNGJig_Ch1.Content = CountNGJig.ToString();
                    });
                }
                callThreadStartLoopCH1();
                return;
            }
            catch (Exception ex)
            {
                logger.Create("Trigger Camera CH1" +ex.Message.ToString(), LogLevel.Error);
            }
        }
        private void TriggerCameraCH2()
        {
            try
            {
                int CountOKJig = 0, CountNGJig = 0;
                OpenCvSharp.Mat src = UiManager.Cam2.CaptureImage();
                Thread.Sleep(10);
                if (src == null)
                {
                    src = UiManager.Cam2.CaptureImage();
                    Thread.Sleep(10);
                    if (src == null)
                    {
                        addLog("Image Camera is null");
                        //displayAlarm(30006);
                        AddError(30006);
                        StopEvent();
                        return;
                    }
                }
                src.SaveImage("temp2.bmp");
                Mat src1 = Cv2.ImRead("temp2.bmp", ImreadModes.Color);
                Stopwatch st = new Stopwatch();
                st.Start();
                vision2.Image1 = src1;
                List<bool> ret = vision2.visionCheck();
                st.Stop();
                this.Dispatcher.Invoke(() =>
                {
                    Img_Main_process_2.Source = vision2.Image1.ToWriteableBitmap(PixelFormats.Bgr24);
                    //Img_Main_process_1.Source = src1.ToWriteableBitmap(PixelFormats.Bgr24);
                });
                if (ManualCH2 == false && AutoCH2 == true)
                {
                    if (!vision2.ImageLog(vision2.Image1, UiManager.appSettings.connection.image.CH2_path))
                        addLog("Không thể Log ảnh, Clear Log Data tại ổ đĩa C");
                }

                for (int i = 0; i < ret.Count; i++)
                {
                    if (ret[i])
                    {
                        UiManager.PLC.WriteBit(DeviceCode.M, 2900 + i, true);
                        
                        CountOKJig++;
                    }
                    else if (!ret[i])
                    {
                        UiManager.PLC.WriteBit(DeviceCode.M, 2900 + i, false);
                        
                        CountNGJig++;
                    }
                    UiManager.PLC.WriteBit(DeviceCode.M, 521, true);
                    addLog("M521 Enter -> PLC");
                }
                if (AutoCH2 == true)
                {
                    //vision2.ImageLog(vision1.Image1, UiManager.appSettings.connection.image.okPath);
                    this.Dispatcher.Invoke(() => {
                        this.lbl_Ch2_TT.Content = st.Elapsed.TotalSeconds.ToString() + "(s)";
                        this.lbl_CountOKJig_Ch2.Content = CountOKJig.ToString();
                        this.lbl_CountNGJig_Ch2.Content = CountNGJig.ToString();
                    });
                }
                callThreadStartLoopCH2();
                return;
            }
            catch (Exception ex)
            {
                logger.Create("Trigger Camera  CH2" + ex.Message.ToString(), LogLevel.Error);
            }
        }
        #endregion



        #region ALARM 
        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (errorCodes.Count >= 1)
            {
                WndAlarm wndAlarm = new WndAlarm();
                wndAlarm.UpdateErrorList(errorCodes);
                wndAlarm.UpdateTimeList(timeerror);
                wndAlarm.Show();
            }

        }
        #region ALARM LOG
        private List<int> errorCodes;
        List<DateTime> timeerror = new List<DateTime>();
        private void InitializeErrorCodes()
        {
            errorCodes = new List<int>();
            timeerror = new List<DateTime>();

        }
        private void AddError(short errorCode)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (errorCode == 0)
                {
                    return;
                }

                if (errorCodes.Contains(errorCode))
                {
                    return;
                }

                else if (errorCodes.Count >= 31)
                {
                    errorCodes.Add(1);
                    return;
                }

                //Thêm lỗi vào SQL
                if (errorCode <= 100)
                {
                    string mes = AlarmInfo.getMessage(errorCode);
                    string sol = AlarmList.GetSolution(errorCode);
                    var alarm = new AlarmInfo(errorCode, mes, sol);
                    DbWrite.createAlarm(alarm);
                }
                else
                {
                    string mes = AlarmList.GetMes(errorCode);
                    string sol = AlarmList.GetSolution(errorCode);
                    var alarm = new AlarmInfo(errorCode, mes, sol);
                    DbWrite.createAlarm(alarm);
                }
                errorCodes.Add(errorCode);
                timeerror.Add(DateTime.Now);
                for (int i = 0; i < errorCodes.Count; i++)

                {
                    int code = errorCodes[i];
                    switch (i)
                    {
                        case 0: this.DisplayAlarm(1, code); break;
                        case 1: this.DisplayAlarm(2, code); break;
                        case 2: this.DisplayAlarm(3, code); break;
                        case 3: this.DisplayAlarm(4, code); break;
                        case 4: this.DisplayAlarm(5, code); break;
                        case 5: this.DisplayAlarm(6, code); break;
                        case 6: this.DisplayAlarm(7, code); break;
                        case 7: this.DisplayAlarm(8, code); break;
                        case 8: this.DisplayAlarm(9, code); break;
                        case 9: this.DisplayAlarm(10, code); break;
                        case 10: this.DisplayAlarm(11, code); break;
                        case 11: this.DisplayAlarm(12, code); break;
                        case 12: this.DisplayAlarm(13, code); break;
                        case 13: this.DisplayAlarm(14, code); break;
                        case 14: this.DisplayAlarm(15, code); break;
                        case 15: this.DisplayAlarm(16, code); break;
                        case 16: this.DisplayAlarm(17, code); break;
                        case 17: this.DisplayAlarm(18, code); break;
                        case 18: this.DisplayAlarm(19, code); break;
                        case 19: this.DisplayAlarm(20, code); break;
                        case 20: this.DisplayAlarm(21, code); break;
                        case 21: this.DisplayAlarm(22, code); break;
                        case 22: this.DisplayAlarm(23, code); break;
                        case 23: this.DisplayAlarm(24, code); break;
                        case 24: this.DisplayAlarm(25, code); break;
                        case 25: this.DisplayAlarm(26, code); break;
                        case 26: this.DisplayAlarm(27, code); break;
                        case 27: this.DisplayAlarm(28, code); break;
                        case 28: this.DisplayAlarm(29, code); break;
                        case 29: this.DisplayAlarm(30, code); break;

                        default:
                            break;
                    }
                }
                if (!isAlarmWindowOpen)
                {
                    this.ShowAlarm();
                }

                this.Number_Alarm();
            });


        }
        private void Number_Alarm()
        {
            int NumberAlarm = errorCodes.Count;
            this.CbShow.Content = NumberAlarm > 0 ? $"Errors : {NumberAlarm}" : "Not Show";
        }
        private void AlarmCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            isAlarmWindowOpen = true;
        }
        private void AlarmCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            isAlarmWindowOpen = false;
        }
        private bool isAlarmWindowOpen = false;
        private void ShowAlarm()
        {
            WndAlarm wndAlarm = new WndAlarm();
            wndAlarm.UpdateErrorList(errorCodes);
            wndAlarm.UpdateTimeList(timeerror);
            if (!isAlarmWindowOpen)
            {
                wndAlarm.Show();
            }
        }
        public void ClearError()
        {
            timeerror.Clear();
            errorCodes.Clear();
            Dispatcher.Invoke(new Action(() =>
            {
                for (int i = 1; i <= 30; i++)
                {
                    var label = (Label)FindName("lbMes" + i);
                    label.Content = "";
                    label.Background = Brushes.Black;
                }
            }));

            WndAlarm wndAlarm = new WndAlarm();
            wndAlarm.UpdateErrorList(errorCodes);
            wndAlarm.UpdateTimeList(timeerror);
            this.Number_Alarm();
        }
        private void DisplayAlarm(int index, int code)
        {
            try
            {
                if (code <= 100)
                {
                    Label label = (Label)FindName($"lbMes{index}");
                    if (label != null)
                    {
                        string mes = AlarmInfo.getMessage(code);
                        this.Dispatcher.Invoke(() =>
                        {
                            DateTime currentTime = DateTime.Now;
                            string currentTimeString = currentTime.ToString();
                            string newContent = currentTime.ToString() + " : " + mes;

                            label.Content = newContent;
                            label.Background = Brushes.Red;
                            //label.FontWeight = FontWeights.ExtraBold;
                            //label.Foreground = Brushes.Black;
                        });
                    }
                }
                else
                {
                    Label label = (Label)FindName($"lbMes{index}");
                    if (label != null)
                    {
                        string mes = AlarmList.GetMes(code);
                        this.Dispatcher.Invoke(() =>
                        {
                            string currentTime = DateTime.Now.ToString("HH:mm:ss");
                            string currentTimeString = currentTime.ToString();
                            string newContent = currentTime.ToString() + " : " + mes;

                            label.Content = newContent;
                            label.Background = Brushes.Red;
                            //label.FontWeight = FontWeights.ExtraBold;
                            //label.Foreground = Brushes.Black;
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Create($"DisplayAlarm PgMain: {ex.Message}", LogLevel.Error);
            }
        }


        #endregion
        #region Show Error Machine
        #region AddLog

        public Boolean uiLogEnable { get; set; } = true;
        private String lastLog = "";
        public ObservableCollection<logEntry> rxLog { get; set; } = new ObservableCollection<logEntry>();
        private bool autoScrollMode = true;
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            try
            {
                if (e.Source.GetType().Equals(typeof(ScrollViewer)))
                {
                    ScrollViewer sv = (ScrollViewer)e.Source;

                    if (sv != null)
                    {
                        if (e.ExtentHeightChange == 0)
                        {
                            if (sv.VerticalOffset == sv.ScrollableHeight)
                            {
                                autoScrollMode = true;
                            }
                            else
                            {
                                autoScrollMode = false;
                            }
                        }
                        if (autoScrollMode && e.ExtentHeightChange != 0)
                        {
                            sv.ScrollToVerticalOffset(sv.ExtentHeight);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Create(String.Format("Scroll View Of Add Log Error: " + ex.Message), LogLevel.Error);
            }
        }
        public ObservableCollection<logEntry> LogEntries { get; set; } = new ObservableCollection<logEntry>();
        private int gLogIndex;
        private void addLog(String log)
        {
            try
            {
                if (log != null && !log.Equals(lastLog))
                {
                    lastLog = log;
                    logger.Create("addLog:" + log, LogLevel.Information);
                    if (uiLogEnable)
                    {
                        string logTime = DateTime.Now.ToString("HH:mm:ss.fff");
                        this.Dispatcher.Invoke(() =>
                        {
                            this.txtLogs.Text += "\r\n" + logTime + ": " + log;
                            this.txtLogs.ScrollToEnd();

                        });

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Create("Add Log Error: " + ex.Message, LogLevel.Error);

            }

        }
        #endregion

    }
    public static class ActionClearAlarm
    {
        public static Action ClearErrorAction { get; set; }
    }
    public class logEntry : PropertyChangedBase
    {
        public int logIndex { get; set; }
        public String logTime { get; set; }
        public string logMessage { get; set; }
    }
    public class PropertyChangedBase : INotifyPropertyChanged
    {
        private static MyLogger Logger = new MyLogger("LogEntry");
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            try
            {
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    PropertyChangedEventHandler handler = PropertyChanged;
                    if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
                }));
            }
            catch (Exception ex)
            {
                Logger.Create(String.Format("Binding Property Of Logger Error: " + ex.Message), LogLevel.Error);
            }
        }
    }
    #endregion
    #endregion


}
