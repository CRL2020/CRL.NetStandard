namespace NoteForCode
{
    partial class Form1
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.txtContent = new System.Windows.Forms.TextBox();
            this.btnChangeName = new System.Windows.Forms.Button();
            this.btnChangeHead = new System.Windows.Forms.Button();
            this.txtName1 = new System.Windows.Forms.TextBox();
            this.txtName2 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtContent
            // 
            this.txtContent.Location = new System.Drawing.Point(186, 21);
            this.txtContent.Multiline = true;
            this.txtContent.Name = "txtContent";
            this.txtContent.Size = new System.Drawing.Size(393, 186);
            this.txtContent.TabIndex = 0;
            // 
            // btnChangeName
            // 
            this.btnChangeName.Location = new System.Drawing.Point(478, 338);
            this.btnChangeName.Name = "btnChangeName";
            this.btnChangeName.Size = new System.Drawing.Size(101, 30);
            this.btnChangeName.TabIndex = 1;
            this.btnChangeName.Text = "修改命名空间";
            this.btnChangeName.UseVisualStyleBackColor = true;
            this.btnChangeName.Click += new System.EventHandler(this.btnChangeName_Click);
            // 
            // btnChangeHead
            // 
            this.btnChangeHead.Location = new System.Drawing.Point(478, 214);
            this.btnChangeHead.Name = "btnChangeHead";
            this.btnChangeHead.Size = new System.Drawing.Size(100, 30);
            this.btnChangeHead.TabIndex = 1;
            this.btnChangeHead.Text = "修改头注释";
            this.btnChangeHead.UseVisualStyleBackColor = true;
            this.btnChangeHead.Click += new System.EventHandler(this.btnChangeHead_Click);
            // 
            // txtName1
            // 
            this.txtName1.Location = new System.Drawing.Point(243, 254);
            this.txtName1.Name = "txtName1";
            this.txtName1.Size = new System.Drawing.Size(336, 25);
            this.txtName1.TabIndex = 2;
            // 
            // txtName2
            // 
            this.txtName2.Location = new System.Drawing.Point(243, 307);
            this.txtName2.Name = "txtName2";
            this.txtName2.Size = new System.Drawing.Size(336, 25);
            this.txtName2.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(182, 257);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "原名称";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(182, 314);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 15);
            this.label2.TabIndex = 3;
            this.label2.Text = "新名称";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtName2);
            this.Controls.Add(this.txtName1);
            this.Controls.Add(this.btnChangeHead);
            this.Controls.Add(this.btnChangeName);
            this.Controls.Add(this.txtContent);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtContent;
        private System.Windows.Forms.Button btnChangeName;
        private System.Windows.Forms.Button btnChangeHead;
        private System.Windows.Forms.TextBox txtName1;
        private System.Windows.Forms.TextBox txtName2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
    }
}

