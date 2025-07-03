using Mitsubishi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace ITM_Semiconductor
{
    /// <summary>
    /// Interaction logic for PgSystemMenu01.xaml
    /// </summary>
    public partial class PgSystemMenu01 : Page
    {
        private bool IsRunning;

        private Color Colo_ON_GR;
        private Color Colo_ON_RE;
        private Color Colo_OFF;


        private bool NGCH1 = false;
        private bool NGCH2 = false;
        private bool PassCH1 = false;
        private bool PassCH2 = false;
        public PgSystemMenu01()
        {
            InitializeComponent();
            this.btSetting1.Click += BtSetting1_Click;
            this.btSetting2.Click += BtSetting2_Click;
            this.Loaded += PgSystemMenu01_Loaded;
            this.Unloaded += PgSystemMenu01_Unloaded;

            //this.btEnablePickNGCH1.Click += BtEnablePickNGCH1_Click;
            //this.btDisablePickNGCH1.Click += BtDisablePickNGCH1_Click;
            //this.btEnablePickNGCH2.Click += BtEnablePickNGCH2_Click;
            //this.btDisablePickNGCH2.Click += BtDisablePickNGCH2_Click;

            //this.btEnableByPassCH1.Click += BtEnableByPassCH1_Click;
            //this.btDisableByPassCH1.Click += BtDisableByPassCH1_Click;

            //this.btEnableByPassCH2.Click += BtEnableByPassCH2_Click;
            //this.btDisableByPassCH2.Click += BtDisableByPassCH2_Click;


        }

        private void BtDisableByPassCH2_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
                return;
            UiManager.PLC.WriteBit(DeviceCode.M, 2265, false);
        }

        private void BtEnableByPassCH2_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
                return;
            UiManager.PLC.WriteBit(DeviceCode.M, 2265, true);
        }

        private void BtDisableByPassCH1_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
                return;
            UiManager.PLC.WriteBit(DeviceCode.M, 2005, false);
        }

        private void BtEnableByPassCH1_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
                return;
            UiManager.PLC.WriteBit(DeviceCode.M, 2005, true);
        }

        private void BtDisablePickNGCH2_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
                return;
            UiManager.PLC.WriteBit(DeviceCode.M, 4380, false);
        }

        private void BtDisablePickNGCH1_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
                return;
            UiManager.PLC.WriteBit(DeviceCode.M, 3680, false);
        }

        private void BtEnablePickNGCH2_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
                return;
            UiManager.PLC.WriteBit(DeviceCode.M, 4380, true);

        }

        private void BtEnablePickNGCH1_Click(object sender, RoutedEventArgs e)
        {
            if (!UiManager.PLC.isOpen())
                return;
            UiManager.PLC.WriteBit(DeviceCode.M, 3680, true);

        }

        private void PgSystemMenu01_Unloaded(object sender, RoutedEventArgs e)
        {
            this.IsRunning = false;
        }

        private void PgSystemMenu01_Loaded(object sender, RoutedEventArgs e)
        {
            Thread TheadReadPLC = new Thread(() => ReadPLC());
            TheadReadPLC.IsBackground = true;
            TheadReadPLC.Start();

            this.IsRunning = true;

            string hexColorOn1 = "#66FF66"; // Mã màu ON (XANH )
            string hexColorOn2 = "#FF0033"; // Mã màu OFF (ĐỎ)
            string hexColorOff = "#EEEEEE"; // Mã màu OFF (TRẮNG)
            Colo_ON_GR = (Color)ColorConverter.ConvertFromString(hexColorOn1);
            Colo_ON_RE = (Color)ColorConverter.ConvertFromString(hexColorOn2);
            Colo_OFF = (Color)ColorConverter.ConvertFromString(hexColorOff);
        }
        private void ReadPLC()
        {
            while (IsRunning)
            {
                if (UiManager.PLC.isOpen())
                {
                    UiManager.PLC.ReadBit(DeviceCode.M, 3680, out NGCH1);
                    UiManager.PLC.ReadBit(DeviceCode.M, 4380, out NGCH2);
                    UiManager.PLC.ReadBit(DeviceCode.M, 2005, out PassCH1);
                    UiManager.PLC.ReadBit(DeviceCode.M, 2265, out PassCH2);

                    this.UpdateUiPLC();
                }
                Thread.Sleep(10);
            }
        }
        private void UpdateUiPLC()
        {
            Dispatcher.Invoke(new Action(() =>
            {


                //this.btEnablePickNGCH1.Background = new SolidColorBrush(NGCH1 ? Colo_ON_GR : Colo_OFF);
                //this.btDisablePickNGCH1.Background = new SolidColorBrush(NGCH1 ? Colo_OFF : Colo_ON_RE);

                //this.btEnablePickNGCH2.Background = new SolidColorBrush(NGCH2 ? Colo_ON_GR : Colo_OFF);
                //this.btDisablePickNGCH2.Background = new SolidColorBrush(NGCH2 ? Colo_OFF : Colo_ON_RE);

                //this.btEnableByPassCH1.Background = new SolidColorBrush(PassCH1 ? Colo_ON_GR : Colo_OFF);
                //this.btDisableByPassCH1.Background = new SolidColorBrush(PassCH1 ? Colo_OFF : Colo_ON_RE);

                //this.btEnableByPassCH2.Background = new SolidColorBrush(PassCH2 ? Colo_ON_GR : Colo_OFF);
                //this.btDisableByPassCH2.Background = new SolidColorBrush(PassCH2 ? Colo_OFF : Colo_ON_RE);




            }));
        }
        private void BtSetting2_Click(object sender, RoutedEventArgs e)
        {
            UserManager.createUserLog(UserActions.PAGE_SYSTEM_MENU_SYSTEM_MACHINE);
            UiManager.Instance.SwitchPage(PAGE_ID.PAGE_SYSTEM_MENU_SYSTEM_MACHINE);
        }

        private void BtSetting1_Click(object sender, RoutedEventArgs e)
        {
            UserManager.createUserLog(UserActions.PAGE_SYSTEM_MENU);
            UiManager.Instance.SwitchPage(PAGE_ID.PAGE_SYSTEM_MENU);
        }
    }
}
