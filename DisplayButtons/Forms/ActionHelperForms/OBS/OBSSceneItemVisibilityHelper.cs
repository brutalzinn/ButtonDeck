﻿using BackendAPI;
using BackendAPI.Objects.Implementation.DeckActions.OBS;
using BackendAPI.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DisplayButtons.Forms.ActionHelperForms.OBS
{
    public partial class OBSSceneItemVisibilityHelper : TemplateForm
    {
        public SceneItemVisibilityAction ModifiableAction { get; set; }

        private void CloseWithResult(DialogResult result)
        {
            DialogResult = result;
            Close();
        }

        public OBSSceneItemVisibilityHelper()
        {
            InitializeComponent();


            appBar1.Text = Texts.rm.GetString("OBS_SCENE_VISIBILITY_ITEM", Texts.cultereinfo);
            label1.Text = Texts.rm.GetString("OBS_SCENE_VISIBILITY_NAME_LABEL", Texts.cultereinfo);
            label2.Text = Texts.rm.GetString("OBS_SCENE_VISIBILITY_ITEM_LABEL", Texts.cultereinfo);
        }

        private void OBSSceneItemVisibilityHelper_Load(object sender, EventArgs e)
        {
            comboBox1.Text = ModifiableAction.SceneName;
            comboBox2.Text = ModifiableAction.SceneItem;
            Thread th = new Thread(() => {
                if (!OBSUtils.IsConnected)
                    OBSUtils.ConnectToServer();
                if (OBSUtils.IsConnected) {
                    Thread th2 = new Thread(() => {
                        var scenes = OBSUtils.GetOBSScenes();
                        comboBox1.Invoke(new Action(() => {
                            comboBox1.Items.AddRange(scenes.Select(c => c.SceneName).ToArray());
                            scenes.All(s => {
                                comboBox1.TextChanged += (ss, ee) => {
                                    if (comboBox1.Text == s.SceneName) {
                                        comboBox2.Items.Clear();
                                        comboBox2.Items.AddRange(s.SceneItems.Select(cc => cc.ItemName).ToArray());
                                    }
                                };
                                return true;
                            });
                            comboBox1.Text = comboBox1.Text;
                        }));
                    });
                    th2.Start();
                }
            });
            th.Start();

        }

        private void ModernButton2_Click(object sender, EventArgs e)
        {
            ModifiableAction.SceneName = comboBox1.Text;
            ModifiableAction.SceneItem = comboBox2.Text;
            CloseWithResult(DialogResult.OK);
        }

        private void ModernButton3_Click(object sender, EventArgs e)
        {
            CloseWithResult(DialogResult.Cancel);
        }
    }
}
