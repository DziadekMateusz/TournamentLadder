using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;

namespace TournamentLadder
{
    public partial class Form1 : Form
    {
        SerialPort serialPort = new SerialPort();   // Serial port (USB/COM)
        List<string> imageFiles = new List<string>(); // List of photo locations
        Random rnd = new Random();                    // Random number generator
        string currentLeft = "";                      // The path of the current photo on the left
        string currentRight = "";                     // The path of the current photo on the right

        public Form1()
        {
            InitializeComponent();
            RefreshPorts();                                    // Search for available COM ports
            button1.Enabled = button2.Enabled = button3.Enabled = false; // Disable buttons until a connection is established
            serialPort.DataReceived += SerialPort_DataReceived; // Handling the data reception event from Arduino
        }

        private void RefreshPorts()
        {
            comboBoxPorts.Items.Clear();                       // Clear the port list
            comboBoxPorts.Items.AddRange(SerialPort.GetPortNames()); // Retrieving available COM ports

            if (comboBoxPorts.Items.Count > 0)
                comboBoxPorts.SelectedIndex = 0;               // Automatic selection of the first port
        }

        private void buttonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (!serialPort.IsOpen)                       // Check if the port is already open
                {
                    serialPort.PortName = comboBoxPorts.SelectedItem.ToString(); // Selected port
                    serialPort.BaudRate = 9600;               // Setting the baud rate (Arduino-compatible)
                    serialPort.Open();                        // Port Opening
                    labelStatus.Text = "Connected to " + serialPort.PortName;
                    button1.Enabled = button2.Enabled = button3.Enabled = true; // Enabling the send buttons
                    LoadImages();                             // Loading photos
                    ShowRandomImages();                       // Random display of two images
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection error: " + ex.Message);
            }
        }

        private void LoadImages()
        {
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pictures"); // Folder location

            if (!Directory.Exists(folder))
            {
                MessageBox.Show("The 'Pictures' folder does not exist in the project directory!");
                return;
            }

            // Download JPG/PNG/JPEG files
            imageFiles = Directory.GetFiles(folder, "*.*")
                                  .Where(f => f.EndsWith(".jpg") || f.EndsWith(".png") || f.EndsWith(".jpeg"))
                                  .ToList();

            if (imageFiles.Count < 2)
            {
                MessageBox.Show("Not enough photos in the Pictures folder (min. 2)");
                return;
            }
        }

        private void ShowRandomImages()
        {
            if (imageFiles.Count < 2) return;

            var list = new List<string>(imageFiles); // A copy of the list for the drawing

            currentLeft = list[rnd.Next(list.Count)]; // Drawing for the photo on the left
            list.Remove(currentLeft);                 // Removing a photo from the pool

            currentRight = list[rnd.Next(list.Count)]; // Drawing for the photo on the right

            pictureBoxLeft.ImageLocation = currentLeft;   // Display the photo on the left
            pictureBoxRight.ImageLocation = currentRight; // Display the photo on the right
        }

        private void ReplaceRightImage()
        {
            imageFiles.Remove(currentRight);         // Removing the photo on the right from the pool
            if (imageFiles.Count == 0) LoadImages(); // If you've run out of photos, reload the page

            string newImg;
            do newImg = imageFiles[rnd.Next(imageFiles.Count)]; // Draw new photo
            while (newImg == currentLeft);                      // Preventing duplication on the left

            currentRight = newImg;
            pictureBoxRight.ImageLocation = newImg;             // Display a new photo
        }

        private void ReplaceLeftImage()
        {
            imageFiles.Remove(currentLeft);          // Delete the photo on the left
            if (imageFiles.Count == 0) LoadImages();

            string newImg;
            do newImg = imageFiles[rnd.Next(imageFiles.Count)];
            while (newImg == currentRight);          // Preventing duplication on the right

            currentLeft = newImg;
            pictureBoxLeft.ImageLocation = newImg;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = serialPort.ReadLine().Trim(); // Reading data from Arduino

            // Triggering changes in the GUI from the UI thread
            this.Invoke(new Action(() =>
            {
                if (data == "A0")
                {
                    ReplaceRightImage();  // If a button is pressed on the Arduino, the left one wins
                }
                else if (data == "A1")
                {
                    ReplaceLeftImage();   // If a button is pressed on the Arduino, the right one wins
                }
            }));
        }

        private void button1_Click(object sender, EventArgs e) => SendData("1"); // Send "1" to Arduino
        private void button2_Click(object sender, EventArgs e) => SendData("2"); // Send "2" to Arduino
        private void button3_Click(object sender, EventArgs e) => SendData("3"); // Send "3" to Arduino

        private void SendData(string data)
        {
            if (serialPort.IsOpen)
                serialPort.WriteLine(data); // Send data to Arduino
            else
                MessageBox.Show("No connection to the port!");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort.IsOpen)
                serialPort.Close(); // Close the port when exiting the application
        }

        private void buttonRefreshPorts_Click(object sender, EventArgs e)
        {
            RefreshPorts();               // Refresh the list of ports
            labelStatus.Text = "COM ports refreshed";
        }
    }
}
