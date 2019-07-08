using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form4_Frequency : Form
    {
        
        double frequency;

        double Fo, Fx, correctedFrequency; //Average, reference, measured frequency
        double sigma, N; //standart deviation, arithmetic average

        bool isSinus; //sinus 1, square 0
        double triggerLevel; // V
        double gateTime; // sec
        double fallRiseTime; // nanosec
        double amplitude; // Vpp
        double noiseOfCounter, noiseOfPulse; // V
        double slewRate; // V/s
        double tJitter, tAcc; // s, when f < 225MHz
        double tRes, tTrigger; //s

        double resolutionUncert, timeBaseAccuracyUncert, timeBaseEffectUncert;
        double systematicUncert;
        double extendedUncert;
        double decleration, certificateValue;

        double heatEffect, aging, lineVoltage;
        double slope, constant;

        double trustabilityRatio = 1;

        string warning = "";

        // // // // // // // // // // // // // // // // // // // // // // 
        private void button1_Click(object sender, EventArgs e) //Start
        {
            AddMeasuredFrequencies(); // average, standerd deviation
            if(measuredFrequencies.Count < 3)
            {
               warning = "Not enough frequency samples";           
            }
            else
            {
                if (textBox20.Text.Equals(""))
                {
                    warning = "Inputs cannot be left empty";
                }
                else
                {
                    label36.Text = "";

                    frequency = ReadFrequency();
                    trustabilityRatio = ReadTrustabilityRatio();  
                    ReadParameters();

                    textBox13.Text = Fo.ToString("0.00000E00");
                    textBox14.Text = sigma.ToString("0.00000E00");
                    textBox10.Text = extendedUncert.ToString("0.00000E00");

                    correctedFrequency = Fo * (1 + 0.000000000002);
                    textBox6.Text = correctedFrequency.ToString("0.00000E00");
                }
            }

            label36.Text = warning;

            textBox17.Text = Fo.ToString();
            textBox19.Text = extendedUncert.ToString();
        }

        // // // // // // // // // // // // // // // // // // // // // //

        void ReadParameters()
        {
            if (comboBox3.Text.Equals("Sinusodial"))
                isSinus = true;
            else if (comboBox3.Text.Equals("Square"))
                isSinus = false;
            else
            {
                isSinus = false;
                warning = "Wave type is not valid";
            }

            if (Fo < 255000000)
                gateTime = 10;
            else if (Fo < 5000000000)
                gateTime = 1;
            else if (Fo < 5700000000)
                gateTime = 0.2;
            else if (Fo < 11300000000)
                gateTime = 0.4;
            else if (Fo < 16900000000)
                gateTime = 0.6;
            else if (Fo < 22500000000)
                gateTime = 0.8;
            else
                gateTime = 1;

            label4.Text = gateTime.ToString();

            fallRiseTime = 2; //ns


            tRes = 0.0000000002;


            timeBaseAccuracyUncert = 0.00000000002;
            heatEffect = 0.00000000002;
            aging = 0.00000000002;
            lineVoltage = 0;

            if (Fo < 255000000)
            {
                triggerLevel = 0.5;
                noiseOfCounter = 0.00035;
                noiseOfPulse = 0.00035;
                amplitude = 10 * Math.Sqrt(2);
                tJitter = 0.000000000003;
                tAcc = 0.00000000001;
                slope = 0; //not used
                constant = 0; //not used
                decleration = slope * Fo + 0.000002; //not used
            }
            else
            {
                triggerLevel = 0;
                noiseOfCounter = 0.0001;
                noiseOfPulse = 0.0001;
                amplitude = 11 * Math.Sqrt(2);
                tJitter = 0; //not used
                tAcc = 0; //not used
                slope = 0; //not used
                constant = 0; //not used
                decleration = 0; //not used
            }


            slewRate = isSinus ? Math.PI * Fo * amplitude * Math.Cos(Math.Asin(2 * triggerLevel / amplitude)) : amplitude * 0.8 * Math.Pow(10, 9) / fallRiseTime;
            label7.Text = slewRate.ToString();

            tTrigger = Math.Sqrt(Math.Pow(noiseOfCounter, 2) + Math.Pow(noiseOfPulse, 2)) / slewRate;
            label5.Text = tTrigger.ToString();

            resolutionUncert = Fo < 255000000 ? Math.Sqrt((24 * Math.Pow(tRes, 2) + 32 * Math.Pow(tTrigger, 2)) / N  + 2 * Math.Pow(tJitter, 2)) / gateTime : Math.Sqrt(Math.Pow(1 / Fo, 2) + Math.Pow(1.4 * tTrigger / gateTime, 2));
            label1.Text = resolutionUncert.ToString();


            timeBaseEffectUncert = Math.Sqrt(Math.Pow(heatEffect, 2) + Math.Pow(aging, 2) + Math.Pow(lineVoltage, 2));
            systematicUncert = tAcc / gateTime;
            extendedUncert = trustabilityRatio * Math.Sqrt(Math.Pow(sigma, 2) + Math.Pow(resolutionUncert * Fo , 2)/3 + Math.Pow(systematicUncert * Fo, 2) / 3 + Math.Pow(timeBaseAccuracyUncert * Fo, 2) + Math.Pow(timeBaseEffectUncert * Fo, 2) / 3);




        }


        private void button2_Click(object sender, EventArgs e) //Clear
        {
            textBox2.Text = "";
            textBox6.Text = "";
            textBox10.Text = "";

            listBox1.Items.Clear();
            measuredFrequencies.Clear();

        }


        private void button4_Click(object sender, EventArgs e) //clear list
        {
            listBox1.Items.Clear();
            measuredFrequencies.Clear();
        }


        List<double> measuredFrequencies = new List<double>();

        private void button5_Click(object sender, EventArgs e)
        {
            //An instance of Form 3 is already created on Form 2 and hid (not closed) when test is selected
            Form2_Login.f3.Show();
            //When new test is clicked this instance is disposed
            this.Close();            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!textBox2.Text.Equals(""))
            {
                listBox1.Items.Add(textBox2.Text);
                textBox2.Text = "";
            }
        }

        private void AddMeasuredFrequencies()
        {
            N = 0; Fo = 0; sigma = 0;
            measuredFrequencies.Clear();
            foreach(string freq in listBox1.Items)
            {
                measuredFrequencies.Add(Double.Parse(freq, System.Globalization.NumberStyles.Float));
            }
            N = measuredFrequencies.Count;
            foreach (double frequency in measuredFrequencies) { Fo += frequency / N; }
            double sumOfDiffrences = 0;
            foreach (double frequency in measuredFrequencies) { sumOfDiffrences += Math.Pow((frequency - Fo), 2); }
            sigma = Math.Sqrt(sumOfDiffrences / (N - 1));
        }

        List<long> knownFrequencies = new List<long>();
        List<double> calFactors = new List<double>();
        List<double> calFacUncerts = new List<double>();        

        public Form4_Frequency()
        {
            InitializeComponent();
        }

        

        double ReadFrequency()
        {
            if (comboBox1.Text.Equals("kHz")) return Convert.ToDouble(textBox20.Text) * Math.Pow(10, 3);
            else if (comboBox1.Text.Equals("MHz")) return Convert.ToDouble(textBox20.Text) * Math.Pow(10, 6);
            else if (comboBox1.Text.Equals("GHz")) return Convert.ToDouble(textBox20.Text) * Math.Pow(10, 9);
            else return 0;
        }

        double ReadTrustabilityRatio()
        {
            if (comboBox2.Text.Equals("68,27%")) return 1;
            else if (comboBox2.Text.Equals("90%")) return 1.645;
            else if (comboBox2.Text.Equals("95%")) return 1.96;
            else if (comboBox2.Text.Equals("95,45% (suggested)")) return 2;
            else if (comboBox2.Text.Equals("99%")) return 2.576;
            else if (comboBox2.Text.Equals("99,73%")) return 3;
            else
            {
                warning = "Scope interval is not valid";
                return 1;
            }
        }

    }
}
