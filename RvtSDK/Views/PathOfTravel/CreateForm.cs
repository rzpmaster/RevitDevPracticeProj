﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PathOfTravel
{
    public partial class CreateForm : Form
    {
        PathCreateOptions m_createOption;
        public CreateForm()
        {
            InitializeComponent();

            radioButton1.Checked = true;
            m_createOption = PathCreateOptions.SingleRoomCornersToSingleDoor;
        }

        /// <summary>
        /// The option for creating Path of Travel.
        /// </summary>
        public PathCreateOptions PathCreateOption
        {
            get
            {
                return m_createOption;
            }
        }

        /// <summary>
        /// Set the CreateOptions.SingleRoomCornersToSingleDoor option.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arg.</param>
        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            m_createOption = PathCreateOptions.SingleRoomCornersToSingleDoor;
        }

        /// <summary>
        /// Set the CreateOptions.AllRoomCenterToSingleDoor option.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arg.</param>
        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            m_createOption = PathCreateOptions.AllRoomCenterToSingleDoor;
        }

        /// <summary>
        /// Set the CreateOptions.AllRoomCornersToAllDoors option.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arg.</param>
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            m_createOption = PathCreateOptions.AllRoomCornersToAllDoors;
        }
    }
}
