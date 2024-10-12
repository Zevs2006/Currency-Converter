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
        private const string apiKey = "89524c813879566616baa9fe"; // Замените на ваш API-ключ
        private const string apiUrl = "https://v6.exchangerate-api.com/v6/{0}/latest/USD";// Обновить для актуальных курсов
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

                    // Проверяем наличие ошибок
                    if (json["result"] != null && json["result"].ToString() != "success")
                    {
                        MessageBox.Show("Ошибка API: " + json["error-type"].ToString());
                        return;
                    }

                    // Очищаем ComboBox перед добавлением новых значений
                    comboBoxFromCurrency.Items.Clear();
                    comboBoxToCurrency.Items.Clear();

                    // Получаем курсы валют
                    var rates = json["conversion_rates"]; // Получаем курсы валют
                    if (rates != null)
                    {
                        foreach (var currency in rates)
                        {
                            string currencyCode = currency.Path; // Код валюты
                            decimal rate = currency.First.Value<decimal>(); // Получаем значение курса

                            currencyRates[currencyCode] = rate;

                            comboBoxFromCurrency.Items.Add(currencyCode); // Добавляем код валюты
                            comboBoxToCurrency.Items.Add(currencyCode);   // Добавляем код валюты
                        }
                    }

                    // Устанавливаем выбранные значения по умолчанию
                    if (comboBoxFromCurrency.Items.Count > 0) comboBoxFromCurrency.SelectedIndex = 0;
                    if (comboBoxToCurrency.Items.Count > 1) comboBoxToCurrency.SelectedIndex = 1;
                }
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show("Ошибка при загрузке валют: " + httpEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }



        private decimal ConvertCurrency(decimal amount, string fromCurrency, string toCurrency)
        {
            if (!currencyRates.ContainsKey(fromCurrency) || !currencyRates.ContainsKey(toCurrency))
                throw new ArgumentException("Неизвестная валюта.");

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
                    MessageBox.Show("Пожалуйста, заполните все поля.");
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
                MessageBox.Show("Введите корректную сумму для конвертации.");
            }
            catch (ArgumentException argEx)
            {
                MessageBox.Show("Ошибка: " + argEx.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message);
            }
        }
    }
}
