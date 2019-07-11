using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace WindowsFormsApp1
{
    public partial class Form5_eppTest : Form
    {
        public Form5_eppTest()
        {
            InitializeComponent();

            try
            {
                DirectoryInfo outputDir = new DirectoryInfo(@"C:\Users\gokhanhalkman\Desktop\EPPlus");
                //if (!outputDir.Exists) throw new Exception("outputDir does not exist!");

                FileInfo newFile = new FileInfo(outputDir.FullName + @"\write.xlsx");
                if (newFile.Exists)
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(outputDir.FullName + @"\write.xlsx");
                }

                ExcelPackage package = new ExcelPackage(newFile);
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Inventory");
                worksheet.Cells["A1"].Value = 123;
                worksheet.Cells["B1"].Value = 456;
                worksheet.Cells["C1"].Value = 789;


                string findMax = "=MAX(A1:C1)";
                worksheet.Cells["D1"].Value = findMax;

                package.Save();

            }
            catch (Exception ex)
            {
                //Console.WriteLine(ex);
            }
            //**//**//**//**//**//**//**//**//**//

            try
            {
                DirectoryInfo outputDir = new DirectoryInfo(@"C:\Users\gokhanhalkman\Desktop\EPPlus");
                //if (!outputDir.Exists) throw new Exception("outputDir does not exist!");

                FileInfo newFile = new FileInfo(outputDir.FullName + @"\read.xlsx");
                if (newFile.Exists)
                {
                    newFile.Delete();  // ensures we create a new workbook
                    newFile = new FileInfo(outputDir.FullName + @"\read.xlsx");
                }
                ExcelPackage newPackage = new ExcelPackage(newFile);
                ExcelWorksheet newWorksheet = newPackage.Workbook.Worksheets.Add("Copied");


                FileInfo existingFile = new FileInfo(@"C:\Users\gokhanhalkman\Desktop\EPPlus\write.xlsx");
                ExcelPackage oldPackage = new ExcelPackage(existingFile);
                ExcelWorksheet oldWorksheet = oldPackage.Workbook.Worksheets[1];


                newWorksheet.Cells["A2"].Value = oldWorksheet.Cells["B1"].Value;
                newWorksheet.Cells["B2"].Value = oldWorksheet.Cells["C1"].Value;
                newWorksheet.Cells["C2"].Value = oldWorksheet.Cells["A1"].Value;
                newWorksheet.Cells["A4"].Value = oldWorksheet.Cells["D1"].Formula;




                newPackage.Save();

            }
            catch(Exception ex)
            {
                //Console.WriteLine(ex);
            }
        }

    }
}
