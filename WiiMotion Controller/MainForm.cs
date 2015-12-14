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
        public static MotionTypes motionState = MotionTypes.None;
        public static float motionTimeout = 0;

        public enum MotionTypes
        {
            Attack,
            Defend,
            None
        }

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
            Thread x = new Thread(EmulationThread);
            x.IsBackground = true;
            x.Start();
		}

        public void ProcessingThread()
        {
            while (true)
            {
                if (state != null)
                {
                    //ChangeLabel(state.AccelState.Values.ToString());
                    if (state.AccelState.Values.Z - Z > 1.5f)
                        motionState = MotionTypes.Attack;
                    else if (state.AccelState.Values.X < -.9f)
                        motionState = MotionTypes.Defend;
                    else
                        motionState = MotionTypes.None;
                    X = state.AccelState.Values.X;
                    Y = state.AccelState.Values.Y;
                    Z = state.AccelState.Values.Z;
                }
                Thread.Sleep(100);
            }
        }

        public void EmulationThread()
        {
            MotionTypes last = motionState;
            while (true)
            {
                ChangeLabel(motionTimeout.ToString());
                bool slept = false;
                switch (motionState)
                {
                    case MotionTypes.Defend:
                        if (motionTimeout == 0 || last == motionState)
                        {
                            ChangeMotionLabel("Defending");
                            last = motionState;
                            TSleep(100);
                            slept = true;
                        }
                        break;
                    case MotionTypes.Attack:
                        if (motionTimeout == 0 || last == motionState)
                        {
                            ChangeMotionLabel("Attacking");
                            last = motionState;
                            motionTimeout = .5f;
                            TSleep(100);
                            slept = true;
                        }
                        break;
                    default:
                        if (motionTimeout == 0 || last == motionState)
                        {
                            ChangeMotionLabel("None");
                            last = motionState;
                            TSleep(50);
                            slept = true;
                        }
                        break;
                }
                if (!slept)
                {
                    TSleep(100);
                }
            }
        }

        public void TSleep(int t)
        {
            Thread.Sleep(t);
            float f = motionTimeout - t / 1000f;
            motionTimeout = f > 0 ? f : 0;
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
