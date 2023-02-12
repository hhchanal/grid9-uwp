using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using System.Threading.Tasks;
using Windows.UI;
using Windows.ApplicationModel.Core;
using Windows.Storage;

namespace App1
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(150, 225));
            ApplicationView.PreferredLaunchViewSize = new Size(210, 315);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            ApplicationViewTitleBar appTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            appTitleBar.ButtonBackgroundColor = Colors.Transparent;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            DataContext = BtnName.GetBtnName(0);
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("PATH"))
            {
                ApplicationData.Current.LocalSettings.Values["PATH"] = "";
            }
            if (!ApplicationData.Current.LocalSettings.Values.ContainsKey("CustomPATH"))
            {
                ApplicationData.Current.LocalSettings.Values["CustomPATH"] = false;
            }
            CodeReader();
            InitializeComponent();
        }

        public static class Global
        {
            public static String[] btn0 = { "一", "丨", "／", "˙", "乙", "口", "十", "〤", "⼌" };
            public static String[] btn1 = { "一", "⼯", "⽟" , "⼚", "⾬", "⾣", "⽯", "耳", "⻑" };
            public static String[] btn2 = { "⼁", "⼘", "⺖" , "⼩", "⺌", "⼱", "中", "⺺", "由" };
            public static String[] btn3 = { "⼃", "⼈", "个", "⻠", "千", "⺮", "⼣", "⼓", "⽩" };
            public static String[] btn4 = { "⼂", "⺀", "⺍", "⼇", "⼧", "⼴", "立", "⺭", "⽧" };
            public static String[] btn5 = { "マ", "ム", "", "⻖", "⼑", "乙", "⼔", "⼸", "ㄠ" };
            public static String[] btn6 = { "口", "⼫", "⽇", "四", "ㄖ", "興", "⽬", "⾜", "⽥" };
            public static String[] btn7 = { "⼗", "⺘", "⼟", "⻘", "木", "老", "⻀", "苦", "車" };
            public static String[] btn8 = { "十", "⼜", "⺨", "⼒", "⼥", "大", "春", "⼽", "七" };
            public static String[] btn9 = { "⼍", "⼌", "⺆", "⻛", "門", "馬", "⼕", "⼰", "山" };
            public static String[][] btns = {btn0, btn1, btn2, btn3, btn4, btn5, btn6, btn7, btn8, btn9};
            public static String[] lines;
            public static String[][] wordset;
            public static int step = 0;
            public static int page = 0;
            public static int pageMax = 0;
            public static int preBtn = 0;
            public static String text = "";
            public static String code = "";
            public static bool overlay = false;
        }

        public class BtnName
        {
            public String[] Btn { get; set; }
            public String Text { get; set; }
            public int Step { get; set; }
            public String Code { get; set; }
            public static BtnName GetBtnName(int n)
            {
                var btnn = new BtnName()
                {
                    Btn = Global.btns[n],
                    Text = Global.text,
                    Step = Global.step,
                    Code = Global.code,
                };
                return btnn;
            }
            public static BtnName GetWordChoice()
            {
                var btnn = new BtnName()
                {
                    Btn = Global.wordset[Global.page],
                    Text = Global.text,
                    Step = Global.step,
                    Code = Global.code,
                };
                return btnn;
            }
        }

        private async void EditPath()
        {
            Windows.Storage.Pickers.FileOpenPicker open = new Windows.Storage.Pickers.FileOpenPicker();
            open.FileTypeFilter.Add(".txt");
            StorageFile file = await open.PickSingleFileAsync();
            if (file != null)
            {
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(file);
                ApplicationData.Current.LocalSettings.Values["CustomPATH"] = true;
                ApplicationData.Current.LocalSettings.Values["PATH"] = file.Path;
                CodeReader();
                Global.text = "New Path: " + (String)ApplicationData.Current.LocalSettings.Values["PATH"];
                RefreshScreen();
                ClickEffect(Exit);
            }
        }

        private async void CodeReader()
        {
            if ((bool)ApplicationData.Current.LocalSettings.Values["CustomPATH"] == true)
            {
                try
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync((String)ApplicationData.Current.LocalSettings.Values["PATH"]);
                    IList<string> ReadLine = await FileIO.ReadLinesAsync(file, Windows.Storage.Streams.UnicodeEncoding.Utf8);
                    Global.lines = ReadLine.ToArray();
                }
                catch (Exception ex)
                {
                    Global.text = ex.Message;
                    try
                    {
                        Global.lines = File.ReadAllLines(@"cmb.txt", Encoding.UTF8);
                    }
                    catch (Exception e)
                    {
                        Global.text += e.Message ;
                    }
                    RefreshScreen();
                }
            }
            else
            {
                try
                {
                    Global.lines = File.ReadAllLines(@"cmb.txt", Encoding.UTF8);
                }
                catch (Exception e)
                {
                    Global.text = e.Message;
                }
            }
        }

        private void CodeChecking() //Transfer Code to String[] of word and to String[][9] to word set;
        {
            String strRegex = @"^" + Global.code;
            Regex reg = new Regex(strRegex);
            String line = "";
            foreach(String c in Global.lines)
            {
                if (reg.IsMatch(c))
                {
                    line = c;
                    break;
                }
            }
            char[] spearator = { ' ' };
            line = line.Split(spearator)[1];
            String[] eachword = line.Select(x => x.ToString()).ToArray();
            Global.pageMax = eachword.Length / 9;
            int counter = 0;
            List<String[]> lists = new List<string[]>();
            List<String> wordlist = new List<string>();
            for (int i = 0; i <= Global.pageMax; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (counter >= eachword.Length)
                    {
                        wordlist.Add("");
                    }
                    else
                    {
                        wordlist.Add(eachword[counter]);
                    }
                    counter++;
                }
                lists.Add(wordlist.ToArray());
                wordlist.Clear();
            }
            Global.wordset = lists.ToArray();
            DataContext = BtnName.GetWordChoice();
        }
        private void WordSelect(int n) 
        {
            Global.text += Global.wordset[Global.page][n-1];
            DataContext = BtnName.GetBtnName(0);
            Global.code = "";
            Global.page = 0;
            Global.step = 0;
        }
        private void ButtonAction(int n)
        {
            Global.preBtn = n;
            Global.step++;
            switch (Global.step)
            {
                case 1:
                    DataContext = BtnName.GetBtnName(n);
                    break;
                case 2:
                    DataContext = BtnName.GetBtnName(0);
                    break;
                case 3:
                    CodeChecking();
                    break;
                case 4:
                    WordSelect(n);
                    break;
            }
        }

        private void RefreshScreen()
        {
            if (Global.step == 0 || Global.step == 2)
            {
                DataContext = BtnName.GetBtnName(0);
            }
            else if (Global.step == 1)
            {
                DataContext = BtnName.GetBtnName(Global.preBtn);
            }
            else
            {
                DataContext = BtnName.GetWordChoice();
            }
        }

        private void Button_ClickE(object sender, RoutedEventArgs e)
        {
            Global.step = 0;
            Global.page = 0;
            Global.code = "";
            DataContext = BtnName.GetBtnName(0);
        }

        private void Button_Click_Next(object sender, RoutedEventArgs e)
        {
            if (Global.step == 0)
            {
                Global.code = "mmm";
                Global.step = 3;
                CodeChecking();
            }
            else if (Global.step == 2)
            {
                Global.code += "m";
                Global.step++;
                CodeChecking();
            }
            else if (Global.pageMax > 0 && Global.step == 3)
            {
                if (Global.page == Global.pageMax)
                {
                    Global.page = 0;
                }
                else
                {
                    Global.page++;
                }
                DataContext = BtnName.GetWordChoice();
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Global.code += "7";
            ButtonAction(1);
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Global.code += "8";
            ButtonAction(2);
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            Global.code += "9";
            ButtonAction(3);
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Global.code += "u";
            ButtonAction(4);
        }
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Global.code += "i";
            ButtonAction(5);
        }
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            Global.code += "o";
            ButtonAction(6);
        }
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            Global.code += "j";
            ButtonAction(7);
        }
        private void Button_Click_8(object sender, RoutedEventArgs e)
        {
            Global.code += "k";
            ButtonAction(8);
        }
        private void Button_Click_9(object sender, RoutedEventArgs e)
        {
            Global.code += "l";
            ButtonAction(9);
        }
        private async void CompactOverlayButton_Click()
        {
            ViewModePreferences compactOptions = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            compactOptions.CustomSize = new Size(150, 225);
            _ = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, compactOptions);
            Global.overlay = true;
        }

        private async void StandardModeButton_Click()
        {
            _ = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default);
            Global.overlay = false;
        }

        private async static void ClickEffect(Button btn)
        {
            var ap = new ButtonAutomationPeer(btn);
            var ip = ap.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            VisualStateManager.GoToState(btn, "Pressed", true);
            ip?.Invoke();
            await Task.Delay(150); // give the eye some time to see the press
            VisualStateManager.GoToState(btn, "Normal", true);
        }

        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs e)
        {
            if (e.VirtualKey == Windows.System.VirtualKey.Enter)
            {
                var dataPackage = new DataPackage();
                dataPackage.SetText(Global.text);
                Clipboard.SetContent(dataPackage);
                Global.text = "";
                RefreshScreen();
            }
            if (e.VirtualKey == Windows.System.VirtualKey.Back)
            {
                if (Global.text.Length > 0)
                {
                    Global.text = Global.text.Substring(0, Global.text.Length - 1);
                    RefreshScreen();
                }
            }
            if (sender.GetKeyState(Windows.System.VirtualKey.Shift) != Windows.UI.Core.CoreVirtualKeyStates.None)
            {
                if ((int)e.VirtualKey >= 65 && (int)e.VirtualKey <= 90)
                {
                    int charactor = (int)e.VirtualKey + (97 - 65);
                    Global.text += (char)charactor;
                }
                else if (e.VirtualKey == Windows.System.VirtualKey.Space)
                {
                    Global.text += " ";
                }
                RefreshScreen();

            }
            else if (sender.GetKeyState(Windows.System.VirtualKey.Control) == Windows.UI.Core.CoreVirtualKeyStates.Down)
            {
                if (e.VirtualKey == Windows.System.VirtualKey.E)
                {
                    EditPath();
                }
                if (e.VirtualKey == Windows.System.VirtualKey.R)
                {
                    ApplicationData.Current.LocalSettings.Values["CustomPATH"] = false;
                    Global.text = "ResetPath";
                    CodeReader();
                    RefreshScreen();
                }
            }
            else
            {
                if (e.VirtualKey == Windows.System.VirtualKey.M)
                {
                    ClickEffect(Next);
                }
                if (e.VirtualKey == Windows.System.VirtualKey.Number7)
                {
                    ClickEffect(Key7);
                }
                if (e.VirtualKey == Windows.System.VirtualKey.Number8)
                {
                    ClickEffect(Key8);
                }
                if (e.VirtualKey == Windows.System.VirtualKey.Number9)
                {
                    ClickEffect(Key9);
                }
                if (e.VirtualKey == Windows.System.VirtualKey.U)
                {
                    ClickEffect(KeyU);
                }
                if (e.VirtualKey == Windows.System.VirtualKey.I)
                {
                    ClickEffect(KeyI);
                }
                if (e.VirtualKey == Windows.System.VirtualKey.O)
                {
                    ClickEffect(KeyO);
                }
                if (e.VirtualKey == Windows.System.VirtualKey.J)
                {
                    ClickEffect(KeyJ);
                }
                if (e.VirtualKey == Windows.System.VirtualKey.K)
                {
                    ClickEffect(KeyK);
                }
                if (e.VirtualKey == Windows.System.VirtualKey.L)
                {
                    ClickEffect(KeyL);
                }
                if (e.VirtualKey == (Windows.System.VirtualKey)190)
                {
                    ClickEffect(Exit);
                }
                if (e.VirtualKey == Windows.System.VirtualKey.Number0)
                {
                    if (Global.overlay == false)
                    {
                        CompactOverlayButton_Click();
                    }
                    else
                    {
                        StandardModeButton_Click();
                    }
                }                
            }
        }

        /*
        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);


        private const int WsExNoactivate = 0x08000000;
        private const int GwlExstyle = -20;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        [DllImport("user32.dll")]
        public static extern int GetKeyboardState(ref byte pbKeyState);

        [DllImport("user32.dll")]
        public static extern int SetKeyboardState(ref byte lppbKeyState);

        const int WM_SETTEXT = 0X000C;
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int uMsg, int wParam, string lParam);
        */
    }
}