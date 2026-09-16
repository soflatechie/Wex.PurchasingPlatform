namespace Wex.PurchasingPlatform.Desktop;

partial class ConversionForm
{
    private System.ComponentModel.IContainer components = null;

    private Label _countryLabel;
    private ComboBox _countryComboBox;
    private Label _currencyLabel;
    private ComboBox _currencyComboBox;
    private Button _convertButton;
    private Button _closeButton;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        _countryLabel = new Label();
        _countryComboBox = new ComboBox();
        _currencyLabel = new Label();
        _currencyComboBox = new ComboBox();
        _convertButton = new Button();
        _closeButton = new Button();
        groupBox1 = new GroupBox();
        _resultPanel = new Panel();
        label1 = new Label();
        _transactionDateLabel = new Label();
        _transactionNumberLabel = new Label();
        lblTransactionID = new Label();
        _purchaseAmountLabel = new Label();
        _purchaseAmountValueLabel = new Label();
        _exchangeRateLabel = new Label();
        _exchangeRateValueLabel = new Label();
        _convertedAmountLabel = new Label();
        _convertedAmountValueLabel = new Label();
        _rateDateLabel = new Label();
        _rateDateValueLabel = new Label();
        _staleWarningLabel = new Label();
        groupBox1.SuspendLayout();
        _resultPanel.SuspendLayout();
        SuspendLayout();
        // 
        // _countryLabel
        // 
        _countryLabel.Location = new Point(12, 15);
        _countryLabel.Name = "_countryLabel";
        _countryLabel.Size = new Size(90, 23);
        _countryLabel.TabIndex = 0;
        _countryLabel.Text = "Country:";
        // 
        // _countryComboBox
        // 
        _countryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _countryComboBox.Location = new Point(110, 12);
        _countryComboBox.Name = "_countryComboBox";
        _countryComboBox.Size = new Size(230, 33);
        _countryComboBox.TabIndex = 1;
        _countryComboBox.SelectedIndexChanged += CountryComboBox_SelectedIndexChanged;
        // 
        // _currencyLabel
        // 
        _currencyLabel.Location = new Point(364, 17);
        _currencyLabel.Name = "_currencyLabel";
        _currencyLabel.Size = new Size(90, 23);
        _currencyLabel.TabIndex = 2;
        _currencyLabel.Text = "Currency:";
        // 
        // _currencyComboBox
        // 
        _currencyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        _currencyComboBox.Location = new Point(460, 14);
        _currencyComboBox.Name = "_currencyComboBox";
        _currencyComboBox.Size = new Size(230, 33);
        _currencyComboBox.TabIndex = 3;
        // 
        // _convertButton
        // 
        _convertButton.Location = new Point(590, 53);
        _convertButton.Name = "_convertButton";
        _convertButton.Size = new Size(100, 30);
        _convertButton.TabIndex = 4;
        _convertButton.Text = "Convert";
        _convertButton.Click += ConvertButton_Click;
        // 
        // _closeButton
        // 
        _closeButton.DialogResult = DialogResult.Cancel;
        _closeButton.Location = new Point(606, 486);
        _closeButton.Name = "_closeButton";
        _closeButton.Size = new Size(90, 30);
        _closeButton.TabIndex = 6;
        _closeButton.Text = "Close";
        // 
        // groupBox1
        // 
        groupBox1.Controls.Add(_resultPanel);
        groupBox1.Location = new Point(24, 119);
        groupBox1.Name = "groupBox1";
        groupBox1.Size = new Size(672, 361);
        groupBox1.TabIndex = 7;
        groupBox1.TabStop = false;
        groupBox1.Text = "Conversion Results";
        // 
        // _resultPanel
        // 
        _resultPanel.Controls.Add(label1);
        _resultPanel.Controls.Add(_transactionDateLabel);
        _resultPanel.Controls.Add(_transactionNumberLabel);
        _resultPanel.Controls.Add(lblTransactionID);
        _resultPanel.Controls.Add(_purchaseAmountLabel);
        _resultPanel.Controls.Add(_purchaseAmountValueLabel);
        _resultPanel.Controls.Add(_exchangeRateLabel);
        _resultPanel.Controls.Add(_exchangeRateValueLabel);
        _resultPanel.Controls.Add(_convertedAmountLabel);
        _resultPanel.Controls.Add(_convertedAmountValueLabel);
        _resultPanel.Controls.Add(_rateDateLabel);
        _resultPanel.Controls.Add(_rateDateValueLabel);
        _resultPanel.Controls.Add(_staleWarningLabel);
        _resultPanel.Location = new Point(8, 41);
        _resultPanel.Name = "_resultPanel";
        _resultPanel.Size = new Size(460, 316);
        _resultPanel.TabIndex = 6;
        // 
        // label1
        // 
        label1.Location = new Point(15, 177);
        label1.Name = "label1";
        label1.Size = new Size(153, 23);
        label1.TabIndex = 12;
        label1.Text = "Transaction Date:";
        // 
        // _transactionDateLabel
        // 
        _transactionDateLabel.BorderStyle = BorderStyle.FixedSingle;
        _transactionDateLabel.Location = new Point(197, 177);
        _transactionDateLabel.Name = "_transactionDateLabel";
        _transactionDateLabel.Size = new Size(220, 26);
        _transactionDateLabel.TabIndex = 13;
        // 
        // _transactionNumberLabel
        // 
        _transactionNumberLabel.BorderStyle = BorderStyle.FixedSingle;
        _transactionNumberLabel.Location = new Point(197, 16);
        _transactionNumberLabel.Name = "_transactionNumberLabel";
        _transactionNumberLabel.Size = new Size(220, 26);
        _transactionNumberLabel.TabIndex = 10;
        // 
        // lblTransactionID
        // 
        lblTransactionID.Location = new Point(16, 14);
        lblTransactionID.Name = "lblTransactionID";
        lblTransactionID.Size = new Size(175, 23);
        lblTransactionID.TabIndex = 11;
        lblTransactionID.Text = "Transaction Number:";
        // 
        // _purchaseAmountLabel
        // 
        _purchaseAmountLabel.Location = new Point(16, 54);
        _purchaseAmountLabel.Name = "_purchaseAmountLabel";
        _purchaseAmountLabel.Size = new Size(100, 23);
        _purchaseAmountLabel.TabIndex = 0;
        _purchaseAmountLabel.Text = "Amount (USD):";
        // 
        // _purchaseAmountValueLabel
        // 
        _purchaseAmountValueLabel.BorderStyle = BorderStyle.FixedSingle;
        _purchaseAmountValueLabel.Location = new Point(197, 54);
        _purchaseAmountValueLabel.Name = "_purchaseAmountValueLabel";
        _purchaseAmountValueLabel.Size = new Size(220, 26);
        _purchaseAmountValueLabel.TabIndex = 1;
        // 
        // _exchangeRateLabel
        // 
        _exchangeRateLabel.Location = new Point(16, 93);
        _exchangeRateLabel.Name = "_exchangeRateLabel";
        _exchangeRateLabel.Size = new Size(100, 23);
        _exchangeRateLabel.TabIndex = 2;
        _exchangeRateLabel.Text = "Exchange rate:";
        // 
        // _exchangeRateValueLabel
        // 
        _exchangeRateValueLabel.BorderStyle = BorderStyle.FixedSingle;
        _exchangeRateValueLabel.Location = new Point(197, 93);
        _exchangeRateValueLabel.Name = "_exchangeRateValueLabel";
        _exchangeRateValueLabel.Size = new Size(220, 26);
        _exchangeRateValueLabel.TabIndex = 3;
        // 
        // _convertedAmountLabel
        // 
        _convertedAmountLabel.Location = new Point(15, 135);
        _convertedAmountLabel.Name = "_convertedAmountLabel";
        _convertedAmountLabel.Size = new Size(100, 23);
        _convertedAmountLabel.TabIndex = 4;
        _convertedAmountLabel.Text = "Converted amount:";
        // 
        // _convertedAmountValueLabel
        // 
        _convertedAmountValueLabel.BorderStyle = BorderStyle.FixedSingle;
        _convertedAmountValueLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        _convertedAmountValueLabel.Location = new Point(197, 134);
        _convertedAmountValueLabel.Name = "_convertedAmountValueLabel";
        _convertedAmountValueLabel.Size = new Size(220, 26);
        _convertedAmountValueLabel.TabIndex = 5;
        // 
        // _rateDateLabel
        // 
        _rateDateLabel.Location = new Point(15, 222);
        _rateDateLabel.Name = "_rateDateLabel";
        _rateDateLabel.Size = new Size(100, 23);
        _rateDateLabel.TabIndex = 6;
        _rateDateLabel.Text = "Rate Date:";
        // 
        // _rateDateValueLabel
        // 
        _rateDateValueLabel.BorderStyle = BorderStyle.FixedSingle;
        _rateDateValueLabel.Location = new Point(197, 222);
        _rateDateValueLabel.Name = "_rateDateValueLabel";
        _rateDateValueLabel.Size = new Size(220, 26);
        _rateDateValueLabel.TabIndex = 7;
        // 
        // _staleWarningLabel
        // 
        _staleWarningLabel.ForeColor = Color.DarkOrange;
        _staleWarningLabel.Location = new Point(15, 260);
        _staleWarningLabel.Name = "_staleWarningLabel";
        _staleWarningLabel.Size = new Size(330, 23);
        _staleWarningLabel.TabIndex = 8;
        _staleWarningLabel.Text = "This rate is more than 3 months old.";
        _staleWarningLabel.Visible = false;
        // 
        // ConversionForm
        // 
        AutoScaleDimensions = new SizeF(10F, 25F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _closeButton;
        ClientSize = new Size(735, 534);
        Controls.Add(groupBox1);
        Controls.Add(_countryLabel);
        Controls.Add(_countryComboBox);
        Controls.Add(_currencyLabel);
        Controls.Add(_currencyComboBox);
        Controls.Add(_convertButton);
        Controls.Add(_closeButton);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "ConversionForm";
        StartPosition = FormStartPosition.CenterParent;
        groupBox1.ResumeLayout(false);
        _resultPanel.ResumeLayout(false);
        ResumeLayout(false);
    }

    private Label _staleWarningLabel;
    private Label _rateDateValueLabel;
    private Label _rateDateLabel;
    private Label _convertedAmountValueLabel;
    private Label _convertedAmountLabel;
    private Label _exchangeRateValueLabel;
    private Label _exchangeRateLabel;
    private Label _purchaseAmountValueLabel;
    private Label _purchaseAmountLabel;
    private Panel _resultPanel;
    private Label _transactionNumberLabel;
    private Label lblTransactionID;
    private GroupBox groupBox1;
    private Label label1;
    private Label _transactionDateLabel;
}
