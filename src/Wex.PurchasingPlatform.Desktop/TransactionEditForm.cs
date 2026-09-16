using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Desktop;

public partial class TransactionEditForm : Form
{
    public TransactionEditForm()
    {
        InitializeComponent();
        Text = "New Transaction";
        _transactionDatePicker.Value = DateTime.Today;
    }

    public TransactionEditForm(PurchaseTransactionDto transaction) : this()
    {
        Text = "Edit Transaction";
        _descriptionTextBox.Text = transaction.Description;
        _transactionDatePicker.Value = transaction.TransactionDate.ToDateTime(TimeOnly.MinValue);
        _purchaseAmountUpDown.Value = transaction.PurchaseAmountUsd;
    }

    public CreatePurchaseTransactionRequest ToCreateRequest()
    {
        return new CreatePurchaseTransactionRequest(
            _descriptionTextBox.Text.Trim(),
            DateOnly.FromDateTime(_transactionDatePicker.Value),
            _purchaseAmountUpDown.Value);
    }

    public UpdatePurchaseTransactionRequest ToUpdateRequest()
    {
        return new UpdatePurchaseTransactionRequest(
            _descriptionTextBox.Text.Trim(),
            DateOnly.FromDateTime(_transactionDatePicker.Value),
            _purchaseAmountUpDown.Value);
    }

    private void OkButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_descriptionTextBox.Text))
        {
            MessageBox.Show(this, "Description is required.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }
}
