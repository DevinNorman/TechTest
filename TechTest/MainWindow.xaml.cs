using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace TechTest
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {

    private Dictionary<char, int> ScrabbleScores = new Dictionary<char, int>()
    {
      { 'a', 1 }, { 'b', 3 }, { 'c', 3 }, { 'd', 2 },
      { 'e', 1 }, { 'f', 4 }, { 'g', 2 }, { 'h', 4 },
      { 'i', 1 }, { 'j', 8 }, { 'k', 5 }, { 'l', 1 },
      { 'm', 3 }, { 'n', 1 }, { 'o', 1 }, { 'p', 3 },
      { 'q', 10 }, { 'r', 1 }, { 's', 1 }, { 't', 1 },
      { 'u', 1 }, { 'v', 4 }, { 'w', 4 }, { 'x', 8 },
      { 'y', 4 }, { 'z', 10 }, { '-', 0 }
    };
    public MainWindow()
    {
      InitializeComponent();
    }

    private void GetFile_Click(object sender, RoutedEventArgs e)
    {
      Result.Text = "Find a txt file";
      Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog()
      {
        // Set filter for file extension and default file extension 
        DefaultExt = ".txt",
        Filter = "Text Files (*.txt)|*.txt"
      };

      // Display OpenFileDialog by calling ShowDialog method 
      Nullable<bool> result = dlg.ShowDialog();

      // Get the selected file 
      if (result == true)
      {
        //address a different thread to update UI
        ThreadPool.QueueUserWorkItem((o) =>
        {
          //user feedback
          Dispatcher.Invoke((Action)(() => Result.Text = "Processing... (Please wait)"));
          Dispatcher.Invoke((Action)(() => FindButton.IsEnabled = false));

          // Open document 
          string filename = dlg.FileName;
          StreamReader reader = new StreamReader(dlg.FileName);

          //setup variables
          string currentLine;
          Dictionary<string, int> words = new Dictionary<string, int>();
          int highScrabbleScore = 0;
          string highScrabbleScoreWord = string.Empty;
          DateTime wordsToHashTimeTaken = DateTime.Now;

          //loop the lines of the document
          while ((currentLine = reader.ReadLine()) != null)
          {
            var splitString = Regex.Replace(currentLine, "[^A-Za-z -]", "").ToLower().Split(' ');
            foreach (string word in splitString)
            {
              if (word == string.Empty)
              {
                continue;
              }
              if (words.ContainsKey(word))
              {
                // The word already exists
                words[word] = words[word] + 1;
              }
              else
              {
                // First occurance of the word
                words.Add(word, 1);

                //get the scrabble score
                int score = 0;
                foreach (char letter in word)
                {
                  try
                  {
                    score += ScrabbleScores[letter];
                  }
                  catch { }
                }
                if (score > highScrabbleScore)
                {
                  highScrabbleScore = score;
                  highScrabbleScoreWord = word;
                }
                else if (score == highScrabbleScore)
                {
                  highScrabbleScoreWord += ", " + word;
                }
              }
            }
          }
          TimeSpan createHashTimeSpan = DateTime.Now - wordsToHashTimeTaken;

          //use linq to get output
          DateTime mostCommonWordTimeTaken = DateTime.Now;
          KeyValuePair<string, int> mostCommonWord = words.FirstOrDefault(x => x.Value == words.Values.Max());
          TimeSpan mostCommonWordTimeSpan = DateTime.Now - mostCommonWordTimeTaken;

          DateTime mostCommonWord7TimeTaken = DateTime.Now;
          Dictionary<string, int> words7 = words.Where(c => c.Key.Length == 7).ToDictionary(p => p.Key, p => p.Value);
          KeyValuePair<string, int> mostCommonWord7 = words7.FirstOrDefault(x => x.Value == words7.Values.Max());
          TimeSpan mostCommonWord7TimeSpan = DateTime.Now - mostCommonWord7TimeTaken;

          string resultText = "Most frequent word: \"" + mostCommonWord.Key + "\" occurred " + mostCommonWord.Value.ToString() + " times";
          resultText += "\n\rMost frequent 7-character word: \"" + mostCommonWord7.Key + "\" occurred " + mostCommonWord7.Value.ToString() + " times";
          resultText += "\n\rHighest scoring word(s) (according to Scrabble): \"" + highScrabbleScoreWord + "\" with a score of " + highScrabbleScore.ToString();

          string processingTime = "Create hash map and find high scrabble score: " + createHashTimeSpan.TotalMilliseconds.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + "ms";
          processingTime += "\n\rFind most frequent word: " + mostCommonWordTimeSpan.TotalMilliseconds.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + "ms";
          processingTime += "\n\rFind most frequent 7 letter word: " + mostCommonWord7TimeSpan.TotalMilliseconds.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture) + "ms";

          //output to user
          Dispatcher.Invoke((Action)(() => Result.Text = resultText));
          Dispatcher.Invoke((Action)(() => FindButton.IsEnabled = true));


          Dispatcher.Invoke((Action)(() => Computations.Text = processingTime));

        });

      }
    }


  }
}
