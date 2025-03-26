using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace TCC
{
    public partial class CriarTabelaWindow : Window
    {
        private List<(string Coluna, string Tipo)> colunas = new List<(string, string)>();
        private string connectionString = "Server=localhost;Database=TCCBase;Uid=root;Pwd=usbw;";

        public CriarTabelaWindow()
        {
            InitializeComponent();
        }

        private void AtualizarPreviewTabela()
        {
            TabelaPreviewListBox.Items.Clear();

            string preview = $"CREATE TABLE `{TabelaTextBox.Text}` (\n  `ID` INT AUTO_INCREMENT PRIMARY KEY,\n";
            preview += string.Join(",\n", colunas.Select(c => $"  `{c.Coluna}` {c.Tipo}"));
            preview += "\n);";

            TabelaPreviewListBox.Items.Add(preview);
        }
        private void AdicionarColunaButton_Click(object sender, RoutedEventArgs e)
        {
            string nomeColuna = ColunaNomeTextBox.Text;
            string tipoDado = (TipoDadoComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string restricoes = "";

            // Adiciona restrições, se selecionadas
            if (NotNullCheckBox.IsChecked == true)
                restricoes += " NOT NULL";
            if (UniqueCheckBox.IsChecked == true)
                restricoes += " UNIQUE";
            if (!string.IsNullOrEmpty(DefaultValueTextBox.Text))
                restricoes += $" DEFAULT '{DefaultValueTextBox.Text}'";

            if (!string.IsNullOrEmpty(nomeColuna) && !string.IsNullOrEmpty(tipoDado))
            {
                colunas.Add((nomeColuna, tipoDado + restricoes));
                ColunasListBox.Items.Add($"{nomeColuna}: {tipoDado}{restricoes}");
                ColunaNomeTextBox.Clear();
                TipoDadoComboBox.SelectedIndex = 0; // Reseta para o primeiro tipo
                NotNullCheckBox.IsChecked = false;
                UniqueCheckBox.IsChecked = false;
                DefaultValueTextBox.Clear();
            }
            else
            {
                MessageBox.Show("Por favor, preencha o nome da coluna e selecione o tipo de dado.");
            }
            AtualizarPreviewTabela();
        }


        private void CriarTabelaButton_Click(object sender, RoutedEventArgs e)
        {
            string nomeTabela = TabelaTextBox.Text;

            if (!string.IsNullOrEmpty(nomeTabela) && colunas.Count > 0)
            {
                try
                {
                    CriarNovaTabela(nomeTabela, colunas);
                    MessageBox.Show($"Tabela '{nomeTabela}' criada com sucesso!");

                    // Define o resultado como true para sinalizar sucesso
                    this.DialogResult = true;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao criar tabela: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Por favor, insira o nome da tabela e pelo menos uma coluna.");
            }
        }

        private void CriarNovaTabela(string nomeTabela, List<(string Coluna, string Tipo)> colunas)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Gera a string de definição de colunas
                string colunasQuery = "`ID` INT AUTO_INCREMENT PRIMARY KEY, "; // ID como PRIMARY KEY
                colunasQuery += string.Join(", ", colunas.Select(c => $"`{c.Coluna}` {c.Tipo}"));

                string query = $"CREATE TABLE `{nomeTabela}` ({colunasQuery})";

                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
