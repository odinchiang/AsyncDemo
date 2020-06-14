using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using BicolorLottery.Common;

namespace BicolorLottery
{
    // 多執行緒雙色球
    // 需求
    // 雙色球：投注號碼由 6 個紅色球號碼和 1 個藍色球號碼組成
    // 紅色球號碼從 01 ~ 33 中選擇，不重複
    // 藍色球號碼從 01 ~ 16 中選擇
    // 一共組合 7 個號碼
    // 要保證隨機性，獲取號碼時，可以隨機的延時

    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly object _lockObj = new object();
        private bool _isGo = true;
        private List<Task> _tasks = new List<Task>();

        #region Data

        // 紅球集合
        private List<string> _redNums = Enumerable.Range(1, 33).Select(x => x.ToString("00")).ToList();

        // 籃球集合
        private List<string> _blueNums = Enumerable.Range(1, 16).Select(x => x.ToString("00")).ToList();

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
            TxtR6.Text = "00";
            TxtB.Text = "00";

            // 1. 獲取號碼
            // 2. 介面展示
            // 3. for 循環
            foreach (UIElement child in GrdBall.Children)
            {
                if (child is TextBlock textBlock)
                {
                    TextBlock tbk = textBlock;
                    if (tbk.Name.Contains("B")) // 藍色球
                    {
                        _tasks.Add(Task.Run(() =>
                        {
                            while (_isGo)
                            {
                                // 注意 Random.Next(min, max) 的最大值為 max - 1，不包含 max 值
                                int index = new RandomHelper().GetRandomNumberDelay(0, _blueNums.Count);

                                Dispatcher.Invoke(() =>
                                {
                                    tbk.Text = _blueNums[index];
                                });
                            }
                        }));
                    }
                    else if (tbk.Name.Contains("R")) // 紅色球
                    {
                        _tasks.Add(Task.Run(() =>
                        {
                            while (_isGo)
                            {
                                // 注意 Random.Next(min, max) 的最大值為 max - 1，不包含 max 值
                                int index = new RandomHelper().GetRandomNumberDelay(0, _redNums.Count);
                                string redNumber = _redNums[index];

                                Dispatcher.Invoke(() =>
                                {
                                    // 利用一個集合，紀錄出現的號碼，賦值的時候判斷是否重複
                                    List<string> uiRedNumbers = GetUiRedNumbers();

                                    // 雖然有一個集合用以判斷是否重複，但若有一個以上的執行緒幾乎同時判斷沒重複，
                                    // 但他們取到的值又相同時，就有可能重複(機率非常低，但還是有可能)，所以這裡
                                    // 要 lock 住，讓每一執行緒排隊進入判斷重複並賦值，這樣就能排除重複的可能。
                                    lock (_lockObj)
                                    {
                                        if (!uiRedNumbers.Contains(redNumber))
                                        {
                                            tbk.Text = redNumber;
                                        }
                                    }
                                });
                            }
                        }));
                    }
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

            // 執行緒不能從外部停止，只能自己停止自己
            _isGo = false;

            Task.Run(() =>
            {
                // 等待所有子執行緒完成，但會造成 Deadlock，所以外面
                // 再開一個執行緒讓這個新開的執行緒去等待所有之前未完成
                // 的子執行緒結束。如此可避免主執行緒與子執行緒互相等待
                // 造成 Deadlock 問題。
                Task.WaitAll(_tasks.ToArray());
            }).ContinueWith(x =>
            {
                Dispatcher.Invoke(ShowResult);
            });

            // 使用 Task.Factory
            //Task.Factory.ContinueWhenAll(_tasks.ToArray(), x =>
            //{
            //    Dispatcher.Invoke(ShowResult);
            //});
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
            if (result.Count(x => x == "00") == 0 && result.Distinct().Count() < 6)
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
                string.Format("本期雙色球結果為：{0} {1} {2} {3} {4} {5} 籃球：{6}",
                TxtR1.Text,
                TxtR2.Text,
                TxtR3.Text,
                TxtR4.Text,
                TxtR5.Text,
                TxtR6.Text,
                TxtB.Text));
        }
    }
}
