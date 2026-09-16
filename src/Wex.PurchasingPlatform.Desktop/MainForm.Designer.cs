namespace Wex.PurchasingPlatform.Desktop;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    private DataGridView _transactionsGrid;
    private Panel _buttonPanel;
    private Button _newButton;
    private Button _editButton;
    private Button _deleteButton;
    private Button _convertButton;
    private Button _refreshButton;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        _transactionsGrid = new DataGridView();
        _buttonPanel = new Panel();
        _newButton = new Button();
        _editButton = new Button();
        _deleteButton = new Button();
        _convertButton = new Button();
        _refreshButton = new Button();

        ((System.ComponentModel.ISupportInitialize)_transactionsGrid).BeginInit();
        _buttonPanel.SuspendLayout();
        SuspendLayout();

        _transactionsGrid.Dock = DockStyle.Fill;
        _transactionsGrid.AllowUserToAddRows = false;
        _transactionsGrid.AllowUserToDeleteRows = false;
        _transactionsGrid.ReadOnly = true;
        _transactionsGrid.MultiSelect = false;
        _transactionsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _transactionsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        _newButton.Text = "New";
        _newButton.Location = new Point(10, 10);
        _newButton.Size = new Size(90, 30);
        _newButton.Click += NewButton_Click;

        _editButton.Text = "Edit";
        _editButton.Location = new Point(110, 10);
        _editButton.Size = new Size(90, 30);
        _editButton.Click += EditButton_Click;

        _deleteButton.Text = "Delete";
        _deleteButton.Location = new Point(210, 10);
        _deleteButton.Size = new Size(90, 30);
        _deleteButton.Click += DeleteButton_Click;

        _convertButton.Text = "Convert...";
        _convertButton.Location = new Point(310, 10);
        _convertButton.Size = new Size(90, 30);
        _convertButton.Click += ConvertButton_Click;

        _refreshButton.Text = "Refresh";
        _refreshButton.Location = new Point(410, 10);
        _refreshButton.Size = new Size(90, 30);
        _refreshButton.Click += RefreshButton_Click;

        _buttonPanel.Dock = DockStyle.Top;
        _buttonPanel.Height = 50;
        _buttonPanel.Controls.Add(_newButton);
        _buttonPanel.Controls.Add(_editButton);
        _buttonPanel.Controls.Add(_deleteButton);
        _buttonPanel.Controls.Add(_convertButton);
        _buttonPanel.Controls.Add(_refreshButton);

        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(900, 500);
        Controls.Add(_transactionsGrid);
        Controls.Add(_buttonPanel);
        Text = "WEX Purchasing Platform";

        ((System.ComponentModel.ISupportInitialize)_transactionsGrid).EndInit();
        _buttonPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
