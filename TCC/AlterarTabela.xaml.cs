using System.Collections.Generic;
using System;
using System.Windows;
using MySql.Data.MySqlClient;
using System.Windows.Controls;
using System.Linq;

namespace TCC
{
    public partial class AlterarTabela : Window
    {
        private string nomeTabelaAT; // Nome da tabela selecionada
        private string nomeColunaAT; // Nome da coluna selecionada
        private Dictionary<int, string> valoresColuna = new Dictionary<int, string>(); // Dicionário para armazenar ID e Valor

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

        // Método para obter os valores de uma coluna com seus IDs
        private Dictionary<int, string> ObterValoresDaColuna(string tabela, string coluna)
        {
            Dictionary<int, string> valores = new Dictionary<int, string>();
            string connectionString = "Server=localhost;Database=TCCBase;Uid=root;Pwd=usbw;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = $"SELECT `ID`, `{coluna}` FROM `{tabela}`";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = reader.GetInt32("ID"); // Captura o ID
                                string valor = reader[coluna]?.ToString(); // Captura o valor da coluna
                                valores[id] = valor; // Adiciona ao dicionário
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
                valoresColuna = ObterValoresDaColuna(nomeTabelaAT, nomeColunaAT);
                if (valoresColuna.Count > 0)
                {
                    ValoresListBox.ItemsSource = valoresColuna.Values.ToList(); // Converte os valores para uma lista e preenche o ListBox
                }
                else
                {
                    ValoresListBox.ItemsSource = null; // Limpa o ListBox caso não haja valores
                    MessageBox.Show("Nenhum valor encontrado para esta coluna.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }



        private void AtualizarListBoxes()
        {
            // Atualiza as colunas
            List<string> colunas = ObterColunas(nomeTabelaAT);
            if (colunas.Count > 0)
            {
                ColunasListBox.ItemsSource = colunas;
            }
            else
            {
                ColunasListBox.ItemsSource = null; // Remove itens se não houver colunas
                MessageBox.Show("Nenhuma coluna encontrada para esta tabela.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            // Se uma coluna já estiver selecionada, atualiza os valores e o dicionário
            if (!string.IsNullOrEmpty(nomeColunaAT))
            {
                valoresColuna = ObterValoresDaColuna(nomeTabelaAT, nomeColunaAT); // Atualiza o dicionário com os valores e IDs
                if (valoresColuna.Count > 0)
                {
                    ValoresListBox.ItemsSource = valoresColuna.Values.ToList(); // Exibe os valores no ListBox
                }
                else
                {
                    ValoresListBox.ItemsSource = null; // Limpa o ListBox caso não haja valores
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
                        AtualizarListBoxes();
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
        private void AtualizarDados(string tabelaSelecionada, int id, string coluna, string valor)
        {
            string connectionString = "Server=localhost;Database=TCCBase;Uid=root;Pwd=usbw;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Usar o ID como condição exclusiva na consulta SQL
                    string query = $"UPDATE `{tabelaSelecionada}` SET `{coluna}` = '{valor}' WHERE `ID` = {id}";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        int linhasAfetadas = command.ExecuteNonQuery();
                        MessageBox.Show($"{linhasAfetadas} registro(s) atualizado(s).");
                        AtualizarListBoxes(); // Atualizar ListBox após alteração
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
            // Verifica se a tabela e o valor foram selecionados
            if (!string.IsNullOrEmpty(nomeTabelaAT) && ValoresListBox.SelectedItem is string valorSelecionado)
            {
                // Busca o ID associado ao valor selecionado
                int idSelecionado = valoresColuna.FirstOrDefault(v => v.Value == valorSelecionado).Key;

                if (idSelecionado != 0)
                {
                    // Solicita o novo valor para a coluna selecionada
                    string novoValor = Microsoft.VisualBasic.Interaction.InputBox($"Insira o novo valor para a coluna '{nomeColunaAT}':", "Novo Valor", "");

                    if (!string.IsNullOrEmpty(novoValor))
                    {
                        // Chama o método para atualizar os dados com base no ID
                        AtualizarDados(nomeTabelaAT, idSelecionado, nomeColunaAT, novoValor);
                    }
                    else
                    {
                        MessageBox.Show("O campo 'Novo Valor' não pode estar vazio.", "Aviso");
                    }
                }
                else
                {
                    MessageBox.Show("Erro ao identificar o registro selecionado. Tente novamente.", "Erro");
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecione uma tabela, uma coluna e um valor antes de atualizar os dados.", "Aviso");
            }
        }



        // Método para excluir dados
        private void ExcluirDados(string tabelaSelecionada, int id)
        {
            string connectionString = "Server=localhost;Database=TCCBase;Uid=root;Pwd=usbw;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    // Usar o ID diretamente na cláusula WHERE
                    string query = $"DELETE FROM `{tabelaSelecionada}` WHERE `ID` = {id}";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        int linhasAfetadas = command.ExecuteNonQuery();
                        MessageBox.Show($"{linhasAfetadas} registro(s) excluído(s).");
                        AtualizarListBoxes(); // Atualizar ListBox após exclusão
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
            // Verifica se a tabela e o valor foram selecionados
            if (!string.IsNullOrEmpty(nomeTabelaAT) && ValoresListBox.SelectedItem is string valorSelecionado)
            {
                // Busca o ID associado ao valor selecionado
                int idSelecionado = valoresColuna.FirstOrDefault(v => v.Value == valorSelecionado).Key;

                if (idSelecionado != 0)
                {
                    // Chama o método para excluir o dado com base no ID
                    ExcluirDados(nomeTabelaAT, idSelecionado);
                }
                else
                {
                    MessageBox.Show("Erro ao identificar o registro selecionado. Tente novamente.", "Erro");
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecione uma tabela, uma coluna e um valor antes de excluir os dados.", "Aviso");
            }
        }

        public event Action TabelaExcluida; // Evento para notificar a exclusão da tabela

        private void ExcluirTabela(string tabelaSelecionada)
        {
            string connectionString = "Server=localhost;Database=TCCBase;Uid=root;Pwd=usbw;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Comando SQL para excluir a tabela
                    string query = $"DROP TABLE `{tabelaSelecionada}`";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.ExecuteNonQuery();
                        MessageBox.Show($"Tabela '{tabelaSelecionada}' excluída com sucesso!");

                        // Dispara o evento para notificar que a tabela foi excluída
                        TabelaExcluida?.Invoke();

                        // Fecha a janela após exclusão
                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao excluir a tabela: {ex.Message}");
                }
            }
        }
        private void ExcluirTabelaButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(nomeTabelaAT))
            {
                // Solicita confirmação do usuário antes de excluir
                MessageBoxResult result = MessageBox.Show($"Tem certeza de que deseja excluir a tabela '{nomeTabelaAT}'? Esta ação não pode ser desfeita.",
                                                          "Confirmação",
                                                          MessageBoxButton.YesNo,
                                                          MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    ExcluirTabela(nomeTabelaAT);
                }
            }
            else
            {
                MessageBox.Show("Nenhuma tabela selecionada para exclusão.", "Aviso");
            }
        }

    }
}
