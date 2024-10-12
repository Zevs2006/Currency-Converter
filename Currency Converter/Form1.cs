using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Currency_Converter
{
    public partial class Form1 : Form
    {
        private const string apiKey = "89524c813879566616baa9fe"; // �������� �� ��� API-����
        private const string apiUrl = "https://v6.exchangerate-api.com/v6/{0}/latest/USD";// �������� ��� ���������� ������
        private Dictionary<string, decimal> currencyRates = new Dictionary<string, decimal>();

        public Form1()
        {
            InitializeComponent();
            LoadCurrencies();
        }

        private async void LoadCurrencies()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(string.Format(apiUrl, apiKey));
                    var json = JObject.Parse(response);

                    // ��������� ������� ������
                    if (json["result"] != null && json["result"].ToString() != "success")
                    {
                        MessageBox.Show("������ API: " + json["error-type"].ToString());
                        return;
                    }

                    // ������� ComboBox ����� ����������� ����� ��������
                    comboBoxFromCurrency.Items.Clear();
                    comboBoxToCurrency.Items.Clear();

                    // �������� ����� �����
                    var rates = json["conversion_rates"]; // �������� ����� �����
                    if (rates != null)
                    {
                        foreach (var currency in rates)
                        {
                            string currencyCode = currency.Path; // ��� ������
                            decimal rate = currency.First.Value<decimal>(); // �������� �������� �����

                            currencyRates[currencyCode] = rate;

                            comboBoxFromCurrency.Items.Add(currencyCode); // ��������� ��� ������
                            comboBoxToCurrency.Items.Add(currencyCode);   // ��������� ��� ������
                        }
                    }

                    // ������������� ��������� �������� �� ���������
                    if (comboBoxFromCurrency.Items.Count > 0) comboBoxFromCurrency.SelectedIndex = 0;
                    if (comboBoxToCurrency.Items.Count > 1) comboBoxToCurrency.SelectedIndex = 1;
                }
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show("������ ��� �������� �����: " + httpEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("������: " + ex.Message);
            }
        }



        private decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            if (!currencyRates.ContainsKey(fromCurrency) || !currencyRates.ContainsKey(toCurrency))
                throw new ArgumentException("����������� ������.");

            decimal fromRate = currencyRates[fromCurrency];
            decimal toRate = currencyRates[toCurrency];

            return amount * (toRate / fromRate);
        }

        private void buttonConvert_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(textBoxAmount.Text) ||
                    comboBoxFromCurrency.SelectedItem == null ||
                    comboBoxToCurrency.SelectedItem == null)
                {
                    MessageBox.Show("����������, ��������� ��� ����.");
                    return;
                }

                decimal amount = decimal.Parse(textBoxAmount.Text);
                string fromCurrency = comboBoxFromCurrency.SelectedItem.ToString();
                string toCurrency = comboBoxToCurrency.SelectedItem.ToString();

                decimal result = ConvertCurrency(amount, fromCurrency, toCurrency);
                labelResult.Text = $"{amount} {fromCurrency} = {result:F2} {toCurrency}";
            }
            catch (FormatException)
            {
                MessageBox.Show("������� ���������� ����� ��� �����������.");
            }
            catch (ArgumentException argEx)
            {
                MessageBox.Show("������: " + argEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("������: " + ex.Message);
            }
        }
    }
}
