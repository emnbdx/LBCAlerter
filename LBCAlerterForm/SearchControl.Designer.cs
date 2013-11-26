namespace LBCAlerter
{
    partial class SearchControl
    {
        /// <summary> 
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur de composants

        /// <summary> 
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas 
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SearchControl));
            this.label1 = new System.Windows.Forms.Label();
            this.lbKeyWord = new System.Windows.Forms.Label();
            this.nudRefreshTime = new System.Windows.Forms.NumericUpDown();
            this.btnStartStop = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudRefreshTime)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            resources.ApplyResources(this.label1, "label1");
            this.label1.Name = "label1";
            // 
            // lbKeyWord
            // 
            resources.ApplyResources(this.lbKeyWord, "lbKeyWord");
            this.lbKeyWord.Name = "lbKeyWord";
            // 
            // nudRefreshTime
            // 
            resources.ApplyResources(this.nudRefreshTime, "nudRefreshTime");
            this.nudRefreshTime.Name = "nudRefreshTime";
            this.nudRefreshTime.ValueChanged += new System.EventHandler(this.nudRefreshTime_ValueChanged);
            // 
            // btnStartStop
            // 
            resources.ApplyResources(this.btnStartStop, "btnStartStop");
            this.btnStartStop.Name = "btnStartStop";
            this.btnStartStop.UseVisualStyleBackColor = true;
            this.btnStartStop.Click += new System.EventHandler(this.btnStartStop_Click);
            // 
            // btnDelete
            // 
            resources.ApplyResources(this.btnDelete, "btnDelete");
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // label2
            // 
            resources.ApplyResources(this.label2, "label2");
            this.label2.Name = "label2";
            // 
            // SearchControl
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnStartStop);
            this.Controls.Add(this.nudRefreshTime);
            this.Controls.Add(this.lbKeyWord);
            this.Controls.Add(this.label1);
            this.Name = "SearchControl";
            ((System.ComponentModel.ISupportInitialize)(this.nudRefreshTime)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lbKeyWord;
        private System.Windows.Forms.NumericUpDown nudRefreshTime;
        private System.Windows.Forms.Button btnStartStop;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Label label2;
    }
}
