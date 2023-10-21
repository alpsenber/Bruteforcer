using System;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace PasswordCracker
{
    struct Params
    {
        public string[] hshs;
        public int start;
        public int stop;
    }
    class Program
    {
        static void Main(string[] args)
        {
            string filePath = "hashes.txt";
            string[] hashes = new string[3];
            int[]? marks;
            try
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    for (int i = 0; i < 3; i++)
                    {
                        hashes[i] = sr.ReadLine();
                    }
                }
                Console.WriteLine("Хэш-значения SHA-256: ");
                foreach (string h in hashes)
                {
                    Console.WriteLine(h);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при чтении файла: " + ex.Message);
            }

            Console.Write("Введите количество необходимых потоков: ");
            string? number = Console.ReadLine();
            int numThreads = 0;

            if (!int.TryParse(number,out numThreads)) 
            {
                Console.WriteLine("Неверно введено количество потоков!");
                return;
            }

            marks = MakeArgs(numThreads);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Thread[] threads = new Thread[numThreads];

            for (int i = 0; i < numThreads; i++)
            {
                Params p = new Params();
                p.hshs = (string[])hashes.Clone();
                p.start = marks[i];
                p.stop = marks[i + 1];
                threads[i] = new Thread(new ParameterizedThreadStart(Crack));
                threads[i].Start(p);
            }
           
            for (int i = 0; i < numThreads; i++)
                threads[i].Join();
            stopwatch.Stop();
            Console.WriteLine("Затраченное время: " + stopwatch.Elapsed);
        }

        static void Crack(object arg)
        {
            if (arg == null || arg.GetType() != typeof(Params)) return;
            Params prms = (Params)arg;
            Console.WriteLine();
            Console.WriteLine("Запущен поток перебора : " + prms.start.ToString() + "-" + prms.stop.ToString());
            List<string> list = prms.hshs.ToList();
            int cnt = list.Count;

            for (char c1 = (char)('a' + prms.start); c1 < (char)('a' + prms.stop); c1++)
            {
                if (cnt == 0) break;
                for (char c2 = 'a'; c2 <= 'z'; c2++)
                {
                    for (char c3 = 'a'; c3 <= 'z'; c3++)
                    {
                        for (char c4 = 'a'; c4 <= 'z'; c4++)
                        {
                            for (char c5 = 'a'; c5 <= 'z'; c5++)
                            {
                                string password = $"{c1}{c2}{c3}{c4}{c5}";
                                string passwordHash = ComputeSha256Hash(password);
                                if (list.Contains(passwordHash))
                                {
                                    Console.WriteLine("В потоке " + prms.start.ToString() + "-" + prms.stop.ToString() + " найдено");
                                    Console.WriteLine("Для хэш-значения: " + passwordHash + ", пароль : " + password);
                                    cnt--;
                                }
                            }
                        }
                    }
                }
            }
        }

        static string ComputeSha256Hash(string input)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        static int[] MakeArgs(int n)
        {
            if (n < 1) n = 1;
            if (n > 4) n = 4;
            int[] res = new int[n + 1];
            int m = 26 / n;
            res[0] = 0;
            for (int i = 1; i < n; i++)
            {
                res[i] = res[i-1]+m;
            }
            res[n] = 26;
            return res;
        }
    }
}