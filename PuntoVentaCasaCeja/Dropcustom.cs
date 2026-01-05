using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PuntoVentaCasaCeja
{
    public partial class Dropcustom : Form
    {
        string[] arr;
        Action<string> refresh;

        

        public Dropcustom()
        {
            InitializeComponent();
        }

        public void setLabelText(string text)
        {
            groupBox1.Text = text;
        }

        public void setItems(string[] items, Action<string> refreshvalue)
        {
            listBox1.DataSource = items;
            arr = items;
            refresh = refreshvalue;
        }

        private void querySelector_TextChanged(object sender, EventArgs e)
        {
            string [] temp = arr.Where(x => x.Contains(querySelector.Text)).ToArray();
            listBox1.DataSource = temp;
            
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            string value = listBox1.SelectedItem?.ToString();
            refresh(value);
            Dispose();
        }

        private void querySelector_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                string value = listBox1.SelectedItem?.ToString();
                refresh(value);
                Dispose();
            }else if(e.KeyCode == Keys.Down)
            {
                listBox1.SelectedIndex += listBox1.Items.Count > listBox1.SelectedIndex+1 ? 1 : 0;
            }
            else if (e.KeyCode == Keys.Up)
            {
                listBox1.SelectedIndex -= 0 <= listBox1.SelectedIndex - 1 ? 1 : 0;
            }
        }

        private void listBox1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string value = listBox1.SelectedItem?.ToString();
                refresh(value);
                Dispose();
            }
        }
    }
}
