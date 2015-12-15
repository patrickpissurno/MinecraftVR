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

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        /*public void DoMouseClick()
        {
            //Call the imported function with the cursor's current position
            int X = Cursor.Position.X;
            int Y = Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, (uint)X, (uint)Y, 0, 0);
        }*/
        public void DoMouseEvent(uint EVENT)
        {
            //Call the imported function with the cursor's current position
            int X = Cursor.Position.X;
            int Y = Cursor.Position.Y;
            //mouse_event(EVENT, (uint)X, (uint)Y, 0, 0);
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

        public void MovementEmulation(float X, float Y)
        {
            bool left = X < -.3f;
            bool right = X > .3f;
            bool up = Y > .3f;
            bool down = Y < -.3f;
        }

        public void ProcessingThread()
        {
            while (true)
            {
                if (state != null)
                {
                    if (state.ExtensionType == ExtensionType.Nunchuk)
                    {
                        float jX = (state.NunchukState.RawJoystick.X - 127f) / 254f;
                        float jY = (state.NunchukState.RawJoystick.Y - 127f) / 254f;
                        jX = (float)Math.Round(jX * 10) / 10;
                        jY = (float)Math.Round(jY * 10) / 10;
                        MovementEmulation(jX, jY);
                    }
                    ChangeLabel(state.AccelState.ToString());
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
                //ChangeLabel(motionTimeout.ToString());
                bool slept = false;
                switch (motionState)
                {
                    case MotionTypes.Defend:
                        if (motionTimeout == 0 && last != motionState)
                        {
                            ChangeMotionLabel("Defending");
                            DoMouseEvent(MOUSEEVENTF_RIGHTDOWN);
                            last = motionState;
                            TSleep(100);
                            slept = true;
                        }
                        break;
                    case MotionTypes.Attack:
                        if (motionTimeout == 0 || last == motionState)
                        {
                            ChangeMotionLabel("Attacking");
                            DoMouseEvent(MOUSEEVENTF_LEFTDOWN);
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
                            switch (last)
                            {
                                case MotionTypes.Attack:
                                    DoMouseEvent(MOUSEEVENTF_LEFTUP);
                                    break;
                                case MotionTypes.Defend:
                                    DoMouseEvent(MOUSEEVENTF_RIGHTUP);
                                    break;
                            }
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
			wm.SetLEDs(true, false, false, false);
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
				wm.SetReportType(InputReport.ExtensionAccel, true);
			else
				wm.SetReportType(InputReport.ButtonsAccel, true);
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			wm.Disconnect();
            Application.Exit();
		}
	}
}
