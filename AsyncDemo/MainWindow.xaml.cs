using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using ThreadState = System.Threading.ThreadState;

namespace AsyncDemo
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 同步呼叫
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SyncMethod_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"---------------------------------------------------------");
            Console.WriteLine(@"SyncMethod_OnClick Start");
            DoSomethingLong("同步呼叫1");
            DoSomethingLong("同步呼叫2");
            Console.WriteLine(@"SyncMethod_OnClick End");
        }

        /// <summary>
        /// 非同步呼叫 (Action)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AsyncMethod_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"---------------------------------------------------------");
            Console.WriteLine($@"AsyncMethod_OnClick Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

            Action<string> action = DoSomethingLong;
            //action.Invoke("同步呼叫1");
            //action("同步呼叫2");

            for (int i = 0; i < 5; i++)
            {
                action.BeginInvoke("非同步呼叫", null, null);
            }

            Console.WriteLine($@"AsyncMethod_OnClick End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
        }

        /// <summary>
        /// 非同步呼叫完成後再執行後續工作 (Action)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AsyncCallback_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"---------------------------------------------------------");
            Console.WriteLine($@"AsyncCallback_OnClick Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

            // 方法 1：非同步呼叫後要執行的工作
            AsyncCallback callback = ar =>
            {
                Console.WriteLine($@"BeginInvoke 的第三個參數：{ar.AsyncState}");
                Console.WriteLine($@"非同步呼叫執行結束 [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
            };

            // 非同步呼叫後要執行的工作 (Local function)
            //void Callback(IAsyncResult ar)
            //{
            //    Console.WriteLine($@"非同步呼叫執行結束 [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
            //}

            Action<string> action = DoSomethingLong;

            // 回調：將後續要做的工作通過回調函數傳到參數裡，子執行緒完成之後，會去調用這個回調委派。
            // 將自定義參數傳至回調函數中
            IAsyncResult asyncResult = action.BeginInvoke("非同步呼叫", callback, "傳入的參數");
            //action.BeginInvoke("非同步呼叫", Callback, null);

            // 方法 2：利用 BeginInvoike 回傳的 IAsyncResult.IsCompleted 屬性判斷是否完成，會鎖住 UI
            {
                //int i = 0;
                //while (!asyncResult.IsCompleted)
                //{
                //    Console.WriteLine(i < 9 ? $@"Loading...{++i * 10}%" : $@"Loading...99.99%");

                //    Thread.Sleep(500);
                //}
            }

            // 方法 3：WaitOne
            {
                //asyncResult.AsyncWaitHandle.WaitOne(); // 一直等待非同步呼叫完成
                //asyncResult.AsyncWaitHandle.WaitOne(-1); // 一直等待非同步呼叫完成
                asyncResult.AsyncWaitHandle.WaitOne(3000);
                Console.WriteLine($@"[[[[等待時間已到]]]]"); // 最多等待 3 秒，超時就不等待，但在等待期間會鎖住 UI
            }

            Console.WriteLine($@"AsyncCallback_OnClick End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
        }

        /// <summary>
        /// 非同步呼叫返回值 (Func)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AsyncReturnValue_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"---------------------------------------------------------");
            Console.WriteLine($@"AsyncReturnValue_OnClick Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

            //Func<int> func = () => DateTime.Now.Year;

            //IAsyncResult asyncResult = func.BeginInvoke(ar =>
            //{
            //    Thread.Sleep(2000);
            //    Console.WriteLine($@"callback function [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
            //}, null);

            //int result = func.EndInvoke(asyncResult); // 會等待非同步呼叫完成才執行，會鎖 UI
            //Console.WriteLine(result);

            Func<string, int> func1 = DoSomethingLongReturn;
            IAsyncResult asyncResult1 = func1.BeginInvoke("AsyncReturnValue_OnClick", ar =>
            {
                Thread.Sleep(2000);
                Console.WriteLine($@"callback function [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
            }, null);

            int result1 = func1.EndInvoke(asyncResult1); // 會等待非同步呼叫完成才執行，會鎖 UI
            Console.WriteLine(result1);

            Console.WriteLine($@"AsyncReturnValue_OnClick End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
        }

        /// <summary>
        /// Thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Thread_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"---------------------------------------------------------");
            Console.WriteLine($@"AsyncReturnValue_OnClick Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

            {
                // ParameterizedThreadStart：有參數無回傳值的委派
                //ParameterizedThreadStart threadStart = ar =>
                //{
                //    DoSomethingLong("Thread_OnClick1");
                //    Console.WriteLine($@"threadStart1 [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
                //};

                //Thread thread = new Thread(threadStart);
                //thread.Start(); // 開啟一個新執行緒
            }

            {
                // ThreadStart：無參數無回傳值的委派
                ThreadStart threadStart = () =>
                {
                    DoSomethingLong("Thread_OnClick2");
                    Console.WriteLine($@"threadStart2 [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
                };

                Thread thread = new Thread(threadStart);
                thread.Start(); // 開啟一個新執行緒

                //thread.Suspend(); // 暫停執行緒，已被棄用
                //thread.Resume(); // 恢復繼續執行緒，已被棄用，無法實時的暫停或恢復執行緒
                
                //thread.Abort(); // 終結執行緒
                //Thread.ResetAbort(); // 取消 Abort

                // 等待執行緒 方法 1 (會鎖 UI)
                //while (thread.ThreadState != ThreadState.Stopped)
                //{
                //    Thread.Sleep(200);
                //}

                // 等待執行緒 方法 2 (會鎖 UI)
                //thread.Join();
                //thread.Join(2000); // 可以限定等待時間

                // 設定優先級
                // 即使設定最高優先級，仍無法保證真的優先執行，只是增加優先的概率。
                thread.Priority = ThreadPriority.Highest;

                // 是否為後台執行緒 (預設為 false)
                // true：背景執行緒 => Process 結束，執行緒也跟著結束
                // false：前景執行緒 => Process 結束後，執行緒會繼續未完的工作直到結束
                thread.IsBackground = true;


            }

            Console.WriteLine($@"AsyncReturnValue_OnClick End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
        }

        private void DoSomethingLong(string text)
        {
            Console.WriteLine($@"==== [{text}] Do Something Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} ====");

            Thread.Sleep(5000);

            Console.WriteLine($@"**** [{text}] Do Something End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} ****");
        }

        private int DoSomethingLongReturn(string text)
        {
            Console.WriteLine($@"==== [{text}] Do Something Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} ====");

            Thread.Sleep(5000);

            Console.WriteLine($@"**** [{text}] Do Something End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} ****");

            return DateTime.Now.Year;
        }
    }
}
