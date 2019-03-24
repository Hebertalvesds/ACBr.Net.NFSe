using ACBr.Net.Core.Extensions;
using ACBr.Net.DFe.Core.Common;
using ACBr.Net.NFSe.Providers;
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
    public partial class FormNova : Form
    {
        #region Fields
        private ACBrNFSe acbrNFSe;
        private ACBrConfig config;
        private string ConfigPath;
        #endregion Fields

        #region Constructors
        public FormNova()
        {
            InitializeComponent();
            config = ACBrConfig.CreateOrLoad();
            UpdateCidades();
            ConfigPath = Application.StartupPath + @"\Configuracoes";
            config = ACBrConfig.CreateOrLoad(Path.Combine(ConfigPath, "nfse.config"));
            acbrNFSe = new ACBrNFSe();

            var municipioArq = config.Get("CaminhoArqCidades", string.Empty) + @"\Municipios.nfse";

            if (!municipioArq.IsEmpty()) LoadMunicipios(municipioArq);
        }
        #endregion Constructors
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

        private bool SaveNewConfig()
        {
            if (ValidarCampos())
            {
                var specificConfig = ACBrConfig.CreateOrLoad(System.IO.Path.Combine(ConfigPath, txtConfigName.Text + ".config"));

                specificConfig.Set("NomeConfig", txtConfigName.Text);
                specificConfig.Set("PrestadorCPFCNPJ", txtCPFCNPJ.Text.OnlyNumbers());
                specificConfig.Set("PrestadorIM", txtIM.Text.OnlyNumbers());
                specificConfig.Set("PrestadorRazaoSocial", txtRazaoSocial.Text);
                specificConfig.Set("PrestadorFantasia", txtFantasia.Text);
                specificConfig.Set("PrestadorFone", txtFone.Text);
                specificConfig.Set("PrestadorCEP", txtCEP.Text);
                specificConfig.Set("PrestadorEndereco", txtEndereco.Text);
                specificConfig.Set("PrestadorNumero", txtNumero.Text);
                specificConfig.Set("PrestadorComplemento", txtComplemento.Text);
                specificConfig.Set("PrestadorBairro", txtBairro.Text);

                specificConfig.Set("Municipio", txtCodCidade.Text.OnlyNumbers());

                //specificConfig.Set("Ambiente", cmbAmbiente.GetSelectedValue<DFeTipoAmbiente>());

                specificConfig.Set("Certificado", txtCertificado.Text);
                specificConfig.Set("Senha", txtSenha.Text);
                specificConfig.Set("NumeroSerie", txtNumeroSerie.Text);

                //specificConfig.Set("CaminhoArqCidades", txtArquivoCidades.Text);
                specificConfig.Set("UriEnvio", UriEnvio.Text);
                specificConfig.Set("UriRetorno", UriRetorno.Text);

                specificConfig.Save();

                return true;
            }

            return false;

        }

        private bool ValidarCampos()
        {
            bool noError = true;
            txtErrors.Text = "";
            if (txtConfigName.Text.IsEmpty())
            {
                txtErrors.Text += "Nome da configuração está vazio. \n";
                noError = false;
            }

            if (txtCPFCNPJ.Text.IsEmpty())
            {
                txtErrors.Text += "CNPJ está vázio. \n";
                noError = false;
            }

            if (txtRazaoSocial.Text.IsEmpty())
            {
                txtErrors.Text += "Razão Social esta vázia. \n";
                noError = false;
            }

            if (txtIM.Text.IsEmpty())
            {
                txtErrors.Text += "Inscrição Municipal esta vázio. \n";
                noError = false;
            }

            if (UriEnvio.Text.IsEmpty())
            {
                txtErrors.Text += "Caminho dos XML's envio deve ser preenchido. \n";
                noError = false;
            }

            if (UriRetorno.Text.IsEmpty())
            {
                txtErrors.Text += "Caminhos dos XML's retorno deve ser preenchido. \n";
                noError = false;
            }

            if (txtUf.Text.IsEmpty())
            {
                txtErrors.Text += "Preencha o código da cidade. \n";
                noError = false;
            }

            if (txtCodCidade.Text.IsEmpty())
            {
                txtErrors.Text += "Preencha o código da cidade. \n";
                noError = false;
            }

            if (txtCodSiafi.Text.IsEmpty())
            {
                txtErrors.Text += "Preencha o código SIAF. \n";
                noError = false;
            }
            
            return noError;
        }

        private void UpdateCidades()
        {
            cmbCidades.DataSource = null;
            cmbCidades.Items.Clear();
            cmbCidades.DisplayMember = "Nome";
            cmbCidades.ValueMember = "Codigo";
            cmbCidades.DataSource = ProviderManager.Municipios;
        }

        private void LoadMunicipios(string arquivo = "")
        {
            ExecuteSafe(() =>
            {
                if (arquivo.IsEmpty())
                    arquivo = Helpers.OpenFile("Arquivo de cidades NFSe (*.nfse)|*.nfse*|Todos os arquivos|*.*", "Selecione o arquivo de cidades");

                if (arquivo.IsEmpty()) return;

                ProviderManager.Load(arquivo);

                LoadData();
            });
        }

        private void LoadData()
        {
            var itens = new List<ListViewItem>();

            foreach (var municipio in ProviderManager.Municipios)
            {
                var item = new ListViewItem(municipio.Nome);
                item.SubItems.Add(municipio.UF.ToString());
                item.SubItems.Add(municipio.Codigo.ToString());
                item.SubItems.Add(municipio.CodigoSiafi.ToString());
                item.SubItems.Add(municipio.Provedor.GetDescription());
                item.Tag = municipio;

                itens.Add(item);
            }

            lstMunicipios.BeginUpdate();

            lstMunicipios.Items.Clear();
            lstMunicipios.Items.AddRange(itens.ToArray());

            lstMunicipios.EndUpdate();

            UpdateCidades();
        }

        #region Events
        private void btnGetCertificate_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                var numeroSerie = acbrNFSe.Configuracoes.Certificados.SelecionarCertificado();
                txtNumeroSerie.Text = numeroSerie;
            });
        }

        private void btnFindCertificate_Click(object sender, EventArgs e)
        {
            ExecuteSafe(() =>
            {
                var file = Helpers.OpenFile("Certificate Files (*.pfx)|*.pfx|All Files (*.*)|*.*", "Selecione o certificado");
                txtCertificado.Text = file;
            });
        }

        private void btnSalvar_Click(object sender, EventArgs e)
        {
            if (SaveNewConfig())
            {
                MessageBox.Show("Arquivo de configurações criado em: " + ConfigPath, "Sucesso!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Hide();
                Dispose();
            }
        }
        #endregion Events

        #region Fields
        private void txtConfigName_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.PrestadorPadrao.CpfCnpj = txtCPFCNPJ.Text.OnlyNumbers();
        }

        private void txtFone_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.PrestadorPadrao.DadosContato.Telefone = txtFone.Text;
        }

        private void txtCEP_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.Cep = txtCEP.Text;
        }

        private void txtIM_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.PrestadorPadrao.InscricaoMunicipal = txtIM.Text.OnlyNumbers();
        }

        private void txtNumero_TextChanged(object sender, EventArgs e)
        {
            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.Numero = txtNumero.Text;
        }
        #endregion Fields

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            Dispose();
            Hide();
        }

        private void cmbCidades_SelectedValueChanged(object sender, EventArgs e)
        {
            var municipio = (ACBrMunicipioNFSe)cmbCidades.SelectedItem;
            if (municipio == null) return;

            txtUf.Text = municipio.UF.ToString();
            txtCodCidade.Text = municipio.Codigo.ToString();
            txtCodSiafi.Text = municipio.CodigoSiafi.ToString();

            acbrNFSe.Configuracoes.WebServices.CodigoMunicipio = municipio.Codigo;
            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.Municipio = municipio.Nome;
            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.CodigoMunicipio = municipio.Codigo;
            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.Uf = municipio.UF.ToString();
        }
    }
}
