using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;

namespace EncodePlus
{
    public sealed partial class MainPage : Page
    {
        public enum OperationType
        {
            Encoding,
            Hash,
            Hex,
        }

        public class Codec
        {
            public OperationType Type { get; set; }
            public Func<string, string> Encode { get; set; }
            public Func<string, string> Decode { get; set; }
        }

        Dictionary<string, Codec> operations = new Dictionary<string, Codec>
        {
            { "Base64", new Codec {
                Type = OperationType.Encoding,
                Encode = input => Convert.ToBase64String(Encoding.UTF8.GetBytes(input)),
                Decode = input => Encoding.UTF8.GetString(Convert.FromBase64String(input))
            }},
            { "URL", new Codec {
                Type = OperationType.Encoding,
                Encode = input => Uri.EscapeDataString(input),
                Decode = input => Uri.UnescapeDataString(input)
            }},
            { "ASCII", new Codec {
                Type = OperationType.Encoding,
                Encode = input => Logic.EncodeInput(Encoding.ASCII, input),
                Decode = input => Logic.DecodeInput(Encoding.UTF8, input)
            }},
            { "Utf-7", new Codec {
                Type = OperationType.Encoding,
                Encode = input => Logic.EncodeInput(Encoding.UTF7, input),
                Decode = input => Logic.DecodeInput(Encoding.UTF7, input)
            }},
            { "Utf-8", new Codec {
                Type = OperationType.Encoding,
                Encode = input => Logic.EncodeInput(Encoding.UTF8, input),
                Decode = input => Logic.DecodeInput(Encoding.UTF8, input)
            }},
            { "Utf-32", new Codec {
                Type = OperationType.Encoding,
                Encode = input => Logic.EncodeInput(Encoding.UTF8, input),
                Decode = input => Logic.DecodeInput(Encoding.UTF8, input)
            }},
            { "Hex", new Codec {
                Type = OperationType.Hex,
                Encode = input => Logic.EncodeInput(null, input, true),
                Decode = input => Logic.DecodeInput(null, input, true)
            }},
            { "SHA-1", new Codec {
                Type = OperationType.Hash,
                Encode = input => Logic.HashInput(SHA1.Create(), input)
            }},
            { "SHA-256", new Codec {
                Type = OperationType.Hash,
                Encode = input => Logic.HashInput(SHA256.Create(), input)
            }},
            { "SHA-512", new Codec {
                Type = OperationType.Hash,
                Encode = input => Logic.HashInput(SHA512.Create(), input)
            }},
            { "MD5", new Codec {
                Type = OperationType.Hash,
                Encode = input => Logic.HashInput(MD5.Create(), input)
            }},
        };

        public MainPage()
        {
            this.InitializeComponent();
            AboutPage.addCoCreator("Co-designer: Kierownik223");
            ComboBox.ItemsSource = operations.Keys;
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

        private void DecodeButton_Click(object sender, RoutedEventArgs e)
        {
            Codec codec = operations[ComboBox.SelectedItem.ToString()];
            TextBoxOutput.Text = codec != null ? operations[ComboBox.SelectedItem.ToString()].Decode(TextBoxInput.Text) : "Can't decode hash";
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DecodeButton.IsEnabled = (operations[ComboBox.SelectedItem.ToString()].Type != OperationType.Hash);
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            DataPackage package = new DataPackage();
            package.SetText(TextBoxOutput.Text);
            Clipboard.SetContent(package);
        }
    }
}
