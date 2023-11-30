namespace PowerPointAddIn1
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button_sansho = new System.Windows.Forms.Button();
            this.object_info = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(269, 19);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "pptxファイル";
            // 
            // button_sansho
            // 
            this.button_sansho.Location = new System.Drawing.Point(287, 10);
            this.button_sansho.Name = "button_sansho";
            this.button_sansho.Size = new System.Drawing.Size(75, 23);
            this.button_sansho.TabIndex = 1;
            this.button_sansho.Text = "参照";
            this.button_sansho.UseVisualStyleBackColor = true;
            // 
            // object_info
            // 
            this.object_info.Location = new System.Drawing.Point(150, 57);
            this.object_info.Name = "object_info";
            this.object_info.Size = new System.Drawing.Size(75, 23);
            this.object_info.TabIndex = 2;
            this.object_info.Text = "button1";
            this.object_info.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(377, 381);
            this.Controls.Add(this.object_info);
            this.Controls.Add(this.button_sansho);
            this.Controls.Add(this.textBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button_sansho;
        private System.Windows.Forms.Button object_info;
    }
}