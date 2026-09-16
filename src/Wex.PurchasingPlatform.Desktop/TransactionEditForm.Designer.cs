namespace Wex.PurchasingPlatform.Desktop;

partial class TransactionEditForm
{
    private System.ComponentModel.IContainer components = null;

    private Label _descriptionLabel;
    private TextBox _descriptionTextBox;
    private Label _transactionDateLabel;
    private DateTimePicker _transactionDatePicker;
    private Label _purchaseAmountLabel;
    private NumericUpDown _purchaseAmountUpDown;
    private Button _okButton;
    private Button _cancelButton;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _descriptionLabel = new Label();
        _descriptionTextBox = new TextBox();
        _transactionDateLabel = new Label();
        _transactionDatePicker = new DateTimePicker();
        _purchaseAmountLabel = new Label();
        _purchaseAmountUpDown = new NumericUpDown();
        _okButton = new Button();
        _cancelButton = new Button();

        ((System.ComponentModel.ISupportInitialize)_purchaseAmountUpDown).BeginInit();
        SuspendLayout();

        _descriptionLabel.Text = "Description:";
        _descriptionLabel.Location = new Point(12, 15);
        _descriptionLabel.Size = new Size(120, 23);

        _descriptionTextBox.Location = new Point(140, 12);
        _descriptionTextBox.Size = new Size(230, 23);
        _descriptionTextBox.MaxLength = 50;

        _transactionDateLabel.Text = "Transaction date:";
        _transactionDateLabel.Location = new Point(12, 48);
        _transactionDateLabel.Size = new Size(120, 23);

        _transactionDatePicker.Location = new Point(140, 45);
        _transactionDatePicker.Size = new Size(230, 23);
        _transactionDatePicker.Format = DateTimePickerFormat.Short;

        _purchaseAmountLabel.Text = "Purchase amount (USD):";
        _purchaseAmountLabel.Location = new Point(12, 81);
        _purchaseAmountLabel.Size = new Size(120, 23);

        _purchaseAmountUpDown.Location = new Point(140, 78);
        _purchaseAmountUpDown.Size = new Size(230, 23);
        _purchaseAmountUpDown.DecimalPlaces = 2;
        _purchaseAmountUpDown.Minimum = 0m;
        _purchaseAmountUpDown.Maximum = 1000000000m;
        _purchaseAmountUpDown.Increment = 0.01m;

        _okButton.Text = "OK";
        _okButton.Location = new Point(140, 120);
        _okButton.Size = new Size(90, 30);
        _okButton.Click += OkButton_Click;

        _cancelButton.Text = "Cancel";
        _cancelButton.Location = new Point(240, 120);
        _cancelButton.Size = new Size(90, 30);
        _cancelButton.DialogResult = DialogResult.Cancel;

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(394, 165);
        Controls.Add(_descriptionLabel);
        Controls.Add(_descriptionTextBox);
        Controls.Add(_transactionDateLabel);
        Controls.Add(_transactionDatePicker);
        Controls.Add(_purchaseAmountLabel);
        Controls.Add(_purchaseAmountUpDown);
        Controls.Add(_okButton);
        Controls.Add(_cancelButton);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        AcceptButton = _okButton;
        CancelButton = _cancelButton;

        ((System.ComponentModel.ISupportInitialize)_purchaseAmountUpDown).EndInit();
        ResumeLayout(false);
    }
}
