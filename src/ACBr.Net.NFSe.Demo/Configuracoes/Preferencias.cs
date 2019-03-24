using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ACBr.Net.NFSe.Demo.Configuracoes
{
    public partial class Preferencias : Form
    {
        #region Fields
        private ACBrNFSe acbrNFSe;
        private ACBrConfig config;
        private string DefaultPath;
        #endregion Fields

        public Preferencias()
        {
            InitializeComponent();
            DefaultPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            ExecuteSafe(() =>
            {
                DefaultPath = Directory.CreateDirectory(DefaultPath + @"\Configuracoes").FullName;
            });

            config = ACBrConfig.CreateOrLoad(Path.Combine(DefaultPath, "nfse.config"));

            txtArquivoCidades.Text = config.Get("CaminhoArqCidades", string.Empty) == string.Empty ? DefaultPath : config.Get("CaminhoArqCidades", string.Empty);

            txtFolderConfig.Text = config.Get("CaminhoConfiguracoes", string.Empty) == string.Empty ? DefaultPath : config.Get("CaminhoConfiguracoes", string.Empty);

            SaveConfig();
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                SaveConfig();
            });
        }

        private void ExecuteSafe(Action action)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnFolderConfig_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                txtFolderConfig.Text = Helpers.SelectFolder();
                txtArquivoCidades.Text = txtFolderConfig.Text;
            });
        }

        private void btnSelecionarSchema_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                txtSchemas.Text = Helpers.SelectFolder();
            });
        }

        private void btnPathXml_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                txtPathXml.Text = Helpers.SelectFolder();
            });
        }

        private void SaveConfig()
        {
            ExecuteSafe(() =>
            {
                config.Set("CaminhoConfiguracoes", txtFolderConfig.Text);
                config.Set("CaminhoArqCidades", txtArquivoCidades.Text);

                config.Save();
            });
        }

    }
}
