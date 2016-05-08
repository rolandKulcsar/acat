﻿////////////////////////////////////////////////////////////////////////////
// <copyright file="MouseGridSettingsForm.cs" company="Intel Corporation">
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
    /// Dialog to set the mouse grid settings.  This
    /// includes the speed of the grid, the speed
    /// of mouse movement, the number of cycles
    /// </summary>
    [DescriptorAttribute("71049A94-0435-4739-AE2C-77E2BD3CB0F0", 
                        "MouseGridSettingsForm",
                        "Mouse Grid Settings Dialog")]
    public partial class MouseGridSettingsForm : Form, IDialogPanel
    {
        /// <summary>
        /// The DialogCommon object
        /// </summary>
        private readonly DialogCommon _dialogCommon;

        /// <summary>
        /// Were any of the settings changed?
        /// </summary>
        private bool _isDirty;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public MouseGridSettingsForm()
        {
            InitializeComponent();

            _dialogCommon = new DialogCommon(this);

            if (!_dialogCommon.Initialize())
            {
                Log.Debug("Initialization error");
            }

            initWidgetSettings(Common.AppPreferences);

            Load += MouseGridSettingsForm_Load;
            FormClosing += MouseGridSettingsForm_FormClosing;
        }

        /// <summary>
        /// Gets the descriptor for this class
        /// </summary>
        public IDescriptor Descriptor
        {
            get { return DescriptorAttribute.GetDescriptor(GetType()); }
        }

        /// <summary>
        /// Gets the synch object for the scanner
        /// </summary>
        public SyncLock SyncObj
        {
            get { return _dialogCommon.SyncObj; }
        }

        /// <summary>
        /// Sets the form styles
        /// </summary>
        protected override CreateParams CreateParams
        {
            get { return DialogCommon.SetFormStyles(base.CreateParams); }
        }

        /// <summary>
        /// Invoked when a widget is actuated
        /// </summary>
        /// <param name="widget">Which one triggered?</param>
        public void OnButtonActuated(Widget widget)
        {
            Log.Debug("**Actuate** " + widget.UIControl.Name + " Value: " + widget.Value);

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
        /// Pauses the scanner
        /// </summary>
        public void OnPause()
        {
            _dialogCommon.OnPause();
        }

        /// <summary>
        /// Resumes paused scanner
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
        /// Form closing. Release resources
        /// </summary>
        /// <param name="e">eent arg</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _dialogCommon.OnFormClosing(e);
            base.OnFormClosing(e);
        }

        /// <summary>
        /// Window proc
        /// </summary>
        /// <param name="m">window message</param>
        [EnvironmentPermissionAttribute(SecurityAction.LinkDemand, Unrestricted = true)]
        protected override void WndProc(ref Message m)
        {
            _dialogCommon.HandleWndProc(m);
            base.WndProc(ref m);
        }

        /// <summary>
        /// Updates settings based on the values set by the
        /// user in the form.  Returns a ACATPreferences object
        /// with the new values.
        /// </summary>
        /// <returns>ACATPreferences object</returns>
        private ACATPreferences getSettingsFromUI()
        {
            var rootWidget = _dialogCommon.GetRootWidget();
            var prefs = ACATPreferences.Load();

            prefs.MouseGridRectangleSpeed = Common.AppPreferences.MouseGridRectangleSpeed = (rootWidget.Finder.FindChild(tbRectangleSpeed.Name) as SliderWidget).GetState(SliderWidget.SliderUnitsOnes);
            prefs.MouseGridRectangleCycles = Common.AppPreferences.MouseGridRectangleCycles = (rootWidget.Finder.FindChild(tbRectangleCycles.Name) as SliderWidget).GetState(SliderWidget.SliderUnitsOnes);
            prefs.MouseGridLineSpeed = Common.AppPreferences.MouseGridLineSpeed = (rootWidget.Finder.FindChild(tbLineSpeed.Name) as SliderWidget).GetState(SliderWidget.SliderUnitsOnes);
            prefs.MouseGridLineCycles = Common.AppPreferences.MouseGridLineCycles = (rootWidget.Finder.FindChild(tbLineCycles.Name) as SliderWidget).GetState(SliderWidget.SliderUnitsOnes);
            prefs.MouseGridLineThickness = Common.AppPreferences.MouseGridLineThickness = (rootWidget.Finder.FindChild(tbLineThickness.Name) as SliderWidget).GetState(SliderWidget.SliderUnitsOnes);

            prefs.MouseGridEnableVerticalRectangleScan = Common.AppPreferences.MouseGridEnableVerticalRectangleScan = (rootWidget.Finder.FindChild(pbEnableVerticalRectScan.Name) as CheckBoxWidget).GetState();

            return prefs;
        }

        /// <summary>
        /// Sets the state of all the controls based on the
        /// settings in the prefs parameter
        /// </summary>
        /// <param name="prefs">ACAT settings object</param>
        private void initWidgetSettings(ACATPreferences prefs)
        {
            var rootWidget = _dialogCommon.GetRootWidget();

            (rootWidget.Finder.FindChild(tbRectangleSpeed.Name) as SliderWidget).SetState(prefs.MouseGridRectangleSpeed, SliderWidget.SliderUnitsOnes);
            (rootWidget.Finder.FindChild(tbRectangleCycles.Name) as SliderWidget).SetState(prefs.MouseGridRectangleCycles, SliderWidget.SliderUnitsOnes);
            (rootWidget.Finder.FindChild(tbLineSpeed.Name) as SliderWidget).SetState(prefs.MouseGridLineSpeed, SliderWidget.SliderUnitsOnes);
            (rootWidget.Finder.FindChild(tbLineCycles.Name) as SliderWidget).SetState(prefs.MouseGridLineCycles, SliderWidget.SliderUnitsOnes);
            (rootWidget.Finder.FindChild(tbLineThickness.Name) as SliderWidget).SetState(prefs.MouseGridLineThickness, SliderWidget.SliderUnitsOnes);

            (rootWidget.Finder.FindChild(pbEnableVerticalRectScan.Name) as CheckBoxWidget).SetState(prefs.MouseGridEnableVerticalRectangleScan);
        }

        /// <summary>
        /// Restores default settings from the preferences file
        /// </summary>
        private void loadDefaultSettings()
        {
            if (DialogUtils.Confirm(this, "Restore default settings?"))
            {
                initWidgetSettings(ACATPreferences.LoadDefaultSettings());
                _isDirty = true;
            }
        }

        /// <summary>
        /// Form is closing. Release resources
        /// </summary>
        private void MouseGridSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _dialogCommon.OnClosing();
        }

        /// <summary>
        /// Form has been loaded. Initialize
        /// </summary>
        private void MouseGridSettingsForm_Load(object sender, EventArgs e)
        {
            _dialogCommon.OnLoad();

            subscribeToEvents();

            Windows.SetText(label1, label1.Text);
            Windows.SetText(lblVerticalSpeed, lblVerticalSpeed.Text);
            Windows.SetText(lblVerticalSweeps, lblVerticalSweeps.Text);
            Windows.SetText(lblHorizontalSpeed, lblHorizontalSpeed.Text);
            Windows.SetText(lblHorizontalSweeps, lblHorizontalSweeps.Text);
            Windows.SetText(lblLineWidth, lblLineWidth.Text);
            Windows.SetText(lblEnableVerticalRectScan, lblEnableVerticalRectScan.Text);

            _dialogCommon.GetAnimationManager().Start(_dialogCommon.GetRootWidget());
        }

        /// <summary>
        /// If dirty, confirm with user and then quit
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
        /// Saves settings and close the dialog. Confirms with the user first
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
        /// User changed something on the form. Sets the dirty flag
        /// </summary>
        /// <param name="sender">event sender</param>
        /// <param name="e">event args</param>
        private void widget_EvtValueChanged(object sender, WidgetEventArgs e)
        {
            _isDirty = true;
        }
    }
}