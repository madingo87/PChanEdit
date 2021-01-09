
using System.Collections.Generic;
using System.Windows;
using System.Xml;
using System.Linq;
using System.Windows.Forms;
using System;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;
using System.IO;
using System.Data.SQLite;

namespace PrivatePhilipsChannelEditor
{
    public partial class MainWindow : Window
    {

        List<ChannelData> _channelList;
        string _channelFile;
        string _imagesFolder;
        string _dummyImagePath = Environment.CurrentDirectory+@"\dummy.png";
        string _workingDir;


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

            var rootFolder = new Environment.SpecialFolder();
            rootFolder = Environment.SpecialFolder.DesktopDirectory;

            FolderBrowserDialog fbd = new FolderBrowserDialog
            {
                Description = "Bitte das 'ChannelMap' Verzeichnis wählen...",
                RootFolder = rootFolder //(@"C:\Users\martin\Desktop\philips_channels");
            };

            fbd.ShowDialog();
            //fbd.SelectedPath = @"C:\Users\martin\Desktop\philips_channels\PhilipsChannelMaps\ChannelMap_100";

            if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
            { 
                _channelFile = fbd.SelectedPath + @"\ChannelList\channellib\DVBC.xml";
                _imagesFolder = fbd.SelectedPath + @"\syslogo\sys";                
                _workingDir = fbd.SelectedPath;

                XmlReader xmlRdr = XmlReader.Create(_channelFile);
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

                xmlRdr.Close();

                UpdateMainBox();
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

        private string ConvertHexChannelNameToString(string hex)
        {
            hex = hex.Replace("0x00","");
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
                    System.Windows.MessageBox.Show("Bild nicht gefunden! Ggf Datenbank nicht extrahiert? \n(" + ex.Message + ")");
                }

                DetailView.Items.Clear();
                DetailView.Items.Add("Channel Number:\t" + selectedChannel.ChannelNumber);
                DetailView.Items.Add("Favorite Number:\t" + selectedChannel.FavoriteNumber);
                DetailView.Items.Add("Frequency:\t" + selectedChannel.Frequency);
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
                UpdateMainBox();
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
                UpdateMainBox();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _channelList.Remove(_channelList[MainBox.SelectedIndex]);
                UpdateMainBox();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Kein/Falscher Index! \n("+ex.Message+")");
            }
        }

        private void UpdateMainBox()
        {
            var lastSelectedIndex = MainBox.SelectedIndex;
            MainBox.Items.Clear();

            var index = 1;
            foreach (var channel in _channelList)
                MainBox.Items.Add(string.Format("{0:000} | {1}", index++, ConvertHexChannelNameToString(channel.ChannelName)));

            if (lastSelectedIndex > _channelList.Count)
                MainBox.SelectedIndex = lastSelectedIndex - 1;
            else
                MainBox.SelectedIndex = lastSelectedIndex;
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
                    
                    UpdateMainBox();
                }
            }
        }

        private void ReadBinary_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            fd.InitialDirectory = @"C:\Users\martin\Desktop\philips_channels\PhilipsChannelMaps\ChannelMap_100\ChannelList";

            fd.ShowDialog();

            int maxLength = 10000;
            byte[] bytes = new byte[maxLength];
            using (BinaryReader binaryReader = new BinaryReader(new FileStream(fd.FileName, FileMode.Open)))
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

            var str = System.Text.Encoding.UTF7.GetString(bytes);
            System.Windows.MessageBox.Show(str);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ClearImages_Click(object sender, RoutedEventArgs e)
        {
            if (_channelList.Count != 0)
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
                    System.Windows.MessageBox.Show("Verbindung zur Bild DB nicht möglich! Ggf Datenbank nicht extrahiert? \n(" + ex.Message + ")");
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
    }

    struct ChannelData {

        //Setup
        public string ChannelNumber; //= "1118" 
        public string ChannelName; //="0x54 0x00 0x65 0x00 0x73 0x00 0x74 0x00 0x73 0x00 0x65 0x00 0x6E 0x00 0x64 0x00 0x65 0x00 0x72 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00 0x00" 
        public string ChannelLock; //="0" 
        public string UserModifiedName; //="1" 
        public string LogoID; //="0" 
        public string UserModifiedLogo; //= "0"
        public string LogoLock; // ="0"
        public string UserHidden; // ="0"
        public string FavoriteNumber; // ="0"

        //Broadcast
        public string ChannelType; // = "3"
        public string Onid; // ="61441"
        public string Tsid; // ="10000"
        public string Sid; // ="428"
        public string Frequency; // ="122"
        public string Modulation; // ="64"
        public string ServiceType; // ="2"
        public string Bandwidth; // ="8" 
        public string SymbolRate; // ="6900000"
        public string DecoderType; // ="2" 
        public string NetworkID; // ="61448"
        public string StreamPriority; // ="0" 
        public string SystemHidden; // ="0"
    }
}
