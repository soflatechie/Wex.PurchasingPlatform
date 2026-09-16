using Wex.PurchasingPlatform.Desktop.ApiClient;
using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Desktop;

public partial class MainForm : Form
{
    private readonly PurchasingApiClient _apiClient;

    public MainForm(PurchasingApiClient apiClient)
    {
        _apiClient = apiClient;
        InitializeComponent();
        Load += MainForm_Load;
    }

    private async void MainForm_Load(object? sender, EventArgs e)
    {
        await RefreshTransactionsAsync();
    }

    private async void RefreshButton_Click(object? sender, EventArgs e)
    {
        await RefreshTransactionsAsync();
    }

    private async void NewButton_Click(object? sender, EventArgs e)
    {
        using var editForm = new TransactionEditForm();
        if (editForm.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            await _apiClient.CreateTransactionAsync(editForm.ToCreateRequest());
            await RefreshTransactionsAsync();
        }
        catch (PurchasingApiException ex)
        {
            ShowError(ex.Message);
        }
    }

    private async void EditButton_Click(object? sender, EventArgs e)
    {
        var selected = GetSelectedTransaction();
        if (selected is null)
        {
            ShowInfo("Select a transaction to edit.");
            return;
        }

        using var editForm = new TransactionEditForm(selected);
        if (editForm.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            await _apiClient.UpdateTransactionAsync(selected.Id, editForm.ToUpdateRequest());
            await RefreshTransactionsAsync();
        }
        catch (PurchasingApiException ex)
        {
            ShowError(ex.Message);
        }
    }

    private async void DeleteButton_Click(object? sender, EventArgs e)
    {
        var selected = GetSelectedTransaction();
        if (selected is null)
        {
            ShowInfo("Select a transaction to delete.");
            return;
        }

        var confirmation = MessageBox.Show(
            this,
            $"Delete \"{selected.Description}\"?",
            "Confirm delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirmation != DialogResult.Yes)
            return;

        try
        {
            await _apiClient.DeleteTransactionAsync(selected.Id);
            await RefreshTransactionsAsync();
        }
        catch (PurchasingApiException ex)
        {
            ShowError(ex.Message);
        }
    }

    private void ConvertButton_Click(object? sender, EventArgs e)
    {
        var selected = GetSelectedTransaction();
        if (selected is null)
        {
            ShowInfo("Select a transaction to convert.");
            return;
        }

        using var conversionForm = new ConversionForm(_apiClient, selected);
        conversionForm.ShowDialog(this);
    }

    private async Task RefreshTransactionsAsync()
    {
        try
        {
            var transactions = await _apiClient.GetTransactionsAsync();
            BindGrid(transactions);
        }
        catch (PurchasingApiException ex)
        {
            ShowError(ex.Message);
        }
    }

    private void BindGrid(IReadOnlyList<PurchaseTransactionDto> transactions)
    {
        _transactionsGrid.DataSource = null;
        _transactionsGrid.DataSource = transactions.ToList();

        if (_transactionsGrid.Columns["Id"] is { } idColumn)
            idColumn.Visible = false;

        if (_transactionsGrid.Columns["TransactionNumber"] is { } transactionNumberColumn)
        {
            transactionNumberColumn.HeaderText = "Transaction #";
            transactionNumberColumn.DefaultCellStyle.Format = "'PT-'000000";
            transactionNumberColumn.DisplayIndex = 0;
        }

        if (_transactionsGrid.Columns["TransactionDate"] is { } dateColumn)
            dateColumn.DefaultCellStyle.Format = "yyyy-MM-dd";

        if (_transactionsGrid.Columns["PurchaseAmountUsd"] is { } amountColumn)
        {
            amountColumn.HeaderText = "Amount (USD)";
            amountColumn.DefaultCellStyle.Format = "N2";
        }
    }

    private PurchaseTransactionDto? GetSelectedTransaction()
    {
        return _transactionsGrid.CurrentRow?.DataBoundItem as PurchaseTransactionDto;
    }

    private void ShowError(string message)
    {
        MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void ShowInfo(string message)
    {
        MessageBox.Show(this, message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
