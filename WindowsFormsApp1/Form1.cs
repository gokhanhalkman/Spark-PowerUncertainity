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
    public partial class Form1 : Form
    {
        
        double frequency;
        double CFstd; 
        double Pref; //reference power

        double Po; //Average measured Power
        double sigma = 0, N = 0; //standart deviation, arithmetic average

        double Ms; //Mismatch coefficent
        double DeltaPm; //Error of powermeter
        double DeltaPmacc; //Error of powermeter accuracy
        double Lps; //Linearity
        double rhoSG, rhoPS; //
        double nominalValue;
        double accuracy;

        //Partial uncertainties
        double UPo; //k=1
        double UCFstd; //k=1/2
        double UMs; //k=1/root2
        double UPm; //k=1/root3
        double UPmacc; //
        double ULps; //

        // Sensitivity coeffients
        /// PR = ((Po + DeltaPm + DeltaPmacc)*Ms)/(CFstd*Lps)
        double CPo, CCFstd, CMs, CPm, CPmacc, CLps;

        //Partial Variances
        /// PVi = Ui^2*k^2*Ci^2
        double PVPo, PVCFstd, PVMs, PVPm, PVPmacc, PVLps;

        
        double TV; //TotalVariance
        double StdUnc; //Standard uncertainty
        double ExtUnc; //Extended uncertainty
        double DecRelUnc; //Decleration of relative uncertainty

        // // // // // // // // // // // // // // // // // // // // // // 
        private void button1_Click(object sender, EventArgs e) //Start
        {
            AddMeasuredPowers(); // average, standerd deviation
            if(measuredPowers.Count < 3)
            {
                label36.Text = "Not enough power samples";           
            }
            else
            {
                if (textBox5.Text.Equals("")|| textBox7.Text.Equals("") || textBox8.Text.Equals("") || textBox12.Text.Equals("") || textBox18.Text.Equals(""))
                {
                    label36.Text = "Inputs cannot be left empty";
                }
                else
                {
                    label36.Text = "";

                    frequency = ReadFrequency();
                    ReadValues();
                    //GetDataFromExcell(); //Power

                    //WriteCalibrationFactorAndUncertainty();
                    //WriteReferencePower();

                    Simulate();

                    //CalculateUncertainties();
                    //CalculateSensitivityCoefficents();
                    //CalculatePartialVariances();
                    //CalculateTotalVariance();

                    WriteUncertainties();

                    textBox13.Text = Po.ToString();
                    textBox14.Text = sigma.ToString();

                    //ListResults();
                }

            }
        }

        void Simulate()
        {
            WriteCalibrationFactorAndUncertainty();
            WriteReferencePower();

            UPo = sigma / Math.Sqrt(N);
            UMs = 2 * rhoPS * rhoSG * 1.5; //connector loss
            //UCFstd //interpolated with calibration factor
            UPm = (accuracy * Po) / nominalValue; //1mW reference
            UPmacc = 0.005 * Po;
            ULps = 0.0025;

            CPo = Ms / (CFstd * Lps); //
            CPm = Ms / (CFstd * Lps); //
            CPmacc = Ms / (CFstd * Lps);
            CMs = (Po + DeltaPm + DeltaPmacc) / (CFstd * Lps );
            CCFstd = ((-1) * (Po + DeltaPm + DeltaPmacc) * Ms) / (Math.Pow(CFstd, 2) * Lps);
            CLps = ((-1) * (Po + DeltaPm + DeltaPmacc) * Ms) / (Math.Pow(Lps, 2) * CFstd);

            PVPo = Math.Pow(UPo * 1 * CPo, 2);               //k=1
            PVCFstd = Math.Pow(UCFstd * 0.5 * CCFstd, 2);    //k=1/2
            PVMs = Math.Pow(UMs * 0.707 * CMs, 2);           //k=1/root2
            PVPm = Math.Pow(UPm * 0.577 * CPm, 2);           //k=1/root3
            PVPmacc = Math.Pow(UPmacc * 0.577 * CPmacc, 2);  //k=1/root3
            PVLps = Math.Pow(ULps * 0.577 * CLps, 2);        //k=1/root3

            TV = PVPo + PVMs + PVCFstd + PVPm + PVPmacc + PVLps;
            textBox15.Text = TV.ToString();
            StdUnc = Math.Sqrt(TV);
            textBox16.Text = StdUnc.ToString();
            ExtUnc = 2 * StdUnc; // 2 --> kapsam aralığı
            textBox10.Text = ExtUnc.ToString();
            DecRelUnc = ExtUnc / Pref;

            textBox17.Text = Po.ToString() + "\u00B1" + ExtUnc.ToString();

        }
        List<double> extuncerts = new List<double>();

        void ListResults()
        {
            for(int i = 0; i < 100; i++)
            {
                frequency = 10000 + i * 10000;
                WriteCalibrationFactorAndUncertainty();
                WriteReferencePower();

                CalculateUncertainties();
                CalculateSensitivityCoefficents();
                CalculatePartialVariances();
                CalculateTotalVariance();
                extuncerts.Add(ExtUnc);
            }

            label4.Text = extuncerts[0].ToString();
            label5.Text = extuncerts[1].ToString();
            label6.Text = extuncerts[2].ToString();
            label7.Text = extuncerts[10].ToString();
            label8.Text = extuncerts[20].ToString();
            label9.Text = extuncerts[80].ToString();
        }

        // // // // // // // // // // // // // // // // // // // // // //

        void WriteUncertainties()
        {
            label4.Text = UPo.ToString();
            label5.Text = UMs.ToString();
            label6.Text = UCFstd.ToString();
            label7.Text = UPm.ToString();
            label8.Text = UPmacc.ToString();
            label9.Text = ULps.ToString();

            label29.Text = CPo.ToString();
            label28.Text = CMs.ToString();
            label27.Text = CCFstd.ToString();
            label26.Text = CPm.ToString();
            label12.Text = CPmacc.ToString();
            label25.Text = CLps.ToString();

            label35.Text = PVPo.ToString();
            label34.Text = PVMs.ToString();
            label32.Text = PVCFstd.ToString();
            label33.Text = PVPm.ToString();
            label30.Text = PVPmacc.ToString();
            label31.Text = PVLps.ToString();
        }

        void CalculateTotalVariance()
        {
            TV = PVPo + PVMs + PVCFstd + PVPm + PVPmacc + PVLps;
            textBox15.Text = TV.ToString();
            StdUnc = Math.Sqrt(TV);
            textBox16.Text = StdUnc.ToString();
            ExtUnc = 2 * StdUnc; // 2 --> kapsam aralığı
            textBox10.Text = ExtUnc.ToString();
            DecRelUnc = ExtUnc / Pref;

            textBox17.Text = Po.ToString() + "\u00B1" + ExtUnc.ToString();
        }
        
        void CalculateUncertainties()
        {
            UPo =    sigma / Math.Sqrt(N);
            UMs =    2 * rhoPS * rhoSG*1.5; //connector loss
            //UCFstd
            UPm =    ( accuracy * Po) / nominalValue; //1mW reference
            UPmacc = 0.005*Po;
            ULps =   0.0025;
        }


        void WriteReferencePower()
        {
            Pref =( (Po + DeltaPm + DeltaPmacc) * Ms) / (Lps * CFstd);
            textBox6.Text = Pref.ToString();
        }

       /* void GetDataFromExcell()
        {
            //Currently read from form, later from excell
            Po = Convert.ToDouble(textBox2.Text);


        }*/

        void ReadValues()
        {
            
            Ms = textBox4.Text.Equals("") ? 1 : Convert.ToInt32(textBox4.Text);
            DeltaPm = textBox3.Text.Equals("") ? 1 : Convert.ToInt32(textBox3.Text);
            DeltaPmacc = textBox1.Text.Equals("") ? 0 : Convert.ToInt32(textBox1.Text);
            Lps = textBox11.Text.Equals("") ? 1 : Convert.ToInt32(textBox11.Text);

            rhoSG = Convert.ToDouble(textBox8.Text);
            rhoPS = Convert.ToDouble(textBox7.Text);
            nominalValue = Convert.ToDouble(textBox5.Text);
            accuracy = Convert.ToDouble(textBox18.Text);
        }


        void CalculatePartialVariances()
        {
            PVPo =    Math.Pow(UPo * 1 * CPo, 2);               //k=1
            PVCFstd = Math.Pow(UCFstd * 0.5 * CCFstd, 2);    //k=1/2
            PVMs =    Math.Pow(UMs * 0.707 * CMs, 2);           //k=1/root2
            PVPm =    Math.Pow(UPm * 0.577 * CPm, 2);           //k=1/root3
            PVPmacc = Math.Pow(UPmacc * 0.577 * CPmacc, 2);  //k=1/root3
            PVLps =   Math.Pow(ULps * 0.577 * CLps, 2);        //k=1/root3
        }

        private void button2_Click(object sender, EventArgs e) //Clear
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
            textBox8.Text = "";
            textBox9.Text = "";
            textBox10.Text = "";
            textBox11.Text = "";
            textBox12.Text = "";
            textBox15.Text = "";
            textBox16.Text = "";
            listBox1.Items.Clear();
            measuredPowers.Clear();

        }

        private void label2_Click(object sender, EventArgs e)
        {
            label2.Text = Round(11.183321, 2).ToString();
        }

        private void button4_Click(object sender, EventArgs e) //clear list
        {
            listBox1.Items.Clear();
            measuredPowers.Clear();
        }

        List<double> measuredPowers = new List<double>();

        private void button3_Click(object sender, EventArgs e)
        {
            if (!textBox2.Text.Equals(""))
            {
                double pow = Convert.ToDouble(textBox2.Text);
                //measuredPowers.Add(pow);
                textBox2.Text = "";
                listBox1.Items.Add(pow);
                //listBox1.Height = measuredPowers.Count * listBox1.Font.Height;
                //if (measuredPowers.Count <= 5) listBox1.Height = 5 * listBox1.Font.Height;
            }
            
        }

        private void AddMeasuredPowers()
        {
            N = 0; Po = 0; sigma = 0;
            measuredPowers.Clear();
            foreach(double pow in listBox1.Items)
            {
                measuredPowers.Add(pow);
            }
            N = measuredPowers.Count;
            foreach (double power in measuredPowers) { Po += power / N; }
            double sumOfDiffrences = 0;
            foreach (double power in measuredPowers) { sumOfDiffrences += Math.Pow((power - Po), 2); }
            sigma = Math.Sqrt(sumOfDiffrences / (N - 1));
        }


        void CalculateSensitivityCoefficents()
        {
            CPo =     Ms / (CFstd * Lps); //
            CPm =     Ms / (CFstd * Lps); //
            CPmacc =  Ms / (CFstd * Lps);
            CMs =    (Po + DeltaPm + DeltaPmacc) / (CFstd * Lps);
            CCFstd = ((-1) * (Po + DeltaPm + DeltaPmacc) * Ms )/ (Math.Pow(CFstd, 2) * Lps);
            CLps =   ((-1) * (Po + DeltaPm + DeltaPmacc) * Ms )/ (Math.Pow(Lps, 2) * CFstd);
        }
 
        List<long> knownFrequencies = new List<long>();
        List<double> calFactors = new List<double>();
        List<double> calFacUncerts = new List<double>();        

        public Form1()
        {
            InitializeComponent();
            MakeKnownList(knownFrequencies, calFactors, calFacUncerts);
        }

        private void MakeKnownList(List<long> kf, List<double> cf, List<double> cfu)
        {
            kf.Add(9000); cf.Add(0.987); cfu.Add(0.0076);//9k
            kf.Add(30000); cf.Add(0.984); cfu.Add(0.0076);
            kf.Add(50000); cf.Add(0.986); cfu.Add(0.0076);
            kf.Add(100000); cf.Add(0.985); cfu.Add(0.0076);
            kf.Add(300000); cf.Add(0.985); cfu.Add(0.0076);
            kf.Add(500000); cf.Add(0.985); cfu.Add(0.0076);//500k

            kf.Add(1000000); cf.Add(0.987); cfu.Add(0.0076);//1M
            kf.Add(3000000); cf.Add(0.968); cfu.Add(0.0076);
            kf.Add(5000000); cf.Add(0.983); cfu.Add(0.0076);

            kf.Add(10000000); cf.Add(0.971); cfu.Add(0.0082);//10M
            kf.Add(30000000); cf.Add(0.992); cfu.Add(0.0082);
            kf.Add(50000000); cf.Add(1); cfu.Add(0.01);

            kf.Add(100000000); cf.Add(0.995); cfu.Add(0.0072);//100M
            kf.Add(300000000); cf.Add(0.992); cfu.Add(0.0073);
            kf.Add(500000000); cf.Add(0.99); cfu.Add(0.0073);
            kf.Add(800000000); cf.Add(0.998); cfu.Add(0.0074);

            kf.Add(1000000000); cf.Add(1.004); cfu.Add(0.0075);//1G
            kf.Add(1200000000); cf.Add(1.013); cfu.Add(0.0076);
            kf.Add(1500000000); cf.Add(1.014); cfu.Add(0.0081);
            kf.Add(2000000000); cf.Add(1.01); cfu.Add(0.0083);
            kf.Add(3000000000); cf.Add(1.005); cfu.Add(0.0086);
            kf.Add(4000000000); cf.Add(0.997); cfu.Add(0.0089);
            kf.Add(5000000000); cf.Add(0.994); cfu.Add(0.0092);
            kf.Add(6000000000); cf.Add(1.001); cfu.Add(0.0096);
            kf.Add(7000000000); cf.Add(0.999); cfu.Add(0.01);
            kf.Add(8000000000); cf.Add(0.994); cfu.Add(0.01);
            kf.Add(9000000000); cf.Add(0.996); cfu.Add(0.011);//9G 
        }


        double ReadFrequency()
        {
            if (comboBox1.Text.Equals("kHz")) return Convert.ToDouble(textBox12.Text) * Math.Pow(10, 3);
            else if (comboBox1.Text.Equals("MHz")) return Convert.ToDouble(textBox12.Text) * Math.Pow(10, 6);
            else if (comboBox1.Text.Equals("GHz")) return Convert.ToDouble(textBox12.Text) * Math.Pow(10, 9);
            else return 0;
        }

        bool WriteCalibrationFactorAndUncertainty()
        {
            double cf = 0;
            double cfu = 0;
            // is the input frequency a known frequency in the list
            for(int i = 0; i < knownFrequencies.Count; i++)
            {
                if (frequency == knownFrequencies[i])
                {
                    cf = calFactors[i];
                    cfu = calFacUncerts[i];
                    CFstd = cf;
                    UCFstd = cfu;
                    textBox9.Text = CFstd.ToString();
                    return true;
                }
            }
            // otherwise which interval is input frequency in? //Assume input is larger than 9e+3 and smaller than 9e+9
            for (int i = 0; i < knownFrequencies.Count-1; i++)
            {
                if (frequency > knownFrequencies[i] && frequency < knownFrequencies[i+1])
                {
                    cf = Interpolate(knownFrequencies[i], knownFrequencies[i+1], calFactors[i], calFactors[i+1], frequency);
                    cfu = Interpolate(knownFrequencies[i], knownFrequencies[i+1], calFacUncerts[i], calFacUncerts[i+1], frequency);
                    CFstd = cf;
                    UCFstd = cfu;
                    textBox9.Text = CFstd.ToString();
                    return true;
                }
            }
            CFstd = cf;
            textBox9.Text = CFstd.ToString();
            return true;
        }

        double Interpolate(long x1, long x2, double y1, double y2, double f)
        {
            double result = 0;
            double w1 = Convert.ToDouble(x1), w2 = Convert.ToDouble(x2);
            result = y1 + (f - w1) * (y2 - y1) / (w2 - w1);
            return result;
        }

        double Round(double d, int i)
        {
            double r = Math.Pow(10, i);
            return Convert.ToDouble(Convert.ToInt32(d * r))/ r;
        }

    }
}
