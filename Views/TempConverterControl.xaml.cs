using System.Windows.Controls;

namespace WPF_utils.Views
{
    public partial class TempConverterControl : UserControl
    {
        private bool isUpdatingTemp = false;

        public TempConverterControl()
        {
            InitializeComponent();
        }

        private void txtCelsius_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isUpdatingTemp) return;
            if (txtCelsius.IsFocused)
            {
                if (double.TryParse(txtCelsius.Text, out double c))
                {
                    isUpdatingTemp = true;
                    txtFahrenheit.Text = (c * 9.0 / 5.0 + 32).ToString("0.##");
                    isUpdatingTemp = false;
                }
                else if (string.IsNullOrWhiteSpace(txtCelsius.Text))
                {
                    LimparTemperaturas();
                }
            }
        }

        private void txtFahrenheit_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isUpdatingTemp) return;
            if (txtFahrenheit.IsFocused)
            {
                if (double.TryParse(txtFahrenheit.Text, out double f))
                {
                    isUpdatingTemp = true;
                    double c = (f - 32) * 5.0 / 9.0;
                    txtCelsius.Text = c.ToString("0.##");
                    isUpdatingTemp = false;
                }
                else if (string.IsNullOrWhiteSpace(txtFahrenheit.Text))
                {
                    LimparTemperaturas();
                }
            }
        }

        private void LimparTemperaturas()
        {
            isUpdatingTemp = true;
            txtCelsius.Text = "";
            txtFahrenheit.Text = "";
            isUpdatingTemp = false;
        }
    }
}