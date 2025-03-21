using System.Collections.Generic;
using System;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Windows.Controls;

namespace TCC
{
    public partial class AlterarTabela : Window
    {
        private string nomeTabelaAT; // Nome da tabela selecionada
        private string nomeColunaAT; // Nome da coluna selecionada

        // Construtor com parâmetro
        public AlterarTabela(string tabelaSelecionada)
        {
            InitializeComponent();
            nomeTabelaAT = tabelaSelecionada;

            // Exibir o nome da tabela
            TabelaTextBox.Text = nomeTabelaAT;

            // Obter e exibir as colunas da tabela selecionada
            List<string> colunas = ObterColunas(nomeTabelaAT);
            if (colunas.Count > 0)
            {
                ColunasListBox.ItemsSource = colunas; // Preenche o ListBox com as colunas
            }
            else
            {
                MessageBox.Show("Nenhuma coluna encontrada para esta tabela.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Método para obter as colunas da tabela
        private List<string> ObterColunas(string nomeTabela)
        {
            List<string> colunas = new List<string>();
            string connectionString = "Server=localhost;Database=TCCBase;Uid=root;Pwd=usbw;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{nomeTabela}'";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                colunas.Add(reader["COLUMN_NAME"].ToString());
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao obter colunas: {ex.Message}");
                }
            }

            return colunas;
        }

        private List<string> ObterValoresDaColuna(string tabela, string coluna)
        {
            List<string> valores = new List<string>();
            string connectionString = "Server=localhost;Database=TCCBase;Uid=root;Pwd=usbw;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = $"SELECT `{coluna}` FROM `{tabela}`";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string valor = reader[coluna]?.ToString();
                                if (!string.IsNullOrEmpty(valor))
                                {
                                    valores.Add(valor);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao obter valores da coluna: {ex.Message}");
                }
            }

            return valores;
        }

        // Evento de seleção de coluna no ListBox
        private void ColunasListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColunasListBox.SelectedItem is string colunaSelecionada)
            {
                nomeColunaAT = colunaSelecionada; // Atualiza a variável com a coluna selecionada
                MessageBox.Show($"Coluna selecionada: {nomeColunaAT}", "Seleção de Coluna");

                // Obter e exibir os valores da coluna selecionada
                List<string> valores = ObterValoresDaColuna(nomeTabelaAT, nomeColunaAT);
                if (valores.Count > 0)
                {
                    ValoresListBox.ItemsSource = valores; // Assume que você tem um ListBox chamado ValoresListBox
                }
                else
                {
                    MessageBox.Show("Nenhum valor encontrado para esta coluna.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }


        // Método para inserir dados na tabela
        private void InserirDados(string tabelaSelecionada, Dictionary<string, string> dados)
        {
            string connectionString = "Server=localhost;Database=TCCBase;Uid=root;Pwd=usbw;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Separar as colunas e os valores
                    string colunas = string.Join("`, `", dados.Keys);
                    string valores = string.Join("', '", dados.Values);
                    string query = $"INSERT INTO `{tabelaSelecionada}` (`{colunas}`) VALUES ('{valores}')";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                        MessageBox.Show($"Registro inserido com sucesso na tabela '{tabelaSelecionada}'!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao inserir dados: {ex.Message}");
                }
            }
        }

        private void InserirDadosButton_Click(object sender, RoutedEventArgs e)
        {
            // Verifica se a tabela e a coluna foram selecionadas
            if (!string.IsNullOrEmpty(nomeTabelaAT) && !string.IsNullOrEmpty(nomeColunaAT))
            {
                // Solicita apenas o valor para a coluna selecionada
                string valor = Microsoft.VisualBasic.Interaction.InputBox($"Insira o valor para '{nomeColunaAT}' na tabela '{nomeTabelaAT}':", "Novo Valor", "");

                if (!string.IsNullOrEmpty(valor))
                {
                    // Prepara os dados para inserção
                    var dados = new Dictionary<string, string>
            {
                { nomeColunaAT, valor } // Usando diretamente a coluna já selecionada
            };

                    // Chama o método para inserir os dados
                    InserirDados(nomeTabelaAT, dados);
                }
                else
                {
                    MessageBox.Show($"O valor para '{nomeColunaAT}' não pode estar vazio.", "Aviso");
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecione uma tabela e uma coluna antes de inserir os dados.", "Aviso");
            }
        }


        // Método para atualizar dados
        private void AtualizarDados(string tabelaSelecionada, string coluna, string valor, string condicao)
        {
            string connectionString = "Server=localhost;Database=TCCBase;Uid=root;Pwd=usbw;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Monta a consulta SQL usando os parâmetros fornecidos
                    string query = $"UPDATE `{tabelaSelecionada}` SET `{coluna}` = '{valor}' WHERE {condicao}";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        int linhasAfetadas = command.ExecuteNonQuery();
                        MessageBox.Show($"{linhasAfetadas} registro(s) atualizado(s).");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao atualizar dados: {ex.Message}");
                }
            }
        }


        // Evento do botão Atualizar Dados
        private void AtualizarDadosButton_Click(object sender, RoutedEventArgs e)
        {
            // Verifica se a tabela, a coluna e o valor foram selecionados
            if (!string.IsNullOrEmpty(nomeTabelaAT) && !string.IsNullOrEmpty(nomeColunaAT) && ValoresListBox.SelectedItem is string valorSelecionado)
            {
                // Solicita o novo valor para a coluna selecionada
                string novoValor = Microsoft.VisualBasic.Interaction.InputBox($"Insira o novo valor para a coluna '{nomeColunaAT}':", "Novo Valor", "");

                if (!string.IsNullOrEmpty(novoValor))
                {
                    // Usa o valor selecionado como condição
                    string condicao = $"{nomeColunaAT} = '{valorSelecionado}'";

                    // Chama o método para atualizar os dados
                    AtualizarDados(nomeTabelaAT, nomeColunaAT, novoValor, condicao);
                }
                else
                {
                    MessageBox.Show("O campo 'Novo Valor' não pode estar vazio.", "Aviso");
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecione uma tabela, uma coluna e um valor antes de atualizar os dados.", "Aviso");
            }
        }



        // Método para excluir dados
        private void ExcluirDados(string tabelaSelecionada, string condicao)
        {
            string connectionString = "Server=localhost;Database=TCCBase;Uid=root;Pwd=usbw;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = $"DELETE FROM `{tabelaSelecionada}` WHERE {condicao}";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        int linhasAfetadas = command.ExecuteNonQuery();
                        MessageBox.Show($"{linhasAfetadas} registro(s) excluído(s).");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir dados: {ex.Message}");
                }
            }
        }


        // Evento do botão Excluir Dados
        private void ExcluirDadosButton_Click(object sender, RoutedEventArgs e)
        {
            // Verifica se a tabela, a coluna e o valor foram selecionados
            if (!string.IsNullOrEmpty(nomeTabelaAT) && !string.IsNullOrEmpty(nomeColunaAT) && ValoresListBox.SelectedItem is string valorSelecionado)
            {
                // Constrói a condição usando a coluna e o valor selecionado
                string condicao = $"{nomeColunaAT} = '{valorSelecionado}'";

                // Chama o método para excluir os dados com base na condição gerada
                ExcluirDados(nomeTabelaAT, condicao);
            }
            else
            {
                MessageBox.Show("Por favor, selecione uma tabela, uma coluna e um valor antes de excluir os dados.", "Aviso");
            }
        }

    }
}
