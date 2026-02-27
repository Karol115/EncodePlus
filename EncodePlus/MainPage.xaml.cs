using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Security.Cryptography;
using System;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Microsoft.UI.Xaml.Controls;

using static EncodePlus.Operations;

namespace EncodePlus
{
    public sealed partial class MainPage : Page
    {
        private Karol115.UpdaterUWP _updater;

        public static string EncodingVariant { get; set; }
        private static string EncodingType;

        private bool _isLoading = true;
        private CancellationTokenSource _cts;

        private Dictionary<string, List<string>> additionalOptions = new Dictionary<string, List<string>>
        {
            { "Hex", new List<string> { "Standard", "Spaces", "C-Style(0xAA, 0xBB)" } },
        };

        private static bool _autoDetect = false;
        private DispatcherTimer _statusTimer;
        private Stopwatch _operationStopwatch = new Stopwatch();

        private Stopwatch _launchStopwatch;

        public MainPage()
        {
            _launchStopwatch = Stopwatch.StartNew();
            this.InitializeComponent();
            _launchStopwatch.Stop();

            AboutPage.AddLoadTime(_launchStopwatch.ElapsedMilliseconds.ToString());
            //AboutPage.addCoCreator("Co-designer: Kierownik223");
            AboutPage.CheckForUpdate += AboutPage_CheckForUpdates;

             _updater = new Karol115.UpdaterUWP(13, AboutPage.CheckForUpdatesTextBlock, "UWP");
            
            ComboBox.ItemsSource = operations.Keys;
            for(int i = 1; i <= Environment.ProcessorCount; i++)
                ComboThreads.Items.Add(i);

            ComboThreads.SelectedItem = Environment.ProcessorCount;

            LoadSettings();
                

            _isLoading = false;

            _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _statusTimer.Tick += (s, a) => UpdateBruteForceStatus();
        }

        private async void AboutPage_CheckForUpdates(object sender, EventArgs e)
        {
            if(_updater != null)
            {
                 //AboutPage.UpdateStatusText("Checking...");

                await _updater.CheckForUpdatesAsync(true);

                //AboutPage.UpdateStatusText($"Status: {_updaterResult.Status}");
                AboutPage.UpdateStatusText("", new SolidColorBrush(Colors.White));
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            TextBoxInput.Text = "";
            TextBoxOutput.Text = "";
        }

        private void EncodeButton_Click(object sender, RoutedEventArgs e)
        {
            TextBoxOutput.Text = operations[ComboBox.SelectedItem.ToString()].Encode(TextBoxInput.Text);
        }

        private async void DecodeButton_Click(object sender, RoutedEventArgs e)
        {
            if(DecodeButton.Content is StackPanel sp && (sp.Children[1] as TextBlock).Text == "Cancel")
            {
                _cts?.Cancel();
                return;
            }

            if (_cts != null) return;

            string input = TextBoxInput.Text;
            if (ComboBox.SelectedItem == null || string.IsNullOrEmpty(input)) return;
            string selectedKey = ComboBox.SelectedItem.ToString();

            var codec = operations[selectedKey];

            if(codec.Type == OperationType.Hash && !BruteForce.IsHashValid(selectedKey, input))
            {
                await ShowErrorInOutput("Invalid format!");
                return;
            }

             _cts = new CancellationTokenSource();

            bool isHash = codec.Type == OperationType.Hash;
            OperationsStatusStackPanel.Visibility = Visibility.Visible;
            StatusSpeedTextBlock.Visibility = isHash ? Visibility.Visible : Visibility.Collapsed;
            StatusTotalTextBlock.Visibility = isHash ? Visibility.Visible : Visibility.Collapsed;

            try
            {
                UpdateDecodeButtonUI(true);
                DecodingProgressBar.Visibility = Visibility.Visible;

                TextBoxOutput.Foreground = new SolidColorBrush(Colors.White); 
                TextBoxOutput.Text = "Calculating...";

                BruteForce.TotalAttempts = 0;
                _operationStopwatch.Restart();
                _statusTimer.Start();

                string result = await Task.Run(() => {
                    if(codec.Type == OperationType.Hash)
                    {
                        switch (selectedKey)
                        {
                            case "SHA-1": return BruteForce.BruteforceHash(SHA1.Create(), input, _cts.Token);
                            case "SHA-256": return BruteForce.BruteforceHash(SHA256.Create(), input, _cts.Token);
                            case "SHA-512": return BruteForce.BruteforceHash(SHA512.Create(), input, _cts.Token);
                            case "MD5": return BruteForce.BruteforceHash(MD5.Create(), input, _cts.Token);
                            default: return "Not Supported";
                        }
                    }
                    else
                    {
                        try
                        {
                            return codec.Decode(input);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                });

                _operationStopwatch.Stop();
                _statusTimer.Stop();
                UpdateBruteForceStatus();

                DecodingProgressBar.Visibility = Visibility.Collapsed;

                TextBoxOutput.Text = result ?? "";
                if(result == null)
                {
                    string message = _cts.IsCancellationRequested ? "Operation cancelled" : "Not found...";
                    await ShowErrorInOutput(message);
                }
            }
            catch
            {
                _operationStopwatch.Stop();
                _statusTimer.Stop();
                DecodingProgressBar.Visibility = Visibility.Collapsed;

                await ShowErrorInOutput("Invalid format!");
            }
            finally
            {
                _statusTimer.Stop();
                DecodingProgressBar.Visibility = Visibility.Collapsed;

                UpdateDecodeButtonUI(false);
                _cts?.Dispose();
                _cts = null;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedItem == null) return;

            EncodingType = ComboBox.SelectedItem.ToString();
            
            if(additionalOptions.ContainsKey(EncodingType))
            {
                ComboBoxAdditional.ItemsSource = additionalOptions[EncodingType];

                ComboBoxAdditional.Visibility = Visibility.Visible;

                if (!string.IsNullOrEmpty(EncodingVariant))
                {
                    if (additionalOptions[EncodingType].Contains(EncodingVariant))
                    {
                        ComboBoxAdditional.SelectedItem = EncodingVariant;
                    }
                }
                else
                {
                    ComboBoxAdditional.SelectedIndex = 0;
                }
            }
            else
            {
                ComboBoxAdditional.Visibility = Visibility.Collapsed;
                ComboBoxAdditional.ItemsSource = null;
                if (!_isLoading) EncodingVariant = null;
            }

            // set text
            bool isHash = (operations[ComboBox.SelectedItem.ToString()].Type == OperationType.Hash);

            var stack = DecodeButton.Content as StackPanel;
            var icon = stack.Children[0] as SymbolIcon;
            var textBlock = stack.Children[1] as TextBlock;

            icon.Symbol = isHash ? Symbol.Permissions : Symbol.View;
            textBlock.Text = isHash ? "Brute Force" : "Decode";

            OperationsStatusStackPanel.Visibility = Visibility.Collapsed;

            if (!_isLoading)
                SaveSettings();
        }
        
        private void ComboBoxAdditional_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading) return;

            EncodingVariant = (sender as ComboBox).SelectedItem?.ToString();
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            DataPackage package = new DataPackage();
            package.SetText(TextBoxOutput.Text);
            Clipboard.SetContent(package);
        }

        private void AutoDetectButton_Click(object sender, RoutedEventArgs e)
        {
            _autoDetect = !_autoDetect;

            UpdateAutoDetectUI();

            if (!_autoDetect)
                DetectionHintTextBlock.Text = "Not Detected";

            SaveSettings();
        }

        private void NumberBox_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            args.Cancel = args.NewText.Any(c => !char.IsDigit(c));
        }

        private void TextBoxInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if(_autoDetect)
            {
                DetectionHintTextBlock.Text = Logic.DetectType((sender as TextBox).Text) ?? "Not Detected";
            }
        }
        
        private void ComboThreads_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isLoading || (sender as ComboBox).SelectedItem == null) return;

            BruteForce.ThreadCount = (int)(sender as ComboBox).SelectedItem;
            SaveSettings();
        }

        private void MaxLengthToBruteforce_ValueChanged(object sender, NumberBoxValueChangedEventArgs args)
        {
            if (_isLoading) return;

            if (!double.IsNaN(args.NewValue))
            {
                BruteForce.SearchDepth = (int)args.NewValue;
                SaveSettings();
            }
        }

        private async Task ShowErrorInOutput(string error)
        {
            TextBoxOutput.Foreground = new SolidColorBrush(Colors.Red);
            TextBoxOutput.Resources["TextControlForegroundPointerOver"] = new SolidColorBrush(Colors.Red);
            TextBoxOutput.Text = error;
            await Task.Delay(1000);
            TextBoxOutput.Foreground = new SolidColorBrush(Colors.White);
            TextBoxOutput.Resources["TextControlForegroundPointerOver"] = new SolidColorBrush(Colors.White);
        }

        private void UpdateBruteForceStatus()
        {
            double elapsed = _operationStopwatch.Elapsed.TotalSeconds;
            long total = BruteForce.TotalAttempts;
            double hps = elapsed > 0 ? total / elapsed : 0;

            StatusTimerTextBlock.Text = Logic.FormatTime(_operationStopwatch.Elapsed);
            StatusSpeedTextBlock.Text = $"Speed: {hps / 1000000:F2}MH/s";
            StatusTotalTextBlock.Text = $"Attempts: {total:N0}";
        }

        private void UpdateDecodeButtonUI(bool isRunning)
        {
            var stack = DecodeButton.Content as StackPanel;
            var icon = stack.Children[0] as SymbolIcon;
            var textBlock = stack.Children[1] as TextBlock;

            if (isRunning && operations[ComboBox.SelectedItem.ToString()].Type == OperationType.Hash)
            {
                icon.Symbol = Symbol.Target;
                textBlock.Text = "Cancel";
            }
            else
            {
                bool isHash = (operations[ComboBox.SelectedItem.ToString()].Type == OperationType.Hash);
                icon.Symbol = isHash ? Symbol.Permissions : Symbol.View;
                textBlock.Text = isHash ? "Brute Force" : "Decode";
            }
        }

        private void SaveSettings()
        {
            if (_isLoading) return;

            var settings = Windows.Storage.ApplicationData.Current.LocalSettings;

            settings.Values["ThreadCount"] = BruteForce.ThreadCount;
            settings.Values["SearchDepth"] = BruteForce.SearchDepth;

            settings.Values["EncodingType"] = EncodingType ?? "Base64";

            settings.Values["AutoDetect"] = _autoDetect;
        }

        private void LoadSettings()
        {
            try
            {
                var settings = Windows.Storage.ApplicationData.Current.LocalSettings;

                if (settings.Values.TryGetValue("ThreadCount", out object tcVal) && int.TryParse(tcVal?.ToString(), out int tc))
                {
                    int validTc = (tc > 0 && tc <= Environment.ProcessorCount) ? tc : Environment.ProcessorCount;
                    BruteForce.ThreadCount = validTc;
                    ComboThreads.SelectedItem = validTc;
                }

                if (settings.Values.TryGetValue("SearchDepth", out object sdVal))
                {
                    int depth = Convert.ToInt32(sdVal);
    
                    BruteForce.SearchDepth = depth;
    
                    MaxLengthToBruteforce.Value = depth; 
                }

                if (settings.Values.TryGetValue("EncodingType", out object etVal))
                {
                    string type = etVal.ToString();

                    if (ComboBox.Items.Contains(type))
                        ComboBox.SelectedItem = type;
                }

                if (settings.Values.TryGetValue("AutoDetect", out object adVal) && adVal is bool ad)
                {
                    _autoDetect = ad;
                    UpdateAutoDetectUI();
                }
            }
            catch
            {
                try
                {
                    Windows.Storage.ApplicationData.Current.LocalSettings.Values.Clear();
                }
                catch { }
            }
        }

        private void UpdateAutoDetectUI()
        {
            if (AutoDetectButton.Content is StackPanel stack && stack.Children[1] is TextBlock textBlock)
            {
                textBlock.Text = $"Auto Detect Encoding Format: {(_autoDetect ? "On" : "Off")}";

                if (_autoDetect)
                    AutoDetectButton.Background = new SolidColorBrush(Color.FromArgb(255, 230, 122, 0));
                else
                    AutoDetectButton.Background = new SolidColorBrush(Colors.Transparent);
            }

            if (_autoDetect && TextBoxInput != null)
            {
                DetectionHintTextBlock.Text = Logic.DetectType(TextBoxInput.Text) ?? "Not Detected";
            }
        }
    }
}
