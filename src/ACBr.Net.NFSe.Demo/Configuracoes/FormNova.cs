using ACbr.Net.Storage.Models;
using ACBr.Net.Core.Extensions;
using ACBr.Net.DFe.Core.Common;
using ACBr.Net.NFSe.Providers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ACbr.Net.Storage.Models;
using LiteDB;

namespace ACBr.Net.NFSe.Demo.Configuracoes
{
    public partial class FormNova : Form
    {
        #region Fields
        private ACBrNFSe acbrNFSe;
        private ACBrConfig config;
        private string ConfigPath;
        private String DataBase;
        private int IdConfig;
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
            cmbAmbiente.EnumDataSource<DFeTipoAmbiente>(DFeTipoAmbiente.Producao);

        }

        public FormNova(string db, int id = 0) : this()
        {
            DataBase = db;
            IdConfig = id;

            if(IdConfig != 0)
            {
                CarregaConfiguracao(IdConfig);
            }
        }

        /*public FormNova(string Configuracao) : this()
        {
            editConfig = ACBrConfig.CreateOrLoad(Configuracao);

            txtConfigName.Text = editConfig.Get("NomeConfig", string.Empty);
            txtCPFCNPJ.Text = editConfig.Get("PrestadorCPFCNPJ", string.Empty);
            txtIM.Text = editConfig.Get("PrestadorIM", string.Empty);
            txtRazaoSocial.Text = editConfig.Get("PrestadorRazaoSocial", string.Empty);
            txtFantasia.Text = editConfig.Get("PrestadorFantasia", string.Empty);
            txtFone.Text = editConfig.Get("PrestadorFone", string.Empty);
            txtCEP.Text = editConfig.Get("PrestadorCEP", string.Empty);
            txtBairro.Text = editConfig.Get("PrestadorBairro", string.Empty);
            txtEndereco.Text = editConfig.Get("PrestadorEndereco", string.Empty);
            txtNumero.Text = editConfig.Get("PrestadorNumero", string.Empty);
            txtComplemento.Text = editConfig.Get("PrestadorComplemento", string.Empty);

            var codMunicipio = editConfig.Get("Municipio", 0);
            if (codMunicipio > 0)
            {
                var municipio = ProviderManager.Municipios.SingleOrDefault(m => m.Codigo == codMunicipio);
                cmbCidades.SelectedItem = municipio;
            }
            txtSenha.Text = editConfig.Get("Senha", string.Empty);
            txtNumeroSerie.Text = editConfig.Get("NumeroSerie", string.Empty);
            UriEnvio.Text = editConfig.Get("UriEnvio", string.Empty);
            UriRetorno.Text = editConfig.Get("UriRetorno", string.Empty);

            var ambiente = editConfig.Get("Ambiente", DFeTipoAmbiente.Producao);

            switch (ambiente)
            {
                case DFeTipoAmbiente.Producao:
                    cmbAmbiente.SelectedIndex = 0;
                    break;
                default:
                    cmbAmbiente.SelectedIndex = 1;
                    break;
            }
        }*/
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
        
        private void CarregaConfiguracao(int id)
        {
            if(id != 0)
            {
                using(var db = new LiteDatabase(DataBase))
                {
                    var prestador = db.GetCollection<Prestador>("prestador").FindById(id);

                    txtConfigName.Text = prestador.NomeConfig;
                    txtCPFCNPJ.Text = prestador.CnpjCpf;
                    txtIM.Text = prestador.IM;
                    txtRazaoSocial.Text = prestador.RazaoSocial;
                    txtFantasia.Text = prestador.NomeFantasia;
                    txtFone.Text = prestador.Telefone;
                    txtEndereco.Text = prestador.Endereco;
                    //txtNumero.Text = prestador.Numero;
                    txtBairro.Text = prestador.Bairro;
                    txtComplemento.Text = prestador.Complemento;
                    txtCEP.Text = prestador.Cep;
                    UriEnvio.Text = prestador.XmlListaPath;
                    UriRetorno.Text = prestador.XmlProcessaPath;
                    txtNumeroSerie.Text = prestador.Serial;
                    txtSenha.Text = prestador.Senha;
                    txtCertificado.Text = prestador.CertPxsPath;

                    var codMunicipio = prestador.CodCidade;
                    if (codMunicipio > 0)
                    {
                        var municipio = ProviderManager.Municipios.SingleOrDefault(x => x.Codigo == codMunicipio);
                        if (municipio != null)
                        {
                            cmbCidades.SelectedItem = municipio;
                            txtUf.Text = municipio.UF.ToString();
                            txtCodSiafi.Text = municipio.CodigoSiafi.ToString();
                            txtCodCidade.Text = municipio.Codigo.ToString();

                            acbrNFSe.Configuracoes.WebServices.CodigoMunicipio = municipio.Codigo;
                            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.Municipio = municipio.Nome;
                            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.CodigoMunicipio = municipio.Codigo;
                            acbrNFSe.Configuracoes.PrestadorPadrao.Endereco.Uf = municipio.UF.ToString();
                        }
                    }

                    switch (prestador.Ambiente)
                    {
                        case "Produção":
                            cmbAmbiente.SelectedIndex = 0;
                            break;
                        case "Homologação":
                            cmbAmbiente.SelectedIndex = 1;
                            break;
                        default:
                            cmbAmbiente.SelectedIndex = 0;
                            break;
                    }

                }
            }
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
            var cnpjCpf = txtCPFCNPJ.Text.OnlyNumbers();
            var cidade = cmbCidades.SelectedText.Trim();
            var Uf = txtUf.Text;
            var codCidade = int.Parse(txtCodCidade.Text);
            var codSiaf = int.Parse(txtCodSiafi.Text);
            
            if (ValidarCampos())
            {
                int count = 0;

                using (var db = new LiteDatabase(DataBase))
                {
                    var prestadores = db.GetCollection<Prestador>();

                    if (IdConfig == 0)
                    {
                        count = prestadores.Find(p => p.CodCidade == codCidade && p.CnpjCpf == cnpjCpf).Count();
                        if (count == 0)
                        {

                            var prestador = new Prestador
                            {
                                NomeConfig = txtConfigName.Text,
                                CnpjCpf = txtCPFCNPJ.Text.OnlyNumbers(),
                                IM = txtIM.Text,
                                RazaoSocial = txtRazaoSocial.Text,
                                NomeFantasia = txtFantasia.Text ?? "",
                                Telefone = txtFone.Text.OnlyNumbers(),
                                Cep = txtCEP.Text.OnlyNumbers(),
                                Endereco = txtEndereco.Text ?? "",
                                Complemento = txtComplemento.Text ?? "",
                                Bairro = txtBairro.Text ?? "",
                                Cidade = cmbCidades.SelectedText,
                                Uf = txtUf.Text,
                                CodCidade = int.Parse(txtCodCidade.Text),
                                CodSiaf = int.Parse(txtCodSiafi.Text),

                                CertPxsPath = txtCertificado.Text ?? "",
                                Senha = txtSenha.Text ?? "",
                                Serial = txtNumeroSerie.Text ?? "",
                                XmlListaPath = UriEnvio.Text,
                                XmlProcessaPath = UriRetorno.Text,

                                Ambiente = cmbAmbiente.SelectedValue.ToString(),
                            };

                            ExecuteSafe(() =>
                            {
                                prestadores.Insert(prestador);
                            });
                        }
                        else
                        {
                            MessageBox.Show("A configuração para este CNPJ e Cidade já existe.", "Aviso!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        if (ValidarCampos())
                        {
                            var update = prestadores.FindById(IdConfig);

                            update.NomeConfig = txtConfigName.Text;
                            update.CnpjCpf = txtCPFCNPJ.Text.OnlyNumbers();
                            update.IM = txtIM.Text;
                            update.RazaoSocial = txtRazaoSocial.Text;
                            update.NomeFantasia = txtFantasia.Text ?? "";
                            update.Telefone = txtFone.Text.OnlyNumbers();
                            update.Cep = txtCEP.Text.OnlyNumbers();
                            update.Endereco = txtEndereco.Text ?? "";
                            update.Complemento = txtComplemento.Text ?? "";
                            update.Bairro = txtBairro.Text ?? "";
                            update.Cidade = cmbCidades.SelectedText;
                            update.Uf = txtUf.Text;
                            update.CodCidade = int.Parse(txtCodCidade.Text);
                            update.CodSiaf = int.Parse(txtCodSiafi.Text);

                            update.CertPxsPath = txtCertificado.Text ?? "";
                            update.Senha = txtSenha.Text ?? "";
                            update.Serial = txtNumeroSerie.Text ?? "";
                            update.XmlListaPath = UriEnvio.Text;
                            update.XmlProcessaPath = UriRetorno.Text;

                            update.Ambiente = cmbAmbiente.SelectedValue.ToString();

                            prestadores.Update(update);
                        }

                    }

                }

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
