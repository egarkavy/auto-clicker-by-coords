using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoClicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int MOUSEEVENTF_MOVE = 0x0001; /* mouse move */
        private const int MOUSEEVENTF_LEFTDOWN = 0x0002; /* left button down */
        private const int MOUSEEVENTF_LEFTUP = 0x0004; /* left button up */
        private const int MOUSEEVENTF_RIGHTDOWN = 0x0008; /* right button down */

        private POINT _savedCursorPosition;
        private CancellationTokenSource _cancellationTokenSource { get; set; }
        private Task _mauseJob;
        private Task _keyboardJob;

        private int ClickInterval { get; set; } = 1000;
        private int KeyPressInterval { get; set; } = 1000;

        private bool AllowDoJow { get; set; }

        [DllImport("user32")]
        public static extern int SetCursorPos(int x, int y);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;
            
            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention= CallingConvention.StdCall)]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public MainWindow()
        {
            InitializeComponent();
            KeyboardHook.CreateHook();
            KeyboardHook.KeyPressed += KeyboardHook_KeyPressed;
        }


        private void KeyboardHook_KeyPressed(object sender, KeyPressedEventArgs e)
        {
            if ((Keyboard.Modifiers == ModifierKeys.Control) && (e.KeyCode == 71))
            {
                ToggleWork();
            }

            if (!(Keyboard.Modifiers == ModifierKeys.Control) && (e.KeyCode == 71))
            {
                GetCursorPos(out _savedCursorPosition);
            }
        }

        private void ToggleWork()
        {
            if(_mauseJob != null)
            {
                _cancellationTokenSource.Cancel();
                _mauseJob = null;
                _keyboardJob = null;
            }
            else
            {
                _cancellationTokenSource = new CancellationTokenSource();

                AllowDoJow = true;

                _mauseJob = MauseJob(_cancellationTokenSource.Token);
                _keyboardJob = KeyboardJob(_cancellationTokenSource.Token);
            }
            
        }

        private async Task MauseJob(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && AllowDoJow)
            {
                SetCursorPos(_savedCursorPosition.X, _savedCursorPosition.Y);
                await Task.Delay(ClickInterval);
                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            }
        }

        private async Task KeyboardJob(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && AllowDoJow)
            {
                await Task.Delay(KeyPressInterval);
                SendKeys.SendWait("{F5}");
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ClickInterval = Int32.Parse(this.clickInterval.Text);
            KeyPressInterval = Int32.Parse(this.keyPressInterval.Text);
        }
    }
}
