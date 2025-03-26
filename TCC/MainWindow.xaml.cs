using System;
using System.Text;
using System.Windows;
using MySql.Data.MySqlClient;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace TCC
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TestarConexaoDB();
            ListarTabelas();
        }
        // Testar conexão com o banco de dados
        private void TestarConexaoDB()
        {
            string connectionString = "Server=localhost;Database=TCCBase;Uid=root;Pwd=usbw;";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    MessageBox.Show("Conexão bem-sucedida!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro: {ex.Message}");
                }
            }
        }
        private void ListarTabelas()
        {
            string connectionString = "Server=localhost;Database=TCCBase;Uid=root;Pwd=usbw;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = "SHOW TABLES";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        TabelasListBox.Items.Clear(); // Limpa os itens existentes no ListBox
                        while (reader.Read())
                        {
                            TabelasListBox.Items.Add(reader.GetString(0)); // Adiciona os nomes das tabelas
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao listar tabelas: {ex.Message}");
                }
            }
        }


        private void CriarTabelaButton_Click(object sender, RoutedEventArgs e)
        {
            // Abre a janela para criar tabela
            var criarTabelaWindow = new CriarTabelaWindow();
            if (criarTabelaWindow.ShowDialog() == true) // Verifica se a tabela foi criada com sucesso
            {
                ListarTabelas(); // Atualiza a lista de tabelas
            }
        }

        private void AlterarTabelaButton_Click(object sender, RoutedEventArgs e)
        {
            // Verifica se uma tabela foi selecionada no ListBox
            if (TabelasListBox.SelectedItem is string nomeTabela)
            {
                // Passa o nome da tabela selecionada como argumento para o construtor da nova janela
                AlterarTabela alterarTabela = new AlterarTabela(nomeTabela);
                alterarTabela.ShowDialog(); // Exibe a janela como modal
            }
            else
            {
                // Exibe uma mensagem caso nenhuma tabela esteja selecionada
                MessageBox.Show("Por favor, selecione uma tabela antes de alterar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }




        // Ler dados
        private void LerDados(string nomeTabela)
        {
            string connectionString = "Server=localhost;Database=TCCBase;Uid=root;Pwd=usbw;";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = $"SELECT * FROM `{nomeTabela}`";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            StringBuilder dados = new StringBuilder();

                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    dados.Append($"{reader.GetName(i)}: {reader.GetValue(i)}\t");
                                }
                                dados.AppendLine();
                            }

                            MessageBox.Show(dados.ToString(), "Dados Lidos");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erro ao ler dados: {ex.Message}");
                }
            }
        }

        private void LerDadosButton_Click(object sender, RoutedEventArgs e)
        {
            if (TabelasListBox.SelectedItem is string nomeTabela)
                LerDados(nomeTabela);
            else
                MessageBox.Show("Por favor, selecione uma tabela.", "Aviso");
        }

        private void TabelasListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabelasListBox.SelectedItem is string nomeTabela)
                MessageBox.Show($"Tabela selecionada: {nomeTabela}", "Seleção de Tabela");
        }




        // Excluir dados


    }
}
