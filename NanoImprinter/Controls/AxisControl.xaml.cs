﻿using NanoImprinter.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WestLakeShape.Motion;

namespace NanoImprinter.Controls
{
    /// <summary>
    /// AxisControl.xaml 的交互逻辑
    /// </summary>
    public partial class AxisControl : UserControl
    {
        public IAxis SelectedAxis { get; set; }

        public static readonly DependencyProperty IsFxiedSpeedProperty = DependencyProperty.Register("IsFixedSpeed",
             typeof(bool), typeof(AxisControl), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty AxesProperty = DependencyProperty.Register("Axes",
            typeof(IEnumerable<IAxis>), typeof(AxisControl), new PropertyMetadata(null));

        public static readonly DependencyProperty UnitNameProperty = DependencyProperty.Register("UnitName",
            typeof(string), typeof(AxisControl), new PropertyMetadata(default(string)));
        
        public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register("SelectedValue",
           typeof(double), typeof(AxisControl), new PropertyMetadata(default(double)));
        public bool IsFixedSpeed
        {
            get => (bool)GetValue(IsFxiedSpeedProperty);
            set
            {
                SetValue(IsFxiedSpeedProperty, value);
                UnitName =value? "mm" : "mm/s";
            }
        }
        public IEnumerable<IAxis> Axes
        {
            get => (IEnumerable<IAxis>)GetValue(AxesProperty);
            set => SetValue(AxesProperty, value);
        }

        public string UnitName
        {
            get => (string)GetValue(UnitNameProperty);
            set => SetValue(UnitNameProperty, value);
        }
        public double SelectedValue
        {
            get => (double)GetValue(SelectedValueProperty);
            set => SetValue(SelectedValueProperty, value);
        }

        public AxisControl()
        {
            InitializeComponent();
        }
        private void btnJogForward_Click(object sender, RoutedEventArgs e)
        {
            SelectedAxis.MoveTo(SelectedValue);
        }

        private void btnJogBack_Click(object sender, RoutedEventArgs e)
        {
            SelectedAxis.MoveTo(SelectedValue);
        }

        private void SliderShowToolTip(object sender, RoutedEventArgs e)
        {
            double maximum = this.sldJogValue.Maximum;
            double minimum = this.sldJogValue.Minimum;
            double currentValue = this.sldJogValue.Value;
            // update the information to tool tip
            string toolTip = string.Format("{0:n} | min:{1:n}, max:{2:n}", currentValue, minimum, maximum);
            this.ToolTip = toolTip;
        }
    }
}
