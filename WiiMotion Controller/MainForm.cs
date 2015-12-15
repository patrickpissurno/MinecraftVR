using System;
using System.Windows.Forms;
using WiimoteLib;
using System.Threading;
using System.Runtime.InteropServices;

namespace WiiMotionController
{
	public partial class MainForm : Form
	{
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);

        #region Keyboard Simulation
        const int INPUT_MOUSE = 0;
        const int INPUT_KEYBOARD = 1;
        const int INPUT_HARDWARE = 2;
        const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
        const uint KEYEVENTF_KEYDOWN = 0;
        const uint KEYEVENTF_KEYUP = 0x0002;
        const uint KEYEVENTF_UNICODE = 0x0004;
        const uint KEYEVENTF_SCANCODE = 0x0008;

        struct INPUT
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEYBDINPUT
        {
            /*Virtual Key code.  Must be from 1-254.  If the dwFlags member specifies KEYEVENTF_UNICODE, wVk must be 0.*/
            public ushort wVk;
            /*A hardware scan code for the key. If dwFlags specifies KEYEVENTF_UNICODE, wScan specifies a Unicode character which is to be sent to the foreground application.*/
            public ushort wScan;
            /*Specifies various aspects of a keystroke.  See the KEYEVENTF_ constants for more information.*/
            public uint dwFlags;
            /*The time stamp for the event, in milliseconds. If this parameter is zero, the system will provide its own time stamp.*/
            public uint time;
            /*An additional value associated with the keystroke. Use the GetMessageExtraInfo function to obtain this information.*/
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetMessageExtraInfo();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);
        public static void SendKeyAsInput(int key, KeyboardSimulationType t)
        {
            INPUT INPUT1 = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)key,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYDOWN,
                        dwExtraInfo = GetMessageExtraInfo(),
                    }
                }
            };

            INPUT INPUT2 = new INPUT
            {
                type = INPUT_KEYBOARD,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = (ushort)key,
                        wScan = 0,
                        dwFlags = KEYEVENTF_KEYUP,
                        dwExtraInfo = GetMessageExtraInfo(),
                    }
                }
            };

            switch (t)
            {
                case KeyboardSimulationType.Press:
                    SendInput(1, new INPUT[] { INPUT1 }, Marshal.SizeOf(typeof(INPUT)));
                    break;
                case KeyboardSimulationType.Release:
                    SendInput(1, new INPUT[] { INPUT2 }, Marshal.SizeOf(typeof(INPUT)));
                    break;
                case KeyboardSimulationType.PressRelease:
                    SendInput(1, new INPUT[] { INPUT1 }, Marshal.SizeOf(typeof(INPUT)));
                    Thread s = new Thread(() =>
                    {
                        Thread.Sleep(100);
                        SendInput(1, new INPUT[] { INPUT2 }, Marshal.SizeOf(typeof(INPUT)));
                    });
                    s.IsBackground = true;
                    s.Start();
                    break;
            }
        }
        #endregion

        public enum KeyboardSimulationType
        {
            Press,
            Release,
            PressRelease
        }

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
        private const uint MOUSEEVENTF_WHEEL = 0x0800;

        public void DoMouseEvent(uint EVENT)
        {
            //Call the imported function with the cursor's current position
            int X = Cursor.Position.X;
            int Y = Cursor.Position.Y;
            mouse_event(EVENT, (uint)X, (uint)Y, 0, 0);
        }
        public void DoMouseEvent(int wheelDelta)
        {
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)(wheelDelta*100), 0);
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
            bool left = X < -.15f;
            bool right = X > .15f;
            bool up = Y > .15f;
            bool down = Y < -.15f;
            if (up)
                SendKeyAsInput(87, KeyboardSimulationType.Press);
            else
            {
                SendKeyAsInput(87, KeyboardSimulationType.Release);
                if (down)
                    SendKeyAsInput(83, KeyboardSimulationType.Press);
            }
            if(!down)
                SendKeyAsInput(83, KeyboardSimulationType.Release);

            if (right)
                SendKeyAsInput(68, KeyboardSimulationType.Press);
            else
            {
                SendKeyAsInput(68, KeyboardSimulationType.Release);
                if (left)
                    SendKeyAsInput(65, KeyboardSimulationType.Press);
            }
            if (!left)
                SendKeyAsInput(65, KeyboardSimulationType.Release);
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
                    if (state.ButtonState.Left)
                        DoMouseEvent(1);
                    else if (state.ButtonState.Right)
                        DoMouseEvent(-1);
                    if (state.ButtonState.A)
                        SendKeyAsInput(32, KeyboardSimulationType.PressRelease);
                    ChangeLabel(state.AccelState.Values.ToString());
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
                            if (last != motionState && state.ButtonState.B)
                            {
                                ChangeMotionLabel("Placing block");
                                DoMouseEvent(MOUSEEVENTF_RIGHTDOWN|MOUSEEVENTF_RIGHTUP);
                            }
                            else
                            {
                                ChangeMotionLabel("Attacking");
                                DoMouseEvent(MOUSEEVENTF_LEFTDOWN);
                                last = motionState;
                                motionTimeout = .5f;
                            }
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
