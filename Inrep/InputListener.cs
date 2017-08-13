using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Gma.System.MouseKeyHook;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace Inrep
{
    class InputListener
    {
        private IKeyboardMouseEvents m_GlobalHook;

        private const int ctrlKey = 162;
        private const int altKey = 164;
        private const int F1Key = 112;
        private const uint WM_LBUTTONDOWN = 0x02;
        private const uint WM_LBUTTONUP = 0x04;

        private const int ALT = 0xA4;
        private const int EXTENDEDKEY = 0x1;
        private const int KEYUP = 0x2;
        private const int SHOW_MAXIMIZED = 3;

        private bool ctrlEnabled = false;
        private bool altEnabled = false;

        private List<IntPtr> windows;
        private Process current;
        private Process[] pcs;
        private Process[] allPcs;
        private Process vlc;

        [DllImport("user32.dll")]
        static extern void PostMessage(IntPtr process, uint Msg, int x, int y);
        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, UIntPtr dwExtraInfo);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("kernel32.dll")]
        static extern int GetProcessId(IntPtr handle);
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll", SetLastError = true)]
        static extern void SwitchToThisWindow(IntPtr hWnd, bool turnOn);
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        public static extern Process GetCurrentProcess();

        public InputListener()
        {   
            pcs = Process.GetProcessesByName("Dofus");
            Console.WriteLine("There is currently {0} process of Dofus running", pcs.Length);
        }

        public void Subscribe()
        {
            // Note: for the application hook, use the Hook.AppEvents() instead
            m_GlobalHook = Hook.GlobalEvents();

            m_GlobalHook.MouseDownExt += GlobalHookMouseDownExt;
            m_GlobalHook.KeyDown += GlobalHookKeyDown;
            m_GlobalHook.KeyUp += GlobalHookKeyUp;
        }

        private void GlobalHookKeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyValue == ctrlKey)
            {
                ctrlEnabled = true;
            }
            else if (e.KeyValue == altKey)
            {
                Console.WriteLine(pcs.Length);
                altEnabled = !altEnabled;
            }
            else
            {
                Console.WriteLine("KeyPress: \t{0}", e.KeyValue);
            }
        }

        private void GlobalHookKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == ctrlKey)
            {
                ctrlEnabled = false;
            }
            else if (e.KeyValue == altKey)
            {
                altEnabled = false;
            }
            if (e.KeyValue == F1Key)
            {
                this.Unsubscribe();
                Application.Exit();
            }
        }

        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            
            Console.WriteLine("MouseDown: \t{0}; \t System Timestamp: \t{1}", e.Button, e.Timestamp);
            Console.WriteLine("posX: {0}, posY: {1}", e.X, e.Y);

            if (altEnabled == true)
            {
                try
                {
                    setCurrentProcess();
                    Console.WriteLine(current.ToString());
                    Console.WriteLine("The modifier was pressed and we should replicate this input on every window");
                    Console.WriteLine("Current process id is : {0}", current.Id);
                    SetCursorPos(e.X, e.Y);
                    foreach (Process process in pcs)
                    {
                        Console.WriteLine("Targeted process id is : {0}", process.Id);
                        if (process.Id != current.Id)
                        {
                            SetForegroundWindow(process.MainWindowHandle);
                            Thread.Sleep(500);
                            /*
                            mouse_event(WM_LBUTTONDOWN, 0, 0, 0, UIntPtr.Zero);
                            Thread.Sleep(500);
                            mouse_event(WM_LBUTTONUP, 0, 0, 0, UIntPtr.Zero);
                            Thread.Sleep(500);
                            */
                        }
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine(exc);
                }
            }
        }

        public int MakeLParam(int LoWord, int HiWord)
        {
            return (int)((HiWord << 16) | (LoWord & 0xFFFF));
        }

        // uncommenting the following line will suppress the middle mouse button click
        // if (e.Buttons == MouseButtons.Middle) { e.Handled = true; }

        public void Unsubscribe()
        {
            m_GlobalHook.MouseDownExt -= GlobalHookMouseDownExt;
            m_GlobalHook.KeyDown -= GlobalHookKeyDown;

            //It is recommened to dispose it
            m_GlobalHook.Dispose();
        }

        public void setCurrentProcess()
        {
            IntPtr ptr = GetForegroundWindow();
            int processId = GetProcessId(ptr);
            current = Process.GetProcessById(processId);
            Console.WriteLine(current);
        }

        private void bringToFront(IntPtr mainWindowHandle)
        {
            // Guard: check if window already has focus.
            if (mainWindowHandle == GetForegroundWindow()) return;
            // Show window maximized.
            ShowWindow(mainWindowHandle, SHOW_MAXIMIZED);
            // Simulate an "ALT" key press.
            keybd_event((byte)ALT, 0x45, EXTENDEDKEY | 0, 0);
            // Simulate an "ALT" key release.
            keybd_event((byte)ALT, 0x45, EXTENDEDKEY | KEYUP, 0);
            // Show window in forground.
            SetForegroundWindow(mainWindowHandle);
        }
    }
}