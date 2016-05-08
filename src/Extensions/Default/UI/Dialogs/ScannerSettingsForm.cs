﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="ScannerSettingsForm.cs" company="Intel Corporation">
//
// Copyright (c) 2013-2015 Intel Corporation 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Security.Permissions;
using System.Windows.Forms;
using ACAT.Lib.Core.PanelManagement;
using ACAT.Lib.Core.Utility;
using ACAT.Lib.Core.WidgetManagement;
using ACAT.Lib.Core.Widgets;
using ACAT.Lib.Extension;

#region SupressStyleCopWarnings

[module: SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1126:PrefixCallsCorrectly",
        Scope = "namespace",
        Justification = "Not needed. ACAT naming conventions takes care of this")]
[module: SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1101:PrefixLocalCallsWithThis",
        Scope = "namespace",
        Justification = "Not needed. ACAT naming conventions takes care of this")]
[module: SuppressMessage(
        "StyleCop.CSharp.ReadabilityRules",
        "SA1121:UseBuiltInTypeAlias",
        Scope = "namespace",
        Justification = "Since they are just aliases, it doesn't really matter")]
[module: SuppressMessage(
        "StyleCop.CSharp.DocumentationRules",
        "SA1200:UsingDirectivesMustBePlacedWithinNamespace",
        Scope = "namespace",
        Justification = "ACAT guidelines")]
[module: SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1309:FieldNamesMustNotBeginWithUnderscore",
        Scope = "namespace",
        Justification = "ACAT guidelines. Private fields begin with an underscore")]
[module: SuppressMessage(
        "StyleCop.CSharp.NamingRules",
        "SA1300:ElementMustBeginWithUpperCaseLetter",
        Scope = "namespace",
        Justification = "ACAT guidelines. Private/Protected methods begin with lowercase")]

#endregion SupressStyleCopWarnings

namespace ACAT.Extensions.Default.UI.Dialogs
{
    /// <summary>
    /// Sets the scanner settings such as the various timings,
    /// number of iterations for scanning etc.
    /// </summary>
    [DescriptorAttribute("3BC26865-9D90-4DFD-BFAB-D7E69DDFA789",
                        "ScannerSettingsForm",
                        "Mute Settings Dialog")]
    public partial class ScannerSettingsForm : Form, IDialogPanel
    {
        /// <summary>
        /// The DialogCommon object
        /// </summary>
        private readonly DialogCommon _dialogCommon;

        /// <summary>
        ///  Did the user change anything?
        /// </summary>
        private bool _isDirty;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ScannerSettingsForm()
        {
            InitializeComponent();

            _dialogCommon = new DialogCommon(this);

            if (!_dialogCommon.Initialize())
            {
                Log.Debug("Initialization error");
            }

            initWidgetSettings(Common.AppPreferences);

            Load += ScannerSettingsForm_Load;
            FormClosing += ScannerSettingsForm_FormClosing;
        }

        /// <summary>
        /// Gets the descriptor for this class
        /// </summary>
        public IDescriptor Descriptor
        {
            get { return DescriptorAttribute.GetDescriptor(GetType()); }
        }

        /// <summary>
        /// Gets the synch object
        /// </summary>
        public SyncLock SyncObj
        {
            get { return _dialogCommon.SyncObj; }
        }

        /// <summary>
        /// Set the form style
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                return DialogCommon.SetFormStyles(base.CreateParams);
            }
        }

        /// <summary>
        /// Triggered when a widget is actuated.
        /// </summary>
        /// <param name="widget">Which one triggered?</param>
        ///
        public void OnButtonActuated(Widget widget)
        {
            Log.Debug("**Actuate** " + widget.Name + " Value: " + widget.Value);

            var value = widget.Value;
            if (String.IsNullOrEmpty(value))
            {
                return;
            }

            Invoke(new MethodInvoker(delegate()
            {
                switch (value)
                {
                    case "valButtonBack":
                        quit();
                        break;

                    case "valButtonSave":
                        saveSettingsAndQuit();
                        break;

                    case "valButtonRestoreDefaults":
                        loadDefaultSettings();
                        break;
                }
            }));
        }

        /// <summary>
        /// Pause the scanner
        /// </summary>
        public void OnPause()
        {
            _dialogCommon.OnPause();
        }

        /// <summary>
        /// Resume paused scanner
        /// </summary>
        public void OnResume()
        {
            _dialogCommon.OnResume();
        }

        /// <summary>
        /// Not used
        /// </summary>
        /// <param name="command"></param>
        /// <param name="handled"></param>
        public void OnRunCommand(string command, ref bool handled)
        {
            switch (command)
            {
                default:
                    handled = false;
                    break;
            }
        }

        /// <summary>
        /// Form is closing release resources
        /// </summary>
        /// <param name="e">event arg</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _dialogCommon.OnFormClosing(e);
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Window proc
        /// </summary>
        /// <param name="m"></param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        protected override void WndProc(ref Message m)
        {
            _dialogCommon.HandleWndProc(m);
            base.WndProc(ref m);
        }

        /// <summary>
        /// Gets the values from the form and updates the settings. Returns
        /// the preferences object with the new settings
        /// </summary>
        /// <returns>The </returns>
        private ACATPreferences getSettingsFromUI()
        {
            var rootWidget = _dialogCommon.GetRootWidget();
            var prefs = ACATPreferences.Load();

            prefs.SelectClick = Common.AppPreferences.SelectClick = (rootWidget.Finder.FindChild(pbSelectingClick.Name) as CheckBoxWidget).GetState();

            prefs.HalfScanIterations = Common.AppPreferences.HalfScanIterations = (rootWidget.Finder.FindChild(tbEveryHalf.Name) as SliderWidget).GetState(SliderWidget.SliderUnitsOnes);
            prefs.RowScanIterations = Common.AppPreferences.RowScanIterations = (rootWidget.Finder.FindChild(tbEveryRow.Name) as SliderWidget).GetState(SliderWidget.SliderUnitsOnes);
            prefs.ColumnScanIterations = Common.AppPreferences.ColumnScanIterations = (rootWidget.Finder.FindChild(tbEveryColumn.Name) as SliderWidget).GetState(SliderWidget.SliderUnitsOnes);
            prefs.WordPredictionScanIterations = Common.AppPreferences.WordPredictionScanIterations = (rootWidget.Finder.FindChild(tbWordPrediction.Name) as SliderWidget).GetState(SliderWidget.SliderUnitsOnes);

            prefs.AcceptTime = Common.AppPreferences.AcceptTime = (rootWidget.Finder.FindChild(tbAcceptTime.Name) as SliderWidget).GetState(SliderWidget.SliderUnitsThousandths);
            prefs.SteppingTime = Common.AppPreferences.SteppingTime = (rootWidget.Finder.FindChild(tbSteppingTime.Name) as SliderWidget).GetState(SliderWidget.SliderUnitsThousandths);
            prefs.HesitateTime = Common.AppPreferences.HesitateTime = (rootWidget.Finder.FindChild(tbHesitateTime.Name) as SliderWidget).GetState(SliderWidget.SliderUnitsThousandths);
            prefs.WordPredictionHesitateTime = Common.AppPreferences.WordPredictionHesitateTime = (rootWidget.Finder.FindChild(tbWordListHesitateTime.Name) as SliderWidget).GetState(SliderWidget.SliderUnitsThousandths);
            prefs.TabScanTime = Common.AppPreferences.TabScanTime = (rootWidget.Finder.FindChild(tbTabScanTime.Name) as SliderWidget).GetState(SliderWidget.SliderUnitsThousandths);
            prefs.FirstRepeatTime = Common.AppPreferences.FirstRepeatTime = (rootWidget.Finder.FindChild(tbFirstRepeatTime.Name) as SliderWidget).GetState(SliderWidget.SliderUnitsThousandths);

            return prefs;
        }

        /// <summary>
        /// Initialize the controls on the form based on
        /// the corresponding values in the preferences
        /// </summary>
        /// <param name="prefs">ACAT preferences</param>
        private void initWidgetSettings(ACATPreferences prefs)
        {
            // TOGGLE IMAGE BUTTON KEYS USED FOR BOTTOM-LEFT PANEL
            var rootWidget = _dialogCommon.GetRootWidget();

            (rootWidget.Finder.FindChild(pbSelectingClick.Name) as CheckBoxWidget).SetState(prefs.SelectClick);

            (rootWidget.Finder.FindChild(tbEveryHalf.Name) as SliderWidget).SetState(prefs.HalfScanIterations, SliderWidget.SliderUnitsOnes);
            (rootWidget.Finder.FindChild(tbEveryRow.Name) as SliderWidget).SetState(prefs.RowScanIterations, SliderWidget.SliderUnitsOnes);
            (rootWidget.Finder.FindChild(tbEveryColumn.Name) as SliderWidget).SetState(prefs.ColumnScanIterations, SliderWidget.SliderUnitsOnes);
            (rootWidget.Finder.FindChild(tbWordPrediction.Name) as SliderWidget).SetState(prefs.WordPredictionScanIterations, SliderWidget.SliderUnitsOnes);

            (rootWidget.Finder.FindChild(tbAcceptTime.Name) as SliderWidget).SetState(prefs.AcceptTime, SliderWidget.SliderUnitsThousandths);
            (rootWidget.Finder.FindChild(tbSteppingTime.Name) as SliderWidget).SetState(prefs.SteppingTime, SliderWidget.SliderUnitsThousandths);
            (rootWidget.Finder.FindChild(tbHesitateTime.Name) as SliderWidget).SetState(prefs.HesitateTime, SliderWidget.SliderUnitsThousandths);
            (rootWidget.Finder.FindChild(tbWordListHesitateTime.Name) as SliderWidget).SetState(prefs.WordPredictionHesitateTime, SliderWidget.SliderUnitsThousandths);
            (rootWidget.Finder.FindChild(tbTabScanTime.Name) as SliderWidget).SetState(prefs.TabScanTime, SliderWidget.SliderUnitsThousandths);
            (rootWidget.Finder.FindChild(tbFirstRepeatTime.Name) as SliderWidget).SetState(prefs.FirstRepeatTime, SliderWidget.SliderUnitsThousandths);
        }

        /// <summary>
        /// Loads default settings from the preferences file
        /// </summary>
        private void loadDefaultSettings()
        {
            if (DialogUtils.Confirm(this, "Restore default settings?"))
            {
                // get entire default file and just set those settings that belong to this preferences screen
                initWidgetSettings(ACATPreferences.LoadDefaultSettings());
                _isDirty = true;
            }
        }

        /// <summary>
        /// Confirms with the user and quits the dialog
        /// </summary>
        private void quit()
        {
            bool quit = true;

            if (_isDirty)
            {
                if (!DialogUtils.Confirm(this, "Changes not saved. Quit?"))
                {
                    quit = false;
                }
            }

            if (quit)
            {
                Windows.CloseForm(this);
            }
        }

        /// <summary>
        /// Saves the settings and quits the dialog
        /// </summary>
        private void saveSettingsAndQuit()
        {
            if (_isDirty && DialogUtils.Confirm(this, "Save settings?"))
            {
                getSettingsFromUI().Save();

                _isDirty = false;
                Common.AppPreferences.NotifyPreferencesChanged();
            }

            Windows.CloseForm(this);
        }

        /// <summary>
        /// Form is closing. Releases resources
        /// </summary>
        private void ScannerSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _dialogCommon.OnClosing();
        }

        /// <summary>
        /// Form has been loaded. Initialize
        /// </summary>
        private void ScannerSettingsForm_Load(object sender, EventArgs e)
        {
            _dialogCommon.OnLoad();

            subscribeToEvents();

            Windows.SetText(label1, label1.Text);
            Windows.SetText(lblNumberofTimes, lblNumberofTimes.Text);
            Windows.SetText(lblEveryHalf, lblEveryHalf.Text);
            Windows.SetText(lblEveryRow, lblEveryRow.Text);
            Windows.SetText(lblEveryColumn, lblEveryColumn.Text);
            Windows.SetText(lblWordPrediction, lblWordPrediction.Text);
            Windows.SetText(lblScanTimes, lblScanTimes.Text);
            Windows.SetText(lblAcceptTime, lblAcceptTime.Text);
            Windows.SetText(lblSteppingTime, lblSteppingTime.Text);
            Windows.SetText(lblHesitateTime, lblHesitateTime.Text);
            Windows.SetText(lblWordListHesitateTime, lblWordListHesitateTime.Text);
            Windows.SetText(lblTabScanTime, lblTabScanTime.Text);
            Windows.SetText(lblFirstRepeatTime, lblFirstRepeatTime.Text);
            Windows.SetText(lblSelectingClick, lblSelectingClick.Text);

            _dialogCommon.GetAnimationManager().Start(_dialogCommon.GetRootWidget());
        }

        /// <summary>
        /// Subscribes to all the events triggered by the
        /// widgets and the interpreter
        /// </summary>
        private void subscribeToEvents()
        {
            var widgetList = new List<Widget>();
            _dialogCommon.GetRootWidget().Finder.FindAllButtons(widgetList);

            foreach (var widget in widgetList)
            {
                widget.EvtValueChanged += widget_EvtValueChanged;
            }

            widgetList.Clear();
            _dialogCommon.GetRootWidget().Finder.FindAllChildren(typeof(SliderWidget), widgetList);
            foreach (var widget in widgetList)
            {
                widget.EvtValueChanged += widget_EvtValueChanged;
            }
        }

        /// <summary>
        /// User changed some setting on the screen. Set
        ///  the dirty flag to indicate this
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void widget_EvtValueChanged(object sender, WidgetEventArgs e)
        {
            _isDirty = true;
        }
    }
}