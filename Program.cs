using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace WordThingy {
    class Program {
        static void Main(string[] args) {

            string[] urls = File.ReadAllLines("urls.txt");

            long totalWords = 0;

            ConcurrentDictionary<string, int> wordIndex = new ConcurrentDictionary<string, int>(8, 100000);

            Parallel.ForEach<string>(urls, (url) => {
                Console.WriteLine("Fetching contents from: " + url);
                WebClient client = new WebClient();
                Task<string> fetchTask = client.DownloadStringTaskAsync(url);
                fetchTask.ContinueWith((text) => {
                    string[] words = text.Result.ToLower().Split(null);
                    Parallel.ForEach<string>(words, (aWord) => {
                        aWord = aWord.Trim();
                        if (!String.IsNullOrWhiteSpace(aWord)) {
                            Interlocked.Increment(ref totalWords);
                            wordIndex.AddOrUpdate(aWord, 1, (key, oldValue) => oldValue++);
                        }
                    });
                }).ContinueWith((countTask) => {
                    countTask.Wait();
                    Console.WriteLine("After {0}, Unique word count = {1}, totalWordCount = {2}", url, wordIndex.Keys.Count, Interlocked.Read(ref totalWords));
                });
            } );

            Console.WriteLine("Done... press enter");
            Console.ReadLine();
        }

    }
}
