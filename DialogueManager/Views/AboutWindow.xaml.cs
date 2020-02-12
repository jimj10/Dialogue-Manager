/* 
 * Copyright(c) 2020 Department of Informatics, University of Sussex.
 * Dr. Kate Howland <grp-1782@sussex.ac.uk>
 * Licensed under the Microsoft Public License; you may not
 * use this file except in compliance with the License. 
 * You may obtain a copy of the License at 
 * https://opensource.org/licenses/MS-PL 
 * 
 */
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace DialogueManager.Views
{

    public partial class AboutWindow : Window
    {
        public string Version { get; set; }
        public string Copyright { get; set; }
        private bool MPLDisplayed = false;
        private bool APLDisplayed = false;
        private bool MitDisplayed = false;
        private bool NewtonSoftDisplayed = false;

        public AboutWindow()
        {
            // Read Version data from Assembly version info (discard revision number)
            string assemblyVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            int idx = assemblyVersion.LastIndexOf('.');
            Version = "Version " + assemblyVersion.Substring(0, idx);
            Copyright = "Copyright © " + DateTime.Now.Year.ToString() + " ";
            InitializeComponent();
            MicrosoftPublicLicense.Visibility = Visibility.Collapsed;
            MicrosoftPublicLicense.Text = LicenseText.MicrosoftPublicLicense;
            ApacheLicense.Visibility = Visibility.Collapsed;
            ApacheLicense.Text = LicenseText.ApacheLicense;
            MitLicense.Visibility = Visibility.Collapsed;
            MitLicense.Text = LicenseText.MitLicense;
            NewtonSoftLicense.Visibility = Visibility.Collapsed;
            NewtonSoftLicense.Text = LicenseText.NewtonSoftLicense;
            DataContext = this;
        }


        // Couldn't get window resizing to work with MVVM
        private void OnNavigate(object sender, RequestNavigateEventArgs e)
        {
            // window size changed with license display
            int mplHeight = 490;
            int aplHeight = 135;
            int mitHeight = 245;
            int stdWidth = 430;
            int extendedWidth = 800;

            if (e.Uri.ToString().Equals("MPL"))
            {
                if (APLDisplayed)
                {
                    ApacheLicense.Visibility = Visibility.Collapsed;
                    AWin.Height -= aplHeight;
                    APLDisplayed = false;
                }
                else if (MitDisplayed || NewtonSoftDisplayed)
                {
                    MitLicense.Visibility = Visibility.Collapsed;
                    NewtonSoftLicense.Visibility = Visibility.Collapsed;
                    AWin.Height -= mitHeight;
                    MitDisplayed = false;
                    NewtonSoftDisplayed = false;
                }
                if (!MPLDisplayed)
                {
                    MicrosoftPublicLicense.Visibility = Visibility.Visible;
                    AWin.Height += mplHeight;
                    AWin.Width = extendedWidth;
                }
                else
                {
                    MicrosoftPublicLicense.Visibility = Visibility.Collapsed;
                    AWin.Height -= mplHeight;
                    AWin.Width = stdWidth;
                }
                MPLDisplayed = !MPLDisplayed;
            }
            else if (e.Uri.ToString().Equals("APL"))
            {
                if (MPLDisplayed)
                {
                    MicrosoftPublicLicense.Visibility = Visibility.Collapsed;
                    AWin.Height -= mplHeight;
                    MPLDisplayed = false;
                }
                else if (MitDisplayed || NewtonSoftDisplayed)
                {
                    MitLicense.Visibility = Visibility.Collapsed;
                    NewtonSoftLicense.Visibility = Visibility.Collapsed;
                    AWin.Height -= mitHeight;
                    MitDisplayed = false;
                    NewtonSoftDisplayed = false;
                }
                else if (!APLDisplayed)
                {
                    ApacheLicense.Visibility = Visibility.Visible;
                    AWin.Height += aplHeight;
                    AWin.Width = extendedWidth;
                }
                else 
                {
                    ApacheLicense.Visibility = Visibility.Collapsed;
                    AWin.Height -= aplHeight;
                    AWin.Width = stdWidth;
                }
                APLDisplayed = !APLDisplayed;
            }
            else if (e.Uri.ToString().Equals("MIT"))
            {
                if (MPLDisplayed)
                {
                    MicrosoftPublicLicense.Visibility = Visibility.Collapsed;
                    AWin.Height -= mplHeight;
                    MPLDisplayed = false;
                }
                else if (APLDisplayed)
                {
                    ApacheLicense.Visibility = Visibility.Collapsed;
                    AWin.Height -= aplHeight;
                    APLDisplayed = false;
                }
                else if (NewtonSoftDisplayed)
                {
                    NewtonSoftLicense.Visibility = Visibility.Collapsed;
                    AWin.Height -= mitHeight;
                    NewtonSoftDisplayed = false;
                }
                else if (!MitDisplayed)
                {
                    MitLicense.Visibility = Visibility.Visible;
                    AWin.Height += mitHeight;
                    AWin.Width = extendedWidth;
                }
                else
                {
                    MitLicense.Visibility = Visibility.Collapsed;
                    AWin.Height -= mitHeight;
                    AWin.Width = stdWidth;
                }
                MitDisplayed = !MitDisplayed;
            }
            else if (e.Uri.ToString().Equals("NewtonSoftLicense"))
            {
                if (MPLDisplayed)
                {
                    MicrosoftPublicLicense.Visibility = Visibility.Collapsed;
                    AWin.Height -= mplHeight;
                    MPLDisplayed = false;
                }
                else if (APLDisplayed)
                {
                    ApacheLicense.Visibility = Visibility.Collapsed;
                    AWin.Height -= aplHeight;
                    APLDisplayed = false;
                }
                else if (MitDisplayed)
                {
                    MitLicense.Visibility = Visibility.Collapsed;
                    AWin.Height -= mitHeight;
                    MitDisplayed = false;
                }
                else if (!NewtonSoftDisplayed)
                {
                    NewtonSoftLicense.Visibility = Visibility.Visible;
                    AWin.Height += mitHeight;
                    AWin.Width = extendedWidth;
                }
                else
                {
                    NewtonSoftLicense.Visibility = Visibility.Collapsed;
                    AWin.Height -= mitHeight;
                    AWin.Width = stdWidth;
                }
                NewtonSoftDisplayed = !NewtonSoftDisplayed;
            }
            else
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            }
            e.Handled = true;
        }
    }
}
