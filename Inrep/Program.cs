using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Inrep
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            Thread listener = new Thread(Program.listen);
            listener.Start();

            while (true) ;
            */

            InputListener il = new InputListener();

            il.Subscribe();

            Application.Run();
        }

        public static void listen()
        {
            InputListener il = new InputListener();

            il.Subscribe();
        }
    }
}