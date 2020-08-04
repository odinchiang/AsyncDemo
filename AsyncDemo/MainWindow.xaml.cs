using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
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
                // 在 .NET Core 中不支援 BeginInvoke，可改為用 Task.Run
                //action.BeginInvoke("非同步呼叫", null, null);

                // .Net Core 使用
                Task.Run (() => DoSomethingLong("非同步呼叫"));
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

                //thread.Abort(); // 終結執行緒，利用拋出 ThreadAbortException 異常停止執行緒
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

        /// <summary>
        /// Thread 回調函數
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThreadCallback_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"---------------------------------------------------------");
            Console.WriteLine($@"ThreadCallback_OnClick Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

            ThreadStart threadStart = () =>
            {
                DoSomethingLong("threadStart");
            };

            Action action = () =>
            {
                DoSomethingLong("callback function");
            };

            ThreadWithCallBack(threadStart, action);

            Console.WriteLine($@"ThreadCallback_OnClick End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
        }

        /// <summary>
        /// 執行完 Thread 執行緒後，才執行特定方法
        /// </summary>
        /// <param name="threadStart"></param>
        /// <param name="actionCallback"></param>
        private void ThreadWithCallBack(ThreadStart threadStart, Action actionCallback)
        {
            // 方法 1：等待執行緒結束在執行特定方法，會鎖 UI
            //Thread thread = new Thread(threadStart);
            //thread.Start();
            //thread.Join(); // 等待執行緒完成，會鎖 UI
            //actionCallback.Invoke(); // 執行 callback 函數

            // 方法 2：將要執行的 ThreadStart 跟回調函數包在同一個 ThreadStart 裡，按順序排放，再開執行緒執行
            ThreadStart threadStartOuter = new ThreadStart(() =>
            {
                threadStart.Invoke();
                actionCallback.Invoke();
            });

            Thread thread = new Thread(threadStartOuter);
            thread.Start();
        }

        /// <summary>
        /// Thread 獲取執行緒返回結果
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThreadReturnValue_OnClick(object sender, RoutedEventArgs e)
        {
            Func<int> func = () =>
            {
                Thread.Sleep(5000);
                return DateTime.Now.Year;
            };

            // 返回一個委派，在需要執行結果的時候，在執行這個委派
            Func<int> funcResult = ThreadWithReturn(func); // 不會鎖住 UI

            Console.WriteLine(@"中間過程");
            Console.WriteLine(@"中間過程");
            Console.WriteLine(@"中間過程");

            int iResult = funcResult.Invoke(); // 會鎖住 UI
            Console.WriteLine(iResult.ToString());
        }

        private Func<T> ThreadWithReturn<T>(Func<T> func)
        {
            T t = default(T);

            ThreadStart threadStart = new ThreadStart(() =>
            {
                t = func.Invoke();
            });

            Thread thread = new Thread(threadStart);
            thread.Start();

            return new Func<T>(() =>
            {
                thread.Join();
                return t;
            });
        }

        /// <summary>
        /// 在 Thread 中對執行緒的管理需要自己去做操作，在不斷的開啟及銷毀執行緒中，
        /// 存在很大的開銷，為了讓執行緒可以反覆使用，所以有 Thread Pool 的概念。
        /// Thread Pool 可以控制執行緒數量，防止濫用，節省資源。
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThreadPool_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"---------------------------------------------------------");
            Console.WriteLine($@"ThreadPool_OnClick Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

            {
                //ThreadPool.QueueUserWorkItem(x =>
                //{
                //    // 開啟一個執行緒
                //    DoSomethingLong("ThreadPool_OnClick");
                //});
            }

            {
                //ThreadPool.QueueUserWorkItem(x =>
                //{
                //    Console.WriteLine($@"第二個參數：{x}");
                //    DoSomethingLong("ThreadPool_OnClick");
                //}, "state");
            }

            {
                ThreadPool.GetMaxThreads(out int maxWorkerThreads, out int maxCompletionPortThreads);
                ThreadPool.GetMinThreads(out int minWorkerThreads, out int minCompletionPortThreads);
                Console.WriteLine($@"Max WorkerThreads: {maxWorkerThreads}，Max CompletionPortThreads: {maxCompletionPortThreads}");
                Console.WriteLine($@"Min WorkerThreads: {minWorkerThreads}，Min CompletionPortThreads: {minCompletionPortThreads}");

                // 設置執行緒數量是全域的，Thread Pool 是全域，Task、async/await 都是來自於 Thread Pool
                // 不建議隨便設置
                Console.WriteLine(@"設置執行緒數量後");
                ThreadPool.SetMaxThreads(100, 200); // 最大數量不能低於當前電腦的 CPU 核數
                ThreadPool.SetMinThreads(20, 40);

                ThreadPool.GetMaxThreads(out int maxWorkerThreads1, out int maxCompletionPortThreads1);
                ThreadPool.GetMinThreads(out int minWorkerThreads1, out int minCompletionPortThreads1);
                Console.WriteLine($@"Max WorkerThreads: {maxWorkerThreads1}，Max CompletionPortThreads: {maxCompletionPortThreads1}");
                Console.WriteLine($@"Min WorkerThreads: {minWorkerThreads1}，Min CompletionPortThreads: {minCompletionPortThreads1}");
            }

            Console.WriteLine($@"ThreadPool_OnClick End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
        }

        /// <summary>
        /// 執行緒等待
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThreadPoolWait_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"---------------------------------------------------------");
            Console.WriteLine($@"ThreadPoolWait_OnClick Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

            //  Thread Pool 中的開關式
            ManualResetEvent manualResetEvent = new ManualResetEvent(false); // 開關關閉

            ThreadPool.QueueUserWorkItem(x =>
            {
                DoSomethingLong("ThreadPoolWait_OnClick");
                manualResetEvent.Set(); // 發出訊號讓後面的 WaitOne 知道執行緒已經執行完畢，讓主執行緒繼續下去
            });

            manualResetEvent.WaitOne(); // 會等待執行緒，直到 Set 發出訊號，會鎖 UI

            Console.WriteLine($@"ThreadPoolWait_OnClick End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
        }

        /// <summary>
        /// Thread Pool 勿隨意設置執行緒數量，容易發生無法預期的錯誤
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThreadPoolSetWrong_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"---------------------------------------------------------");
            Console.WriteLine($@"ThreadPoolSetWrong_OnClick Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

            ThreadPool.SetMaxThreads(8, 8);
            ManualResetEvent manualResetEvent = new ManualResetEvent(false); // 開關關閉
            for (int i = 0; i < 10; i++)
            {
                int k = i;
                ThreadPool.QueueUserWorkItem(x =>
                {
                    Console.WriteLine($@"QueueUserWorkItem [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

                    if (k == 9) // 執行緒最大數量被設定為 8 個，所以一直無法進去進行 Set，導致死鎖
                        manualResetEvent.Set();
                    else
                        manualResetEvent.WaitOne();
                });
            }

            if (manualResetEvent.WaitOne())
                Console.WriteLine(@"執行成功！");

            Console.WriteLine($@"ThreadPool_OnClick End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
        }

        /// <summary>
        /// Task 裡的執行緒是來自於 Thread Pool
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskMethod_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"---------------------------------------------------------");
            Console.WriteLine($@"TaskMethod_OnClick Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

            {
                Task task = new Task(() =>
                {
                    DoSomethingLong("TaskMethod_OnClick1");
                });

                task.Start(); // 開啟一個新的執行緒
            }

            {
                Task.Run(() =>
                {
                    DoSomethingLong("TaskMethod_OnClick2");
                });
            }

            {
                //TaskFactory taskFactory = new TaskFactory();
                TaskFactory taskFactory = Task.Factory;
                taskFactory.StartNew(() =>
                {
                    DoSomethingLong("TaskMethod_OnClick3");
                });
            }

            {
                // 延遲 2 秒後執行後續的動作
                Task task = Task.Delay(2000).ContinueWith(x =>
                {
                    DoSomethingLong("TaskMethod_OnClick4");
                    Thread.Sleep(2000);
                    Console.WriteLine(@"回調已完成");
                });
            }

            Console.WriteLine($@"TaskMethod_OnClick End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
        }

        private void TaskWait_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"---------------------------------------------------------");
            Console.WriteLine($@"TaskWait_OnClick Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

            // 多執行緒使用的前提：可以並發執行的時候就可以使用多執行緒
            Console.WriteLine(@"老師開始講課！");
            Teach("Lesson 1");
            Teach("Lesson 2");
            Teach("Lesson 3");
            Teach("Lesson 4");
            Teach("Lesson 5");
            Console.WriteLine(@"課程結束，開始實戰專案練習！實戰專案內容較複雜，需要同學合作完成！");

            List<Task> tasks = new List<Task>();

            TaskFactory taskFactory = new TaskFactory();
            //tasks.Add(taskFactory.StartNew(() => Code("Mark", "系統管理")));
            //tasks.Add(taskFactory.StartNew(() => Code("John", "部門管理")));
            //tasks.Add(taskFactory.StartNew(() => Code("Mary", "客戶管理")));
            //tasks.Add(taskFactory.StartNew(() => Code("Jason", "介面管理")));
            //tasks.Add(taskFactory.StartNew(() => Code("Andy", "API")));

            // StartNew 的第二個參數提供回傳值
            tasks.Add(taskFactory.StartNew(o => Code("Mark", "系統管理"), "Mark"));
            tasks.Add(taskFactory.StartNew(o => Code("John", "部門管理"), "John"));
            tasks.Add(taskFactory.StartNew(o => Code("Mary", "客戶管理"), "Mary"));
            tasks.Add(taskFactory.StartNew(o => Code("Jason", "介面管理"), "Jason"));
            tasks.Add(taskFactory.StartNew(o => Code("Andy", "API"), "Andy"));

            // WaitAny 及 WaitAll 會 block 主執行緒
            // 若不 block 主執行緒，可改用 ContinueWhenAny 及 ContinueWhenAll 進行回調

            taskFactory.ContinueWhenAny(tasks.ToArray(), x =>
            {
                // 只要有一個執行緒完成即進入此回掉函數，不會 block 主執行緒，使用的可能是新執行緒，也有可能
                // 是剛完成任務的執行緒。
                // 利用 AsyncState 取得傳入的值
                Console.WriteLine($@"ContinueWhenAny {x.AsyncState} 獲取一個獎勵！ [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
            });

            // 要限制這個在"模組均已完成"訊息之前出現，則須將此也放入 task list 中
            //taskFactory.ContinueWhenAll(tasks.ToArray(), t =>
            //{
            //    Console.WriteLine($@"ContinueWhenAll 所有人任務開發完畢，慶祝一下！ [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
            //});
            tasks.Add(taskFactory.ContinueWhenAll(tasks.ToArray(), t =>
            {
                Console.WriteLine($@"ContinueWhenAll 所有人任務開發完畢，慶祝一下！ [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
            }));

            Task.WaitAny(tasks.ToArray()); // 有任一個執行緒完成即繼續往下執行
            Console.WriteLine($@"老師開始準備部署環境！ [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

            Task.WaitAll(tasks.ToArray()); // 需等待所有執行緒完成才會往下執行，會 block 主執行緒
            Console.WriteLine($@"模組均已完成，老師評論！ [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

            Console.WriteLine($@"TaskWait_OnClick End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
        }

        /// <summary>
        /// Task 返回值
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskReturnValue_OnClick(object sender, RoutedEventArgs e)
        {
            {
                // 會鎖住 UI
                Task<int> result = Task.Run<int>(() =>
                {
                    Thread.Sleep(5000);
                    return DateTime.Now.Year;
                });
                int iResult = result.Result;
                Console.WriteLine(iResult);
            }

            {
                // 不會鎖住 UI
                Task.Run<int>(() =>
                {
                    Thread.Sleep(5000);
                    return DateTime.Now.Year;
                }).ContinueWith(intT =>
                {
                    int i = intT.Result;
                    Console.WriteLine(i);
                });
            }
        }

        /// <summary>
        /// Parallel 對 Task 進行進一步的封裝
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ParallelMethod_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"---------------------------------------------------------");
            Console.WriteLine($@"ParallelMethod_OnClick Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

            {
                // Parallel 主要可以控制執行緒數量
                // Parallel 並發執行委派，開啟新執行緒，主執行緒也參與計算，UI 會鎖住
                // Task WaitAll + 主執行緒
                //Parallel.Invoke(() => DoSomethingLong("Parallel.Invoke_1"),
                //    () => DoSomethingLong("Parallel.Invoke_2"),
                //    () => DoSomethingLong("Parallel.Invoke_3"));
            }

            {
                // 按順序執行，鎖 UI
                // 會同時開啟 10 個執行緒
                Parallel.For(0, 10, t => DoSomethingLong($"Parallel.For ({t})"));

                //設定 ParallelOptions 的 MaxDegreeOfParallelism，會限制執行緒的數量
                //ParallelOptions parallelOptions = new ParallelOptions()
                //{
                //    MaxDegreeOfParallelism = 3 // 控制執行緒的最大數量
                //};
                //Parallel.For(0, 10, parallelOptions, t => DoSomethingLong($"Parallel.For ({t})"));
            }

            {
                // 按順序執行，鎖 UI
                //Parallel.ForEach(new int[] {12, 13, 14, 15},
                //    t => DoSomethingLong($"Parallel.ForEach ({t})"));
            }

            {
                // 控制執行緒數量
                //ParallelOptions parallelOptions = new ParallelOptions()
                //{
                //    MaxDegreeOfParallelism = 3 // 控制執行緒的最大數量
                //};
                //Parallel.ForEach(new int[] { 12, 13, 14, 15, 16, 17 }, parallelOptions,
                //    t => DoSomethingLong($"Parallel.ForEach ({t})"));
            }

            {
                // 若不希望主執行緒也參與計算，則另外開一個執行緒將目前的任務包進去
                //Task.Run(() =>
                //{
                //    Parallel.For(0, 10, t => DoSomethingLong($"Parallel.For ({t})"));
                //});
            }


            Console.WriteLine($@"ParallelMethod_OnClick End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
        }

        /// <summary>
        /// 多執行緒的異常處理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThreadException_OnClick(object sender, RoutedEventArgs e)
        {
            // 多執行緒中如果某一個執行緒異常，就會終止當前的執行緒；對其他的執行緒沒有影響。
            {
                //Thread thread = new Thread();
                //thread.Abort(); // Abort 會拋出一個異常，讓執行緒中止
            }

            {
                try
                {
                    List<Task> taskList = new List<Task>();
                    for (int i = 0; i < 20; i++)
                    {
                        string name = $"ThreadException_OnClick_{i}";
                        int k = i;
                        taskList.Add(Task.Run(() =>
                        {
                            if (k == 5)
                            {
                                throw new Exception($"{name} Exception!");
                            }
                            else if (k == 6)
                            {
                                throw new Exception($"{name} Exception!");
                            }
                            else if (k == 10)
                            {
                                throw new Exception($"{name} Exception!");
                            }

                            Console.WriteLine($@"{name} ok!");
                        }));
                    }

                    // 只有 WaitAll 之後才能捕捉到所有的異常信息
                    // 實際開發中是不允許在子執行緒中出現異常的
                    Task.WaitAll(taskList.ToArray());
                }
                catch (AggregateException aex)
                {
                    foreach (var ex in aex.InnerExceptions)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// 取消執行緒
        /// 若有一個執行緒出現異常，需要停止其他的執行緒
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelThread_OnClick(object sender, RoutedEventArgs e)
        {
            // 在 Task 中開啟的執行緒無法從外部取消，只能自己停止自己

            // 執行緒安全
            CancellationTokenSource cts = new CancellationTokenSource();

            // CancellationTokenSource 有一個狀態值 IsCancellationRequested，預設值為 false
            // 提供一個 Cancel 方法改變 IsCancellationRequested 狀態，false 改為 true
            // 且只能改變一次，改為 true 之後就不能再被修改，避免其他的操作去改變它

            // 1. 創建 CancellationTokenSource 變數
            // 2. try-catch 捕捉異常
            // 3. 在開啟的執行緒中判斷業務邏輯
            // 4. 若子執行緒有異常發生時，就要停止其他的子執行緒，讓未開啟的執行緒不開啟執行，則傳遞 Token 到 Task.Run中

            try
            {
                List<Task> taskList = new List<Task>();
                for (int i = 0; i < 50; i++)
                {
                    string name = $"ThreadException_OnClick_{i}";
                    int k = i;

                    Thread.Sleep(new Random().Next(50, 100)); // 休息 50 ~ 100 ms

                    taskList.Add(Task.Run(() =>
                    {
                        try
                        {
                            if (!cts.IsCancellationRequested)
                            {
                                Console.WriteLine($@"{name} [{Thread.CurrentThread.ManagedThreadId:00}] Start");
                            }

                            if (k == 5)
                            {
                                throw new Exception($"{name} [{Thread.CurrentThread.ManagedThreadId:00}] Exception!");
                            }
                            else if (k == 6)
                            {
                                throw new Exception($"{name} [{Thread.CurrentThread.ManagedThreadId:00}] Exception!");
                            }

                            if (!cts.IsCancellationRequested)
                            {
                                Console.WriteLine($@"{name} [{Thread.CurrentThread.ManagedThreadId:00}] Finished!");
                            }
                            else
                            {
                                Console.WriteLine($@"{name} [{Thread.CurrentThread.ManagedThreadId:00}] Cancel!");
                            }
                        }
                        catch (Exception ex)
                        {
                            cts.Cancel(); // 可以根據自己的業務需求直接 Cancel
                            Console.WriteLine(ex.Message);
                        }

                        // 若有異常出現，要讓未開啟的執行緒直接停止不開啟，要用到 Task.Run 的 第二個參數
                        // Task.Run 的多載第二個參數 CancellationToken
                        // cts.Token 會引發一個 ThrowIfCancellationRequested 異常，並取消未啟動的執行緒
                        // 若不加第二個參數，每個執行緒都會開啟執行，在子執行緒中必須自行判斷 IsCancellationRequested 狀態
                    }, cts.Token));
                }

                Task.WaitAll(taskList.ToArray());
            }
            catch (AggregateException aex)
            {
                foreach (var ex in aex.InnerExceptions)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// 執行緒的臨時變數
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TemporaryVariable_OnClick(object sender, RoutedEventArgs e)
        {
            Console.WriteLine(@"---------------------------------------------------------");
            Console.WriteLine($@"TemporaryVariable_OnClick Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

            //Console.WriteLine($@"{DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");

            //for (int i = 0; i < 20; i++) // for 循環速度很快
            //{
            //    // 開啟執行緒，不會阻塞，執行緒會延遲啟動，在第一個執行緒啟動前，
            //    // for 迴圈可能已經循環完畢，所以印出的 i 值，有可能都是 20。
            //    // 為了避免此種狀況，要用一個臨時的變數，每次循環都將 i 賦值給他。
            //    Task.Run(() =>
            //    {
            //        Console.WriteLine($@"i = {i} [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
            //    });
            //}

            for (int i = 0; i < 20; i++)
            {
                // 作用域，k 只針對某一次 for 循環
                // 循環 20 次，就會有 20 個 k 值。
                int k = i;
                Task.Run(() =>
                {
                    Console.WriteLine($@"k = {k} [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
                });
            }

            Console.WriteLine($@"TemporaryVariable_OnClick End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff}");
        }

        /// <summary>
        /// 執行緒安全
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ThreadSafe_OnClick(object sender, RoutedEventArgs e)
        {
            {
                int numOne = 0;
                int numTwo = 0;

                List<Task> taskList = new List<Task>();

                for (int i = 0; i < 10000; i++)
                {
                    numOne += 1;
                }

                for (int i = 0; i < 10000; i++)
                {
                    taskList.Add(Task.Run(() =>
                    {
                        numTwo += 1;
                    }));
                }

                Task.WaitAll(taskList.ToArray());
                Console.WriteLine($@"numOne: {numOne}");
                Console.WriteLine($@"numTow: {numTwo}");
            }
            

            // 執行緒安全：程式碼用多執行緒運行和單執行緒運行的結果相同
            // 執行緒不安全一般出現在全域變數/共享變數/磁碟機文件/靜態變數/資料庫的值，
            // 只要是多項去存取修改的時候，就可能會出現執行緒不安全問題。
            // 磁碟機文件：若開啟多個執行緒去操作文件，A--操作，B--也操作

            // 解決執行緒安全問題

            // 1. 使用 lock 鎖。
            // Lock 鎖：可以鎖住一個引用，避免多個執行緒同時併發。
            // 使用鎖：private static readonly object _objLock = new object()
            // Lock：建議使用 object 來鎖，不能鎖 null，也不建議鎖 string、this...
            // 鎖的作用：排他。

            {
                // 鎖 this 範例
                Test test = new Test();
                test.DoTest();
            }

            {
                // lock 較標準作法，用 object
                List<Task> taskList = new List<Task>();
                int numTwo = 0;

                object objLock = new object();
                for (int i = 0; i < 10000; i++)
                {
                    taskList.Add(Task.Run(() =>
                    {
                        // lock 可以避免多執行緒併發，但如果鎖住，這裡跟單執行緒基本上沒有區別
                        lock (objLock)
                        {
                            numTwo += 1;
                        }
                    }));
                }

                Task.WaitAll(taskList.ToArray());
                Console.WriteLine($@"numTow (lock): {numTwo}");
            }

            // 2. 使用執行緒安全集合
            // System.Collections.Concurrent 命名空間下的類別
            
            // 3. 將要處理的資料拆分，避免多個執行緒操作同一堆資料
            
        }

        public class Test
        {
            private int _testNum = 0;

            public void DoTest()
            {
                // 會不會 deadlock？不會！因為是同一個執行緒，this 表示 Test 的實例
                // lock 主要是排他
                // 此處是單執行緒且是同一個實例
                lock (this)
                {
                    Thread.Sleep(200);
                    _testNum += 1;
                    if (DateTime.Now.Day < 15 && _testNum < 5)
                    {
                        DoTest();
                    }
                    else
                    {
                        Console.WriteLine(@"結束了");
                    }
                }
            }
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

        private void Teach(string lesson)
        {
            Console.WriteLine($@"==== 課程：{lesson} Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} ====");

            Thread.Sleep(200);

            Console.WriteLine($@"==== 課程：{lesson} End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} ====");
        }

        private void Code(string name, string projectName)
        {
            Console.WriteLine($@"==== [{name}] Coding [{projectName}] Start [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} ====");

            Thread.Sleep(2000);

            Console.WriteLine($@"==== [{name}] Coding [{projectName}] End [{Thread.CurrentThread.ManagedThreadId:00}] {DateTime.Now:yyyy/MM/dd HH:mm:ss:fff} ====");
        }


        
    }
}
