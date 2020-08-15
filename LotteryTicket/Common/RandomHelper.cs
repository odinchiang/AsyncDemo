using System;
using System.Threading;

namespace BicolorLottery.Common
{
    /// <summary>
    /// 取隨機數
    /// </summary>
    /// <remarks>
    /// new Random().Next(1, 100)：多執行緒同時執行時，會有很高機率相同
    /// 因為用的是當前時間為其 seed，時間相同結果會相同
    /// 解決隨機數重複問題
    /// </remarks>
    public class RandomHelper
    {
        /// <summary>
        /// 隨機獲取隨機數字並等待 0.5 ~ 1s
        /// </summary>
        /// <param name="min">亂數最小值</param>
        /// <param name="max">亂數最大值</param>
        /// <returns></returns>
        public int GetRandomNumberDelay(int min, int max)
        {
            Thread.Sleep(GetRandomNumber(300, 700)); // 休息時間隨機
            return GetRandomNumber(min, max);
        }

        /// <summary>
        /// 獲取隨機數，更大機率的避免重複問題
        /// </summary>
        /// <param name="min">亂數最小值</param>
        /// <param name="max">亂數最大值</param>
        /// <returns>隨機值</returns>
        public int GetRandomNumber(int min, int max)
        {
            Guid guid = Guid.NewGuid();
            string sGuid = guid.ToString();
            int seed = DateTime.Now.Millisecond;

            for (int i = 0; i < sGuid.Length; i++)
            {
                switch (sGuid[i])
                {
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                        seed += 1;
                        break;
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                        seed += 2;
                        break;
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                        seed += 3;
                        break;
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                        seed += 4;
                        break;
                    default:
                        seed += 5;
                        break;
                }
            }

            Random random = new Random(seed);
            return random.Next(min, max);
        }
    }
}
