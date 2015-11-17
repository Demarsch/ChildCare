﻿using Microsoft.Practices.Unity;
using PatientRecordsModule.ViewModels;
using System;
using System.Collections.Generic;
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

namespace PatientRecordsModule.Views
{
    /// <summary>
    /// Interaction logic for NewVisitCreating.xaml
    /// </summary>
    public partial class VisitCloseView : UserControl
    {
        public VisitCloseView()
        {
            InitializeComponent();
        }

        [Dependency]
        public VisitCloseView ContentViewModel
        {
            get { return DataContext as VisitCloseView; }
            set { DataContext = value; }
        }
    }
}