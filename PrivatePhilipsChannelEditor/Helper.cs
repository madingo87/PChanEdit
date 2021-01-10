
using System.Collections.Generic;
using System;
using System.Windows.Forms;

namespace PrivatePhilipsChannelEditor
{
    public static class Helper
    {
        public static void UpdateMainBox(this System.Windows.Controls.ListBox mainBox, IList<ChannelData> channelList)
        {
            var lastSelectedIndex = mainBox.SelectedIndex;
            mainBox.Items.Clear();

            var index = 1;
            foreach (var channel in channelList)
                mainBox.Items.Add(string.Format("{0:000} | {1}", index++, ConvertHexChannelNameToString(channel.ChannelName)));

            if (lastSelectedIndex > channelList.Count)
                mainBox.SelectedIndex = lastSelectedIndex - 1;
            else
                mainBox.SelectedIndex = lastSelectedIndex;
        }

        public static void ShowError(Exception ex, string message) 
        {
            MessageBox.Show($"An Error occured!\r\n{message}\r\n\r\n{ex.Message}");
        }

        public static void ShowWarning(string message)
        {
            MessageBox.Show($"An Error occured!\r\n{message}");
        }

        public static DialogResult PromtInformation(string message)
        {
            return MessageBox.Show(
                    message,
                    "Info",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information);
        }

        private static string ConvertHexChannelNameToString(string hex)
        {
            hex = hex.Replace("0x00", "");
            hex = hex.Replace("0x", "");

            var hexLetters = hex.Split(' ');
            var letters = new List<char>();

            foreach (var letter in hexLetters)
            {
                if (!string.IsNullOrWhiteSpace(letter))
                    letters.Add(Convert.ToChar(Convert.ToUInt32(letter, 16)));
            }

            return new string(letters.ToArray());
        }
    }
}
