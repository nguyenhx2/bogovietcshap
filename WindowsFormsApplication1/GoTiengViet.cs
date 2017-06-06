﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;


using System.Media;
using Microsoft.Win32;
using BoGoViet.TiengViet;
using WindowsInput;
using WindowsInput.Native;
using BoGoViet;
using System.Diagnostics;

namespace WindowsFormsApplication1
{

    public partial class Form1 : Form
    {
        private IKeyboardMouseEvents m_Events;
        
        // che do go tieng anh
        private bool goEnglish = false;
        private RegistryKey rk;
        private TiengViet tv = new TiengViet();
        private bool isAltPress = false;
        private InputSimulator sim;
        // private bool pressLeftShift = false;
        // private bool canGoTiengViet = true;

        public Form1()
        {
            InitializeComponent();
            this.selectTypeInput.SelectedItem = "Telex";
            
            SubscribeGlobal();
            FormClosing += Form1_Closing;

            rk = Registry.CurrentUser.OpenSubKey
            ("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            rk.SetValue("Gõ Tiếng Việt", Application.ExecutablePath.ToString());

            sim = new InputSimulator();
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            Unsubscribe();
        }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
            }
        }

        private void SubscribeApplication()
        {
            Unsubscribe();
            Subscribe(Hook.AppEvents());
        }

        private void SubscribeGlobal()
        {
            Unsubscribe();
            Subscribe(Hook.GlobalEvents());
        }

        private void Subscribe(IKeyboardMouseEvents events)
        {
            m_Events = events;
            m_Events.KeyDown += OnKeyDown;
            m_Events.KeyUp += OnKeyUp;
            m_Events.MouseClick += OnMouseClick;
        }

        private void Unsubscribe()
        {
            if (m_Events == null) return;
            m_Events.KeyDown -= OnKeyDown;
            m_Events.KeyUp -= OnKeyUp;
            m_Events.MouseClick -= OnMouseClick;

            m_Events.Dispose();
            m_Events = null;
        }

        

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            tv.OnMouseClick(ref sender, ref e);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            /*
            if (this.special.Checked)
            {
                if (e.KeyCode == Keys.Capital)
                {
                    e.Handled = true;
                    return;
                }
            }
            
            if (e.KeyCode == Keys.LShiftKey)
            {
                pressLeftShift = true;
            }
            */
            if (e.KeyCode == Keys.LMenu)
            {
                isAltPress = true;
            }

            if (isAltPress && e.KeyCode == Keys.Z)
            {
                ChangeVietEng(this.bogoviet);
                return;
            }

            m_Events.KeyDown -= OnKeyDown;
            m_Events.KeyUp -= OnKeyUp;
            if (!goEnglish) { 
                tv.OnKeyDown(ref sender, ref e);
            }
            /*
            if (this.special.Checked)
            {
                if (e.KeyCode == Keys.PageUp)
                {
                    e.Handled = true;
                    sim.Keyboard.KeyPress(VirtualKeyCode.INSERT);
                }
                else if (pressLeftShift && e.KeyCode == Keys.PageDown)
                {
                    e.Handled = true;

                    sim.Keyboard.KeyUp(VirtualKeyCode.LSHIFT);
                    sim.Keyboard.KeyPress(VirtualKeyCode.PRIOR);
                    sim.Keyboard.KeyDown(VirtualKeyCode.LSHIFT);
                }
            }
            */
            m_Events.KeyDown += OnKeyDown;
            m_Events.KeyUp += OnKeyUp;
        }

        
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            Debug.WriteLine(e.KeyCode);
            /*
            if (this.special.Checked)
            {
                if (e.KeyCode == Keys.Capital)
                {
                    e.Handled = true;
                    return;
                }
            }
            
            if (e.KeyCode == Keys.LShiftKey)
            {
                pressLeftShift = false;
            }
            */
            if (e.KeyCode == Keys.LMenu)
            {
                isAltPress = false;
            }
            tv.OnKeyUp(ref sender, ref e);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
        }

        private void Form1_Move(object sender, EventArgs e)
        {
            if (FormWindowState.Minimized == WindowState)
                Hide();
        }
        void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            

        }

        private void ChangeVietEng(object sender)
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            goEnglish = !goEnglish;
            tv.Reset();
            SystemSounds.Beep.Play();

            NotifyIcon notifyIcon = (NotifyIcon)sender;
            if (goEnglish)
            {
                notifyIcon.Icon = new Icon(baseDir + "./english.ico");
                notifyIcon.Text = "Bộ Gõ Việt (Tắt)";
            }
            else
            {
                notifyIcon.Icon = new Icon(baseDir + "./vietnam.ico");
                notifyIcon.Text = "Bộ Gõ Việt (Bật)";
            }
        }

        private void bogoviet_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ChangeVietEng(sender);
            }
        }

        

        private void chkStartUp_CheckedChanged(object sender, EventArgs e)
        {
            if (chkStartUp.Checked) { 
                rk.SetValue("Gõ Tiếng Việt", Application.ExecutablePath.ToString());
            }
            else { 
                rk.DeleteValue("Gõ Tiếng Việt", false);
            }
        }

        private void buttonGioiThieu_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void thoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void selectTypeInput_SelectedIndexChanged(object sender, EventArgs e)
        {
            string kieuGo = (string) this.selectTypeInput.SelectedItem;
            Debug.WriteLine(kieuGo);
            tv.SetKieuGo(kieuGo);
        }
    }
}