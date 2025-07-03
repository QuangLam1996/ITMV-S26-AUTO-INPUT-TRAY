using KeyPad;
using System;
using Mitsubishi;
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
using DTO;
using System.Threading;

namespace ITM_Semiconductor
{
    /// <summary>
    /// Interaction logic for PgSystemMenu.xaml
    /// </summary>
    public partial class PgSystemMenu : Page
    {
        MyLogger logger = new MyLogger("PG_SYSTEM_MES");



        private System.Timers.Timer cycleTimer;

        private List<bool> M_0_1000 = new List<bool>();
        private List<bool> M_1000_2000 = new List<bool>();

        private List<int> D_7000_7900 = new List<int>();
        public PgSystemMenu()
        {
            InitializeComponent();
            this.btSetting2.Click += BtSetting2_Click;

            this.cycleTimer = new System.Timers.Timer(100);
            this.cycleTimer.AutoReset = true;
            this.cycleTimer.Elapsed += CycleTimer_Elapsed;

            this.Loaded += PgSystemMenu_Loaded;
            this.Unloaded += PgSystemMenu_Unloaded;

            this.btnVisionOn_ch1.Click += BtnVisionOn_ch1_Click;
            this.btnVisionOff_ch1.Click += BtnVisionOff_ch1_Click;
            this.btnVisionOn_ch2.Click += BtnVisionOn_ch2_Click;
            this.btnVisionOff_ch2.Click += BtnVisionOff_ch2_Click;
            this.btnLightCurtainOn_ch1.Click += BtnLightCurtainOn_ch1_Click;
            this.btnLightCurtainOff_ch1.Click += BtnLightCurtainOff_ch1_Click;

            this.btnLightCurtainOn_ch2.Click += BtnLightCurtainOn_ch2_Click;
            this.btnLightCurtainOff_ch2.Click += BtnLightCurtainOff_ch2_Click;
            this.btnDoorOn_ch1.Click += BtnDoorOn_ch1_Click;
            this.btnDoorOff_ch1.Click += BtnDoorOff_ch1_Click;
            this.btnDoorOn_ch2.Click += BtnDoorOn_ch2_Click;
            this.btnDoorOff_ch2.Click += BtnDoorOff_ch2_Click;

            //this.txtPickRetCh1.TextChanged += TxtPickRetCh1_TextChanged;
            //this.txtPickRetCh2.TextChanged += TxtPickRetCh2_TextChanged;

            //this.txtBlowDelayCH1.TextChanged += TxtBlowDelayCH1_TextChanged;
            //this.txtBlowDelayCH2.TextChanged += TxtBlowDelayCH2_TextChanged;

            //this.txtVacDelayCH2.TextChanged += TxtVacDelayCH2_TextChanged;
            //this.txtVacDelayCH1.TextChanged += TxtVacDelayCH1_TextChanged;

            this.btnSave.Click += BtnSave_Click;

            //this.btnCCLOn.Click += btnCCLOn_Click;
            //this.btnCCLOff.Click += btnCCLOff_Click;

        }

        private void BtSetting2_Click(object sender, RoutedEventArgs e)
        {
            UserManager.createUserLog(UserActions.PAGE_SYSTEM_MENU_SYSTEM_MACHINE);
            UiManager.Instance.SwitchPage(PAGE_ID.PAGE_SYSTEM_MENU_SYSTEM_MACHINE);
        }

        private void btnCCLOff_Click(object sender, RoutedEventArgs e)
        {
            var converter = new BrushConverter();
            //btnCCLOff.Background = Brushes.Cyan;
            //recCCLOff.Fill = Brushes.Yellow;
            //recCCLOn.Fill = Brushes.DarkRed;
            //btnCCLOn.Background = (Brush)converter.ConvertFromString("#D5D5D5");
            UiManager.appSettings.connection.SelectModeCOM = false;
            UiManager.SaveAppSetting();
        }
        private void btnCCLOn_Click(object sender, RoutedEventArgs e)
        {
            var converter = new BrushConverter();
            //btnCCLOn.Background = Brushes.Cyan;
            //recCCLOn.Fill = Brushes.Yellow;
            //recCCLOff.Fill = Brushes.DarkRed;
            //btnCCLOff.Background = (Brush)converter.ConvertFromString("#D5D5D5");
            UiManager.appSettings.connection.SelectModeCOM = true;
            UiManager.SaveAppSetting();

        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var converter = new BrushConverter();
            btnSave.Background = Brushes.Cyan;
            btnSaveName.Fill = Brushes.Yellow;
            //if (!plcComm.IsConnected())
            //{
            //    return;
            //}
            if (!(txtModelName.Text == "") && !(txtModelName.Text == UiManager.appSettings.connection.modelName))
            {
                UiManager.appSettings.connection.modelName = this.txtModelName.Text;
            }


            UiManager.SaveAppSetting();
            btnSaveName.Fill = Brushes.DarkRed;
            btnSave.Background = (Brush)converter.ConvertFromString("#D5D5D5");
        }
        private void TxtVacDelayCH1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            //if (txtVacDelayCH1.Text == "")
            //    return;
            //UiManager.PLC.WriteDoubleWord(DeviceCode.D, 7390, int.Parse(txtVacDelayCH1.Text));
            //plcComm.WriteDoubleWord(MCProtocol.DeviceCode.D, 7390, new int[] { int.Parse(txtVacDelayCH1.Text) });

        }
        private void TxtVacDelayCH2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            //if (txtVacDelayCH2.Text == "")
            //    return;
            //UiManager.PLC.WriteDoubleWord(DeviceCode.D, 7396, int.Parse(txtVacDelayCH2.Text));
            //plcComm.WriteDoubleWord(MCProtocol.DeviceCode.D, 7396, new int[] { int.Parse(txtVacDelayCH2.Text) });
        }
        private void TxtBlowDelayCH2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            //if (txtBlowDelayCH2.Text == "")
            //    return;
            //UiManager.PLC.WriteDoubleWord(DeviceCode.D, 7392, int.Parse(txtBlowDelayCH2.Text));
            //plcComm.WriteDoubleWord(MCProtocol.DeviceCode.D, 7392, new int[] { int.Parse(txtBlowDelayCH2.Text) });
        }
        private void TxtBlowDelayCH1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            //if (txtBlowDelayCH1.Text == "")
            //    return;
            //UiManager.PLC.WriteDoubleWord(DeviceCode.D, 7386, int.Parse(txtBlowDelayCH1.Text));
            //plcComm.WriteDoubleWord(MCProtocol.DeviceCode.D, 7386, new int[] { int.Parse(txtBlowDelayCH1.Text) });
        }
        private void TxtPickRetCh2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            //if (txtPickRetCh2.Text == "")
            //    return;
            //UiManager.PLC.WriteDoubleWord(DeviceCode.D, 7394, int.Parse(txtPickRetCh2.Text));
            //plcComm.WriteDoubleWord(MCProtocol.DeviceCode.D, 7394, new int[] { int.Parse(txtPickRetCh2.Text) });
        }
        private void TxtPickRetCh1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            //if (txtPickRetCh1.Text == "")
            //    return;
            //UiManager.PLC.WriteDoubleWord(DeviceCode.D, 7388, int.Parse(txtPickRetCh1.Text));
            //plcComm.WriteDoubleWord(MCProtocol.DeviceCode.D, 7388, new int[] { int.Parse(txtPickRetCh1.Text) });
        }
        private void BtnDoorOff_ch2_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            UiManager.PLC.WriteBit(DeviceCode.M, 1163, true);
            //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1163);
        }
        private void BtnDoorOn_ch2_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            UiManager.PLC.WriteBit(DeviceCode.M, 1163, false);
            //plcComm.RstBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1163);
        }
        private void BtnDoorOff_ch1_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            UiManager.PLC.WriteBit(DeviceCode.M, 1163, true);
            //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1163);
        }
        private void BtnDoorOn_ch1_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            UiManager.PLC.WriteBit(DeviceCode.M, 1163, false);
            //plcComm.RstBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1163);
        }
        private void BtnLightCurtainOff_ch2_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            UiManager.PLC.WriteBit(DeviceCode.M, 1162, true);
            //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1162);
        }
        private void BtnLightCurtainOn_ch2_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            UiManager.PLC.WriteBit(DeviceCode.M, 1162, false);
            //plcComm.RstBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1162);
        }
        private void BtnLightCurtainOff_ch1_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            UiManager.PLC.WriteBit(DeviceCode.M, 1161, true);
            //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1161);
        }
        private void BtnLightCurtainOn_ch1_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            UiManager.PLC.WriteBit(DeviceCode.M, 1161, false);
            //plcComm.RstBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1161);
        }
        private void BtnVisionOff_ch2_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            UiManager.PLC.WriteBit(DeviceCode.M, 1157, true);
            //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1157);
        }
        private void BtnVisionOn_ch2_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            UiManager.PLC.WriteBit(DeviceCode.M, 1157, false);
            //plcComm.RstBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1157);
        }
        private void BtnVisionOff_ch1_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            UiManager.PLC.WriteBit(DeviceCode.M, 1156, true);
            //plcComm.SetBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1156);
        }
        private void BtnVisionOn_ch1_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
            {
                return;
            }
            UiManager.PLC.WriteBit(DeviceCode.M, 1156, false);
            //plcComm.RstBit(MCProtocol.DevicePLC.M, MCProtocol.DeviceCode.M, 1156);
        }

        #region Load Page
        private void PgSystemMenu_Unloaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void PgSystemMenu_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtModelName.Text = UiManager.appSettings.connection.modelName;
            bool ccLinkMode = UiManager.appSettings.connection.SelectModeCOM;
            if (ccLinkMode)
            {
                var converter = new BrushConverter();
                //btnCCLOn.Background = Brushes.Cyan;
                //recCCLOn.Fill = Brushes.LightGreen;
                //recCCLOff.Fill = Brushes.OrangeRed;
                //btnCCLOff.Background = (Brush)converter.ConvertFromString("#D5D5D5");
            }
            else
            {
                var converter = new BrushConverter();
                //btnCCLOff.Background = Brushes.Cyan;
                //recCCLOff.Fill = Brushes.LightGreen;
                //recCCLOn.Fill = Brushes.OrangeRed;
                //btnCCLOn.Background = (Brush)converter.ConvertFromString("#D5D5D5");

            }
            Task tsk = new Task(() =>
            {
                cycleTimer.Start();
                this.UpdateUIOneShort();
            });
            tsk.Start();
            Thread.Sleep(100);
           
            //UpdateUIData();
        }
        private void UpdateUIOneShort()
        {
            if (!UiManager.PLC.isOpen())
                return;
            UiManager.PLC.ReadMultiDoubleWord(DeviceCode.D, 7000, 900, out D_7000_7900);
           
            this.Dispatcher.Invoke(() =>
            {
                //this.txtBlowDelayCH1.Text = D_7000_7900[386].ToString();
                //this.txtBlowDelayCH2.Text = D_7000_7900[392].ToString();
                //this.txtVacDelayCH1.Text = D_7000_7900[390].ToString();
                //this.txtVacDelayCH2.Text = D_7000_7900[396].ToString();
                //this.txtPickRetCh1.Text = D_7000_7900[388].ToString();
                //this.txtPickRetCh2.Text = D_7000_7900[394].ToString();
            });

        }

        private void CycleTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
                return;
            try
            {
                UiManager.PLC.ReadMultiBits(DeviceCode.M, 0, 1000, out M_0_1000);
                UiManager.PLC.ReadMultiBits(DeviceCode.M, 1000, 1000, out M_1000_2000);

               

                //M = plcComm.ReadMultiBits(MCProtocol.DeviceCode.M, 0, 460);
                UpdateUIData();
                return;
            }
            catch (Exception ex)
            {
                logger.Create("PLC Com Err" + ex.ToString(),LogLevel.Error);
                return;
            }
        }
        private void UpdateUIData()
        {
            if (!UiManager.PLC.isOpen())
                return;
            var converter = new BrushConverter();
            //Motor Control
            this.Dispatcher.Invoke(() =>
            {
                if (TabControl.SelectedIndex == 0)
                {
                    if (!M_1000_2000[156])
                    {
                        recVisionOn_ch1.Fill = Brushes.LightGreen;
                        recVisionOff_ch1.Fill = Brushes.LightGray;
                    }
                    else if (M_1000_2000[156])
                    {
                        recVisionOn_ch1.Fill = Brushes.LightGray;
                        recVisionOff_ch1.Fill = Brushes.LightGreen;
                    }
                }
                else if (TabControl.SelectedIndex == 1)
                {
                    if (!M_1000_2000[157])
                    {
                        recVisionOn_ch2.Fill = Brushes.LightGreen;
                        recVisionOff_ch2.Fill = Brushes.LightGray;
                    }
                    else if (M_1000_2000[157])
                    {
                        recVisionOn_ch2.Fill = Brushes.LightGray;
                        recVisionOff_ch2.Fill = Brushes.LightGreen;
                    }
                }
                if (!M_1000_2000[156])
                {
                    btnVisionOn_ch1.Background = Brushes.Cyan;
                    btnVisionOff_ch1.Background = (Brush)converter.ConvertFromString("#D5D5D5");
                }
                else if (M_1000_2000[156])
                {
                    btnVisionOff_ch1.Background = Brushes.Cyan;
                    btnVisionOn_ch1.Background = (Brush)converter.ConvertFromString("#D5D5D5");
                }
                if (!M_1000_2000[157])
                {
                    btnVisionOn_ch2.Background = Brushes.Cyan;
                    btnVisionOff_ch2.Background = (Brush)converter.ConvertFromString("#D5D5D5");
                }
                else if (M_1000_2000[157])
                {
                    btnVisionOff_ch2.Background = Brushes.Cyan;
                    btnVisionOn_ch2.Background = (Brush)converter.ConvertFromString("#D5D5D5");
                }
                

            });
        }
        #endregion
    }

}
