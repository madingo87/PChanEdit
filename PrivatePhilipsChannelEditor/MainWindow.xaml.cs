﻿
using System.Collections.Generic;
using System.Windows;
using System.Xml;
using System.Linq;
using System;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using System.IO;
using System.Data.SQLite;
using System.Windows.Forms;

namespace PrivatePhilipsChannelEditor
{
    public partial class MainWindow : Window
    {
        List<ChannelData> _channelList;
        string _channelFile;
        string _imagesFolder;
        string _dummyImagePath = Environment.CurrentDirectory+@"\dummy.png";

        public MainWindow()
        {
            InitializeComponent();

            _channelList = new List<ChannelData>();
            ImageView.Source = new BitmapImage(new Uri(_dummyImagePath));
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            MainBox.Items.Clear();
            _channelList.Clear();
            

            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                Description = "Please choose the 'ChannelMap_' directory ...",
                SelectedPath = Environment.SpecialFolder.DesktopDirectory.ToString()
            };

            //fbd.SelectedPath = @"C:\Users\martin\Workspace\data\PhilipsChannelMaps\ChannelMap_100";

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {

                _channelFile = fbd.SelectedPath + @"\ChannelList\channellib\DVBC.xml";
                _imagesFolder = fbd.SelectedPath + @"\syslogo\sys";

                if (!File.Exists(_channelFile)) 
                {
                    Helper.ShowWarning($"Could not find DVBC file in path {_channelFile}");
                    return;
                }

                XmlReader xmlRdr;

                try
                {
                    xmlRdr = XmlReader.Create(_channelFile);
                }
                catch (Exception ex) {
                    Helper.ShowError(ex, "Error Creating XmlReader");
                    return;
                }

                try
                {
                    xmlRdr.MoveToContent();
                    ChannelData currentChannel = new ChannelData();

                    while (xmlRdr.Read())
                    {
                        // Only detect start elements.
                        if (xmlRdr.IsStartElement())
                        {
                            // Get element name and switch on it.
                            switch (xmlRdr.Name)
                            {
                                case "Channel":
                                    // Log "begin new Channel"
                                    currentChannel = new ChannelData();
                                    break;
                                case "Setup":
                                    // Log "begin new Channel"
                                    currentChannel.ChannelNumber = xmlRdr["ChannelNumber"];
                                    currentChannel.ChannelName = xmlRdr["ChannelName"];
                                    currentChannel.ChannelLock = xmlRdr["ChannelLock"];
                                    currentChannel.UserModifiedName = xmlRdr["UserModifiedName"];
                                    currentChannel.LogoID = xmlRdr["LogoID"];
                                    currentChannel.LogoLock = xmlRdr["LogoLock"];
                                    currentChannel.UserModifiedLogo = xmlRdr["UserModifiedLogo"];
                                    currentChannel.FavoriteNumber = xmlRdr["FavoriteNumber"];
                                    currentChannel.UserHidden = xmlRdr["UserHidden"];
                                    break;
                                case "Broadcast":
                                    // Log "begin new Channel"
                                    currentChannel.ChannelType = xmlRdr["ChannelType"];
                                    currentChannel.Onid = xmlRdr["Onid"];
                                    currentChannel.Tsid = xmlRdr["Tsid"];
                                    currentChannel.Sid = xmlRdr["Sid"];
                                    currentChannel.Frequency = xmlRdr["Frequency"];
                                    currentChannel.Modulation = xmlRdr["Modulation"];
                                    currentChannel.ServiceType = xmlRdr["ServiceType"];
                                    currentChannel.Bandwidth = xmlRdr["Bandwidth"];
                                    currentChannel.SymbolRate = xmlRdr["SymbolRate"];
                                    currentChannel.DecoderType = xmlRdr["DecoderType"];
                                    currentChannel.NetworkID = xmlRdr["NetworkID"];
                                    currentChannel.StreamPriority = xmlRdr["StreamPriority"];
                                    currentChannel.SystemHidden = xmlRdr["SystemHidden"];
                                    break;
                            }
                        }
                        else
                        {
                            if (xmlRdr.Name == "Channel")
                                _channelList.Add(currentChannel);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Helper.ShowError(ex, "Error while reading xml file");
                    xmlRdr.Close();
                }
                finally {
                    xmlRdr.Close();
                    MainBox.UpdateMainBox(_channelList);
                }
            }           
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings() {
                Indent = true,
                IndentChars = "\t",
                NewLineOnAttributes = true              
            };

            //var newFile = _channelFile.Replace("DVBC.xml", "DVBC_new.xml");
            XmlWriter xmlWrt = XmlWriter.Create(_channelFile, xmlWriterSettings);

            xmlWrt.WriteStartElement("ChannelMap");

            var i = 1;
            foreach (var channel in _channelList)
            {
                xmlWrt.WriteStartElement("Channel");

                xmlWrt.WriteStartElement("Setup");
                xmlWrt.WriteAttributeString("ChannelNumber", i++.ToString()); //channel.ChannelNumber);// 
                xmlWrt.WriteAttributeString("ChannelName", channel.ChannelName);
                xmlWrt.WriteAttributeString("ChannelLock", channel.ChannelLock);
                xmlWrt.WriteAttributeString("UserModifiedName", channel.UserModifiedName);
                xmlWrt.WriteAttributeString("LogoID", channel.LogoID);
                xmlWrt.WriteAttributeString("UserModifiedLogo", channel.UserModifiedLogo);
                xmlWrt.WriteAttributeString("LogoLock", channel.LogoLock);
                xmlWrt.WriteAttributeString("UserHidden", channel.UserHidden);
                xmlWrt.WriteAttributeString("FavoriteNumber", channel.FavoriteNumber);
                xmlWrt.WriteFullEndElement();

                xmlWrt.WriteStartElement("Broadcast");
                xmlWrt.WriteAttributeString("ChannelType", channel.ChannelNumber);
                xmlWrt.WriteAttributeString("Onid", channel.Onid);
                xmlWrt.WriteAttributeString("Tsid", channel.Tsid);
                xmlWrt.WriteAttributeString("Sid", channel.Sid);
                xmlWrt.WriteAttributeString("Frequency", channel.Frequency);
                xmlWrt.WriteAttributeString("Modulation", channel.Modulation);
                xmlWrt.WriteAttributeString("ServiceType", channel.ServiceType);
                xmlWrt.WriteAttributeString("Bandwidth", channel.Bandwidth);
                xmlWrt.WriteAttributeString("SymbolRate", channel.SymbolRate);
                xmlWrt.WriteAttributeString("DecoderType", channel.DecoderType);
                xmlWrt.WriteAttributeString("NetworkID", channel.NetworkID);
                xmlWrt.WriteAttributeString("StreamPriority", channel.StreamPriority);
                xmlWrt.WriteAttributeString("SystemHidden", channel.SystemHidden);
                xmlWrt.WriteFullEndElement();

                xmlWrt.WriteEndElement();
            }

            xmlWrt.WriteEndElement();

            xmlWrt.Flush();
            xmlWrt.Close();
        }

        private void MainBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (MainBox.SelectedIndex >= 0)
            {
                var selectedChannel = _channelList[MainBox.SelectedIndex];
                try
                {
                    var path = selectedChannel.LogoID != "0" ? string.Format(@"{0}\{1}.png", _imagesFolder, selectedChannel.LogoID) : _dummyImagePath;
                    ImageView.Source = new BitmapImage(new Uri(path));
                }
                catch (Exception ex)
                {
                    Helper.ShowError(ex, "Image not found! Make sure to have the images 'syslogo.tar' extracted");
                }

                DetailView.Items.Clear();
                DetailView.Items.Add("Channel Number:\t\t" + selectedChannel.ChannelNumber);
                DetailView.Items.Add("Favorite Number:\t\t" + selectedChannel.FavoriteNumber);
                DetailView.Items.Add("Frequency:\t\t" + selectedChannel.Frequency);
                DetailView.Items.Add("UserModifiedName:\t" + selectedChannel.UserModifiedName);
            }
        }

        private void Up_Click(object sender, RoutedEventArgs e)
        {
            if (MainBox.SelectedIndex > 0)
            {
                var previous = _channelList[MainBox.SelectedIndex - 1];
                var selected = _channelList[MainBox.SelectedIndex];

                _channelList[MainBox.SelectedIndex] = previous;
                _channelList[MainBox.SelectedIndex - 1] = selected;

                MainBox.SelectedIndex--;
                MainBox.UpdateMainBox(_channelList);
            }
        }

        private void Down_Click(object sender, RoutedEventArgs e)
        {
            if (MainBox.SelectedIndex + 1 < _channelList.Count)
            {
                var next = _channelList[MainBox.SelectedIndex + 1];
                var selected = _channelList[MainBox.SelectedIndex];

                _channelList[MainBox.SelectedIndex] = next;
                _channelList[MainBox.SelectedIndex + 1] = selected;
                MainBox.SelectedIndex++;
                MainBox.UpdateMainBox(_channelList);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _channelList.Remove(_channelList[MainBox.SelectedIndex]);
                MainBox.UpdateMainBox(_channelList);
            }
            catch (Exception ex)
            {
                Helper.ShowError(ex, "No Index found!");
            }
        }

        private void Postion_Click(object sender, RoutedEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            if (!string.IsNullOrWhiteSpace(PositionText.Text) && !regex.IsMatch(PositionText.Text))
            {
                var pos = Convert.ToInt32(PositionText.Text);
                if (pos >= 0 && pos < _channelList.Count)
                {
                    var selected = _channelList[MainBox.SelectedIndex];

                    _channelList.Remove(selected);
                    _channelList.Insert(pos, selected);

                    MainBox.UpdateMainBox(_channelList);
                }
            }
        }

        private void ReadBinary_Click(object sender, RoutedEventArgs e)
        {
            var promtResult = Helper.PromtInformation("This reads the binary files in the 'ChannelMap_100\\ChannelList' directory");

            var fileDialog = new OpenFileDialog();
            fileDialog.InitialDirectory = @"C:\Users\martin\Workspace\data\PhilipsChannelMaps\ChannelMap_100\ChannelList";

            if (promtResult == System.Windows.Forms.DialogResult.OK && fileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int maxLength = 10000;
                byte[] bytes = new byte[maxLength];
                using (BinaryReader binaryReader = new BinaryReader(new FileStream(fileDialog.FileName, FileMode.Open)))
                {
                    int i = 0;
                    while (binaryReader.BaseStream.Position != binaryReader.BaseStream.Length)
                    {
                        if (i < maxLength)
                            bytes[i++] = binaryReader.ReadByte();
                        else
                            break;
                    }
                }

                var str = System.Text.Encoding.UTF8.GetString(bytes);
                System.Windows.MessageBox.Show(str);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ClearImages_Click(object sender, RoutedEventArgs e)
        {
            var promtResult = Helper.PromtInformation("This removes all unused images of the channel's logoTable");

            if (promtResult == System.Windows.Forms.DialogResult.OK && _channelList.Count != 0)
            {
                var fileCounter = 0;
                var affectedCounter = 0;
                var allLogosInList = _channelList.Select(i => string.Format(@"{0}\{1}.png", _imagesFolder, i.LogoID));
                SQLiteConnection dbConn = null;

                try
                {
                    dbConn = new SQLiteConnection(string.Format(@"DataSource={0}\PreDef.db", _imagesFolder));
                    dbConn.Open();
                }
                catch (Exception ex)
                {
                    Helper.ShowError(ex, "Could not connect to the image db!");
                }

                var allLogosInDir = Directory.GetFiles(_imagesFolder,"*.png");                                  
                foreach (var logo in allLogosInDir)
                {
                    var exists = allLogosInList.Contains(logo);
                    if (!exists)
                    {
                        fileCounter++;
                        File.Delete(logo);
                    }
                }

                var allLogosInDb = new List<string>();
                var queryResult = new SQLiteCommand($"select * from ChannelLogoTable", dbConn).ExecuteReader();
                while (queryResult.Read())
                {
                    var logoId = queryResult["LogoId"];
                    var exists = allLogosInList.Select(i => i.Substring(i.LastIndexOf('\\')+1).Replace(".png","")).Contains(logoId.ToString());
                    if (!exists)                      
                        affectedCounter += new SQLiteCommand($"delete from ChannelLogoTable where LogoId={logoId}", dbConn).ExecuteNonQuery();
                }

                DetailView.Items.Clear();
                DetailView.Items.Add($"{fileCounter} Images removed!");
                DetailView.Items.Add($"{affectedCounter} affected in DB!");
                dbConn.Close();
            }
        }

        private void ShowInformation_Click(object sender, RoutedEventArgs e)
        {
            var infoString = "Private Philips Channel Editor.\n\nVersion 0.0.1\n\nFree to use :)";
            System.Windows.MessageBox.Show(infoString);
        }
    }
}
