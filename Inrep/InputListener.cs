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

        private bool ctrlEnabled = false;
        private bool altEnabled = false;

        private List<IntPtr> windows;
        private IntPtr currentProcess;
        private Process[] pcs;

        [DllImport("user32.dll")]
        static extern void PostMessage(IntPtr process, uint Msg, int x, int y);

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, UIntPtr dwExtraInfo);

        //[DllImport("user32.dll")]
        //public static extern Process GetCurrentProcess();

        //[DllImport("user32.dll")]
        //public static extern HANDLE getCurrentProcess();

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
            /*
            if (e.KeyValue == ctrlKey)
            {
                ctrlEnabled = false;
            }
            else if (e.KeyValue == altKey)
            {
                altEnabled = false;
            }
            else if (e.KeyValue == F1Key)
            {
                this.Unsubscribe();
                Application.Exit();
            }
            */
        }

        private void GlobalHookMouseDownExt(object sender, MouseEventExtArgs e)
        {
            
            Console.WriteLine("MouseDown: \t{0}; \t System Timestamp: \t{1}", e.Button, e.Timestamp);
            Console.WriteLine("posX: {0}, posY: {1}", e.X, e.Y);

            if (altEnabled == true)
            {
                Process current = Process.GetCurrentProcess();
                /*
                Console.WriteLine("The modifier was pressed and we should replicate this input on every window");
                foreach (Process process in pcs)
                {
                    if (process.Id != current.Id)
                    {
                        bool result = SetForegroundWindow(process.MainWindowHandle);
                        Console.WriteLine(result);
                        mouse_event(0x02, Convert.ToUInt16(e.X), Convert.ToUInt16(e.Y), 0, UIntPtr.Zero);
                        Thread.Sleep(100);
                        mouse_event(0x04, Convert.ToUInt16(e.X), Convert.ToUInt16(e.Y), 0, UIntPtr.Zero);
                    }
                }
                */
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
    }
}