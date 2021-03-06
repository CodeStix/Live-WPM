﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace LiveWPM
{
    public partial class FormWPM : Form
    {
        private int[] history;
        private int currentKeyPresses = 0;
        private bool showSuffix = true;
        private bool enableSmoothing = true;
        private float lerpingWPM = 0f;
        private float currentWPM = 0f;

        public const int MOVING_AVERAGE = 5;
        public static readonly Keys[] IGNORED_KEYS = new Keys[] { 
            Keys.Back, Keys.ShiftKey, Keys.LShiftKey, Keys.RShiftKey,
            Keys.Shift, Keys.Alt, Keys.Control, Keys.ControlKey, Keys.RControlKey, Keys.LControlKey, 
            Keys.CapsLock, Keys.LWin, Keys.RWin
        };

        public FormWPM()
        {
            InitializeComponent();
            FormClosing += FormWPM_FormClosing;
            showWPMSuffixToolStripMenuItem.Checked = showSuffix;
            enableSmoothingToolStripMenuItem.Checked = enableSmoothing;
        }

        private void FormWPM_FormClosing(object sender, FormClosingEventArgs e)
        {
            WindowsKeyboardHook.OnGlobalKey -= WindowsKeyboardHook_OnGlobalKey;
        }

        private void FormWPM_Load(object sender, EventArgs e)
        {
            history = new int[MOVING_AVERAGE];
            WindowsKeyboardHook.OnGlobalKey += WindowsKeyboardHook_OnGlobalKey;
        }

        private void WindowsKeyboardHook_OnGlobalKey(Keys obj)
        {
            if (IGNORED_KEYS.Contains(obj)) 
                return;
            currentKeyPresses++;
        }

        private void ShiftHistory(int lastValue)
        {
            for(int i = 0; i < history.Length - 1; i++)
                history[i] = history[i + 1];

            history[history.Length - 1] = lastValue;
        }

        private float CalculateWPM()
        {
            float wpm = 0;
            for(int i = 0; i < history.Length; i++)
            {
                wpm += history[i] / 5f * 60f;
            }
            return wpm / history.Length;
        }

        private void updateTimer_Tick(object sender, EventArgs e)
        {
            ShiftHistory(currentKeyPresses);
            currentKeyPresses = 0;
            currentWPM = CalculateWPM();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
            //Environment.Exit(0);
        }

        private void showWPMSuffixToolStripMenuItem_CheckedChanged(object sender, EventArgs e)
        {
            showSuffix = showWPMSuffixToolStripMenuItem.Checked;
        }

        private float Lerp(float a, float b, float t)
        {
            return (1f - t) * a + t * b;
        }

        private void lerpTimer_Tick(object sender, EventArgs e)
        {
            if (enableSmoothing)
            {
                lerpingWPM = Lerp(lerpingWPM, currentWPM, 0.1f);
            }
            else
            {
                lerpingWPM = currentWPM;
            }

            labelWPM.Text = $"{Math.Round(lerpingWPM)}" + (showSuffix ? " WPM" : "");
        }

        private void enableSmoothingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            enableSmoothing = enableSmoothingToolStripMenuItem.Checked;
        }
    }
}
