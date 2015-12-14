using System;
using System.Windows.Forms;
using WiimoteLib;
using System.Threading;
using System.Runtime.InteropServices;

namespace WiiMotionController
{
	public partial class MainForm : Form
	{
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

		Wiimote wm = new Wiimote();
        WiimoteState state = null;
        public static float X = 0;
        public static float Y = 0;
        public static float Z = 0;

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public void DoMouseClick()
        {
            //Call the imported function with the cursor's current position
            int X = Cursor.Position.X;
            int Y = Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)X, (uint)Y, 0, 0);
        }
        public void DoMouseRClick()
        {
            //Call the imported function with the cursor's current position
            int X = Cursor.Position.X;
            int Y = Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_RIGHTDOWN | MOUSEEVENTF_RIGHTUP, (uint)X, (uint)Y, 0, 0);
        }

		public MainForm()
		{
			InitializeComponent();
            Thread t = new Thread(ProcessingThread);
            t.IsBackground = true;
            t.Start();
		}

        public void ProcessingThread()
        {
            while (true)
            {
                if (state != null)
                {
                    ChangeLabel(state.AccelState.Values.ToString());
                    /*if (Math.Abs(state.AccelState.Values.Y - Y) > Math.Abs(state.AccelState.Values.X - X) &&
                        state.AccelState.Values.Y - Y < -2f)
                    {
                        //DoMouseClick();
                    }
                    else if (state.AccelState.Values.X - X > 2f)
                    {
                        //DoMouseRClick();
                    }*/
                    if (state.AccelState.Values.X < -.9f)
                        ChangeMotionLabel("Defending");
                    else if (state.AccelState.Values.Z - Z > 1.5f)
                        ChangeMotionLabel("Attacking");
                    else
                        ChangeMotionLabel("None");
                    X = state.AccelState.Values.X;
                    Y = state.AccelState.Values.Y;
                    Z = state.AccelState.Values.Z;
                }
                Thread.Sleep(100);
            }
        }

		private void Form1_Load(object sender, EventArgs e)
		{
			wm.WiimoteChanged += wm_WiimoteChanged;
			wm.WiimoteExtensionChanged += wm_WiimoteExtensionChanged;
			wm.Connect();
			wm.SetReportType(InputReport.ButtonsAccel, true);
			wm.SetLEDs(false, true, true, false);
		}

        private void ChangeLabel(string str)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => { ChangeLabel(str); }));
            }
            else
            {
                accelLabel.Text = str;
            }
        }
        private void ChangeMotionLabel(string str)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => { ChangeMotionLabel(str); }));
            }
            else
            {
                motionLabel.Text = "Motion: " + str;
            }
        }

		private void wm_WiimoteChanged(object sender, WiimoteChangedEventArgs args)
		{
            if (args.WiimoteState != null)
                state = args.WiimoteState;
		}

		private void wm_WiimoteExtensionChanged(object sender, WiimoteExtensionChangedEventArgs args)
		{
			if(args.Inserted)
				wm.SetReportType(InputReport.IRExtensionAccel, true);
			else
				wm.SetReportType(InputReport.IRAccel, true);
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			wm.Disconnect();
		}
	}
}
