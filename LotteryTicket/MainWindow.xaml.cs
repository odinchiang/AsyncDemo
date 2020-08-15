using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using BicolorLottery.Common;

namespace LotteryTicket
{
    // 需求
    // 大樂透：投注號碼由 5 個紅色球號碼和 2 個藍色球號碼組成
    // 紅色球號碼從 01 ~ 35 中選擇，不重複
    // 藍色球號碼從 01 ~ 12 中選擇，不重複
    // 一共組合 7 個號碼
    // 要保證隨機性，獲取號碼時，可以隨機的延時

    // 需求拆解
    // 1. 彩球開始沒有順序 - 隨機
    // 2. 彩球在停止變化的時候 - 隨機
    // 3. 彩球在變化的間隔時間 - 隨機
    // 4. 開獎的時候，從用戶體驗來說，不卡頓介面

    // 技術選擇
    // 多執行緒：為何要使用多執行緒
    // 1. 不卡頓介面 - 非同步
    // 2. 每個執行緒相對獨立
    // 3. 執行緒是延遲啟動，不知道究竟是何時啟動
    // 4. 執行緒停止運行無法預估

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly object _lockObj = new object();
        private bool _isGo = true;
        private List<Task> _tasks = new List<Task>();

        #region Data

        // 紅球集合
        private List<string> _redNums = Enumerable.Range(1, 35).Select(x => x.ToString("00")).ToList();

        // 籃球集合
        private List<string> _blueNums = Enumerable.Range(1, 12).Select(x => x.ToString("00")).ToList();

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            BtnStart.IsEnabled = true;
            BtnStop.IsEnabled = false;
        }

        /// <summary>
        /// Start 按鈕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
            _isGo = true;
            _tasks = new List<Task>();
            BtnStart.Content = "正在開獎";
            BtnStart.IsEnabled = false;
            BtnStop.IsEnabled = true;
            TxtR1.Text = "00";
            TxtR2.Text = "00";
            TxtR3.Text = "00";
            TxtR4.Text = "00";
            TxtR5.Text = "00";
            TxtB1.Text = "00";
            TxtB2.Text = "00";

            foreach (UIElement child in GrdBall.Children)
            {
                if (!(child is TextBlock)) continue;

                // 紅色球變化的規則和藍色球不一樣，需要分開處理
                TextBlock tbk = (TextBlock)child;
                if (tbk.Name.Contains("B")) // 藍色球
                {
                    _tasks.Add(Task.Run(() =>
                    {
                        while (_isGo)
                        {
                            int index = new RandomHelper().GetRandomNumberDelay(0, _blueNums.Count);
                            string blueNum = _blueNums[index];

                            Dispatcher.Invoke(() =>
                            {
                                List<string> currentBlueNum = GetUiBlueNumbers();
                                if (!currentBlueNum.Contains(blueNum))
                                {
                                    lock (_lockObj)
                                    {
                                        if (!currentBlueNum.Contains(blueNum))
                                        {
                                            tbk.Text = blueNum;
                                        }
                                    }
                                }
                            });
                        }
                    }));
                }
                else // 紅色球
                {
                    _tasks.Add(Task.Run(() =>
                    {
                        while (_isGo)
                        {
                            int index = new RandomHelper().GetRandomNumberDelay(0, _redNums.Count);
                            string redNum = _redNums[index];
                            
                            Dispatcher.Invoke(() =>
                            {
                                List<string> currentRedNum = GetUiRedNumbers();
                                if (!currentRedNum.Contains(redNum))
                                {
                                    lock (_lockObj)
                                    {
                                        if (!currentRedNum.Contains(redNum))
                                        {
                                            tbk.Text = redNum;
                                        }
                                    }
                                }
                            });
                        }
                    }));
                }
            }
        }

        /// <summary>
        /// Stop 按鈕
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnStop_OnClick(object sender, RoutedEventArgs e)
        {
            BtnStart.Content = "開始";
            BtnStart.IsEnabled = true;
            BtnStop.IsEnabled = false;

            _isGo = false;

            Task.Run(() =>
            {
                Task.WaitAll(_tasks.ToArray());
            }).ContinueWith(x =>
            {
                Dispatcher.Invoke(ShowResult);
            });
        }

        /// <summary>
        /// 取得 UI 上所有紅色球的號碼
        /// </summary>
        /// <returns>紅色球號碼的集合</returns>
        private List<string> GetUiRedNumbers()
        {
            List<string> result = new List<string>();

            foreach (UIElement child in GrdBall.Children)
            {
                if (child is TextBlock textBlock)
                {
                    TextBlock tbk = textBlock;
                    if (tbk.Name.Contains("R"))
                    {
                        result.Add(tbk.Text);
                    }
                }
            }

            // 檢驗是否重複
            if (result.Count(x => x == "00") == 0 && 
                result.Distinct().Count() < 5)
            {
                Console.WriteLine($@"執行緒 Id：[{Thread.CurrentThread.ManagedThreadId}]");
                foreach (var item in result)
                {
                    Console.WriteLine($@"執行緒 Id：[{Thread.CurrentThread.ManagedThreadId}]，號碼：{item}");
                }
            }

            return result;
        }

        /// <summary>
        /// 取得 UI 上所有藍色球的號碼
        /// </summary>
        /// <returns>藍色球號碼的集合</returns>
        private List<string> GetUiBlueNumbers()
        {
            List<string> result = new List<string>();

            foreach (UIElement child in GrdBall.Children)
            {
                if (child is TextBlock textBlock)
                {
                    TextBlock tbk = textBlock;
                    if (tbk.Name.Contains("B"))
                    {
                        result.Add(tbk.Text);
                    }
                }
            }

            // 檢驗是否重複
            if (result.Count(x => x == "00") == 0 && 
                result.Distinct().Count() < 2)
            {
                Console.WriteLine($@"執行緒 Id：[{Thread.CurrentThread.ManagedThreadId}]");
                foreach (var item in result)
                {
                    Console.WriteLine($@"執行緒 Id：[{Thread.CurrentThread.ManagedThreadId}]，號碼：{item}");
                }
            }

            return result;
        }

        /// <summary>
        /// 輸出結果
        /// </summary>
        private void ShowResult()
        {
            MessageBox.Show(
                string.Format("本期雙色球結果為：{0} {1} {2} {3} {4} 籃球：{5} {6}",
                    TxtR1.Text,
                    TxtR2.Text,
                    TxtR3.Text,
                    TxtR4.Text,
                    TxtR5.Text,
                    TxtB1.Text,
                    TxtB2.Text));
        }
    }
}
