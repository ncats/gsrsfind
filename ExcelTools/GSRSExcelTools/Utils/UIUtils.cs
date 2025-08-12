using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;

namespace GSRSExcelTools.Utils
{
    class UIUtils
    {
        public static void ShowMessageToUser(string message)
        {
            MessageBox.Show(message);
        }

        public static bool GetUserYesNo(string message)
        {
            DialogResult result= MessageBox.Show(message, "Select 'Yes' or 'No' ", 
                MessageBoxButtons.YesNoCancel);
            return (result == DialogResult.Yes);
        }

        public static DialogYesNoCancel GetUserYesNoCancel(string message,
            string title = "Select 'Yes', 'No' or 'Cancel'")
        {
            DialogResult result = MessageBox.Show(message, title,
                MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Yes) return DialogYesNoCancel.Yes;
            if (result == DialogResult.No) return DialogYesNoCancel.No;
            return DialogYesNoCancel.Cancel;
        }

        public static string GetUserFileSelection(string fileFilter, string header)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = fileFilter;
            openFileDialog.Title = header;
            openFileDialog.Multiselect = false;
            if ( openFileDialog.ShowDialog() == DialogResult.OK)
            {
                return openFileDialog.FileName;
            }
            return string.Empty;
        }
    }
}
