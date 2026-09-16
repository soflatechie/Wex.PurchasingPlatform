using Wex.PurchasingPlatform.Desktop.ApiClient;
using Wex.PurchasingPlatform.Models;

namespace Wex.PurchasingPlatform.Desktop;

public partial class ConversionForm : Form
{
    private readonly PurchasingApiClient _apiClient;
    private readonly PurchaseTransactionDto _transaction;

    public ConversionForm(PurchasingApiClient apiClient, PurchaseTransactionDto transaction)
    {
        _apiClient = apiClient;
        _transaction = transaction;
        InitializeComponent();
        Text = $"Convert \"{transaction.Description}\"";
        Load += ConversionForm_Load;
    }

    private async void ConversionForm_Load(object? sender, EventArgs e)
    {
        try
        {
            var countries = await _apiClient.GetCountriesAsync();

            _countryComboBox.Items.Clear();
            foreach (var country in countries)
                _countryComboBox.Items.Add(country);
        }
        catch (PurchasingApiException ex)
        {
            ShowError(ex.Message);
        }
    }

    private async void CountryComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        _currencyComboBox.Items.Clear();

        if (_countryComboBox.SelectedItem is not string country)
            return;

        try
        {
            var currencies = await _apiClient.GetCurrenciesAsync(country, _transaction.TransactionDate);

            foreach (var currency in currencies)
                _currencyComboBox.Items.Add(currency.CurrencyName);
        }
        catch (PurchasingApiException ex)
        {
            ShowError(ex.Message);
        }
    }

    private async void ConvertButton_Click(object? sender, EventArgs e)
    {
        if (_countryComboBox.SelectedItem is not string country || _currencyComboBox.SelectedItem is not string currency)
        {
            ShowError("Select a country and currency first.");
            return;
        }

        try
        {
            var converted = await _apiClient.GetConversionAsync(_transaction.Id, country, currency);
            DisplayResult(converted);
        }
        catch (PurchasingApiException ex)
        {
            ShowError(ex.Message);
        }
    }

    private void DisplayResult(ConvertedPurchaseTransactionDto converted)
    {
        _transactionNumberLabel.Text = converted.TransactionNumber.ToString();
        _purchaseAmountValueLabel.Text = converted.PurchaseAmountUsd.ToString("N2");
        _exchangeRateValueLabel.Text = converted.ExchangeRate.ToString("0.####");
        _convertedAmountValueLabel.Text = $"{converted.ConvertedAmount:N2} {converted.CurrencyName}";
        _transactionDateLabel.Text = converted.TransactionDate.ToString("MM-dd-yyyy");
        _rateDateValueLabel.Text = converted.RateDate.ToString("MM-dd-yyyy");
        _staleWarningLabel.Visible = converted.IsStale;
    }

    private void ShowError(string message)
    {
        MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
